using DiscordUnity2.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Text;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Collections.Generic;
using DiscordUnity2.API;
using DiscordUnity2.State;

namespace DiscordUnity2
{
    // Finish Http Calls
    // - Extra guild calls and some others
    // Finish Internal Models
    // - AuditLogChangeKey?
    // Finish State
    // - More and better states
    // - Fill in easy access methods to Http Calls

    public static partial class DiscordAPI
    {
        private static string url;
        private static string token;
        private static string session;
        private static bool acked = false;
        private static int sequence;

        private static Task listener;
        private static ClientWebSocket socket;

        public static bool IsActive { get; private set; }
        public static ILogger Logger { get; set; }

        internal static readonly JsonSerializer JsonSerializer;
        private static readonly JsonSerializerSettings JsonSettings;
        private static readonly SemaphoreSlim sendLock;
        private static TaskCompletionSource<bool> startTask;
        private static Queue<Action> callbacks;
        internal static CancellationTokenSource CancelSource;
        internal static DiscordInterfaces interfaces;

        static DiscordAPI()
        {
            Logger = new Logger();
            interfaces = new DiscordInterfaces();
            sendLock = new SemaphoreSlim(1, 1);
            callbacks = new Queue<Action>();

            JsonSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                }
            };

            JsonSerializer = JsonSerializer.CreateDefault(JsonSettings);
        }

        public static void RegisterEventsHandler(IDiscordInterface e)
            => interfaces.AddEventHandler(e);
        public static void UnregisterEventsHandler(IDiscordInterface e) 
            => interfaces.RemoveEventHandler(e);
        internal static void Sync(Action callback)
            => callbacks.Enqueue(callback);

        /// <summary> Starts DiscordUnity with a bot token. </summary>
        /// <param name="botToken">A token received by creating a bot on Discord's developer portal.</param>
        public static async Task<bool> StartWithBot(string botToken)
        {
            if (IsActive) return false;

            if (string.IsNullOrWhiteSpace(botToken))
            {
                Logger.LogError("Token is invalid!");
                return false;
            }

            token = botToken;
            IsActive = true;
            InitializeState();
            CancelSource = new CancellationTokenSource();
            Client = new HttpClient();
            Client.DefaultRequestHeaders.Add("User-Agent", $"DiscordBot ({"https://github.com/DiscordUnity/DiscordUnity"}, {"1.0"})");
            Client.DefaultRequestHeaders.Add("Authorization", $"Bot {token}");

            var gatewayResult = await GetBotGateway();

            if (!gatewayResult)
            {
                IsActive = false;
                Logger.LogError("Retrieving gateway failed: " + gatewayResult.Exception);
                Stop();
                return false;
            }

            url = gatewayResult.Data.Url + "?v=6&encoding=json";
            Logger.Log("Gateway received: " + url);

            socket = new ClientWebSocket();
            await socket.ConnectAsync(new Uri(url), CancelSource.Token);

            if (socket.State != WebSocketState.Open)
            {
                IsActive = false;
                Logger.LogError("Could not connect with Discord: " + socket.CloseStatusDescription);
                Stop();
                return false;
            }

            Logger.Log("Connected.");
            listener = Listen();

            PayloadModel<IdentityModel> identity = new PayloadModel<IdentityModel>
            {
                Op = 2,
                Data = new IdentityModel
                {
                    Token = token,
                    Properties = new Dictionary<string,string>()
                    {
                        { "$os", Environment.OSVersion.ToString() },
                        { "$browser", "DiscordUnity" },
                        { "$device", "DiscordUnity" }
                    }
                }
            };

            startTask = new TaskCompletionSource<bool>();
            await Send(JsonConvert.SerializeObject(identity, JsonSettings));
            return await startTask.Task;
        }

        /// <summary> Stops DiscordUnity. </summary>
        public static void Stop()
        {
            if (startTask != null && !startTask.Task.IsCompleted)
                Sync(() => startTask.SetResult(false));

            startTask = null;
            IsActive = false;
            url = null;
            token = null;
            session = null;
            socket?.Dispose();
            socket = null;
            Client?.Dispose();
            Client = null;
            CancelSource?.Cancel();
            CancelSource = null;
            Logger.Log("DiscordUnity stopped.");
            interfaces.OnDiscordAPIClosed();
        }

        /// <summary> Updates DiscordUnity and hooks async calls back to calling thread. Without this, DiscordUnity will not function. </summary>
        public static void Update()
        {
            while (callbacks.Count > 0)
            {
                try
                {
                    callbacks.Dequeue()();
                }

                catch (Exception e)
                {
                    Logger.LogError("Error occured in a callback.", e);
                }
            }
        }

        private static async Task Resume()
        {
            listener.Dispose();
            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Failed heartbeat ack", CancelSource.Token);
            await socket.ConnectAsync(new Uri(url), CancelSource.Token);
            listener = Listen();

            PayloadModel<ResumeModel> resume = new PayloadModel<ResumeModel>
            {
                Op = 6,
                Data = new ResumeModel
                {
                    Token = token,
                    SessionId = session,
                    Sequence = sequence
                }
            };

            await Send(JsonConvert.SerializeObject(resume, JsonSettings));
        }

        private static async Task Listen()
        {
            byte[] buffer = new byte[8192];

            while (IsActive && socket?.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancelSource.Token);

                if (!result.EndOfMessage)
                {
                    Logger.LogWarning("Received unexpected partial message.");
                    continue;
                }

                switch (result.MessageType)
                {
                    case WebSocketMessageType.Text:
                        await OnSocketMessage(Encoding.UTF8.GetString(buffer));
                        break;

                    case WebSocketMessageType.Close:
                        Logger.LogError($"Socket closed: ({socket.CloseStatus}) {socket.CloseStatusDescription}");
                        Stop();
                        return;

                    default:
                    case WebSocketMessageType.Binary:
                        Logger.LogWarning("Received unexpected type of message: " + result.MessageType);
                        break;
                }

                buffer = new byte[8192];
            }
        }

        private static async void Heartbeat(int interval)
        {
            while (IsActive && socket?.State == WebSocketState.Open)
            {
                if (acked)
                {
                    acked = false;

                    await Send(JsonConvert.SerializeObject(new PayloadModel<int>
                    {
                        Op = 1,
                        Data = sequence
                    }, JsonSettings));

                    Logger.Log("Heartbeat");
                    await Task.Delay(interval);
                }

                else
                {
                    await Resume();
                }
            }
        }

        private static async Task Send(string message)
        {
            Logger.Log("Send Message: " + message);
            var encoded = Encoding.UTF8.GetBytes(message);
            var buffer = new ArraySegment<byte>(encoded, 0, encoded.Length);

            await sendLock.WaitAsync();

            try
            {
                await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancelSource.Token);
            }

            finally
            {
                sendLock.Release();
            }
        }

        private static async Task OnSocketMessage(string json)
        {
            try
            {
                Logger.Log("Received Message: " + json);
                PayloadModel payload = JsonConvert.DeserializeObject<PayloadModel>(json, JsonSettings);

                if (payload.Sequence.HasValue)
                    sequence = payload.Sequence.Value;

                switch (payload.Op)
                {
                    case 0: // Event Dispatch
                        break;
                    case 1: // Heartbeat
                        await Send(JsonConvert.SerializeObject(new PayloadModel<int>
                        {
                            Op = 1,
                            Data = sequence
                        }, JsonSettings));

                        Logger.Log("Heartbeat");
                        break;
                    case 7: // Reconnect
                        Logger.Log("Reconnect Requested");
                        await Resume();
                        break;
                    case 9: // Invalid Session
                        Logger.LogError("Received invalid session from discord.");
                        var resume = payload.As<bool>().Data;
                        if (resume) await Resume();
                        else Stop();
                        break;
                    case 10: // Hello
                        acked = true;
                        var heartbeat = payload.As<HeartbeatModel>().Data;
                        Heartbeat(heartbeat.HeartbeatInterval);
                        break;
                    case 11: // Heartbeat Ack
                        Logger.Log("Heatbeat Ack");
                        acked = true;
                        break;
                }

                if (!string.IsNullOrEmpty(payload.Event))
                {
                    switch (payload.Event.ToLower())
                    {
                        // ------ API ------ 

                        case "ready":
                            {
                                Logger.Log("Ready.");

                                var ready = payload.As<ReadyModel>().Data;

                                Sync(() =>
                                {
                                    Version = ready.Version;
                                    session = ready.SessionId;
                                    User = new DiscordUser(ready.User);

                                    foreach (var channel in ready.PrivateChannels)
                                        PrivateChannels[channel.Id] = new DiscordChannel(channel);

                                    foreach (var guild in ready.Guilds)
                                        Servers[guild.Id] = new DiscordServer(guild);

                                    startTask.SetResult(true);
                                    interfaces.OnDiscordAPIOpen();
                                });
                            }
                            break;
                        case "resumed":
                            {
                                Logger.Log("Resumed.");
                                Sync(() => interfaces.OnDiscordAPIResumed());
                            }
                            break;
                        case "reonnect":
                            {
                                Logger.Log("Reconnect.");
                                await Resume();
                            }
                            break;

                        //  ------ Channels ------ 

                        case "channel_create":
                            {
                                var channel = payload.As<ChannelModel>().Data;

                                Sync(() =>
                                {
                                    if (!string.IsNullOrEmpty(channel.GuildId))
                                    {
                                        Servers[channel.GuildId].Channels[channel.Id] = new DiscordChannel(channel);
                                        interfaces.OnChannelCreated(Servers[channel.GuildId].Channels[channel.Id]);
                                    }

                                    else
                                    {
                                        PrivateChannels[channel.Id] = new DiscordChannel(channel);
                                        interfaces.OnChannelCreated(Servers[channel.GuildId].Channels[channel.Id]);
                                    }
                                });
                            }
                            break;
                        case "channel_update":
                            {
                                var channel = payload.As<ChannelModel>().Data;

                                Sync(() =>
                                {
                                    if (!string.IsNullOrEmpty(channel.GuildId))
                                    {
                                        Servers[channel.GuildId].Channels[channel.Id] = new DiscordChannel(channel);
                                        interfaces.OnChannelUpdated(Servers[channel.GuildId].Channels[channel.Id]);
                                    }

                                    else
                                    {
                                        PrivateChannels[channel.Id] = new DiscordChannel(channel);
                                        interfaces.OnChannelUpdated(Servers[channel.GuildId].Channels[channel.Id]);
                                    }
                                });
                            }
                            break;
                        case "channel_delete":
                            {
                                var channel = payload.As<ChannelModel>().Data;

                                Sync(() =>
                                {
                                    if (!string.IsNullOrEmpty(channel.GuildId))
                                    {
                                        interfaces.OnChannelDeleted(Servers[channel.GuildId].Channels[channel.Id]);
                                        Servers[channel.GuildId].Channels.Remove(channel.Id);
                                    }

                                    else
                                    {
                                        interfaces.OnChannelDeleted(Servers[channel.GuildId].Channels[channel.Id]);
                                        PrivateChannels.Remove(channel.Id);
                                    }
                                });
                            }
                            break;
                        case "channel_pins_update":
                            {
                                var channelPin = payload.As<ChannelPinsModel>().Data;

                                Sync(() =>
                                {
                                    if (!string.IsNullOrEmpty(channelPin.GuildId))
                                    {
                                        Servers[channelPin.GuildId].Channels[channelPin.ChannelId].LastPinTimestamp = channelPin.LastPinTimestamp;
                                        interfaces.OnChannelPinsUpdated(Servers[channelPin.GuildId].Channels[channelPin.ChannelId], channelPin.LastPinTimestamp);
                                    }

                                    else
                                    {
                                        PrivateChannels[channelPin.ChannelId].LastPinTimestamp = channelPin.LastPinTimestamp;
                                        interfaces.OnChannelPinsUpdated(Servers[channelPin.GuildId].Channels[channelPin.ChannelId], channelPin.LastPinTimestamp);
                                    }
                                });
                            }
                            break;

                        //  ------ Servers ------ 

                        case "guild_create":
                            {
                                var guild = payload.As<GuildModel>().Data;

                                Sync(() =>
                                {
                                    Servers[guild.Id] = new DiscordServer(guild);
                                    interfaces.OnServerJoined(Servers[guild.Id]);
                                });
                            }
                            break;
                        case "guild_update":
                            {
                                var guild = payload.As<GuildModel>().Data;

                                Sync(() =>
                                {
                                    Servers[guild.Id] = new DiscordServer(guild);
                                    interfaces.OnServerUpdated(Servers[guild.Id]);
                                });
                            }
                            break;
                        case "guild_delete":
                            {
                                var guild = payload.As<GuildModel>().Data;

                                Sync(() =>
                                {
                                    interfaces.OnServerLeft(Servers[guild.Id]);
                                    Servers.Remove(guild.Id);
                                });
                            }
                            break;
                        case "guild_ban_add":
                            {
                                var guildBan = payload.As<GuildBanModel>().Data;

                                Sync(() =>
                                {
                                    Servers[guildBan.GuildId].Bans[guildBan.User.Id] = new DiscordUser(guildBan.User);
                                    interfaces.OnServerBan(Servers[guildBan.GuildId], Servers[guildBan.GuildId].Bans[guildBan.User.Id]);
                                });
                            }
                            break;
                        case "guild_ban_remove":
                            {
                                var guildBan = payload.As<GuildBanModel>().Data;

                                Sync(() =>
                                {
                                    interfaces.OnServerUnban(Servers[guildBan.GuildId], Servers[guildBan.GuildId].Bans[guildBan.User.Id]);
                                    Servers[guildBan.GuildId].Bans.Remove(guildBan.User.Id);
                                });
                            }
                            break;
                        case "guild_emojis_update":
                            {
                                var guildEmojis = payload.As<GuildEmojisModel>().Data;
                                Servers[guildEmojis.GuildId].Emojis = guildEmojis.Emojis.ToDictionary(x => x.Id, x => new DiscordEmoji(x));
                            }
                            break;
                        case "guild_member_add":
                            {
                                var guildMember = payload.As<GuildMemberModel>().Data;
                                Servers[guildMember.GuildId].Members[guildMember.User.Id] = new DiscordServerMember(guildMember);
                            }
                            break;
                        case "guild_member_remove":
                            {
                                var guildMember = payload.As<GuildMemberModel>().Data;
                                Servers[guildMember.GuildId].Members.Remove(guildMember.User.Id);
                            }
                            break;
                        case "guild_member_update":
                            {
                                var guildMember = payload.As<GuildMemberModel>().Data;
                                Servers[guildMember.GuildId].Members[guildMember.User.Id] = new DiscordServerMember(guildMember);
                            }
                            break;
                        case "guild_members_chunk":
                            {
                                var guildMembersChunk = payload.As<GuildMembersChunkModel>().Data;
                            }
                            break;
                        case "guild_role_create":
                            {
                                var guildRole = payload.As<GuildRoleModel>().Data;
                                Servers[guildRole.GuildId].Roles[guildRole.Role.Id] = new DiscordRole(guildRole.Role);
                            }
                            break;
                        case "guild_role_update":
                            {
                                var guildRole = payload.As<GuildRoleModel>().Data;
                                Servers[guildRole.GuildId].Roles[guildRole.Role.Id] = new DiscordRole(guildRole.Role);
                            }
                            break;
                        case "guild_role_delete":
                            {
                                var guildRoleId = payload.As<GuildRoleIdModel>().Data;
                                Servers[guildRoleId.GuildId].Roles.Remove(guildRoleId.RoleId);
                            }
                            break;

                        //  ------ Invites ------ 

                        case "invite_create":
                            {
                                var invite = payload.As<InviteModel>().Data;
                                Servers[invite.GuildId].Invites[invite.Code] = new DiscordInvite(invite);
                            }
                            break;
                        case "invite_delete":
                            {
                                var invite = payload.As<InviteModel>().Data;
                                Servers[invite.GuildId].Invites.Remove(invite.Code);
                            }
                            break;

                        //  ------ Messages ------ 

                        case "message_create":
                            {
                                var message = payload.As<MessageModel>().Data;

                                Sync(() =>
                                {
                                    interfaces.OnMessageCreated(new DiscordMessage(message));
                                });
                            }
                            break;
                        case "message_update":
                            {
                                var message = payload.As<MessageModel>().Data;

                                Sync(() =>
                                {
                                    interfaces.OnMessageUpdated(new DiscordMessage(message));
                                });
                            }
                            break;
                        case "message_delete":
                            {
                                var message = payload.As<MessageModel>().Data;

                                Sync(() =>
                                {
                                    interfaces.OnMessageDeleted(new DiscordMessage(message));
                                });
                            }
                            break;
                        case "message_delete_bulk":
                            {
                                var messageBulk = payload.As<MessageBulkModel>().Data;
                            }
                            break;
                        case "message_reaction_add":
                            {
                                var messageReaction = payload.As<MessageReactionModel>().Data;
                            }
                            break;
                        case "message_reaction_remove":
                            {
                                var messageReaction = payload.As<MessageReactionModel>().Data;
                            }
                            break;
                        case "message_reaction_remove_all":
                            {
                                var messageReaction = payload.As<MessageReactionModel>().Data;
                            }
                            break;
                        case "message_reaction_remove_emoji":
                            {
                                var messageReaction = payload.As<MessageReactionModel>().Data;
                            }
                            break;

                        //  ------ Status ------ 

                        case "presence_update":
                            {
                                var presence = payload.As<PresenceModel>().Data;
                            }
                            break;

                        case "typing_start":
                            {
                                var typing = payload.As<TypingModel>().Data;
                            }
                            break;

                        case "user_update":
                            {
                                var user = payload.As<UserModel>().Data;
                            }
                            break;

                        //  ------ Voice ------ 

                        case "voice_state_update":
                            {
                                var voiceState = payload.As<VoiceStateModel>().Data;
                            }
                            break;
                        case "voice_server_update":
                            {
                                var voiceServer = payload.As<VoiceServerModel>().Data;
                            }
                            break;

                        //  ------ Webhooks ------ 

                        case "webhooks_update":
                            {
                                var webhook = payload.As<ServerWebhookModel>().Data;
                            }
                            break;
                    }
                }
            }

            catch (Exception exception)
            {
                Logger.LogError("Exception occured while processing message.", exception);
            }
        }
    }
}

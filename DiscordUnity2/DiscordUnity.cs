﻿using DiscordUnity2.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Text;
using System.Net.Http;
using System.Threading;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Collections.Generic;
using DiscordUnity2.Rest;

namespace DiscordUnity2
{
    // Create API - Think of a infrastructure
    // Finish Models
    // - Audio Logs
    // - Invite
    // Finish Http Calls
    // - All calls

    public static class DiscordUnity
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

        internal static JsonSerializer JsonSerializer => JsonSerializer.CreateDefault(JsonSettings);
        private static readonly JsonSerializerSettings JsonSettings;
        private static readonly SemaphoreSlim sendLock;
        private static TaskCompletionSource<bool> startTask;
        internal static Queue<Action> callbacks;
        internal static CancellationTokenSource CancelSource;

        static DiscordUnity()
        {
            Logger = new Logger();
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
        }

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

            CancelSource = new CancellationTokenSource();
            DiscordRest.Client = new HttpClient();
            DiscordRest.Client.DefaultRequestHeaders.Add("User-Agent", $"DiscordBot ({"https://github.com/DiscordUnity/DiscordUnity"}, {"1.0"})");
            DiscordRest.Client.DefaultRequestHeaders.Add("Authorization", $"Bot {token}");

            var gatewayResult = await DiscordRest.GetBotGateway();

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
                callbacks.Enqueue(() => startTask.SetResult(false));

            startTask = null;
            IsActive = false;
            url = null;
            token = null;
            session = null;
            socket?.Dispose();
            socket = null;
            DiscordRest.Client?.Dispose();
            DiscordRest.Client = null;
            CancelSource?.Cancel();
            CancelSource = null;
            Logger.Log("DiscordUnity stopped.");
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
                    Logger.LogWarning("Error occured in a callback: " + e.Message);
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
                        case "ready":
                            {
                                Logger.Log("Ready.");

                                var ready = payload.As<ReadyModel>().Data;

                                callbacks.Enqueue(() => startTask.SetResult(true));
                            }
                            break;
                        case "resumed":
                            {
                                Logger.Log("Resumed.");
                            }
                            break;
                        case "reonnect":
                            {
                                Logger.Log("Reconnect.");
                                await Resume();
                            }
                            break;

                        case "channel_create":
                            {
                                var channel = payload.As<ChannelModel>().Data;
                            }
                            break;
                        case "channel_update":
                            {
                                var channel = payload.As<ChannelModel>().Data;
                            }
                            break;
                        case "channel_delete":
                            {
                                var channel = payload.As<ChannelModel>().Data;
                            }
                            break;
                        case "channel_pins_update":
                            {
                                var channelPins = payload.As<ChannelPinsModel>().Data;
                            }
                            break;

                        case "guild_create":
                            {
                                var guild = payload.As<GuildModel>().Data;
                            }
                            break;
                        case "guild_update":
                            {
                                var guild = payload.As<GuildModel>().Data;
                            }
                            break;
                        case "guild_delete":
                            {
                                var guild = payload.As<GuildModel>().Data;
                            }
                            break;
                        case "guild_ban_add":
                            {
                                var guildBan = payload.As<GuildBanModel>().Data;
                            }
                            break;
                        case "guild_ban_remove":
                            {
                                var guildBan = payload.As<GuildBanModel>().Data;
                            }
                            break;
                        case "guild_emojis_update":
                            {
                                var guildEmojis = payload.As<GuildEmojisModel>().Data;
                            }
                            break;
                        case "guild_member_add":
                            {
                                var guildMember = payload.As<GuildMemberModel>().Data;
                            }
                            break;
                        case "guild_member_remove":
                            {
                                var guildMember = payload.As<GuildMemberModel>().Data;
                            }
                            break;
                        case "guild_member_update":
                            {
                                var guildMember = payload.As<GuildMemberModel>().Data;
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
                            }
                            break;
                        case "guild_role_update":
                            {
                                var guildRole = payload.As<GuildRoleModel>().Data;
                            }
                            break;
                        case "guild_role_delete":
                            {
                                var guildRoleId = payload.As<GuildRoleIdModel>().Data;
                            }
                            break;

                        case "invite_create":
                            {
                                var invite = payload.As<InviteModel>().Data;
                            }
                            break;
                        case "invite_delete":
                            {
                                var invite = payload.As<InviteModel>().Data;
                            }
                            break;

                        case "message_create":
                            {
                                var message = payload.As<MessageModel>().Data;
                            }
                            break;
                        case "message_update":
                            {
                                var message = payload.As<MessageModel>().Data;
                            }
                            break;
                        case "message_delete":
                            {
                                var message = payload.As<MessageModel>().Data;
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

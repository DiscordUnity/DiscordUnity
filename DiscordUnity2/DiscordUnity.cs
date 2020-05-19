using DiscordUnity2.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Text;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.WebSockets;

namespace DiscordUnity2
{
    public static class DiscordUnity
    {
        private const string API = "https://discord.com/api";

        private static string url;
        private static string token;
        private static string session;
        private static Task listener;
        private static HttpClient client;
        private static ClientWebSocket socket;
        private static CancellationTokenSource cancelSource;
        private static bool acked = false;
        private static int sequence;

        public static bool IsActive { get; private set; }
        public static ILogger Logger { get; set; }

        private static readonly JsonSerializerSettings JsonSettings;
        private static readonly SemaphoreSlim sendLock;
        private static TaskCompletionSource<bool> startTask;
        internal static Queue<Action> callbacks;

        static DiscordUnity()
        {
            Logger = new Logger();
            sendLock = new SemaphoreSlim(1, 1);
            callbacks = new Queue<Action>();
            JsonSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
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

            cancelSource = new CancellationTokenSource();
            client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bot {token}");
            HttpResponseMessage response = await client.GetAsync(API + "/gateway/bot", cancelSource.Token);

            if (!response.IsSuccessStatusCode)
            {
                IsActive = false;
                Logger.LogError("Retrieving gateway failed: " + response.ReasonPhrase);
                Stop();
                return false;
            }

            GatewayModel gateway = JsonConvert.DeserializeObject<GatewayModel>(await response.Content.ReadAsStringAsync());
            url = gateway.Url;
            Logger.Log("Gateway received!");

            socket = new ClientWebSocket();
            await socket.ConnectAsync(new Uri(url + "?v=6&encoding=json"), cancelSource.Token);

            if (socket.State != WebSocketState.Open)
            {
                IsActive = false;
                Logger.LogError("Could not connect with Discord: " + socket.CloseStatusDescription);
                Stop();
                return false;
            }

            Logger.Log("Connected!");
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
            client?.Dispose();
            client = null;
            cancelSource?.Cancel();
            cancelSource = null;
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
            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Failed heartbeat ack", cancelSource.Token);
            await socket.ConnectAsync(new Uri(url + "?v=6&encoding=json"), cancelSource.Token);
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
                WebSocketReceiveResult result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), cancelSource.Token);

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
                await socket.SendAsync(buffer, WebSocketMessageType.Text, true, cancelSource.Token);
            }

            finally
            {
                sendLock.Release();
            }
        }

        private static async Task OnSocketMessage(string message)
        {
            try
            {
                Logger.Log("Received Message: " + message);
                PayloadModel payload = JsonConvert.DeserializeObject<PayloadModel>(message);

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
                        Stop();
                        break;
                    case 10: // Hello
                        acked = true;
                        var heartbeat = payload.As<HeartbeatModel>();
                        Heartbeat(heartbeat.Data.HeartbeatInterval);
                        break;
                    case 11: // Heartbeat Ack
                        Logger.Log("Heatbeat Ack");
                        acked = true;
                        break;
                }

                if (!string.IsNullOrEmpty(payload.Event))
                {
                    Logger.Log("Received event: " + payload.Event.ToLower());

                    switch (payload.Event.ToLower())
                    {
                        case "ready":
                            {
                                Logger.Log("Ready!");
                                callbacks.Enqueue(() => startTask.SetResult(true));
                            }
                            break;
                        case "resumed":
                            {

                            }
                            break;
                        case "reonnect":
                            {
                                await Resume();
                            }
                            break;

                        case "channel_create":
                            {

                            }
                            break;
                        case "channel_update":
                            {

                            }
                            break;
                        case "channel_delete":
                            {

                            }
                            break;
                        case "channel_pins_update":
                            {

                            }
                            break;

                        case "guild_create":
                            {

                            }
                            break;
                        case "guild_update":
                            {

                            }
                            break;
                        case "guild_delete":
                            {

                            }
                            break;
                        case "guild_ban_add":
                            {

                            }
                            break;
                        case "guild_ban_remove":
                            {

                            }
                            break;
                        case "guild_emojis_update":
                            {

                            }
                            break;
                        case "guild_member_add":
                            {

                            }
                            break;
                        case "guild_member_remove":
                            {

                            }
                            break;
                        case "guild_member_update":
                            {

                            }
                            break;
                        case "guild_members_chunk":
                            {

                            }
                            break;
                        case "guild_role_create":
                            {

                            }
                            break;
                        case "guild_role_update":
                            {

                            }
                            break;
                        case "guild_role_delete":
                            {

                            }
                            break;

                        case "invite_create":
                            {

                            }
                            break;
                        case "invite_delete":
                            {

                            }
                            break;

                        case "message_create":
                            {

                            }
                            break;
                        case "message_update":
                            {

                            }
                            break;
                        case "message_delete":
                            {

                            }
                            break;
                        case "message_create_bulk":
                            {

                            }
                            break;
                        case "message_reaction_add":
                            {

                            }
                            break;
                        case "message_reaction_remove":
                            {

                            }
                            break;
                        case "message_reaction_remove_all":
                            {

                            }
                            break;
                        case "message_reaction_remove_emoji":
                            {

                            }
                            break;

                        case "presence_update":
                            {

                            }
                            break;

                        case "typing_start":
                            {

                            }
                            break;

                        case "user_update":
                            {

                            }
                            break;

                        case "voice_state_update":
                            {

                            }
                            break;
                        case "voice_server_update":
                            {

                            }
                            break;

                        case "webhooks_update":
                            {

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

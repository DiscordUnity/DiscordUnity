using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using WebSocketSharp;
using UnityEngine;

namespace DiscordUnity
{
    public partial class DiscordClient : IDisposable
    {
        #region Fields
        public bool isOnline { get; internal set; }
        public bool isBot { get; internal set; }
        public DiscordUser user { get; internal set; }

        public DiscordServer[] servers { get { return !isOnline ? new DiscordServer[0] : _servers.Values.ToArray(); } }
        internal Dictionary<string, DiscordServer> _servers;

        public DiscordChannel[] channels { get { return !isOnline ? new DiscordChannel[0] : _channels.Values.ToArray(); } }
        internal Dictionary<string, DiscordChannel> _channels
        {
            get
            {
                Dictionary<string, DiscordChannel> channelList = new Dictionary<string, DiscordChannel>();

                foreach (DiscordServer server in servers)
                {
                    foreach (DiscordChannel channel in server.channels)
                    {
                        channelList.Add(channel.ID, channel);
                    }
                }

                return channelList;
            }
        }

        public DiscordPrivateChannel[] privateChannels { get { return !isOnline ? new DiscordPrivateChannel[0] : _privateChannels.Values.ToArray(); } }
        internal Dictionary<string, DiscordPrivateChannel> _privateChannels;

        internal string token;
        internal string sessionID;
        internal bool hasToken { get { return !string.IsNullOrEmpty(token); } }
        internal Queue<Action> unityInvoker;

        private WebSocket socket;
        private Dictionary<string, DiscordAudioClient> audioClients;
        private int sequence = 0;
        private int heartbeat = 41250;
        private Thread heartbeatThread;
        private Thread typingThread;
        #endregion

        #region Events
        private void ProcessMessage(string t, string payload)
        {
            try
            {
                switch (t)
                {
                    case "READY":
                        {
                            ReadyJSON e = JsonUtility.FromJson<ReadyJSON>(payload);
                            heartbeat = e.heartbeat_interval;
                            heartbeatThread = new Thread(KeepAlive);
                            heartbeatThread.Start();
                            SetupClient(e);
                            Debug.Log("DiscordApi Started!");
                            isOnline = true;
                            unityInvoker.Enqueue(() => OnClientOpened(this, new DiscordEventArgs() { client = this }));
                        }
                        break;

                    case "CHANNEL_CREATE":
                        {
                            DiscordChannelJSON channel = JsonUtility.FromJson<DiscordChannelJSON>(payload);

                            if (channel.guild_id == null)
                            {
                                if (_privateChannels.ContainsKey(channel.id)) _privateChannels.Remove(channel.id);
                                DiscordPrivateChannel result2 = new DiscordPrivateChannel(this, JsonUtility.FromJson<DiscordPrivateChannelJSON>(payload));
                                _privateChannels.Add(result2.ID, result2);
                                unityInvoker.Enqueue(() => OnDMCreated(this, new DiscordDMArgs() { channel = result2, client = this }));
                                break;
                            }

                            DiscordServer server = _servers[channel.guild_id];
                            if (server._channels.ContainsKey(channel.id)) server._channels.Remove(channel.id);
                            DiscordChannel result = new DiscordChannel(this, channel);
                            server._channels.Add(channel.id, new DiscordChannel(this, channel));
                            unityInvoker.Enqueue(() => OnChannelCreated(server, new DiscordChannelArgs() { channel = result, client = this }));
                        }
                        break;

                    case "CHANNEL_UPDATE":
                        {
                            DiscordChannelJSON channel = JsonUtility.FromJson<DiscordChannelJSON>(payload);
                            DiscordServer server = _servers[channel.guild_id];
                            if (server._channels.ContainsKey(channel.id)) server._channels.Remove(channel.id);
                            DiscordChannel result = new DiscordChannel(this, channel);
                            server._channels.Add(channel.id, new DiscordChannel(this, channel));
                            unityInvoker.Enqueue(() => OnChannelUpdated(server, new DiscordChannelArgs() { channel = result, client = this }));
                        }
                        break;

                    case "CHANNEL_DELETE":
                        {
                            DiscordChannelJSON channel = JsonUtility.FromJson<DiscordChannelJSON>(payload);

                            if (channel.guild_id == null)
                            {
                                if (_privateChannels.ContainsKey(channel.id)) _privateChannels.Remove(channel.id);
                                DiscordPrivateChannel result2 = new DiscordPrivateChannel(this, JsonUtility.FromJson<DiscordPrivateChannelJSON>(payload));
                                unityInvoker.Enqueue(() => OnDMDeleted(this, new DiscordDMArgs() { channel = result2, client = this }));
                                break;
                            }

                            DiscordServer server = _servers[channel.guild_id];
                            if (server._channels.ContainsKey(channel.id)) server._channels.Remove(channel.id);
                            DiscordChannel result = new DiscordChannel(this, channel);
                            unityInvoker.Enqueue(() => OnChannelDeleted(server, new DiscordChannelArgs() { channel = result, client = this }));
                        }
                        break;

                    case "MESSAGE_CREATE":
                        {
                            DiscordMessageJSON message = JsonUtility.FromJson<DiscordMessageJSON>(payload);

                            if (!_channels.ContainsKey(message.channel_id))
                            {
                                if (!_privateChannels.ContainsKey(message.channel_id))
                                {
                                    _privateChannels.Add(message.channel_id, new DiscordPrivateChannel(this, new DiscordPrivateChannelJSON() { id = message.channel_id, is_private = true, recipient = message.author, last_message_id = message.id }));
                                }

                                unityInvoker.Enqueue(() => OnMessageCreated(_privateChannels[message.channel_id], new DiscordMessageArgs() { message = new DiscordMessage(this, message), client = this }));
                            }

                            else
                            {
                                if (_servers[_channels[message.channel_id].serverID]._members.ContainsKey(message.author.id))
                                {
                                    _servers[_channels[message.channel_id].serverID]._members[message.author.id].isTyping = false;
                                    unityInvoker.Enqueue(() => OnTypingStopped(_servers[_channels[message.channel_id].serverID]._channels[message.channel_id], new DiscordMemberArgs() { member = _servers[_channels[message.channel_id].serverID]._members[message.author.id], client = this }));
                                }

                                unityInvoker.Enqueue(() => OnMessageCreated(_channels[message.channel_id], new DiscordMessageArgs() { message = new DiscordMessage(this, message), client = this }));
                            }
                        }
                        break;

                    case "MESSAGE_UPDATE":
                        {
                            DiscordMessageJSON message = JsonUtility.FromJson<DiscordMessageJSON>(payload);

                            if (!_channels.ContainsKey(message.channel_id))
                            {
                                unityInvoker.Enqueue(() => OnMessageUpdated(_privateChannels[message.channel_id], new DiscordMessageArgs() { message = new DiscordMessage(this, message), client = this }));
                            }

                            else
                            {
                                unityInvoker.Enqueue(() => OnMessageUpdated(_channels[message.channel_id], new DiscordMessageArgs() { message = new DiscordMessage(this, message), client = this }));
                            }
                        }
                        break;

                    case "MESSAGE_DELETE":
                        {
                            DiscordMessageJSON message = JsonUtility.FromJson<DiscordMessageJSON>(payload);

                            if (!_channels.ContainsKey(message.channel_id))
                            {
                                unityInvoker.Enqueue(() => OnMessageDeleted(_privateChannels[message.channel_id], new DiscordMessageArgs() { message = new DiscordMessage(this, message), client = this }));
                            }

                            else
                            {
                                unityInvoker.Enqueue(() => OnMessageDeleted(_channels[message.channel_id], new DiscordMessageArgs() { message = new DiscordMessage(this, message), client = this }));
                            }
                        }
                        break;

                    case "MESSAGE_ACK":
                        //Useless in this API.
                        break;

                    case "PRESENCE_UPDATE":
                        {
                            DiscordPresenceJSON presence = JsonUtility.FromJson<DiscordPresenceJSON>(payload);
                            if (!_servers.ContainsKey(presence.guild_id)) break;
                            if (!_servers[presence.guild_id]._members.ContainsKey(presence.user.id)) break;
                            DiscordMember member = _servers[presence.guild_id]._members[presence.user.id];
                            unityInvoker.Enqueue(() => OnPresenceUpdated(member, new DiscordPresenceArgs() { presence = new DiscordPresence(member, presence), client = this }));
                        }
                        break;

                    case "TYPING_START":
                        {
                            DiscordTypingJSON e = JsonUtility.FromJson<DiscordTypingJSON>(payload);
                            //DiscordChannel.serverID == null
                            if (!_channels.ContainsKey(e.channel_id)) break;
                            if (!_servers.ContainsKey(_channels[e.channel_id].serverID)) break;
                            if (!_servers[_channels[e.channel_id].serverID]._members.ContainsKey(e.user_id)) break;
                            _servers[_channels[e.channel_id].serverID]._members[e.user_id].isTyping = true;
                            typingThread = new Thread(StopTyping);
                            typingThread.Start(e);
                            unityInvoker.Enqueue(() => OnTypingStarted(_channels[e.channel_id], new DiscordMemberArgs() { member = _servers[_channels[e.channel_id].serverID]._members[e.user_id], client = this }));
                        }
                        break;

                    case "GUILD_CREATE":
                        {
                            DiscordServerJSON server = JsonUtility.FromJson<DiscordServerJSON>(payload);
                            if (_servers.ContainsKey(server.id)) _servers.Remove(server.id);
                            DiscordServer result = new DiscordServer(this, server);
                            _servers.Add(server.id, result);
                            unityInvoker.Enqueue(() => OnServerCreated(this, new DiscordServerArgs() { server = result, client = this }));
                        }
                        break;

                    case "GUILD_UPDATE":
                        {
                            DiscordServerJSON server = JsonUtility.FromJson<DiscordServerJSON>(payload);
                            if (_servers.ContainsKey(server.id)) _servers.Remove(server.id);
                            DiscordServer result = new DiscordServer(this, server);
                            _servers.Add(server.id, result);
                            unityInvoker.Enqueue(() => OnServerUpdated(this, new DiscordServerArgs() { server = result, client = this }));
                        }
                        break;

                    case "GUILD_DELETE":
                        {
                            DiscordServerJSON server = JsonUtility.FromJson<DiscordServerJSON>(payload);
                            if (_servers.ContainsKey(server.id)) _servers.Remove(server.id);
                            DiscordServer result = new DiscordServer(this, server);
                            unityInvoker.Enqueue(() => OnServerDeleted(this, new DiscordServerArgs() { server = result, client = this }));
                        }
                        break;

                    case "GUILD_INTEGRATIONS_UPDATE":
                        {
                            //Useless in this API.
                        }
                        break;

                    case "GUILD_EMOJIS_UPDATE":
                        {
                            DiscordServerEmojisJSON serverEmojis = JsonUtility.FromJson<DiscordServerEmojisJSON>(payload);
                            _servers[serverEmojis.guild_id]._emojis = new Dictionary<string, DiscordEmoji>();

                            foreach (DiscordEmojiJSON emoji in serverEmojis.emojis)
                            {
                                _servers[serverEmojis.guild_id]._emojis.Add(emoji.id, new DiscordEmoji(emoji));
                            }

                            unityInvoker.Enqueue(() => OnServerUpdated(this, new DiscordServerArgs() { server = _servers[serverEmojis.guild_id], client = this }));
                        }
                        break;

                    case "GUILD_MEMBER_ADD":
                        {
                            DiscordMemberJSON member = JsonUtility.FromJson<DiscordMemberJSON>(payload);
                            if (_servers[member.guild_id]._members.ContainsKey(member.user.id))
                                _servers[member.guild_id]._members.Remove(member.user.id);
                            DiscordMember result = new DiscordMember(this, member);
                            _servers[member.guild_id]._members.Add(member.user.id, result);
                            unityInvoker.Enqueue(() => OnMemberJoined(_servers[member.guild_id], new DiscordMemberArgs() { member = result, client = this }));
                        }
                        break;

                    case "GUILD_MEMBER_UPDATE":
                        {
                            DiscordMemberJSON member = JsonUtility.FromJson<DiscordMemberJSON>(payload);
                            if (_servers[member.guild_id]._members.ContainsKey(member.user.id))
                                _servers[member.guild_id]._members.Remove(member.user.id);
                            DiscordMember result = new DiscordMember(this, member);
                            _servers[member.guild_id]._members.Add(member.user.id, result);
                            unityInvoker.Enqueue(() => OnMemberUpdated(_servers[member.guild_id], new DiscordMemberArgs() { member = result, client = this }));
                        }
                        break;

                    case "GUILD_MEMBER_REMOVE":
                        {
                            DiscordMemberJSON member = JsonUtility.FromJson<DiscordMemberJSON>(payload);
                            if (_servers[member.guild_id]._members.ContainsKey(member.user.id))
                                _servers[member.guild_id]._members.Remove(member.user.id);
                            DiscordMember result = new DiscordMember(this, member);
                            unityInvoker.Enqueue(() => OnMemberLeft(_servers[member.guild_id], new DiscordMemberArgs() { member = result, client = this }));
                        }
                        break;

                    case "GUILD_MEMBER_CHUNK":
                        {
                            DiscordMembersJSON members = JsonUtility.FromJson<DiscordMembersJSON>(payload);

                            foreach (var member in members.members)
                            {
                                if (_servers[member.guild_id]._members.ContainsKey(member.user.id))
                                    _servers[member.guild_id]._members.Remove(member.user.id);
                                DiscordMember result = new DiscordMember(this, member);
                                _servers[member.guild_id]._members.Add(member.user.id, result);
                                unityInvoker.Enqueue(() => OnMemberUpdated(_servers[member.guild_id], new DiscordMemberArgs() { member = result, client = this }));
                            }
                        }
                        break;

                    case "GUILD_BAN_ADD":
                        {
                            DiscordBanJSON ban = JsonUtility.FromJson<DiscordBanJSON>(payload);
                            _servers[ban.guild_id]._members[ban.user.id].isBanned = true;
                            unityInvoker.Enqueue(() => OnMemberBanned(_servers[ban.guild_id], new DiscordMemberArgs() { member = _servers[ban.guild_id]._members[ban.user.id], client = this }));
                        }
                        break;

                    case "GUILD_BAN_REMOVE":
                        {
                            DiscordBanJSON ban = JsonUtility.FromJson<DiscordBanJSON>(payload);
                            unityInvoker.Enqueue(() => OnMemberUnbanned(_servers[ban.guild_id], new DiscordUserArgs() { user = new DiscordUser(this, ban.user), client = this }));
                        }
                        break;

                    case "GUILD_ROLE_CREATE":
                        {
                            DiscordRoleEventJSON role = JsonUtility.FromJson<DiscordRoleEventJSON>(payload);
                            if (_servers[role.guild_id]._roles.ContainsKey(role.role.id))
                                _servers[role.guild_id]._roles.Remove(role.role.id);
                            DiscordRole result = new DiscordRole(role.role);
                            _servers[role.guild_id]._roles.Add(role.role.id, result);
                            unityInvoker.Enqueue(() => OnRoleCreated(_servers[role.guild_id], new DiscordRoleArgs() { role = result, client = this }));
                        }
                        break;

                    case "GUILD_ROLE_UPDATE":
                        {
                            DiscordRoleEventJSON role = JsonUtility.FromJson<DiscordRoleEventJSON>(payload);
                            if (_servers[role.guild_id]._roles.ContainsKey(role.role.id))
                                _servers[role.guild_id]._roles.Remove(role.role.id);
                            DiscordRole result = new DiscordRole(role.role);
                            _servers[role.guild_id]._roles.Add(role.role.id, result);
                            unityInvoker.Enqueue(() => OnRoleUpdated(_servers[role.guild_id], new DiscordRoleArgs() { role = result, client = this }));
                        }
                        break;

                    case "GUILD_ROLE_DELETE":
                        {
                            DiscordRoleEventJSON role = JsonUtility.FromJson<DiscordRoleEventJSON>(payload);
                            DiscordRole result = null;

                            if (_servers[role.guild_id]._roles.ContainsKey(role.role_id))
                            {
                                result = _servers[role.guild_id]._roles[role.role_id];
                                _servers[role.guild_id]._roles.Remove(role.role_id);
                                unityInvoker.Enqueue(() => OnRoleDeleted(_servers[role.guild_id], new DiscordRoleArgs() { role = result, client = this }));
                            }
                        }
                        break;

                    case "USER_UPDATE":
                        {
                            DiscordUserJSON userJ = JsonUtility.FromJson<DiscordUserJSON>(payload);
                            user = new DiscordUser(this, userJ);
                            unityInvoker.Enqueue(() => OnUserUpdated(this, new DiscordUserArgs() { user = user, client = this }));
                        }
                        break;

                    case "USER_SETTINGS_UPDATE":
                        {
                            DiscordUserJSON userJ = JsonUtility.FromJson<DiscordUserJSON>(payload);
                            user = new DiscordUser(this, userJ);
                            unityInvoker.Enqueue(() => OnUserUpdated(this, new DiscordUserArgs() { user = user, client = this }));
                        }
                        break;

                    case "VOICE_STATE_UPDATE":
                        {
                            try
                            {
                                DiscordVoiceStateJSON voiceState = JsonUtility.FromJson<DiscordVoiceStateJSON>(payload);
                                VoiceStateUpdateEvents(voiceState);
                            }

                            catch (Exception e)
                            {
                                Debug.LogError("Problem was here all along: " + e.Message);
                            }
                        }
                        break;

                    case "VOICE_SERVER_UPDATE":
                        {
                            DiscordVoiceServerStateJSON voiceState = JsonUtility.FromJson<DiscordVoiceServerStateJSON>(payload);
                            audioClients[voiceState.guild_id].Start(_servers[voiceState.guild_id], voiceState.endpoint, voiceState.token);
                        }
                        break;
                }
            }

            catch (Exception e)
            {
                Debug.LogError(e.Message);
                Debug.LogError(e.Source);
                Debug.LogError(e.TargetSite.ToString());
            }
        }

        #endregion

        #region Methods
        public void Start(string email, string password)
        {
            if (isOnline) return;
            LoginArgs login = new LoginArgs() { email = email, password = password };
            POST("https://discordapp.com/api/auth/login", JsonUtility.ToJson(login), OnStart);
        }

        public void StartBot(string botToken)
        {
            if (isOnline) return;
            token = botToken;
            StartEventListener();
        }

        public void Update()
        {
            while (unityInvoker != null && unityInvoker.Count > 0)
            {
                try
                {
                    Action action = unityInvoker.Dequeue();
                    Debug.Log(action.Method.GetType().Name + ": " + (action != null));
                    action();
                }

                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
        }

        public void Stop()
        {
            foreach (DiscordAudioClient audioClient in audioClients.Values)
            {
                if (audioClient != null)
                {
                    audioClient.Dispose();
                }
            }

            audioClients.Clear();
            POST("https://discordapp.com/api/auth/logout", JsonUtility.ToJson(new DiscordTokenJSON() { token = token }), null);
            socket.CloseAsync();
        }

        public void Dispose()
        {
            try
            {
                isOnline = false;
                sequence = 0;
                heartbeat = 41250;
                token = "";
                socket = null;
                _servers = null;
                _privateChannels = null;
                user = null;
                unityInvoker.Clear();
                unityInvoker = null;
                isBot = false;
                Debug.Log("DiscordApi Stopped!");
                OnClientClosed(this, new DiscordEventArgs() { client = this });
            }

            catch { }
        }

        public void CreatePrivateChannel(DiscordUser recipient)
        {
            CreatePrivateChannel(user.ID, recipient.ID);
        }

        /// <summary>
        /// Creates a server.
        /// </summary>
        /// <param name="servername">The name of the server.</param>
        /// <param name="region">The region for the server.</param>
        /// <param name="icon">The icon for the server.</param>
        public void CreateServer(string servername, DiscordRegion region, Texture2D icon = null)
        {
            if (!isOnline) return;
            Createserver(servername, region.name.ToLower().Replace(' ', '-'), icon);
        }
        /// <summary>
        /// Creates a server.
        /// </summary>
        /// <param name="servername">The name of the server.</param>
        /// <param name="region">The region for the server.</param>
        /// <param name="icon">The icon for the server.</param>
        public void CreateServer(string servername, string region, Texture2D icon = null)
        {
            if (!isOnline) return;
            Createserver(servername, region.ToLower().Replace(' ', '-'), icon);
        }

        /// <summary>
        /// Gets more info about the invite;
        /// </summary>
        /// <param name="invite">The code of the invite.</param>
        public void GetInvite(string invite)
        {
            if (!isOnline) return;
            Getinvite(invite);
        }

        /// <summary>
        /// Accepts the invite.
        /// </summary>
        /// <param name="invite">The code of the invite.</param>
        public void AcceptInvite(string invite)
        {
            if (!isOnline) return;
            if (isBot) return;
            Acceptinvite(invite);
        }

        /// <summary>
        /// Deletes the invite.
        /// </summary>
        /// <param name="invite">The code of the invite.</param>
        public void DeleteInvite(string invite)
        {
            if (!isOnline) return;
            Deleteinvite(invite);
        }

        /// <summary>
        /// Sets the status for the current user.
        /// </summary>
        /// <param name="game">The game.</param>
        public void SetStatus(bool idle, string game)
        {
            if (!isOnline) return;
            string specialJson = "{\"idle_since\":" + (idle ? ((int)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds).ToString() : "null") + ",\"game\":{\"name\":" + (string.IsNullOrEmpty(game) ? "null" : "\"" + game + "\"") + "}}";
            Debugger.WriteLine("SocketSend: " + specialJson);
            socket.Send(specialJson);
        }

        /// <summary>
        /// Gets more info about active maintenances;
        /// </summary>
        public void GetActiveMaintenances()
        {
            if (!isOnline) return;
            GetactiveMaintenances();
        }

        /// <summary>
        /// Gets more info about upcoming maintenances;
        /// </summary>
        public void GetUpcomingMaintenances()
        {
            if (!isOnline) return;
            GetupcomingMaintenances();
        }

        /// <summary>
        /// Edits the profile of the current connected account.
        /// </summary>
        /// <param name="avatar">A new avatar..</param>
        /// <param name="email">A new email.</param>
        /// <param name="username">A new username.</param>
        /// <param name="password">The old password.</param>
        /// <param name="new_password">A new password.</param>
        public void EditProfile(Texture2D avatar, string email, string username, string password, string newPassword)
        {
            if (!isOnline) return;
            Editprofile(avatar, email, newPassword, password, username);
        }

        /// <summary>
        /// Gets info about regions.
        /// </summary>
        public void GetRegions()
        {
            if (!isOnline) return;
            GetServerRegions();
        }

        [Obsolete("AudioClient is work in progress.", true)]
        public void GetAudioClient(DiscordChannel channel, bool muted = false, bool deaf = false)
        {
            string guildID = "";
            if (channel.type != DiscordChannelType.Voice) return;
            if (audioClients.ContainsKey(guildID)) if (audioClients[guildID].isOnline) audioClients[guildID].Dispose();
            DiscordAudioClient audioClient = new DiscordAudioClient(this, channel);
            audioClients.Add(guildID, audioClient);

            PayloadArgs<JoinVoiceArgs> args = new PayloadArgs<JoinVoiceArgs>()
            {
                op = 4,
                d = new JoinVoiceArgs()
                {
                    guild_id = channel.serverID,
                    channel_id = channel.ID,
                    self_mute = muted,
                    self_deaf = deaf
                }
            };

            Debugger.WriteLine("SocketSend: " + JsonUtility.ToJson(args));
            socket.Send(JsonUtility.ToJson(args));
        }
        #endregion

        #region Private Methods
        private void OnStart(string result)
        {
            token = JsonUtility.FromJson<DiscordTokenJSON>(result).token;
            StartEventListener();
        }


        private void StartEventListener()
        {
            isOnline = false;
            isBot = false;
            unityInvoker = new Queue<Action>();
            audioClients = new Dictionary<string, DiscordAudioClient>();
            GetGatewayUrl();
        }

        private void GetGatewayUrl()
        {
            if (token == null)
            {
                Debug.LogError("Token is null!");
                return;
            }

            GET("https://discordapp.com/api/gateway", OnGetGatewayUrl);
        }

        internal void GetOfflineServerMembers(string serverID, string filter, int limit)
        {
            MemberChunkArgs args = new MemberChunkArgs() { guild_id = serverID, query = filter, limit = limit };
            Debugger.WriteLine("SocketSend: " + JsonUtility.ToJson(args));
            socket.SendAsync(JsonUtility.ToJson(args), null);
        }

        private void OnGetGatewayUrl(string result)
        {
            string url = JsonUtility.FromJson<GatewayJSON>(result).url;

            if (url == "")
            {
                Debug.LogError("Gateway URL was null or empty?!");
                return;
            }

            socket = new WebSocket(url);

            socket.OnMessage += (sender, message) =>
            {
                DiscordEventJSON e = JsonUtility.FromJson<DiscordEventJSON>(message.Data);

                if (string.IsNullOrEmpty(e.t))
                {
                    return;
                }

                sequence = e.s;
                int payloadIndex = message.Data.IndexOf("\"d\":{");
                string payload = message.Data.Substring(payloadIndex + 4, message.Data.Length - payloadIndex - 5);
                Debugger.WriteLine("Event " + e.t + ": " + payload);
                ProcessMessage(e.t, payload);
            };

            socket.OnOpen += (sender, e) =>
            {
                SendIdentifyPacket();
            };

            socket.OnClose += (sender, e) =>
            {
                if (!e.WasClean)
                {
                    Debug.LogError("Socket closed2: " + e.Code);
                }

                Debug.LogWarning("Socket closed: " + e.Reason);

                StopEventListener();
            };

            socket.OnError += (sender, e) =>
            {
                Debug.LogError("Socket error: " + e.Message);
            };

            socket.Connect();
        }

        private void SendIdentifyPacket()
        {
            string specialJson = "{\"op\": 2,\"d\": { \"token\": \"" + token + "\", \"v\": 4, \"properties\": { \"$os\": \"" + Environment.OSVersion.ToString() + "\",\"$browser\": \"DiscordUnity\",\"$device\": \"DiscordUnity\",\"$referrer\": \"\",\"$referring_domain\":\"\"}}}";
            socket.Send(specialJson);
            Debugger.WriteLine("SocketSend: " + specialJson);
        }

        private void StopEventListener()
        {
            unityInvoker.Enqueue(Dispose);
        }

        private void KeepAlive()
        {
            while (socket != null && hasToken)
            {
                Thread.Sleep(heartbeat);
                KeepAliveArgs args = new KeepAliveArgs() { op = 1, d = sequence };

                if (socket != null && hasToken)
                {
                    Debugger.WriteLine("SocketBeat: " + JsonUtility.ToJson(args));
                    socket.Send(JsonUtility.ToJson(args));
                }
            }
        }

        private void StopTyping(object e)
        {
            DiscordTypingJSON typing = (DiscordTypingJSON)e;
            Thread.Sleep(9000);
            _servers[_channels[typing.channel_id].serverID]._members[typing.user_id].isTyping = false;
            Thread.Sleep(5000);
            if (!_servers[_channels[typing.channel_id].serverID]._members[typing.user_id].isTyping)
                unityInvoker.Enqueue(() => OnTypingStopped(_servers[_channels[typing.channel_id].serverID]._channels[typing.channel_id], new DiscordMemberArgs() { member = _servers[_channels[typing.channel_id].serverID]._members[typing.user_id], client = this }));
        }

        private void SetupClient(ReadyJSON e)
        {
            user = new DiscordUser(this, e.user);
            isBot = e.user.bot;

            Dictionary<string, DiscordPrivateChannel> privateChannels = new Dictionary<string, DiscordPrivateChannel>();

            foreach (var channel in e.private_channels)
            {
                privateChannels.Add(channel.id, new DiscordPrivateChannel(this, channel));
            }

            Dictionary<string, DiscordServer> servers = new Dictionary<string, DiscordServer>();

            foreach (var server in e.guilds)
            {
                servers.Add(server.id, new DiscordServer(this, server));
            }

            _servers = servers;
            _privateChannels = privateChannels;
            sessionID = e.session_id;
        }

        private void VoiceStateUpdateEvents(DiscordVoiceStateJSON voiceState)
        {
            if (!_servers.ContainsKey(voiceState.guild_id)) return;
            if (!_servers[voiceState.guild_id]._members.ContainsKey(voiceState.user_id)) return;

            DiscordMember member = _servers[voiceState.guild_id]._members[voiceState.user_id];

            if (string.IsNullOrEmpty(voiceState.channel_id))
            {
                unityInvoker.Enqueue(() => audioClients[voiceState.guild_id].OnVoiceUserLeft(this, new DiscordUserArgs() { client = this, user = member.user }));
                return;
            }

            if (member.user != null)
            {
                member.muted = voiceState.mute;
                member.deaf = voiceState.suppress;

                if (!string.IsNullOrEmpty(voiceState.session_id))
                {
                    if (member.user == user)
                    {
                        _servers[voiceState.guild_id]._members[user.ID].muted = voiceState.self_mute;
                        _servers[voiceState.guild_id]._members[user.ID].deaf = voiceState.self_deaf;
                        if (audioClients[voiceState.guild_id] != null) audioClients[voiceState.guild_id].sessionID = voiceState.session_id;
                    }
                }

                if (audioClients[voiceState.guild_id] != null)
                {
                    unityInvoker.Enqueue(() => audioClients[voiceState.guild_id].OnVoiceState(audioClients[voiceState.guild_id], new DiscordMemberArgs() { client = this, member = member }));
                }

                _servers[voiceState.guild_id]._members[voiceState.user_id] = member;
            }
        }
        #endregion

        #region Http
        internal delegate void CallResult(string result);

        internal class RequestState
        {
            public CallResult result;
            public HttpWebRequest request;
        }

        internal class RequestStateJSON : RequestState
        {
            public string content;
        }

        internal string APIurl = "https://discordapp.com/api/";

        internal void POST(string url, string content, CallResult result)
        {
            HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(url);
            if (hasToken) httpRequest.Headers["authorization"] = isBot ? "Bot " + token : token;
            httpRequest.ContentType = "application/json";
            httpRequest.Method = "POST";
            httpRequest.UserAgent = isBot ? "DiscordBot DiscordUnity" : "Custom Discord Client DiscordUnity";
            httpRequest.BeginGetRequestStream(new AsyncCallback(OnRequestStream), new RequestStateJSON() { content = content, result = result, request = httpRequest });
        }

        internal void POST(string url, CallResult result)
        {
            HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(url);
            if (hasToken) httpRequest.Headers["authorization"] = isBot ? "Bot " + token : token;
            httpRequest.ContentType = "application/json";
            httpRequest.Method = "POST";
            httpRequest.UserAgent = isBot ? "DiscordBot DiscordUnity" : "Custom Discord Client DiscordUnity";
            httpRequest.BeginGetResponse(new AsyncCallback(OnGetResponse), new RequestState() { result = result, request = httpRequest });
        }

        internal void GET(string url, CallResult result)
        {
            HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(url);
            if (hasToken) httpRequest.Headers["authorization"] = isBot ? "Bot " + token : token;
            httpRequest.ContentType = "application/json";
            httpRequest.Method = "GET";
            httpRequest.UserAgent = isBot ? "DiscordBot DiscordUnity" : "Custom Discord Client DiscordUnity";
            httpRequest.BeginGetResponse(new AsyncCallback(OnGetResponse), new RequestState() { result = result, request = httpRequest });
        }

        internal void PATCH(string url, string content, CallResult result)
        {
            HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(url);
            if (hasToken) httpRequest.Headers["authorization"] = isBot ? "Bot " + token : token;
            httpRequest.ContentType = "application/json";
            httpRequest.Method = "PATCH";
            httpRequest.UserAgent = isBot ? "DiscordBot DiscordUnity" : "Custom Discord Client DiscordUnity";
            httpRequest.BeginGetRequestStream(new AsyncCallback(OnRequestStream), new RequestStateJSON() { content = content, result = result, request = httpRequest });
        }

        internal void PUT(string url, string content, CallResult result)
        {
            HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(url);
            if (hasToken) httpRequest.Headers["authorization"] = isBot ? "Bot " + token : token;
            httpRequest.ContentType = "application/json";
            httpRequest.Method = "PUT";
            httpRequest.UserAgent = isBot ? "DiscordBot DiscordUnity" : "Custom Discord Client DiscordUnity";
            httpRequest.BeginGetRequestStream(new AsyncCallback(OnRequestStream), new RequestStateJSON() { content = content, result = result, request = httpRequest });
        }

        internal void PUT(string url, CallResult result)
        {
            HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(url);
            if (hasToken) httpRequest.Headers["authorization"] = isBot ? "Bot " + token : token;
            httpRequest.ContentType = "application/json";
            httpRequest.Method = "PUT";
            httpRequest.UserAgent = isBot ? "DiscordBot DiscordUnity" : "Custom Discord Client DiscordUnity";
            httpRequest.BeginGetResponse(new AsyncCallback(OnGetResponse), new RequestState() { result = result, request = httpRequest });
        }

        internal void DELETE(string url, CallResult result)
        {
            HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(url);
            if (hasToken) httpRequest.Headers["authorization"] = isBot ? "Bot " + token : token;
            httpRequest.ContentType = "application/json";
            httpRequest.Method = "DELETE";
            httpRequest.UserAgent = isBot ? "DiscordBot DiscordUnity" : "Custom Discord Client DiscordUnity";
            httpRequest.BeginGetResponse(new AsyncCallback(OnGetResponse), new RequestState() { result = result, request = httpRequest });
        }

        private void OnRequestStream(IAsyncResult result)
        {
            RequestStateJSON state = (RequestStateJSON)result.AsyncState;

            using (StreamWriter writer = new StreamWriter(state.request.EndGetRequestStream(result)))
            {
                Debugger.WriteLine("Send: " + state.content);
                writer.Write(state.content);
                writer.Flush();
                writer.Close();
            }

            state.request.BeginGetResponse(new AsyncCallback(OnGetResponse), state);
        }

        private void OnGetResponse(IAsyncResult result)
        {
            RequestState state = (RequestState)result.AsyncState;

            try
            {
                HttpWebResponse httpResponse = (HttpWebResponse)state.request.EndGetResponse(result);

                if (httpResponse.StatusCode == HttpStatusCode.Unauthorized)
                {
                    Debug.LogError("Unauthorized  request!");
                }

                using (StreamReader reader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    string response = reader.ReadToEnd();
                    Debugger.WriteLine("Received: " + response);
                    state.result.Invoke(response);
                }
            }

            catch (WebException e)
            {
                Debug.LogError(e.Message);
            }
        }

        public void UploadFile(string url, string file)
        {
            Debug.Log("Setting up File ...");
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(url);
            httpRequest.Headers["authorization"] = isBot ? "Bot " + token : token;
            httpRequest.ContentType = "multipart/form-data; boundary=" + boundary;
            httpRequest.Method = "POST";
            httpRequest.UserAgent = isBot ? "DiscordBot DiscordUnity" : "Custom Discord Client DiscordUnity";
            httpRequest.KeepAlive = true;

            httpRequest.BeginGetRequestStream(new AsyncCallback(OnRequestFileStream), new RequestStateJSON() { content = file, request = httpRequest });
        }

        private void OnRequestFileStream(IAsyncResult result)
        {
            RequestStateJSON state = (RequestStateJSON)result.AsyncState;
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

            Debug.Log("Sending File ...");
            using (Stream stream = state.request.EndGetRequestStream(result))
            {
                stream.Write(boundarybytes, 0, boundarybytes.Length);
                string header = "Content-Disposition: form-data; name=\"file\"; filename=\"" + state.content + "\"\r\nContent-Type: image/jpeg\r\n\r\n";
                byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
                stream.Write(headerbytes, 0, headerbytes.Length);

                byte[] fileData = File.ReadAllBytes(state.content);
                stream.Write(fileData, 0, fileData.Length);

                byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
                stream.Write(trailer, 0, trailer.Length);
                stream.Close();
            }

            Debug.Log("File send.");
            state.request.BeginGetResponse(new AsyncCallback(OnGetResponse), state);
        }
        #endregion

        #region API
        //
        // Channels
        //

        private string channelurl = "https://discordapp.com/api/channels/";

        internal void GetChannel(string channelID)
        {
            GET(channelurl + channelID, null);
        }

        internal void CreateChannel(string serverID, string channelname, string channeltype = "text")
        {
            CreateChannelArgs args = new CreateChannelArgs() { name = channelname, type = channeltype };
            POST(APIurl + "guilds/" + serverID + "/channels", JsonUtility.ToJson(args), null);
        }

        internal void EditChannel(string channelID, string channelname, string topic, int position = 0)
        {
            EditChannelArgs args = new EditChannelArgs() { name = channelname, position = position, topic = topic };
            PATCH(channelurl + channelID, JsonUtility.ToJson(args), null);
        }

        internal void DeleteChannel(string channelID)
        {
            DELETE(channelurl + channelID, null);
        }

        internal void BroadcastTyping(string channelID)
        {
            POST(channelurl + channelID + "/typing", null);
        }

        //
        // Messages
        //

        internal void GetMessages(string channelID, int limit, string messageID, bool before)
        {
            string url = channelurl + channelID + "/messages?&limit=" + limit;
            if (before) url += "&before=" + messageID;
            else url += "&after=" + messageID;

            GET(url, (result) =>
            {
                string substring = "{\"messages\":";
                result = result.Insert(0, substring).Insert(result.Length + substring.Length, "}");
                DiscordMessageJSONWrapper wrapper = JsonUtility.FromJson<DiscordMessageJSONWrapper>(result);

                foreach (DiscordMessageJSON message in wrapper.messages)
                {
                    unityInvoker.Enqueue(() => OnMessageUpdated(_channels[message.channel_id], new DiscordMessageArgs() { message = new DiscordMessage(this, message), client = this }));
                }
            });
        }

        internal void GetMessages(string channelID, int limit)
        {
            string url = channelurl + channelID + "/messages?&limit=" + limit;

            GET(url, (result) =>
            {
                string substring = "{\"messages\":";
                result = result.Insert(0, substring).Insert(result.Length + substring.Length, "}");
                DiscordMessageJSONWrapper wrapper = JsonUtility.FromJson<DiscordMessageJSONWrapper>(result);

                foreach (DiscordMessageJSON message in wrapper.messages)
                {
                    unityInvoker.Enqueue(() => OnMessageUpdated(_channels[message.channel_id], new DiscordMessageArgs() { message = new DiscordMessage(this, message), client = this }));
                }
            });
        }

        internal void SendFile(string channelID, string file)
        {
            UploadFile(channelurl + channelID + "/messages", file);
        }

        internal void SendMessage(string channelID, string content, int nonce, bool textToSpeech)
        {
            SendMessageArgs args = new SendMessageArgs() { content = content, nonce = nonce, tts = textToSpeech };
            POST(channelurl + channelID + "/messages", JsonUtility.ToJson(args), null);
        }

        internal void EditMessage(string channelID, string messageID, string content)
        {
            EditMessageArgs args = new EditMessageArgs() { content = content };
            PATCH(channelurl + channelID + "/messages/" + messageID, JsonUtility.ToJson(args), null);
        }

        internal void DeleteMessage(string channelID, string messageID)
        {
            DELETE(channelurl + channelID + "/messages/" + messageID, null);
        }

        internal void AcknowledgeMessage(string channelID, string messageID)
        {
            POST(channelurl + channelID + "/messages/" + messageID + "/ack", null);
        }

        //
        // Permissions
        //

        internal void CreateOrEditPermission(string channelID, string targetID, DiscordPermission[] allowed, DiscordPermission[] denied, TargetType type)
        {
            CreateOrEditPermissionArgs args = new CreateOrEditPermissionArgs() { allow = Utils.GetPermissions(allowed), deny = Utils.GetPermissions(denied), id = targetID, type = type == TargetType.Member ? "member" : "role" };
            PUT(channelurl + channelID + "/permissions/" + targetID, JsonUtility.ToJson(args), (result) =>
            {
                Debug.Log("Permissions Event: " + result);
            });
        }

        internal void DeletePermission(string channelID, string targetID)
        {
            DELETE(channelurl + channelID + "/permissions/" + targetID, (result) =>
            {
                Debug.Log("Permissions Event: " + result);
            });
        }

        //
        // Servers
        //

        private static string serverurl = "https://discordapp.com/api/guilds/";

        internal void Createserver(string servername, string region, Texture2D icon)
        {
            string iconData = null;

            if (icon != null)
            {
                iconData = "data:image/jpeg;base64," + Convert.ToBase64String(icon.EncodeToJPG());
            }

            CreateServerArgs args = new CreateServerArgs() { name = servername, region = region, icon = iconData };
            POST(serverurl.TrimEnd('/'), JsonUtility.ToJson(args), null);
        }

        internal void EditServer(string serverID, string servername, string ownerID, string region, int? verificationLevel, string afkchannelID, int? timeout, Texture2D icon, Texture2D splash)
        {
            string iconData = icon == null ? null : "data:image/jpeg;base64," + Convert.ToBase64String(icon.EncodeToJPG());
            string splashData = splash == null ? null : "data:image/jpeg;base64," + Convert.ToBase64String(splash.EncodeToJPG());
            string specialJson = "{";
            specialJson += string.IsNullOrEmpty(servername) ? "" : string.Format("\"name\":\"{0}\",", servername);
            specialJson += string.IsNullOrEmpty(region) ? "" : string.Format("\"region\":\"{0}\",", region);
            specialJson += verificationLevel == null ? "" : string.Format("\"verification_level\":{0},", verificationLevel.Value);
            specialJson += string.IsNullOrEmpty(afkchannelID) ? "" : string.Format("\"afk_channel_id\":\"{0}\",", afkchannelID);
            specialJson += timeout == null ? "" : string.Format("\"afk_timeout\":{0},", timeout.Value);
            specialJson += icon == null ? "" : string.Format("\"icon\":\"{0}\",", iconData);
            specialJson += string.IsNullOrEmpty(ownerID) ? "" : string.Format("\"owner_id\":\"{0}\",", ownerID);
            specialJson += splash == null ? "" : string.Format("\"splash\":\"{0}\",", splashData);
            specialJson = specialJson.TrimEnd(',');
            specialJson += "}";
            PATCH(serverurl + serverID, specialJson, null);
        }

        internal void LeaveServer(string serverID)
        {
            DELETE(APIurl + "users/@me/guilds/" + serverID, null);
        }

        internal void DeleteServer(string serverID)
        {
            DELETE(serverurl + serverID, null);
        }

        internal void GetServers()
        {
            GET(APIurl + "users/@me/guilds", null);
        }

        internal void GetServerChannels(string serverID)
        {
            GET(serverurl + serverID + "/channels", (result) =>
            {
                string substring = "{\"channels\":";
                result = result.Insert(0, substring).Insert(result.Length + substring.Length, "}");
                DiscordChannelJSONWrapper wrapper = JsonUtility.FromJson<DiscordChannelJSONWrapper>(result);

                foreach (DiscordChannelJSON channel in wrapper.channels)
                {
                    unityInvoker.Enqueue(() => OnChannelUpdated(_servers[channel.guild_id], new DiscordChannelArgs() { channel = new DiscordChannel(this, channel), client = this }));
                }
            });
        }

        //
        // Members
        //

        internal void EditMember(string serverID, string userID, EditMemberArgs args)
        {
            PATCH(serverurl + serverID + "/members/" + userID, JsonUtility.ToJson(args), null);
        }

        internal void KickMember(string serverID, string userID)
        {
            DELETE(serverurl + serverID + "/members/" + userID, null);
        }

        //
        // Bans
        //

        internal void GetBans(string serverID)
        {
            GET(serverurl + serverID + "/bans", null);
        }

        internal void AddBan(string serverID, string userID, int clearPreviousDays)
        {
            PUT(serverurl + serverID + "/bans/" + userID + "?delete-message-days=" + clearPreviousDays, null);
        }

        internal void RemoveBan(string serverID, string userID)
        {
            DELETE(serverurl + serverID + "/bans/" + userID, null);
        }

        //
        // Roles
        //

        internal void CreateRole(string serverID)
        {
            POST(serverurl + serverID + "/roles", null);
        }

        internal void EditRole(string serverID, string roleID, uint color, bool hoist, string name, DiscordPermission[] permissions)
        {
            EditRoleArgs args = new EditRoleArgs() { color = color, hoist = hoist, name = name, permissions = Utils.GetPermissions(permissions) };
            PATCH(serverurl + serverID + "/roles/" + roleID, JsonUtility.ToJson(args), null);
        }

        internal void ReorderRoles(string serverID, DiscordRole[] roles)
        {
            PATCH(serverurl + serverID + "/roles", JsonUtility.ToJson(GetRolesOrdered(roles)), null);
        }

        internal void DeleteRole(string serverID, string roleID)
        {
            DELETE(serverurl + serverID + "/roles/" + roleID, null);
        }

        private static DiscordRoleJSON[] GetRolesOrdered(DiscordRole[] roles)
        {
            List<DiscordRoleJSON> rolesOrdered = new List<DiscordRoleJSON>();

            for (int x = 0; x < roles.Length; ++x)
            {
                DiscordRole role = roles[x - 1];

                rolesOrdered.Add(new DiscordRoleJSON()
                {
                    id = role.ID,
                    position = x,
                    hoist = role.hoist,
                    color = Utils.GetIntFromColor(role.color),
                    managed = role.managed,
                    name = role.name,
                    permissions = Utils.GetPermissions(role.permissions)
                });
            }

            return rolesOrdered.ToArray();
        }

        //
        // Invites
        //

        private static string inviteurl = "https://discordapp.com/api/invite/";

        internal void Getinvite(string inviteID)
        {
            GET(inviteurl + inviteID, (result) =>
            {
                DiscordBasicInviteJSON invite = JsonUtility.FromJson<DiscordBasicInviteJSON>(result);
                unityInvoker.Enqueue(() => OnInviteUpdated(this, new DiscordInviteArgs() { invite = new DiscordInvite(this, invite), client = this }));
            });
        }

        internal void Acceptinvite(string inviteID)
        {
            POST(inviteurl + inviteID, (result) =>
            {
                DiscordBasicInviteJSON invite = JsonUtility.FromJson<DiscordBasicInviteJSON>(result);
                unityInvoker.Enqueue(() => OnInviteAccepted(this, new DiscordInviteArgs() { invite = new DiscordInvite(this, invite), client = this }));
            });
        }

        internal void CreateInvite(string channelID, int maxAge, int maxUses, bool temporary, bool xkcdpass)
        {
            DiscordInviteJSON args = new DiscordInviteJSON() { max_age = maxAge, max_uses = maxUses, temporary = temporary, xkcdpass = xkcdpass };

            POST(APIurl + "channels/" + channelID + "/invites", JsonUtility.ToJson(args), (result) =>
            {
                DiscordRichInviteJSON invite = JsonUtility.FromJson<DiscordRichInviteJSON>(result);
                unityInvoker.Enqueue(() => OnInviteCreated(this, new DiscordInviteArgs() { invite = new DiscordInvite(this, invite), client = this }));
            });
        }

        internal void Deleteinvite(string inviteID)
        {
            DELETE(inviteurl + inviteID, (result) =>
            {
                DiscordBasicInviteJSON invite = JsonUtility.FromJson<DiscordBasicInviteJSON>(result);
                unityInvoker.Enqueue(() => OnInviteDeleted(this, new DiscordInviteArgs() { invite = new DiscordInvite(this, invite), client = this }));
            });
        }

        internal void GetServerInvites(string serverID)
        {
            GET(APIurl + "guilds/" + serverID + "/invites", (result) =>
            {
                string substring = "{\"invites\":";
                result = result.Insert(0, substring).Insert(result.Length + substring.Length, "}");
                DiscordRichInviteJSONWrapper wrapper = JsonUtility.FromJson<DiscordRichInviteJSONWrapper>(result);

                foreach (DiscordRichInviteJSON invite in wrapper.invites)
                {
                    unityInvoker.Enqueue(() => OnInviteUpdated(this, new DiscordInviteArgs() { invite = new DiscordInvite(this, invite), client = this }));
                }
            });
        }

        internal void GetChannelInvites(string channelID)
        {
            GET(APIurl + "channels/" + channelID + "/invites", (result) =>
            {
                string substring = "{\"invites\":";
                result = result.Insert(0, substring).Insert(result.Length + substring.Length, "}");
                DiscordRichInviteJSONWrapper wrapper = JsonUtility.FromJson<DiscordRichInviteJSONWrapper>(result);

                foreach (DiscordRichInviteJSON invite in wrapper.invites)
                {
                    unityInvoker.Enqueue(() => OnInviteUpdated(this, new DiscordInviteArgs() { invite = new DiscordInvite(this, invite), client = this }));
                }
            });
        }

        //
        // Maintenances
        //

        private static string statusurl = "https://status.discordapp.com/api/v2/sheduled-maintenances/";

        private void OnStatusPacket(string result)
        {
            DiscordStatusPacketJSON status = JsonUtility.FromJson<DiscordStatusPacketJSON>(result);
            unityInvoker.Enqueue(() => OnStatusReceived(this, new DiscordStatusArgs() { packet = new DiscordStatusPacket(status), client = this }));
        }

        internal void GetactiveMaintenances()
        {
            GET(statusurl + "active.json", OnStatusPacket);
        }

        internal void GetupcomingMaintenances()
        {
            GET(statusurl + "upcoming.json", OnStatusPacket);
        }

        //
        // Users
        //

        private static string userurl = "https://discordapp.com/api/users/";

        internal void CreatePrivateChannel(string userID, string recipientID)
        {
            CreatePrivateChannelArgs args = new CreatePrivateChannelArgs() { recipient_id = recipientID };
            POST(userurl + userID + "/channels", JsonUtility.ToJson(args), null);
        }

        //
        // Profile
        //

        internal void Editprofile(Texture2D avatar, string email, string new_password, string password, string username)
        {
            string avatarData = "data:image/jpeg;base64," + Convert.ToBase64String(avatar.EncodeToJPG());
            EditProfileArgs args = new EditProfileArgs() { avatar = avatarData, email = email, new_password = new_password, password = password, username = username };
            PATCH(userurl + "@me", JsonUtility.ToJson(args), (result) =>
            {
                DiscordProfileJSON profile = JsonUtility.FromJson<DiscordProfileJSON>(result);
                unityInvoker.Enqueue(() => OnProfileUpdated(this, new DiscordUserArgs() { user = new DiscordUser(this, profile), client = this }));
            });
        }

        //
        // General
        //

        private static string voiceurl = "https://discordapp.com/api/voice/";

        internal void GetServerRegions()
        {
            GET(voiceurl + "regions", (result) =>
            {
                string substring = "{\"regions\":";
                result = result.Insert(0, substring).Insert(result.Length + substring.Length, "}");
                DiscordRegionJSONWrapper wrapper = JsonUtility.FromJson<DiscordRegionJSONWrapper>(result);
                List<DiscordRegion> regionList = new List<DiscordRegion>();

                foreach (DiscordRegionJSON region in wrapper.regions)
                {
                    regionList.Add(new DiscordRegion(region));
                }

                unityInvoker.Enqueue(() => OnRegionsReceived(this, new DiscordRegionArgs() { regions = regionList.ToArray(), client = this }));
            });
        }

        internal void MoveMember(string serverID, string memberID, string channelID)
        {
            MoveMemberArgs args = new MoveMemberArgs() { channel_id = channelID };
            PATCH(APIurl + "guilds/" + serverID + "/members/" + memberID, JsonUtility.ToJson(args), null);
        }
        #endregion
    }
}
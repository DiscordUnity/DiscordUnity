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
        /// <summary> Is this client online? </summary>
        public bool isOnline { get; internal set; }
        /// <summary> Is this client a bot? </summary>
        public bool isBot { get; internal set; }
        /// <summary> Is this client blocked because of sendrate limit? </summary>
        public bool isSendRateExceeded => blockSend;
        /// <summary> The user of this client. </summary>
        public DiscordUser user { get; internal set; }

        /// <summary> The servers of this user. </summary>
        public DiscordServer[] servers { get { return !isOnline ? new DiscordServer[0] : _servers.Values.ToArray(); } }
        internal Dictionary<string, DiscordServer> _servers;

        /// <summary> The channels of this user. </summary>
        public DiscordTextChannel[] channels { get { return !isOnline ? new DiscordTextChannel[0] : _channels.Values.ToArray(); } }
        internal Dictionary<string, DiscordTextChannel> _channels
        {
            get
            {
                Dictionary<string, DiscordTextChannel> channelList = new Dictionary<string, DiscordTextChannel>();

                foreach (DiscordServer server in servers)
                {
                    foreach (DiscordTextChannel channel in server.channels)
                    {
                        channelList.Add(channel.ID, channel);
                    }
                }

                return channelList;
            }
        }

        /// <summary> The channels of this user. </summary>
        public DiscordVoiceChannel[] voicechannels { get { return !isOnline ? new DiscordVoiceChannel[0] : _voicechannels.Values.ToArray(); } }
        internal Dictionary<string, DiscordVoiceChannel> _voicechannels
        {
            get
            {
                Dictionary<string, DiscordVoiceChannel> channelList = new Dictionary<string, DiscordVoiceChannel>();

                foreach (DiscordServer server in servers)
                {
                    foreach (DiscordVoiceChannel channel in server.voicechannels)
                    {
                        channelList.Add(channel.ID, channel);
                    }
                }

                return channelList;
            }
        }

        /// <summary> The private channels of this user. </summary>
        public DiscordPrivateChannel[] privateChannels { get { return !isOnline ? new DiscordPrivateChannel[0] : _privateChannels.Values.ToArray(); } }
        internal Dictionary<string, DiscordPrivateChannel> _privateChannels;

        internal string token;
        internal string sessionID;
        internal bool blockSend = false;
        internal bool hasToken { get { return !string.IsNullOrEmpty(token); } }
        internal Queue<Action> unityInvoker;
        internal WebSocket socket;
        internal Dictionary<string, DiscordVoiceClient> voiceClients;

        private int sequence = 0;
        private int heartbeat = 41250;
        private Thread heartbeatThread;
        private Thread typingThread;
        private Thread blockThread;
        private DiscordCallback logincallback;
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
                            DiscordTextChannel result = new DiscordTextChannel(this, channel);
                            server._channels.Add(channel.id, new DiscordTextChannel(this, channel));
                            unityInvoker.Enqueue(() => OnChannelCreated(server, new DiscordTextChannelArgs() { channel = result, client = this }));
                        }
                        break;

                    case "CHANNEL_UPDATE":
                        {
                            DiscordChannelJSON channel = JsonUtility.FromJson<DiscordChannelJSON>(payload);
                            DiscordServer server = _servers[channel.guild_id];
                            if (server._channels.ContainsKey(channel.id)) server._channels.Remove(channel.id);
                            DiscordTextChannel result = new DiscordTextChannel(this, channel);
                            server._channels.Add(channel.id, new DiscordTextChannel(this, channel));
                            unityInvoker.Enqueue(() => OnChannelUpdated(server, new DiscordTextChannelArgs() { channel = result, client = this }));
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
                            DiscordTextChannel result = new DiscordTextChannel(this, channel);
                            unityInvoker.Enqueue(() => OnChannelDeleted(server, new DiscordTextChannelArgs() { channel = result, client = this }));
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
                            DiscordUser member = _servers[presence.guild_id]._members[presence.user.id];
                            unityInvoker.Enqueue(() => OnPresenceUpdated(member, new DiscordPresenceArgs() { presence = new DiscordPresence(this, member, presence), client = this }));
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
                            DiscordUser result = new DiscordUser(this, member);
                            _servers[member.guild_id]._members.Add(member.user.id, result);
                            unityInvoker.Enqueue(() => OnMemberJoined(_servers[member.guild_id], new DiscordMemberArgs() { member = result, client = this }));
                        }
                        break;

                    case "GUILD_MEMBER_UPDATE":
                        {
                            DiscordMemberJSON member = JsonUtility.FromJson<DiscordMemberJSON>(payload);
                            if (_servers[member.guild_id]._members.ContainsKey(member.user.id))
                                _servers[member.guild_id]._members.Remove(member.user.id);
                            DiscordUser result = new DiscordUser(this, member);
                            _servers[member.guild_id]._members.Add(member.user.id, result);
                            unityInvoker.Enqueue(() => OnMemberUpdated(_servers[member.guild_id], new DiscordMemberArgs() { member = result, client = this }));
                        }
                        break;

                    case "GUILD_MEMBER_REMOVE":
                        {
                            DiscordMemberJSON member = JsonUtility.FromJson<DiscordMemberJSON>(payload);
                            if (_servers[member.guild_id]._members.ContainsKey(member.user.id))
                                _servers[member.guild_id]._members.Remove(member.user.id);
                            DiscordUser result = new DiscordUser(this, member);
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
                                DiscordUser result = new DiscordUser(this, member);
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
                            DiscordRole result = new DiscordRole(this, role.role, role.guild_id);
                            _servers[role.guild_id]._roles.Add(role.role.id, result);
                            unityInvoker.Enqueue(() => OnRoleCreated(_servers[role.guild_id], new DiscordRoleArgs() { role = result, client = this }));
                        }
                        break;

                    case "GUILD_ROLE_UPDATE":
                        {
                            DiscordRoleEventJSON role = JsonUtility.FromJson<DiscordRoleEventJSON>(payload);
                            if (_servers[role.guild_id]._roles.ContainsKey(role.role.id))
                                _servers[role.guild_id]._roles.Remove(role.role.id);
                            DiscordRole result = new DiscordRole(this, role.role, role.guild_id);
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
                            DiscordVoiceStateJSON voiceState = JsonUtility.FromJson<DiscordVoiceStateJSON>(payload);
                            VoiceStateUpdateEvents(voiceState);
                        }
                        break;

                    case "VOICE_SERVER_UPDATE":
                        {
                            DiscordVoiceServerStateJSON voiceState = JsonUtility.FromJson<DiscordVoiceServerStateJSON>(payload);
                            voiceClients[voiceState.guild_id].Start(_servers[voiceState.guild_id], voiceState.endpoint, voiceState.token);
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
        /// <summary> Starts this client with user credentials. </summary>
        /// <param name="email">The email of the user.</param>
        /// <param name="password">The password of the user.</param>
        public void Start(string email, string password, DiscordCallback callback)
        {
            if (isOnline) return;
            logincallback = callback;
            LoginArgs login = new LoginArgs() { email = email, password = password };
            Call(HttpMethod.Post, "https://discordapp.com/api/auth/login", OnStart, (result) => { unityInvoker.Enqueue(() => logincallback(this, null, new DiscordError(result))); }, JsonUtility.ToJson(login));
        }

        /// <summary> Starts this client as a bot. </summary>
        /// <param name="botToken">A token received by creating a bot.</param>
        public void StartBot(string botToken, DiscordCallback callback)
        {
            if (isOnline) return;
            logincallback = callback;
            token = botToken;
            StartEventListener();
        }

        /// <summary> Updates the events. </summary>
        public void Update()
        {
            if (unityInvoker == null)
            {
                return;
            }

            while (unityInvoker.Count > 0)
            {
                try
                {
                    unityInvoker.Dequeue()();
                }

                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
        }

        /// <summary> Stops this client. </summary>
        /// <param name="callback">The callback when client is closed, be aware this is the only callback that isn't on the main unity thread.</param>
        public void Stop(DiscordCallback callback)
        {
            foreach (DiscordVoiceClient voiceClient in voiceClients.Values)
            {
                if (voiceClient != null)
                {
                    voiceClient.Dispose();
                }
            }

            voiceClients.Clear();
            Call(HttpMethod.Post, "https://discordapp.com/api/auth/logout", (result) => { callback(this, "Client logged out.", new DiscordError()); }, (result) => { callback(this, "Client logged out.", new DiscordError()); }, JsonUtility.ToJson(new DiscordTokenJSON() { token = token }));
            socket.CloseAsync();
        }

        /// <summary> You should call Stop(); </summary>
        public void Dispose()
        {
            if (isOnline)
            {
                socket.CloseAsync();
                return;
            }

            try
            {
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
                heartbeatThread.Abort();
                heartbeatThread = null;
                OnClientClosed(this, new DiscordEventArgs() { client = this });
            }

            catch { }
        }

        /// <summary> Creates a private channel. </summary>
        /// <param name="recipient">The recipient for this private channel.</param>
        public void CreatePrivateChannel(DiscordUser recipient, DiscordPrivateChannelCallback callback)
        {
            CreatePrivateChannel(user.ID, recipient.ID, callback);
        }

        /// <summary>
        /// Creates a server.
        /// </summary>
        /// <param name="servername">The name of the server.</param>
        /// <param name="region">The region for the server.</param>
        /// <param name="icon">The icon for the server.</param>
        public void CreateServer(string servername, DiscordRegion region, Texture2D icon, DiscordServerCallback callback)
        {
            if (!isOnline) return;
            Createserver(servername, region.name.ToLower().Replace(' ', '-'), icon, callback);
        }

        /// <summary>
        /// Creates a server.
        /// </summary>
        /// <param name="servername">The name of the server.</param>
        /// <param name="region">The region for the server.</param>
        /// <param name="icon">The icon for the server.</param>
        public void CreateServer(string servername, string region, Texture2D icon, DiscordServerCallback callback)
        {
            if (!isOnline) return;
            Createserver(servername, region.ToLower().Replace(' ', '-'), icon, callback);
        }

        /// <summary>
        /// Gets a fresh copy of the servers.
        /// </summary>
        public void GetServerList(DiscordServersCallback callback)
        {
            GetServers(callback);
        }

        /// <summary>
        /// Gets more info about the invite;
        /// </summary>
        /// <param name="invite">The code of the invite.</param>
        public void GetInvite(string invite, DiscordInviteCallback callback)
        {
            if (!isOnline) return;
            Getinvite(invite, callback);
        }

        /// <summary>
        /// Accepts the invite.
        /// </summary>
        /// <param name="invite">The code of the invite.</param>
        public void AcceptInvite(string invite, DiscordInviteCallback callback)
        {
            if (!isOnline) return;
            if (isBot) return;
            Acceptinvite(invite, callback);
        }

        /// <summary>
        /// Deletes the invite.
        /// </summary>
        /// <param name="invite">The code of the invite.</param>
        public void DeleteInvite(string invite, DiscordInviteCallback callback)
        {
            if (!isOnline) return;
            Deleteinvite(invite, callback);
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
        public void GetActiveMaintenances(DiscordStatusCallback callback)
        {
            if (!isOnline) return;
            GetactiveMaintenances(callback);
        }

        /// <summary>
        /// Gets more info about upcoming maintenances;
        /// </summary>
        public void GetUpcomingMaintenances(DiscordStatusCallback callback)
        {
            if (!isOnline) return;
            GetupcomingMaintenances(callback);
        }

        /// <summary>
        /// Edits the profile of the current connected account.
        /// </summary>
        /// <param name="avatar">A new avatar.</param>
        /// <param name="email">A new email.</param>
        /// <param name="username">A new username.</param>
        /// <param name="password">The old password.</param>
        /// <param name="new_password">A new password.</param>
        public void EditProfile(Texture2D avatar, string email, string username, string password, string newPassword, DiscordUserCallback callback)
        {
            if (!isOnline) return;
            Editprofile(avatar, email, newPassword, password, username, callback);
        }

        /// <summary>
        /// Gets info about regions.
        /// </summary>
        public void GetRegions(DiscordRegionsCallback callback)
        {
            if (!isOnline) return;
            GetServerRegions(callback);
        }

        /// <summary> Starts a voiceclient with a channel. </summary>
        /// <param name="channel">The channel for this voiceclient (one per server).</param>
        /// <param name="muted">Is this voiceclient muted?</param>
        /// <param name="deaf">Is this voiceclient deaf?</param>
        public void GetVoiceClient(DiscordVoiceChannel channel, bool muted, bool deaf, DiscordVoiceCallback callback)
        {
            if (voiceClients.ContainsKey(channel.serverID))
            {
                unityInvoker.Enqueue(() => OnVoiceClientOpened(this, new DiscordVoiceClientArgs() { client = this, voiceClient = voiceClients[channel.serverID] }));
                return;
            }

            DiscordVoiceClient voiceClient = new DiscordVoiceClient(this, channel);
            voiceClients.Add(channel.serverID, voiceClient);

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

        public void GetChannelByID(string channelID, DiscordChannelCallback callback)
        {
            GetChannel(channelID, callback);
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
            voiceClients = new Dictionary<string, DiscordVoiceClient>();
            GetGatewayUrl();
        }

        private void GetGatewayUrl()
        {
            if (token == null)
            {
                Debug.LogError("Token is null!");
                return;
            }

            Call(HttpMethod.Get, "https://discordapp.com/api/gateway", OnGetGatewayUrl, (result) => { unityInvoker.Enqueue(() => logincallback(this, null, new DiscordError(result))); });
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
                unityInvoker.Enqueue(() => logincallback(this, "Client opened.", new DiscordError()));
            };

            socket.OnClose += (sender, e) =>
            {
                if (!e.WasClean)
                {
                    Debug.LogError("Socket closed: " + e.Code);
                    Debug.LogError("Socket closed: " + e.Reason);
                }
                
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
            PayloadArgs<DiscordIdentityArgs> args = new PayloadArgs<DiscordIdentityArgs>()
            {
                op = 2,
                d = new DiscordIdentityArgs()
                {
                    token = token,
                    v = 4,
                    properties = new DiscordPropertiesArgs()
                    {
                        os = Environment.OSVersion.ToString(),
                        browser = "DiscordUnity",
                        device = "DiscordUnity",
                        referrer = "",
                        referring_domain = ""

                    }
                }
            };

            socket.Send(JsonUtility.ToJson(args));
            Debugger.WriteLine("SocketSend: " + JsonUtility.ToJson(args));
        }

        private void StopEventListener()
        {
            isOnline = false;
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
            
            DiscordUser member = _servers[voiceState.guild_id]._members[voiceState.user_id];
            
            if (string.IsNullOrEmpty(voiceState.channel_id))
            {
                unityInvoker.Enqueue(() => voiceClients[voiceState.guild_id].OnVoiceUserLeft(this, new DiscordUserArgs() { client = this, user = member }));
                return;
            }
            
            member.muted = voiceState.mute;
            member.deaf = voiceState.suppress;
            
            if (voiceClients.ContainsKey(voiceState.guild_id))
            {
                if (!string.IsNullOrEmpty(voiceState.session_id))
                {
                    if (member == user)
                    {
                        user.muted = voiceState.self_mute;
                        user.deaf = voiceState.self_deaf;

                        if (voiceClients[voiceState.guild_id] != null)
                        {
                            if (voiceClients[voiceState.guild_id] != null) voiceClients[voiceState.guild_id].sessionID = voiceState.session_id;
                        }
                    }
                }
                
                if (voiceClients[voiceState.guild_id] != null)
                {
                    unityInvoker.Enqueue(() => voiceClients[voiceState.guild_id].OnVoiceState(voiceClients[voiceState.guild_id], new DiscordMemberArgs() { client = this, member = member }));
                }

                _servers[voiceState.guild_id]._members[voiceState.user_id] = member;
            }
        }
        #endregion

        #region Http
        internal delegate void CallResult(string result);

        internal enum HttpMethod
        {
            Post,
            Get,
            Patch,
            Put,
            Delete
        }

        internal class RequestState
        {
            public HttpMethod method;
            public CallResult result;
            public CallResult error;
            public HttpWebRequest request;
        }

        internal class RequestStateJSON : RequestState
        {
            public string content;
        }

        internal static string APIurl = "https://discordapp.com/api/";

        internal void Call(HttpMethod method, string url, CallResult result = null, CallResult error = null, string content = null)
        {
            if (blockSend)
            {
                Debug.LogWarning("Rest is on hold because of sendrate is exceeded.");
                return;
            }

            try
            {
                HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(url);
                if (hasToken) httpRequest.Headers["authorization"] = isBot ? "Bot " + token : token;
                httpRequest.ContentType = "application/json";
                httpRequest.UserAgent = "DiscordBot (https://github.com/robinhood128/DiscordUnity, 0.0.0)";

                switch (method)
                {
                    case HttpMethod.Post:
                        httpRequest.Method = "POST";
                        if (content == null) httpRequest.BeginGetResponse(new AsyncCallback(OnGetResponse), new RequestState() { method = method, result = result, error = error, request = httpRequest });
                        else httpRequest.BeginGetRequestStream(new AsyncCallback(OnRequestStream), new RequestStateJSON() { method = method, content = content, result = result, request = httpRequest });
                        break;

                    case HttpMethod.Get:
                        httpRequest.Method = "GET";
                        httpRequest.BeginGetResponse(new AsyncCallback(OnGetResponse), new RequestState() { method = method, result = result, error = error, request = httpRequest });
                        break;

                    case HttpMethod.Patch:
                        httpRequest.Method = "PATCH";
                        httpRequest.BeginGetRequestStream(new AsyncCallback(OnRequestStream), new RequestStateJSON() { method = method, content = content, result = result, error = error, request = httpRequest });
                        break;

                    case HttpMethod.Put:
                        httpRequest.Method = "PUT";
                        if (content == null) httpRequest.BeginGetResponse(new AsyncCallback(OnGetResponse), new RequestState() { method = method, result = result, error = error, request = httpRequest });
                        else httpRequest.BeginGetRequestStream(new AsyncCallback(OnRequestStream), new RequestStateJSON() { method = method, content = content, result = result, error = error, request = httpRequest });
                        break;

                    case HttpMethod.Delete:
                        httpRequest.Method = "DELETE";
                        httpRequest.BeginGetResponse(new AsyncCallback(OnGetResponse), new RequestState() { method = method, result = result, error = error, request = httpRequest });
                        break;
                }
            }

            catch (Exception e)
            {
                Debug.LogError("#Main Call");
                Debug.LogError(e.Message);
                error(e.Message);
            }
        }

        private void OnRequestStream(IAsyncResult result)
        {
            RequestStateJSON state = (RequestStateJSON)result.AsyncState;

            try
            {
                using (StreamWriter writer = new StreamWriter(state.request.EndGetRequestStream(result)))
                {
                    //Debugger.WriteLine("Send: " + state.content);
                    writer.Write(state.content);
                    writer.Flush();
                    writer.Close();
                }

                state.request.BeginGetResponse(new AsyncCallback(OnGetResponse), state);
            }

            catch (Exception e)
            {
                Debug.LogError("#Request Call");
                Debug.LogError(e.Message);
                state.error(e.Message);
            }
        }

        private void OnGetResponse(IAsyncResult result)
        {
            RequestState state = (RequestState)result.AsyncState;

            if (blockSend)
            {
                return;
            }

            try
            {
                HttpWebResponse httpResponse = (HttpWebResponse)state.request.EndGetResponse(result);
                
                using (StreamReader reader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    string response = reader.ReadToEnd();
                    //Debugger.WriteLine("Received: " + response);
                    state.result(response);
                }
            }

            catch (WebException e)
            {
                if (e.Message.Contains("(429)") && !blockSend) // Rate limit
                {
                    int duration = 0;

                    using (StreamReader s = new StreamReader(e.Response.GetResponseStream()))
                    {
                        string response = s.ReadToEnd();
                        RateLimitJSON limit = JsonUtility.FromJson<RateLimitJSON>(response);
                        duration = limit.retry_after;
                    }

                    Debug.LogWarning("SendRateExceeded, you're blocked for " + duration + "ms.");
                    unityInvoker.Enqueue(() => OnSendBlocked(this, new DiscordSendRateArgs() { client = this, duration = duration }));
                    blockThread = new Thread(DoBlock);
                    blockThread.Start(duration);
                    return;
                }

                if (e.Message.Contains("(502)")) // Bad request
                {
                    Thread.Sleep(2000);
                    Call(state.method, state.request.RequestUri.AbsolutePath, state.result, state.error, (state.GetType() == typeof(RequestStateJSON)) ? ((RequestStateJSON)state).content : null);
                    return;
                }

                state.error(e.Message);
            }
        }

        private void DoBlock(object duration)
        {
            blockSend = true;
            Thread.Sleep((int)duration);
            blockSend = false;
            Debug.LogWarning("SendRateExceeded, you're unblocked.");
            unityInvoker.Enqueue(() => OnSendUnblocked(this, new DiscordSendRateArgs() { client = this, duration = 0 }));
        }

        private void UploadFile(string url, string file, CallResult result, CallResult error)
        {
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(url);
            httpRequest.Headers["authorization"] = isBot ? "Bot " + token : token;
            httpRequest.ContentType = "multipart/form-data; boundary=" + boundary;
            httpRequest.Method = "POST";
            httpRequest.UserAgent = "DiscordBot (https://github.com/robinhood128/DiscordUnity, 0.0.0)";
            httpRequest.KeepAlive = true;
            httpRequest.Credentials = CredentialCache.DefaultCredentials;

            httpRequest.BeginGetRequestStream(new AsyncCallback(OnRequestFileStream), new RequestStateJSON() { content = file + boundary, result = result, error = error, request = httpRequest });
        }

        private void OnRequestFileStream(IAsyncResult result)
        {
            RequestStateJSON state = (RequestStateJSON)result.AsyncState;
            string file = state.content.Split(new string[1] { "---------------------------" }, StringSplitOptions.None)[0];
            string boundary = "---------------------------" + state.content.Split(new string[1] { "---------------------------" }, StringSplitOptions.None)[1];
            byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
            
            using (Stream stream = state.request.EndGetRequestStream(result))
            {
                stream.Write(boundarybytes, 0, boundarybytes.Length);
                string header = "Content-Disposition: form-data; name=\"file\"; filename=\"" + file + "\"\r\nContent-Type: image/jpeg\r\n\r\n";
                byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
                stream.Write(headerbytes, 0, headerbytes.Length);

                using (Stream fs = File.Open(file, FileMode.Open))
                {
                    byte[] fileData = new byte[fs.Length - fs.Position];
                    fs.Read(fileData, 0, fileData.Length);
                    stream.Write(fileData, 0, fileData.Length);
                }
                
                byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
                stream.Write(trailer, 0, trailer.Length);
            }
            
            state.request.BeginGetResponse(new AsyncCallback(OnGetResponse), state);
        }
        #endregion

        #region API
        //
        // Channels
        //

        private string channelurl = "https://discordapp.com/api/channels/";

        internal void GetChannel(string channelID, DiscordChannelCallback callback)
        {
            Call(HttpMethod.Get, channelurl + channelID, (result) =>
            {
                DiscordChannelJSON json = JsonUtility.FromJson<DiscordChannelJSON>(result);
                unityInvoker.Enqueue(() => callback(this, (json.type == "text") ? new DiscordTextChannel(this, json) as DiscordChannel : new DiscordVoiceChannel(this, json) as DiscordChannel, new DiscordError()));
            }, (result) => { unityInvoker.Enqueue(() => callback(this, null, new DiscordError(result))); });
        }

        internal void CreateChannel(string serverID, string channelname, string channeltype, DiscordChannelCallback callback)
        {
            CreateChannelArgs args = new CreateChannelArgs() { name = channelname, type = channeltype };
            Call(HttpMethod.Post, APIurl + "guilds/" + serverID + "/channels", (result) => 
            {
                DiscordChannelJSON json = JsonUtility.FromJson<DiscordChannelJSON>(result);
                unityInvoker.Enqueue(() => callback(this, (json.type == "text") ? new DiscordTextChannel(this, json) as DiscordChannel : new DiscordVoiceChannel(this, json) as DiscordChannel, new DiscordError()));
            }, (result) => { unityInvoker.Enqueue(() => callback(this, null, new DiscordError(result))); }, JsonUtility.ToJson(args));
        }

        internal void EditChannel(string channelID, string channelname, string topic, int position, DiscordTextChannelCallback callback)
        {
            EditChannelArgs args = new EditChannelArgs() { name = channelname, position = position, topic = topic };
            Call(HttpMethod.Patch, channelurl + channelID, (result) =>
            {
                DiscordChannelJSON json = JsonUtility.FromJson<DiscordChannelJSON>(result);
                unityInvoker.Enqueue(() => callback(this, new DiscordTextChannel(this, json), new DiscordError()));
            }, (result) => { unityInvoker.Enqueue(() => callback(this, null, new DiscordError(result))); }, JsonUtility.ToJson(args));
        }

        internal void EditVoiceChannel(string channelID, string channelname, int position, int bitrate, int limit, DiscordVoiceChannelCallback callback)
        {
            EditVoiceChannelArgs args = new EditVoiceChannelArgs() { name = channelname, position = position, bitrate = bitrate, user_limit = limit };
            Call(HttpMethod.Patch, channelurl + channelID, (result) =>
            {
                DiscordChannelJSON json = JsonUtility.FromJson<DiscordChannelJSON>(result);
                unityInvoker.Enqueue(() => callback(this, new DiscordVoiceChannel(this, json), new DiscordError()));
            }, (result) => { unityInvoker.Enqueue(() => callback(this, null, new DiscordError(result))); }, JsonUtility.ToJson(args));
        }

        internal void DeleteChannel(string channelID, DiscordChannelCallback callback)
        {
            Call(HttpMethod.Delete, channelurl + channelID, (result) =>
            {
                DiscordChannelJSON json = JsonUtility.FromJson<DiscordChannelJSON>(result);
                unityInvoker.Enqueue(() => callback(this, (json.type == "text") ? new DiscordTextChannel(this, json) as DiscordChannel : new DiscordVoiceChannel(this, json) as DiscordChannel, new DiscordError()));
            }, (result) => { unityInvoker.Enqueue(() => callback(this, null, new DiscordError(result))); });
        }

        internal void BroadcastTyping(string channelID, DiscordCallback callback)
        {
            Call(HttpMethod.Post, channelurl + channelID + "/typing", (result) =>
            {
                unityInvoker.Enqueue(() => callback(this, "Typing broadcasted.", new DiscordError()));
            }, (result) => { unityInvoker.Enqueue(() => callback(this, null, new DiscordError(result))); });
        }

        //
        // Messages
        //

        internal DiscordMessage[] GetMessagesArray(DiscordMessageJSON[] messages)
        {
            List<DiscordMessage> result = new List<DiscordMessage>();

            foreach (DiscordMessageJSON message in messages)
            {
                result.Add(new DiscordMessage(this, message));
            }

            return result.ToArray();
        }

        internal void GetMessages(string channelID, int limit, string messageID, bool before, DiscordMessagesCallback callback)
        {
            string url = channelurl + channelID + "/messages?&limit=" + limit;
            if (before) url += "&before=" + messageID;
            else url += "&after=" + messageID;

            Call(HttpMethod.Get, url, (result) =>
            {
                string substring = "{\"messages\":";
                result = result.Insert(0, substring).Insert(result.Length + substring.Length, "}");
                DiscordMessageJSONWrapper wrapper = JsonUtility.FromJson<DiscordMessageJSONWrapper>(result);
                unityInvoker.Enqueue(() => callback(this, GetMessagesArray(wrapper.messages), new DiscordError()));
            }, (result) => { unityInvoker.Enqueue(() => callback(this, null, new DiscordError(result))); });
        }

        internal void GetMessages(string channelID, int limit, DiscordMessagesCallback callback)
        {
            string url = channelurl + channelID + "/messages?&limit=" + limit;

            Call(HttpMethod.Get, url, (result) =>
            {
                string substring = "{\"messages\":";
                result = result.Insert(0, substring).Insert(result.Length + substring.Length, "}");
                DiscordMessageJSONWrapper wrapper = JsonUtility.FromJson<DiscordMessageJSONWrapper>(result);
                unityInvoker.Enqueue(() => callback(this, GetMessagesArray(wrapper.messages), new DiscordError()));
            }, (result) => { unityInvoker.Enqueue(() => callback(this, null, new DiscordError(result))); });
        }

        internal void SendFile(string channelID, string file, DiscordCallback callback)
        {
            UploadFile(channelurl + channelID + "/messages", file, (result) => { unityInvoker.Enqueue(() => callback(this, "File send.", new DiscordError())); }, (result) => { unityInvoker.Enqueue(() => callback(this, null, new DiscordError(result))); });
        }

        internal void SendMessage(string channelID, string content, int nonce, bool textToSpeech, DiscordMessageCallback callback)
        {
            SendMessageArgs args = new SendMessageArgs() { content = content, nonce = nonce, tts = textToSpeech };
            Call(HttpMethod.Post, channelurl + channelID + "/messages", (result) =>
            {
                DiscordMessageJSON json = JsonUtility.FromJson<DiscordMessageJSON>(result);
                unityInvoker.Enqueue(() => callback(this, new DiscordMessage(this, json), new DiscordError()));
            }, (result) => { unityInvoker.Enqueue(() => callback(this, null, new DiscordError(result))); }, JsonUtility.ToJson(args));
        }

        internal void EditMessage(string channelID, string messageID, string content, DiscordMessageCallback callback)
        {
            EditMessageArgs args = new EditMessageArgs() { content = content };
            Call(HttpMethod.Patch, channelurl + channelID + "/messages/" + messageID, (result) =>
            {
                DiscordMessageJSON json = JsonUtility.FromJson<DiscordMessageJSON>(result);
                unityInvoker.Enqueue(() => callback(this, new DiscordMessage(this, json), new DiscordError()));
            }, (result) => { unityInvoker.Enqueue(() => callback(this, null, new DiscordError(result))); }, JsonUtility.ToJson(args));
        }

        internal void DeleteMessage(string channelID, string messageID, DiscordMessageCallback callback)
        {
            Call(HttpMethod.Delete, channelurl + channelID + "/messages/" + messageID, (result) =>
            {
                DiscordMessageJSON json = JsonUtility.FromJson<DiscordMessageJSON>(result);
                unityInvoker.Enqueue(() => callback(this, new DiscordMessage(this, json), new DiscordError()));
            }, (result) => { unityInvoker.Enqueue(() => callback(this, null, new DiscordError(result))); });
        }

        internal void AcknowledgeMessage(string channelID, string messageID, DiscordMessageCallback callback)
        {
            Call(HttpMethod.Post, channelurl + channelID + "/messages/" + messageID + "/ack", (result) =>
            {
                DiscordMessageJSON json = JsonUtility.FromJson<DiscordMessageJSON>(result);
                unityInvoker.Enqueue(() => callback(this, new DiscordMessage(this, json), new DiscordError()));
            }, (result) => { unityInvoker.Enqueue(() => callback(this, null, new DiscordError(result))); });
        }

        //
        // Permissions
        //

        internal void CreateOrEditPermissionRole(string channelID, string roleID, DiscordPermission[] allowed, DiscordPermission[] denied, DiscordRoleCallback callback)
        {
            CreateOrEditPermissionArgs args = new CreateOrEditPermissionArgs() { allow = Utils.GetPermissions(allowed), deny = Utils.GetPermissions(denied), id = roleID, type = "role" };
            Call(HttpMethod.Put, channelurl + channelID + "/permissions/" + roleID, (result) =>
            {
                Debug.Log(result);
                //DiscordRoleJSON json = JsonUtility.FromJson<DiscordRoleJSON>(result);
                //unityInvoker.Enqueue(() => callback(this, new DiscordRole(this, json, ""), new DiscordError()));
            }, (result) => { unityInvoker.Enqueue(() => callback(this, null, new DiscordError(result))); }, JsonUtility.ToJson(args));
        }

        internal void CreateOrEditPermissionUser(string channelID, string userID, DiscordPermission[] allowed, DiscordPermission[] denied, DiscordUserCallback callback)
        {
            CreateOrEditPermissionArgs args = new CreateOrEditPermissionArgs() { allow = Utils.GetPermissions(allowed), deny = Utils.GetPermissions(denied), id = userID, type = "member" };
            Call(HttpMethod.Put, channelurl + channelID + "/permissions/" + userID, (result) =>
            {
                Debug.Log(result);
                //DiscordMemberJSON json = JsonUtility.FromJson<DiscordMemberJSON>(result);
                //unityInvoker.Enqueue(() => callback(this, new DiscordUser(this, json, ""), new DiscordError()));
            }, (result) => { unityInvoker.Enqueue(() => callback(this, null, new DiscordError(result))); }, JsonUtility.ToJson(args));
        }

        internal void DeletePermission(string channelID, string targetID, DiscordCallback callback)
        {
            Call(HttpMethod.Delete, channelurl + channelID + "/permissions/" + targetID, (result) => { unityInvoker.Enqueue(() => callback(this, "Permission deleted.", new DiscordError())); }, (result) => { unityInvoker.Enqueue(() => callback(this, null, new DiscordError(result))); });

        }

        //
        // Servers
        //

        private static string serverurl = "https://discordapp.com/api/guilds/";

        internal void Createserver(string servername, string region, Texture2D icon, DiscordServerCallback callback)
        {
            string iconData = null;

            if (icon != null)
            {
                iconData = "data:image/jpeg;base64," + Convert.ToBase64String(icon.EncodeToJPG());
            }

            CreateServerArgs args = new CreateServerArgs() { name = servername, region = region, icon = iconData };
            Call(HttpMethod.Post, serverurl.TrimEnd('/'), (result) =>
            {
                DiscordServerJSON json = JsonUtility.FromJson<DiscordServerJSON>(result);
                unityInvoker.Enqueue(() => callback(this, new DiscordServer(this, json), new DiscordError()));
            }, (result) => { unityInvoker.Enqueue(() => callback(this, null, new DiscordError(result))); }, JsonUtility.ToJson(args));
        }

        internal void EditServer(string serverID, string servername, string ownerID, string region, int? verificationLevel, string afkchannelID, int? timeout, Texture2D icon, Texture2D splash, DiscordServerCallback callback)
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
            Call(HttpMethod.Patch, serverurl + serverID, (result) =>
            {
                DiscordServerJSON json = JsonUtility.FromJson<DiscordServerJSON>(result);
                unityInvoker.Enqueue(() => callback(this, new DiscordServer(this, json), new DiscordError()));
            }, (result) => { unityInvoker.Enqueue(() => callback(this, null, new DiscordError(result))); }, specialJson);
        }

        internal void LeaveServer(string serverID, DiscordServerCallback callback)
        {
            Call(HttpMethod.Delete, APIurl + "users/@me/guilds/" + serverID, (result) =>
            {
                DiscordServerJSON json = JsonUtility.FromJson<DiscordServerJSON>(result);
                unityInvoker.Enqueue(() => callback(this, new DiscordServer(this, json), new DiscordError()));
            }, (result) => { unityInvoker.Enqueue(() => callback(this, null, new DiscordError(result))); });
        }

        internal void DeleteServer(string serverID, DiscordServerCallback callback)
        {
            Call(HttpMethod.Delete, serverurl + serverID, (result) =>
            {
                DiscordServerJSON json = JsonUtility.FromJson<DiscordServerJSON>(result);
                unityInvoker.Enqueue(() => callback(this, new DiscordServer(this, json), new DiscordError()));
            }, (result) => { unityInvoker.Enqueue(() => callback(this, null, new DiscordError(result))); });
        }

        internal void GetServers(DiscordServersCallback callback)
        {
            Call(HttpMethod.Get, APIurl + "users/@me/guilds", (result) =>
            {
                string substring = "{\"servers\":";
                result = result.Insert(0, substring).Insert(result.Length + substring.Length, "}");
                DiscordServerJSONWrapper wrapper = JsonUtility.FromJson<DiscordServerJSONWrapper>(result);
                unityInvoker.Enqueue(() => callback(this, GetServersArray(wrapper.servers), new DiscordError()));
            }, (result) => { unityInvoker.Enqueue(() => callback(this, null, new DiscordError(result))); });
        }

        internal void GetServerChannels(string serverID, DiscordChannelsCallback callback)
        {
            Call(HttpMethod.Get, serverurl + serverID + "/channels", (result) =>
            {
                string substring = "{\"channels\":";
                result = result.Insert(0, substring).Insert(result.Length + substring.Length, "}");
                DiscordChannelJSONWrapper wrapper = JsonUtility.FromJson<DiscordChannelJSONWrapper>(result);
                unityInvoker.Enqueue(() => callback(this, GetChannelsArray(wrapper.channels), new DiscordError()));
            }, (result) => { unityInvoker.Enqueue(() => callback(this, null, new DiscordError(result))); });
        }

        internal DiscordServer[] GetServersArray(DiscordServerJSON[] servers)
        {
            List<DiscordServer> result = new List<DiscordServer>();

            foreach (DiscordServerJSON server in servers)
            {
                result.Add(new DiscordServer(this, server));
            }

            return result.ToArray();
        }

        internal DiscordChannel[] GetChannelsArray(DiscordChannelJSON[] channels)
        {
            List<DiscordChannel> result = new List<DiscordChannel>();

            foreach (DiscordChannelJSON channel in channels)
            {
                result.Add((channel.type == "text") ? new DiscordTextChannel(this, channel) as DiscordChannel : new DiscordVoiceChannel(this, channel) as DiscordChannel);
            }

            return result.ToArray();
        }

        //
        // Members
        //

        internal void EditMember(string serverID, string userID, EditMemberArgs args, DiscordUserCallback callback)
        {
            Call(HttpMethod.Patch, serverurl + serverID + "/members/" + userID, (result) =>
            {
                DiscordMemberJSON json = JsonUtility.FromJson<DiscordMemberJSON>(result);
                unityInvoker.Enqueue(() => callback(this, new DiscordUser(this, json), new DiscordError()));
            }, (result) => { unityInvoker.Enqueue(() => callback(this, null, new DiscordError(result))); }, JsonUtility.ToJson(args));
        }

        internal void KickMember(string serverID, string userID, DiscordUserCallback callback)
        {
            Call(HttpMethod.Delete, serverurl + serverID + "/members/" + userID, (result) =>
            {
                DiscordMemberJSON json = JsonUtility.FromJson<DiscordMemberJSON>(result);
                unityInvoker.Enqueue(() => callback(this, new DiscordUser(this, json), new DiscordError()));
            }, (result) => { unityInvoker.Enqueue(() => callback(this, null, new DiscordError(result))); });
        }

        //
        // Bans
        //

        internal DiscordUser[] GetUsersArray(DiscordMemberJSON[] users)
        {
            List<DiscordUser> result = new List<DiscordUser>();

            foreach (DiscordMemberJSON user in users)
            {
                result.Add(new DiscordUser(this, user));
            }

            return result.ToArray();
        }

        internal void GetBans(string serverID, DiscordUsersCallback callback)
        {
            Call(HttpMethod.Get, serverurl + serverID + "/bans", (result) =>
            {
                Debug.Log(result);
                //string substring = "{\"members\":";
                //result = result.Insert(0, substring).Insert(result.Length + substring.Length, "}");
                //DiscordMemberJSONWrapper wrapper = JsonUtility.FromJson<DiscordMemberJSONWrapper>(result);
                //unityInvoker.Enqueue(() => callback(this, GetUsersArray(wrapper.members), new DiscordError()));
            }, (result) => { unityInvoker.Enqueue(() => callback(this, null, new DiscordError(result))); });
        }

        internal void AddBan(string serverID, string userID, int clearPreviousDays, DiscordCallback callback)
        {
            Call(HttpMethod.Put, serverurl + serverID + "/bans/" + userID + "?delete-message-days=" + clearPreviousDays, (result) =>
            {
                Debug.Log(result);
            }, (result) => { unityInvoker.Enqueue(() => callback(this, null, new DiscordError(result))); });
        }

        internal void RemoveBan(string serverID, string userID, DiscordCallback callback)
        {
            Call(HttpMethod.Delete, serverurl + serverID + "/bans/" + userID, (result) =>
            {
                Debug.Log(result);
            }, (result) => { unityInvoker.Enqueue(() => callback(this, null, new DiscordError(result))); });
        }

        //
        // Roles
        //

        internal void CreateRole(string serverID, DiscordRoleCallback callback)
        {
            Call(HttpMethod.Post, serverurl + serverID + "/roles", (result) =>
            {
                DiscordRoleJSON json = JsonUtility.FromJson<DiscordRoleJSON>(result);
                unityInvoker.Enqueue(() => callback(this, new DiscordRole(this, json, serverID), new DiscordError()));
            }, (result) => { unityInvoker.Enqueue(() => callback(this, null, new DiscordError(result))); });
        }

        internal void EditRole(string serverID, string roleID, uint color, bool hoist, string name, DiscordPermission[] permissions, DiscordRoleCallback callback)
        {
            EditRoleArgs args = new EditRoleArgs() { color = color, hoist = hoist, name = name, permissions = Utils.GetPermissions(permissions) };
            Call(HttpMethod.Patch, serverurl + serverID + "/roles/" + roleID, (result) =>
            {
                DiscordRoleJSON json = JsonUtility.FromJson<DiscordRoleJSON>(result);
                unityInvoker.Enqueue(() => callback(this, new DiscordRole(this, json, serverID), new DiscordError()));
            }, (result) => { unityInvoker.Enqueue(() => callback(this, null, new DiscordError(result))); }, JsonUtility.ToJson(args));
        }

        internal void ReorderRoles(string serverID, DiscordRole[] roles, DiscordRolesCallback callback)
        {
            Call(HttpMethod.Patch, serverurl + serverID + "/roles", (result) =>
            {
                string substring = "{\"roles\":";
                result = result.Insert(0, substring).Insert(result.Length + substring.Length, "}");
                DiscordRoleJSONWrapper wrapper = JsonUtility.FromJson<DiscordRoleJSONWrapper>(result);
                unityInvoker.Enqueue(() => callback(this, GetRolesArray(serverID, wrapper.roles), new DiscordError()));
            }, (result) => { unityInvoker.Enqueue(() => callback(this, null, new DiscordError(result))); }, JsonUtility.ToJson(GetRolesOrdered(roles)));
        }

        internal void DeleteRole(string serverID, string roleID, DiscordRoleCallback callback)
        {
            Call(HttpMethod.Delete, serverurl + serverID + "/roles/" + roleID, (result) =>
            {
                DiscordRoleJSON json = JsonUtility.FromJson<DiscordRoleJSON>(result);
                unityInvoker.Enqueue(() => callback(this, new DiscordRole(this, json, serverID), new DiscordError()));
            }, (result) => { unityInvoker.Enqueue(() => callback(this, null, new DiscordError(result))); });
        }

        internal static DiscordRoleJSON[] GetRolesOrdered(DiscordRole[] roles)
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

        internal DiscordRole[] GetRolesArray(string serverID, DiscordRoleJSON[] roles)
        {
            List<DiscordRole> result = new List<DiscordRole>();

            foreach (DiscordRoleJSON role in roles)
            {
                result.Add(new DiscordRole(this, role, serverID));
            }

            return result.ToArray();
        }

        //
        // Invites
        //

        private static string inviteurl = "https://discordapp.com/api/invite/";

        internal void Getinvite(string inviteID, DiscordInviteCallback callback)
        {
            Call(HttpMethod.Get, inviteurl + inviteID, (result) =>
            {
                DiscordBasicInviteJSON invite = JsonUtility.FromJson<DiscordBasicInviteJSON>(result);
                unityInvoker.Enqueue(() => callback(this, new DiscordInvite(this, invite), new DiscordError()));
            }, (result) => { unityInvoker.Enqueue(() => callback(this, null, new DiscordError(result))); });
        }

        internal void Acceptinvite(string inviteID, DiscordInviteCallback callback)
        {
            Call(HttpMethod.Post, inviteurl + inviteID, (result) =>
            {
                DiscordBasicInviteJSON invite = JsonUtility.FromJson<DiscordBasicInviteJSON>(result);
                unityInvoker.Enqueue(() => callback(this, new DiscordInvite(this, invite), new DiscordError()));
            }, (result) => { unityInvoker.Enqueue(() => callback(this, null, new DiscordError(result))); });
        }

        internal void CreateInvite(string channelID, int maxAge, int maxUses, bool temporary, bool xkcdpass, DiscordInviteCallback callback)
        {
            DiscordInviteJSON args = new DiscordInviteJSON() { max_age = maxAge, max_uses = maxUses, temporary = temporary, xkcdpass = xkcdpass };

            Call(HttpMethod.Post, APIurl + "channels/" + channelID + "/invites", (result) =>
            {
                DiscordRichInviteJSON invite = JsonUtility.FromJson<DiscordRichInviteJSON>(result);
                unityInvoker.Enqueue(() => callback(this, new DiscordInvite(this, invite), new DiscordError()));
            }, (result) => { unityInvoker.Enqueue(() => callback(this, null, new DiscordError(result))); }, JsonUtility.ToJson(args));
        }

        internal void Deleteinvite(string inviteID, DiscordInviteCallback callback)
        {
            Call(HttpMethod.Delete, inviteurl + inviteID, (result) =>
            {
                DiscordBasicInviteJSON invite = JsonUtility.FromJson<DiscordBasicInviteJSON>(result);
                unityInvoker.Enqueue(() => callback(this, new DiscordInvite(this, invite), new DiscordError()));
            }, (result) => { unityInvoker.Enqueue(() => callback(this, null, new DiscordError(result))); });
        }

        internal void GetServerInvites(string serverID, DiscordInvitesCallback callback)
        {
            Call(HttpMethod.Get, APIurl + "guilds/" + serverID + "/invites", (result) =>
            {
                string substring = "{\"invites\":";
                result = result.Insert(0, substring).Insert(result.Length + substring.Length, "}");
                DiscordRichInviteJSONWrapper wrapper = JsonUtility.FromJson<DiscordRichInviteJSONWrapper>(result);
                unityInvoker.Enqueue(() => callback(this, GetInvitesArray(wrapper.invites), new DiscordError()));
            }, (result) => { unityInvoker.Enqueue(() => callback(this, null, new DiscordError(result))); });
        }

        internal void GetChannelInvites(string channelID, DiscordInvitesCallback callback)
        {
            Call(HttpMethod.Get, APIurl + "channels/" + channelID + "/invites", (result) =>
            {
                string substring = "{\"invites\":";
                result = result.Insert(0, substring).Insert(result.Length + substring.Length, "}");
                DiscordRichInviteJSONWrapper wrapper = JsonUtility.FromJson<DiscordRichInviteJSONWrapper>(result);
                unityInvoker.Enqueue(() => callback(this, GetInvitesArray(wrapper.invites), new DiscordError()));
            }, (result) => { unityInvoker.Enqueue(() => callback(this, null, new DiscordError(result))); });
        }

        internal DiscordInvite[] GetInvitesArray(DiscordRichInviteJSON[] invites)
        {
            List<DiscordInvite> result = new List<DiscordInvite>();

            foreach (DiscordRichInviteJSON invite in invites)
            {
                result.Add(new DiscordInvite(this, invite));
            }

            return result.ToArray();
        }

        //
        // Maintenances
        //

        private static string statusurl = "https://status.discordapp.com/api/v2/sheduled-maintenances/";

        internal void GetactiveMaintenances(DiscordStatusCallback callback)
        {
            Call(HttpMethod.Get, statusurl + "active.json", (result) =>
            {
                DiscordStatusPacketJSON status = JsonUtility.FromJson<DiscordStatusPacketJSON>(result);
                unityInvoker.Enqueue(() => callback(this, new DiscordStatusPacket(status), new DiscordError()));
            }, (result) => { unityInvoker.Enqueue(() => callback(this, null, new DiscordError(result))); });
        }

        internal void GetupcomingMaintenances(DiscordStatusCallback callback)
        {
            Call(HttpMethod.Get, statusurl + "upcoming.json", (result) =>
            {
                DiscordStatusPacketJSON status = JsonUtility.FromJson<DiscordStatusPacketJSON>(result);
                unityInvoker.Enqueue(() => callback(this, new DiscordStatusPacket(status), new DiscordError()));
            }, (result) => { unityInvoker.Enqueue(() => callback(this, null, new DiscordError(result))); });
        }

        //
        // Users
        //

        private static string userurl = "https://discordapp.com/api/users/";

        internal void CreatePrivateChannel(string userID, string recipientID, DiscordPrivateChannelCallback callback)
        {
            CreatePrivateChannelArgs args = new CreatePrivateChannelArgs() { recipient_id = recipientID };
            Call(HttpMethod.Post, userurl + userID + "/channels", (result) =>
            {
                DiscordPrivateChannelJSON channel = JsonUtility.FromJson<DiscordPrivateChannelJSON>(result);
                unityInvoker.Enqueue(() => callback(this, new DiscordPrivateChannel(this, channel), new DiscordError()));
            }, (result) => { unityInvoker.Enqueue(() => callback(this, null, new DiscordError(result))); }, JsonUtility.ToJson(args));
        }

        //
        // Profile
        //

        internal void Editprofile(Texture2D avatar, string email, string new_password, string password, string username, DiscordUserCallback callback)
        {
            string avatarData = "data:image/jpeg;base64," + Convert.ToBase64String(avatar.EncodeToJPG());
            EditProfileArgs args = new EditProfileArgs() { avatar = avatarData, email = email, new_password = new_password, password = password, username = username };
            Call(HttpMethod.Patch, userurl + "@me", (result) =>
            {
                DiscordProfileJSON profile = JsonUtility.FromJson<DiscordProfileJSON>(result);
                unityInvoker.Enqueue(() => callback(this, new DiscordUser(this, profile), new DiscordError()));
            }, (result) => { unityInvoker.Enqueue(() => callback(this, null, new DiscordError(result))); }, JsonUtility.ToJson(args));
        }

        //
        // General
        //

        private static string voiceurl = "https://discordapp.com/api/voice/";

        internal void GetServerRegions(DiscordRegionsCallback callback)
        {
            Call(HttpMethod.Get, voiceurl + "regions", (result) =>
            {
                string substring = "{\"regions\":";
                result = result.Insert(0, substring).Insert(result.Length + substring.Length, "}");
                DiscordRegionJSONWrapper wrapper = JsonUtility.FromJson<DiscordRegionJSONWrapper>(result);
                unityInvoker.Enqueue(() => callback(this, GetRegionsArray(wrapper.regions), new DiscordError()));
            }, (result) => { unityInvoker.Enqueue(() => callback(this, null, new DiscordError(result))); });
        }

        internal void MoveMember(string serverID, string memberID, string channelID, DiscordServerCallback callback)
        {
            MoveMemberArgs args = new MoveMemberArgs() { channel_id = channelID };
            Call(HttpMethod.Patch, APIurl + "guilds/" + serverID + "/members/" + memberID, (result) =>
            {
                DiscordServerJSON server = JsonUtility.FromJson<DiscordServerJSON>(result);
                unityInvoker.Enqueue(() => callback(this, new DiscordServer(this, server), new DiscordError()));
            }, (result) => { unityInvoker.Enqueue(() => callback(this, null, new DiscordError(result))); }, JsonUtility.ToJson(args));
        }

        internal DiscordRegion[] GetRegionsArray(DiscordRegionJSON[] regions)
        {
            List<DiscordRegion> result = new List<DiscordRegion>();

            foreach (DiscordRegionJSON region in regions)
            {
                result.Add(new DiscordRegion(region));
            }

            return result.ToArray();
        }
        #endregion
    }
}
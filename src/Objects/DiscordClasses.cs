using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DiscordUnity
{
    public enum MemberStatus
    {
        Idle,
        Online,
        Offline
    }

    public enum TextureState
    {
        NoTexture,
        Unloaded,
        Loading,
        Loaded
    }

    public enum TargetType
    {
        Member,
        Role
    }

    public enum DiscordChannelType
    {
        Text,
        Voice
    }

    public enum DiscordPermission : byte
    {
        CreateInstanceInvite = 0,
        KickMembers = 1,
        BanMembers = 2,
        ManageRoles = 3,
        ManageChannels = 4,
        ManageServer = 5,

        ReadMessages = 10,
        SendMessages = 11,
        SendTTSMessages = 12,
        ManageMessages = 13,
        EmbedLinks = 14,
        AttachFiles = 15,
        ReadMessageHistory = 16,
        MentionEveryone = 17,

        VoiceConnect = 20,
        VoiceSpeak = 21,
        VoiceMuteMembers = 22,
        VoiceDeafenMembers = 23,
        VoiceMoveMembers = 24,
        VoiceUseActivationDetection = 25
    }

    public class DiscordRegion
    {
        /// <summary> The name of this region. </summary>
        public string name { get; internal set; }
        /// <summary> The host of this region. </summary>
        public string hostname { get; internal set; }
        /// <summary> The port of this region used by voiceClient. </summary>
        public int port { get; internal set; }
        /// <summary> Is this region for vips? </summary>
        public bool vip { get; internal set; }
        /// <summary> Is this the best server? </summary>
        public bool optimal { get; internal set; }

        internal string ID;

        internal DiscordRegion(DiscordRegionJSON e)
        {
            name = e.name;
            hostname = e.sample_hostname;
            port = e.sample_port;
            ID = e.id;
            vip = e.vip;
            optimal = e.optimal;
        }
    }

    public class DiscordInvite
    {
        /// <summary> The code of this invite. </summary>
        public string code { get; internal set; }
        /// <summary> The passcode of this invite. </summary>
        public string xkcdpass { get; internal set; }
        /// <summary> The amount of uses left. </summary>
        public int uses { get; internal set; }
        /// <summary> The maximun amount of uses. </summary>
        public int maxUses { get; internal set; }
        /// <summary> The age. </summary>
        public int maxAge { get; internal set; }
        /// <summary> Is this a temporary invite? </summary>
        public bool temporary { get; internal set; }
        /// <summary> Is this invite revoked? </summary>
        public bool revoked { get; internal set; }
        /// <summary> Who created this invite. </summary>
        public DiscordUser inviter { get; internal set; }
        /// <summary> For what server is this invite. </summary>
        public DiscordServer server { get; internal set; }
        /// <summary> For what channel is this invite. </summary>
        public DiscordChannel channel { get; internal set; }
        /// <summary> When is this invite created? </summary>
        public DateTime createdAt { get; internal set; }

        internal DiscordInvite(DiscordClient parent, DiscordBasicInviteJSON invite)
        {
            code = invite.code;
            server = new DiscordServer(parent, invite.guild);
            channel = (invite.channel.type == "text") ? new DiscordTextChannel(parent, invite.channel) as DiscordChannel : new DiscordVoiceChannel(parent, invite.channel) as DiscordChannel;
            xkcdpass = invite.xkcdpass;
        }

        internal DiscordInvite(DiscordClient parent, DiscordRichInviteJSON invite) : this(parent, (DiscordBasicInviteJSON)invite)
        {
            uses = invite.uses;
            maxUses = invite.max_uses;
            maxAge = invite.max_age;
            temporary = invite.temporary;
            revoked = invite.revoked;
            inviter = new DiscordUser(parent, invite.inviter);
            createdAt = DateTime.Parse(invite.created_at);
        }
    }

    public class DiscordPage
    {
        /// <summary> The name of this page. </summary>
        public string name { get; internal set; }
        /// <summary> The url of this page. </summary>
        public string url { get; internal set; }
        /// <summary> When is this page updated? </summary>
        public DateTime updatedAt { get; internal set; }

        internal string ID;

        internal DiscordPage(DiscordPageJSON e)
        {
            name = e.name;
            url = e.url;
            ID = e.id;
            updatedAt = DateTime.Parse(e.updated_at);
        }

        public override bool Equals(object o)
        {
            return base.Equals(o);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(DiscordPage a, DiscordPage b)
        {
            return a.ID == b.ID;
        }

        public static bool operator !=(DiscordPage a, DiscordPage b)
        {
            return a.ID != b.ID;
        }
    }

    public class DiscordIncident
    {
        /// <summary> The status of this incident. </summary>
        public string status { get; internal set; }
        /// <summary> The content of this incident. </summary>
        public string body { get; internal set; }
        /// <summary> When is this incident-information created? </summary>
        public DateTime createdAt { get; internal set; }
        /// <summary> When is this incident-information updated? </summary>
        public DateTime updatedAt { get; internal set; }
        /// <summary> When is this incident-information displayed? </summary>
        public DateTime displayedAt { get; internal set; }

        internal string ID;
        internal string incidentID;

        internal DiscordIncident(DiscordIncidentJSON e)
        {
            status = e.status;
            body = e.body;
            createdAt = DateTime.Parse(e.created_at);
            updatedAt = DateTime.Parse(e.updated_at);
            displayedAt = DateTime.Parse(e.display_at);
            ID = e.id;
            incidentID = e.incident_id;
        }

        public override bool Equals(object o)
        {
            return base.Equals(o);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(DiscordIncident a, DiscordIncident b)
        {
            return a.ID == b.ID;
        }

        public static bool operator !=(DiscordIncident a, DiscordIncident b)
        {
            return a.ID != b.ID;
        }
    }

    public class DiscordMaintenance
    {
        /// <summary> The name of this maintenance. </summary>
        public string name { get; internal set; }
        /// <summary> The status of this maintenance. </summary>
        public string status { get; internal set; }
        /// <summary> The link to this maintenance in the browser. </summary>
        public string link { get; internal set; }
        /// <summary> The impact of this maintenance. </summary>
        public string impact { get; internal set; }
        /// <summary> The collection of incidents of this maintenance. </summary>
        public DiscordIncident[] incidents { get; internal set; }
        /// <summary> When is this maintenance-information created? </summary>
        public DateTime createdAt { get; internal set; }
        /// <summary> When is this maintenance-information updated? </summary>
        public DateTime updatedAt { get; internal set; }
        /// <summary> When will this maintenance be monitored? </summary>
        public DateTime monitoringAt { get; internal set; }
        /// <summary> When is this maintenance resolved? </summary>
        public DateTime resolvedAt { get; internal set; }
        /// <summary> When is this maintenance scheduled for? </summary>
        public DateTime scheduledFor { get; internal set; }
        /// <summary> When is this maintenance scheduled until? </summary>
        public DateTime scheduledUntil { get; internal set; }

        internal string ID;
        internal string pageID;

        internal DiscordMaintenance(DiscordMaintenanceJSON e)
        {
            name = e.name;
            status = e.status;
            link = e.shortlink;
            impact = e.impact;
            createdAt = DateTime.Parse(e.created_at);
            updatedAt = DateTime.Parse(e.updated_at);
            monitoringAt = DateTime.Parse(e.monitoring_at);
            resolvedAt = DateTime.Parse(e.resolved_at);
            scheduledFor = DateTime.Parse(e.scheduled_for);
            scheduledUntil = DateTime.Parse(e.scheduled_until);
            ID = e.id;
            pageID = e.page_id;
            List<DiscordIncident> incidentsList = new List<DiscordIncident>();

            foreach (var incident in e.incident_updates)
            {
                incidentsList.Add(new DiscordIncident(incident));
            }

            incidents = incidentsList.ToArray();
        }

        public override bool Equals(object o)
        {
            return base.Equals(o);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(DiscordMaintenance a, DiscordMaintenance b)
        {
            return a.ID == b.ID;
        }

        public static bool operator !=(DiscordMaintenance a, DiscordMaintenance b)
        {
            return a.ID != b.ID;
        }
    }

    public class DiscordStatusPacket
    {
        /// <summary> The page with information. </summary>
        public DiscordPage page { get; internal set; }
        /// <summary> The collection of maintenances. </summary>
        public DiscordMaintenance[] maintenances { get; internal set; }

        internal DiscordStatusPacket(DiscordStatusPacketJSON e)
        {
            page = new DiscordPage(e.page);
            List<DiscordMaintenance> maintenancesList = new List<DiscordMaintenance>();

            foreach (var maintenance in e.scheduled_maintenances)
            {
                maintenancesList.Add(new DiscordMaintenance(maintenance));
            }

            maintenances = maintenancesList.ToArray();
        }
    }

    public class DiscordGame
    {
        /// <summary> The name of the game this user is playing. </summary>
        public string name { get; internal set; }
    }

    public class DiscordPresence
    {
        /// <summary> The status of this user. </summary>
        public MemberStatus status { get; internal set; }
        /// <summary> The collections of roles this user has. </summary>
        public DiscordRole[] roles { get { return _roles.Values.ToArray(); } }
        /// <summary> The name of the game this user is playing. </summary>
        public string game { get; internal set; }

        internal string serverID;
        internal Dictionary<string, DiscordRole> _roles;

        internal DiscordPresence(DiscordClient parent, DiscordUser user, DiscordPresenceJSON e)
        {
            serverID = e.guild_id;

            if (!string.IsNullOrEmpty(e.status))
            {
                status = e.status == "online" ? MemberStatus.Online : e.status == "idle" ? MemberStatus.Idle : MemberStatus.Offline;
                user.status = e.status == "online" ? MemberStatus.Online : e.status == "idle" ? MemberStatus.Idle : MemberStatus.Offline;
            }

            if (e.game != null)
            {
                user.game = e.game.name;
                game = e.game.name;
            }

            _roles = new Dictionary<string, DiscordRole>();

            foreach (var role in e.roles)
            {
                if (role.id == null) continue;
                _roles.Add(role.id, new DiscordRole(parent, role, serverID));
            }
        }
    }

    public class DiscordServer
    {
        /// <summary> The collection of members this server has. </summary>
        public DiscordUser[] members { get { return _members.Values.ToArray(); } }
        /// <summary> The collection of banned members this server has. </summary>
        public DiscordUser[] bannedMembers { get { return _members.Values.Where(x => x.isBanned).ToArray(); } }
        /// <summary> The collection of channels this server has. </summary>
        public DiscordTextChannel[] channels { get { return _channels.Values.ToArray(); } }
        /// <summary> The collection of channels this server has. </summary>
        public DiscordVoiceChannel[] voicechannels { get { return _voicechannels.Values.ToArray(); } }
        /// <summary> The collection of private emojis this server has. </summary>
        public DiscordEmoji[] emojis { get { return _emojis.Values.ToArray(); } }
        /// <summary> The collection of roles this server has. </summary>
        public DiscordRole[] roles { get { return _roles.Values.ToArray(); } }
        /// <summary> The region of this server. </summary>
        public string region { get; internal set; }
        /// <summary> The name of this server. </summary>
        public string name { get; internal set; }
        /// <summary> When we joined this server. </summary>
        public DateTime joinedAt { get; internal set; }
        /// <summary> The icon of this server. </summary>
        public Texture2D icon { get; internal set; }
        /// <summary> The splash of this server. </summary>
        public Texture2D splash { get; internal set; }
        /// <summary> The state of this server's icon. </summary>
        public TextureState iconState { get; internal set; }
        /// <summary> The state of this server's splash. </summary>
        public TextureState splashState { get; internal set; }
        /// <summary> The afk channel of this server. </summary>
        public DiscordVoiceChannel afkchannel { get { return voicechannels.Where(x => x.ID == afkChannelID).FirstOrDefault(); } }
        /// <summary> The embedded channel of this server. </summary>
        public DiscordTextChannel embedchannel { get { return channels.Where(x => x.ID == embedChannelID).FirstOrDefault(); } }

        internal string ID;
        internal string iconID;
        internal string splashID;
        internal string ownerID;
        internal string afkChannelID;
        internal int afkTimout;
        internal string embedChannelID;
        internal bool isEmbed;
        internal int verificationLevel;
        internal Dictionary<string, DiscordUser> _members;
        internal Dictionary<string, DiscordTextChannel> _channels;
        internal Dictionary<string, DiscordVoiceChannel> _voicechannels;
        internal Dictionary<string, DiscordEmoji> _emojis;
        internal Dictionary<string, DiscordRole> _roles;
        internal DiscordClient client;

        internal DiscordServer(DiscordClient parent, DiscordServerJSON e)
        {
            ID = e.id;
            try { if (e.unavailable) return; } catch { } // -> mostly returns null
            client = parent;
            afkChannelID = e.afk_channel_id;
            embedChannelID = e.embed_channel_id;
            afkTimout = e.afk_timeout;
            verificationLevel = e.verification_level;
            try { isEmbed = e.embed_enabled; } catch { } // -> sometimes returns null
            region = e.region;
            ownerID = e.owner_id;
            name = e.name;
            if (!string.IsNullOrEmpty(e.joined_at)) joinedAt = DateTime.Parse(e.joined_at);

            if (!string.IsNullOrEmpty(e.icon))
            {
                iconID = e.icon;
                iconState = TextureState.Unloaded;
            }

            else
            {
                iconState = TextureState.NoTexture;
            }

            if (!string.IsNullOrEmpty(e.splash))
            {
                splashID = e.splash;
                splashState = TextureState.Unloaded;
            }

            else
            {
                splashState = TextureState.NoTexture;
            }

            _members = new Dictionary<string, DiscordUser>();
            _channels = new Dictionary<string, DiscordTextChannel>();
            _voicechannels = new Dictionary<string, DiscordVoiceChannel>();
            _emojis = new Dictionary<string, DiscordEmoji>();
            _roles = new Dictionary<string, DiscordRole>();

            if (e.members != null)
            {
                foreach (var member in e.members)
                {
                    _members.Add(member.user.id, new DiscordUser(client, member, ID));
                }
            }

            if (e.channels != null)
            {
                foreach (var channel in e.channels)
                {
                    if (channel.type == "text")
                        _channels.Add(channel.id, new DiscordTextChannel(client, channel, ID));
                    else
                        _voicechannels.Add(channel.id, new DiscordVoiceChannel(client, channel, ID));
                }
            }

            if (e.emojis != null)
            {
                foreach (var emoji in e.emojis)
                {
                    _emojis.Add(emoji.id, new DiscordEmoji(emoji));
                }
            }

            if (e.roles != null)
            {
                foreach (var role in e.roles)
                {
                    _roles.Add(role.id, new DiscordRole(client, role, ID));
                }
            }
        }

        /// <summary> Reorders the roles. </summary>
        /// <param name="roles">The roles in a specific order.</param>
        public void ReorderRoles(DiscordRole[] roles, DiscordRolesCallback callback)
        {
            client.ReorderRoles(ID, roles, callback);
        }

        /// <summary> Creates a channel. </summary>
        /// <param name="channelname">The name of the channel you want to create.</param>
        /// <param name="type">The type of channel you want to create.</param>
        public void CreateTextChannel(string channelname, DiscordChannelCallback callback)
        {
            client.CreateChannel(ID, channelname, "text", callback);
        }

        /// <summary> Creates a channel. </summary>
        /// <param name="channelname">The name of the channel you want to create.</param>
        /// <param name="type">The type of channel you want to create.</param>
        public void CreateVoiceChannel(string channelname, DiscordChannelCallback callback)
        {
            client.CreateChannel(ID, channelname, "voice", callback);
        }

        /// <summary> Edits this server. </summary>
        /// <param name="servername">The servername of this server.</param>
        /// <param name="region">The region of this server.</param>
        /// <param name="verificationLevel">The verification level of this server.</param>
        /// <param name="afkchannel">The afkchannel of this server.</param>
        /// <param name="timeout">The afk timeout of this server.</param>
        /// <param name="icon">The icon of this server.</param>
        /// <param name="splash">The splash of this server.</param>
        public void Edit(string servername, string region, int? verificationLevel, DiscordVoiceChannel afkchannel, int? timeout, Texture2D icon, Texture2D splash, DiscordServerCallback callback)
        {
            client.EditServer(ID, servername, null, region, verificationLevel, afkchannel == null ? null : afkchannel.ID, timeout, icon, splash, callback);
        }

        /// <summary> Changes the owner. </summary>
        /// <param name="newOwner">The new owner for this server.</param>
        public void ChangeOwner(DiscordUser newOwner, DiscordServerCallback callback)
        {
            client.EditServer(ID, null, newOwner.ID, null, null, null, null, null, null, callback);
        }

        /// <summary> Leaves this server. </summary>
        public void Leave(DiscordServerCallback callback)
        {
            client.LeaveServer(ID, callback);
        }

        /// <summary> Deletes this server if you're the owner. </summary>
        public void Delete(DiscordServerCallback callback)
        {
            if (client.isBot) return;
            client.DeleteServer(ID, callback);
        }

        /// <summary> Creates a new role. </summary>
        public void CreateRole(DiscordRoleCallback callback)
        {
            client.CreateRole(ID, callback);
        }

        /// <summary> Gets the invites. </summary>
        public void GetInvites(DiscordInvitesCallback callback)
        {
            client.GetServerInvites(ID, callback);
        }

        /// <summary> Gets offline members. </summary>
        public void GetOfflineMembers(string filter, int limit, DiscordUsersCallback callback)
        {
            client.GetOfflineServerMembers(ID, filter, limit);
        }

        /// <summary> Kicks the member from the server. </summary>
        public void KickMember(DiscordUser member, DiscordUserCallback callback)
        {
            client.KickMember(ID, member.ID, callback);
        }

        /// <summary> Bans the member from the server. </summary>
        public void BanMember(DiscordUser member, int clearPreviousDays, DiscordCallback callback)
        {
            client.AddBan(ID, member.ID, clearPreviousDays, callback);
        }

        /// <summary> Unbans the member from the server. </summary>
        public void UnBanMember(DiscordUser member, DiscordCallback callback)
        {
            client.RemoveBan(ID, member.ID, callback);
        }

        /// <summary> Loads the icon for this server. </summary>
        public IEnumerator LoadIcon(DiscordTextureCallback callback)
        {
            if (iconState == TextureState.Unloaded)
            {
                iconState = TextureState.Loading;
                WWW www = new WWW("https://cdn.discordapp.com/icons/" + ID + "/" + iconID + ".jpg");
                yield return www;
                Texture2D result = www.texture;

                if (result == null)
                {
                    iconState = TextureState.NoTexture;
                    yield break;
                }

                icon = www.texture;
                iconState = TextureState.Loaded;
            }
        }

        /// <summary> Loads the splash for this server. </summary>
        public IEnumerator LoadSplash(DiscordTextureCallback callback)
        {
            if (splashState == TextureState.Unloaded)
            {
                splashState = TextureState.Loading;
                WWW www = new WWW("https://cdn.discordapp.com/splashes/" + ID + "/" + splashID + ".jpg");
                yield return www;
                Texture2D result = www.texture;

                if (result == null)
                {
                    splashState = TextureState.NoTexture;
                    yield break;
                }

                splash = www.texture;
                splashState = TextureState.Loaded;
            }
        }

        public override bool Equals(object o)
        {
            return base.Equals(o);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(DiscordServer a, DiscordServer b)
        {
            return a.ID == b.ID;
        }

        public static bool operator !=(DiscordServer a, DiscordServer b)
        {
            return a.ID != b.ID;
        }
    }

    public class DiscordRole
    {
        /// <summary> The name of this role. </summary>
        public string name { get; internal set; }
        /// <summary> Is this role hoisted?. </summary>
        public bool hoist { get; internal set; }
        /// <summary> Is this role managed? </summary>
        public bool managed { get; internal set; }
        /// <summary> The color of this role. </summary>
        public Color color { get; internal set; }
        /// <summary> The collection of permissions for this role. </summary>
        public DiscordPermission[] permissions { get; internal set; }
        /// <summary> The server this role is created in. </summary>
        public DiscordServer server { get { return client._servers[serverID]; } }
        /// <summary> The position of this role. </summary>
        public int position { get { return pos; } }

        internal string ID;
        internal string serverID;
        internal int pos;
        internal DiscordClient client;

        internal DiscordRole(DiscordClient parent, DiscordRoleJSON role, string serverid)
        {
            ID = role.id;
            client = parent;
            serverID = serverid;
            hoist = role.hoist;
            managed = role.managed;
            name = role.name;
            pos = role.position;
            color = Utils.GetColorFromInt(role.color);
            permissions = Utils.GetPermissions(role.permissions);
        }

        /// <summary> Edits the role. </summary>
        /// <param name="color">The color of this server.</param>
        /// <param name="hoist">Whether this should be hoisted.</param>
        /// <param name="name">The name of this role.</param>
        /// <param name="permissions">The permissions for this role.</param>
        public void EditRole(Color color, bool hoist, string name, DiscordPermission[] permissions, DiscordRoleCallback callback)
        {
            client.EditRole(serverID, ID, Utils.GetIntFromColor(color), hoist, name, permissions, callback);
        }

        /// <summary> Deletes this role if you have the permission. </summary>
        public void DeleteRole(DiscordRoleCallback callback)
        {
            client.DeleteRole(serverID, ID, callback);
        }

        public override bool Equals(object o)
        {
            return base.Equals(o);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(DiscordRole a, DiscordRole b)
        {
            return a.ID == b.ID;
        }

        public static bool operator !=(DiscordRole a, DiscordRole b)
        {
            return a.ID != b.ID;
        }
    }

    public class DiscordEmoji
    {
        /// <summary> The name of this emoji. </summary>
        public string name { get; internal set; }
        /// <summary> The texture of this emoji. </summary>
        public Texture2D emoji { get; internal set; }
        /// <summary> The state of this emoji's texture. </summary>
        public TextureState emojiState { get; internal set; }

        internal string ID;

        internal DiscordEmoji(DiscordEmojiJSON e)
        {
            ID = e.id;
            name = e.name;
            emojiState = TextureState.Unloaded;
        }

        public IEnumerator GetEmoji(DiscordTextureCallback callback)
        {
            if (emojiState == TextureState.Unloaded)
            {
                emojiState = TextureState.Loading;
                Debug.Log("https://cdn.discordapp.com/emojis/" + ID + ".png");
                WWW www = new WWW("https://cdn.discordapp.com/emojis/" + ID + ".png");
                yield return www;
                Texture2D result = www.texture;

                if (result == null)
                {
                    emojiState = TextureState.NoTexture;
                    yield break;
                }

                emoji = www.texture;
                emojiState = TextureState.Loaded;
            }
        }

        public override bool Equals(object o)
        {
            return base.Equals(o);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(DiscordEmoji a, DiscordEmoji b)
        {
            return a.ID == b.ID;
        }

        public static bool operator !=(DiscordEmoji a, DiscordEmoji b)
        {
            return a.ID != b.ID;
        }
    }

    public class DiscordUser
    {
        /// <summary> The name of this user. </summary>
        public string name { get; internal set; }
        /// <summary> Is this user's email verified? </summary>
        public bool verifiedEmail { get; internal set; }
        /// <summary> The email of this user. </summary>
        public string email { get; internal set; }
        /// <summary> The avatar of this user. </summary>
        public Texture2D avatar { get; internal set; }
        /// <summary> The state of this user's avatar. </summary>
        public TextureState avatarState { get; internal set; }

        /// <summary> Is this member muted? </summary>
        public bool muted { get; internal set; }
        /// <summary> Is this member deaf? </summary>
        public bool deaf { get; internal set; }
        /// <summary> Is this member typing? </summary>
        public bool isTyping { get; internal set; }
        /// <summary> Is this member banned in this server? </summary>
        public bool isBanned { get; internal set; }
        /// <summary> When did this member join this server. </summary>
        public DateTime joinedAt { get; internal set; }
        /// <summary> The name of the game this member is playing. </summary>
        public string game { get; internal set; }
        /// <summary> The status of this member. </summary>
        public MemberStatus status { get; internal set; }
        /// <summary> The server this member is in </summary>
        public DiscordServer server { get { return client._servers[serverID]; } }

        internal string ID;
        internal string avatarID;
        internal string serverID;
        internal DiscordClient client;

        internal DiscordUser(DiscordClient parent, DiscordUserJSON user)
        {
            ID = user.id;
            client = parent;
            name = user.username;

            try
            {
                verifiedEmail = user.verified;
                if (user.verified) email = user.email;
            }
            catch { }

            if (!string.IsNullOrEmpty(user.avatar))
            {
                avatarID = user.avatar;
                avatarState = TextureState.Unloaded;
            }

            else
            {
                avatarState = TextureState.NoTexture;
            }
        }

        internal DiscordUser(DiscordClient parent, DiscordProfileJSON profile)
        {
            ID = profile.id;
            client = parent;
            name = profile.username;
            verifiedEmail = profile.verified;
            if (profile.verified) email = profile.email;

            if (profile.avatar != null)
            {
                avatarID = profile.avatar;
                avatarState = TextureState.Unloaded;
            }

            else
            {
                avatarState = TextureState.NoTexture;
            }
        }

        internal DiscordUser(DiscordClient parent, DiscordMemberJSON member) : this(parent, member.user)
        {
            isTyping = false;
            isBanned = false;
            serverID = member.guild_id;
            try { muted = member.mute; } catch { }
            try { deaf = member.deaf; } catch { }
            try { deaf = member.deaf; } catch { }
            status = MemberStatus.Offline;
            game = "";
            if (!string.IsNullOrEmpty(member.joined_at)) joinedAt = DateTime.Parse(member.joined_at);
        }

        internal DiscordUser(DiscordClient parent, DiscordMemberJSON member, string guild_id) : this(parent, member.user)
        {
            isTyping = false;
            isBanned = false;
            serverID = guild_id;
            try { muted = member.mute; } catch { }
            try { deaf = member.deaf; } catch { }
            if (!string.IsNullOrEmpty(member.joined_at)) joinedAt = DateTime.Parse(member.joined_at);
        }

        /// <summary> Loads the avatar of this user. </summary>
        public IEnumerator GetAvatar(DiscordTextureCallback callback)
        {
            if (avatarState == TextureState.Unloaded)
            {
                avatarState = TextureState.Loading;
                WWW www = new WWW("https://cdn.discordapp.com/avatars/" + ID + "/" + avatarID + ".jpg");
                yield return www;
                Texture2D result = www.texture;

                if (result == null)
                {
                    avatarState = TextureState.NoTexture;
                    yield break;
                }

                avatar = www.texture;
                avatarState = TextureState.Loaded;
            }
        }

        /// <summary> Edits this user. </summary>
        /// <param name="nick">The nick of this user.</param>
        /// <param name="roles">The roles of this user.</param>
        /// <param name="mute">Whether this user should be muted.</param>
        /// <param name="deaf">Whether this user should be deaf.</param>
        /// <param name="channel">The channel of this user.</param>
        public void Edit(string nick, DiscordRole[] roles, bool? mute, bool? deaf, DiscordChannel channel, DiscordUserCallback callback)
        {
            EditMemberArgs args = new EditMemberArgs() { nick = nick, roles = Utils.GetRoleIDs(roles), channel_id = channel.ID };
            if (mute != null) args.mute = mute.Value;
            if (deaf != null) args.deaf = deaf.Value;
            client.EditMember(serverID, ID, args, callback);
        }

        public override bool Equals(object o)
        {
            return base.Equals(o);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(DiscordUser a, DiscordUser b)
        {
            return a.ID == b.ID;
        }

        public static bool operator !=(DiscordUser a, DiscordUser b)
        {
            return a.ID == b.ID;
        }
    }

    public class DiscordProvider
    {
        /// <summary> The name of this provider. </summary>
        public string name { get; internal set; }
        /// <summary> The url of this provider. </summary>
        public string url { get; internal set; }
    }

    public class DiscordThumbnail
    {
        /// <summary> The url of this thumbnail. </summary>
        public string url { get; internal set; }
        /// <summary> The poxy_url of this thumbnail. </summary>
        public string poxy_url { get; internal set; }
        /// <summary> The height of this thumbnail. </summary>
        public int height { get; internal set; }
        /// <summary> The width of this thumbnail. </summary>
        public int width { get; internal set; }
    }

    public class DiscordEmbed
    {
        /// <summary> The title of this embed. </summary>
        public string title { get; internal set; }
        /// <summary> The type of this embed. </summary>
        public string type { get; internal set; }
        /// <summary> The description of this embed. </summary>
        public string description { get; internal set; }
        /// <summary> The url of this embed. </summary>
        public string url { get; internal set; }
        /// <summary> The thumbnail of this embed. </summary>
        public DiscordThumbnail thumbnail { get; internal set; }
        /// <summary> The provider of this embed. </summary>
        public DiscordProvider provider { get; internal set; }
    }

    public class DiscordMessage
    {
        /// <summary> The author of this message. </summary>
        public DiscordUser author { get; internal set; }
        /// <summary> The content of this message. </summary>
        public string content { get; internal set; }
        /// <summary> The collection this message has. </summary>
        public DiscordUser[] mentions { get { return _mentions.ToArray(); } }
        /// <summary> The collection of embeds this message has. </summary>
        public DiscordEmbed[] embeds { get { return _embeds.ToArray(); } }
        /// <summary> When is this message created? </summary>
        public DateTime createdAt { get; internal set; }
        /// <summary> When is this message edited? </summary>
        public DateTime editedAt { get; internal set; }
        /// <summary> The channel this message is created in. </summary>
        public DiscordTextChannel channel { get { return client._channels[channelID]; } }

        internal string ID;
        internal string channelID;
        internal DiscordClient client;
        internal List<DiscordUser> _mentions;
        internal List<DiscordEmbed> _embeds;

        internal DiscordMessage(DiscordClient parent, DiscordMessageJSON e)
        {
            ID = e.id;
            client = parent;
            channelID = e.channel_id;
            author = new DiscordUser(client, e.author);
            content = e.content;
            _mentions = new List<DiscordUser>();
            if (e.embeds != null) _embeds = new List<DiscordEmbed>(e.embeds);
            else _embeds = new List<DiscordEmbed>();
            if (!string.IsNullOrEmpty(e.timestamp)) createdAt = DateTime.Parse(e.timestamp);
            else createdAt = DateTime.Now;
            if (!string.IsNullOrEmpty(e.edited_timestamp)) editedAt = DateTime.Parse(e.edited_timestamp);
            else editedAt = DateTime.Now;

            if (e.mentions != null)
            {
                foreach (var mention in e.mentions)
                {
                    _mentions.Add(new DiscordUser(client, mention));
                }
            }
        }

        /// <summary> Edits this message. </summary>
        /// <param name="content">The content of this message.</param>
        public void Edit(string content, DiscordMessageCallback callback)
        {
            client.EditMessage(channelID, ID, content, callback);
        }

        /// <summary> Deletes this message. </summary>
        public void Delete(DiscordMessageCallback callback)
        {
            client.DeleteMessage(channelID, ID, callback);
        }

        /// <summary> Sets this message as read. </summary>
        public void Acknowledge(DiscordMessageCallback callback)
        {
            client.AcknowledgeMessage(channelID, ID, callback);
        }

        public override bool Equals(object o)
        {
            return base.Equals(o);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(DiscordMessage a, DiscordMessage b)
        {
            return a.ID == b.ID;
        }

        public static bool operator !=(DiscordMessage a, DiscordMessage b)
        {
            return a.ID != b.ID;
        }
    }

    public class DiscordChannel
    {
        public int position { get { return pos; } }

        internal string ID;
        internal int pos;
        internal DiscordClient client;

        public override bool Equals(object o)
        {
            return base.Equals(o);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(DiscordChannel a, DiscordChannel b)
        {
            return a.ID == b.ID;
        }

        public static bool operator !=(DiscordChannel a, DiscordChannel b)
        {
            return a.ID != b.ID;
        }
    }
    
    public class DiscordTextChannelBase : DiscordChannel
    {
        /// <summary> Deletes this channel. </summary>
        public void Delete(DiscordChannelCallback callback)
        {
            client.DeleteChannel(ID, callback);
        }

        /// <summary> Broadcasts for when you start typing. </summary>
        public void BroadcastTyping(DiscordCallback callback)
        {
            client.BroadcastTyping(ID, callback);
        }

        /// <summary> Gets the messages in this channel. </summary>
        /// <param name="limit">The limit of the amount of messages.</param>
        public void GetMessages(int limit, DiscordMessagesCallback callback)
        {
            client.GetMessages(ID, limit, callback);
        }

        /// <summary> Gets the messages in this channel. </summary>
        /// <param name="limit">The limit of the amount of messages.</param>
        /// <param name="message">A message to focus on.</param>
        /// <param name="before">Whether we should get all the message before or after the message that is focused.</param>
        public void GetMessages(int limit, DiscordMessage message, bool before, DiscordMessagesCallback callback)
        {
            client.GetMessages(ID, limit, message.ID, before, callback);
        }

        /// <summary> Sends a message to this channel. </summary>
        /// <param name="content">The content of this message.</param>
        /// <param name="textToSpeech">Whether this message should be used by text to speech.</param>
        public void SendMessage(string content, bool textToSpeech, DiscordMessageCallback callback)
        {
            client.SendMessage(ID, content, 0, textToSpeech, callback);
        }

        /// <summary> Sends a file to this channel. </summary>
        /// <param name="filePath">The path of this file.</param>
        public void SendFile(string filePath, DiscordCallback callback)
        {
            client.SendFile(ID, filePath, callback);
        }
    }
    
    public class DiscordTextChannel : DiscordTextChannelBase
    {
        /// <summary> The name of this channel. </summary>
        public string name { get; internal set; }
        /// <summary> The server where this channel is created in. </summary>
        public DiscordServer server { get { return client._servers[serverID]; } }

        internal string serverID;
        internal string lastMessageID;
        internal int bitrate = -1;

        internal DiscordTextChannel(DiscordClient parent, DiscordChannelJSON e)
        {
            ID = e.id;
            client = parent;
            serverID = e.guild_id;
            name = e.name;
            pos = e.position;
            if (e.last_message_id != null) lastMessageID = e.last_message_id;
        }

        internal DiscordTextChannel(DiscordClient parent, DiscordChannelJSON e, string guild_id)
        {
            ID = e.id;
            client = parent;
            serverID = guild_id;
            name = e.name;
            pos = e.position;
            if (e.last_message_id != null) lastMessageID = e.last_message_id;
        }

        /// <summary> Edits this channel. </summary>
        /// <param name="channelname">The name of this channel.</param>
        /// <param name="topic">The topic of this channel.</param>
        /// <param name="bitrate">The bitrate for this channel.(between 8000 to 96000)</param>
        /// <param name="limit">The max amount of users for this channel.</param>
        public void Edit(string channelname, string topic, DiscordTextChannelCallback callback)
        {
            Edit(channelname, topic, pos, callback);
        }

        /// <summary> Edits this channel. </summary>
        /// <param name="channelname">The name of this channel.</param>
        /// <param name="topic">The topic of this channel.</param>
        /// <param name="position">The position of this channel.</param>
        /// <param name="bitrate">The bitrate for this channel.(between 8000 to 96000)</param>
        /// <param name="limit">The max amount of users for this channel.</param>
        public void Edit(string channelname, string topic, int position, DiscordTextChannelCallback callback)
        {
            client.EditChannel(ID, channelname, topic, position, callback);
        }

        /// <summary> Creates an invite for this channel. </summary>
        /// <param name="maxAge">The age for this invite.</param>
        /// <param name="maxUses">The maximun amount of uses for this invite.</param>
        /// <param name="temporary">Whether this invite is temporary.</param>
        /// <param name="xkcdpass">The this invite should be in the form of a pass.</param>
        public void CreateInvite(int maxAge = 86400, int maxUses = 0, bool temporary = false, bool xkcdpass = false, DiscordInviteCallback callback = null)
        {
            client.CreateInvite(ID, maxAge, maxUses, temporary, xkcdpass, callback);
        }

        /// <summary> Gets the invites for this channel. </summary>
        public void GetInvites(DiscordInvitesCallback callback)
        {
            client.GetServerInvites(ID, callback);
        }

        /// <summary> Creates or edits custom permissions for a user. </summary>
        /// <param name="user">The user.</param>
        /// <param name="allowed">What this user is allowed to do.</param>
        /// <param name="denied">What this user is denied to do.</param>
        public void CreateEditPermission(DiscordUser user, DiscordPermission[] allowed, DiscordPermission[] denied, DiscordUserCallback callback)
        {
            client.CreateOrEditPermissionUser(ID, user.ID, allowed, denied, callback);
        }

        /// <summary> Creates or edits custom permissions for a role. </summary>
        /// <param name="role">The role.</param>
        /// <param name="allowed">What this role is allowed to do.</param>
        /// <param name="denied">What this role is denied to do.</param>
        public void CreateEditPermission(DiscordRole role, DiscordPermission[] allowed, DiscordPermission[] denied, DiscordRoleCallback callback)
        {
            client.CreateOrEditPermissionRole(ID, role.ID, allowed, denied, callback);
        }

        /// <summary> Deletes the custom permissions of a user. </summary>
        public void DeletePermission(DiscordUser user, DiscordCallback callback)
        {
            client.DeletePermission(ID, user.ID, callback);
        }

        /// <summary> Deletes the custom permissions of a role. </summary>
        public void DeletePermission(DiscordRole role, DiscordCallback callback)
        {
            client.DeletePermission(ID, role.ID, callback);
        }
    }

    public class DiscordVoiceChannel : DiscordChannel
    {
        /// <summary> The name of this channel. </summary>
        public string name { get; internal set; }
        /// <summary> The server where this channel is created in. </summary>
        public DiscordServer server { get { return client._servers[serverID]; } }

        internal string serverID;
        internal string lastMessageID;
        internal int bitrate = -1;

        internal DiscordVoiceChannel(DiscordClient parent, DiscordChannelJSON e)
        {
            ID = e.id;
            client = parent;
            serverID = e.guild_id;
            name = e.name;
            pos = e.position;
        }

        internal DiscordVoiceChannel(DiscordClient parent, DiscordChannelJSON e, string guild_id)
        {
            ID = e.id;
            client = parent;
            serverID = guild_id;
            name = e.name;
            pos = e.position;
        }

        /// <summary> Edits this channel. </summary>
        /// <param name="channelname">The name of this channel.</param>
        /// <param name="topic">The topic of this channel.</param>
        /// <param name="bitrate">The bitrate for this channel.(between 8000 to 96000)</param>
        /// <param name="limit">The max amount of users for this channel.</param>
        public void Edit(string channelname, int bitrate, int limit, DiscordVoiceChannelCallback callback)
        {
            Edit(channelname, pos, bitrate, limit, callback);
        }

        /// <summary> Edits this channel. </summary>
        /// <param name="channelname">The name of this channel.</param>
        /// <param name="topic">The topic of this channel.</param>
        /// <param name="position">The position of this channel.</param>
        /// <param name="bitrate">The bitrate for this channel.(between 8000 to 96000)</param>
        /// <param name="limit">The max amount of users for this channel.</param>
        public void Edit(string channelname, int position, int bitrate, int limit, DiscordVoiceChannelCallback callback)
        {
            client.EditVoiceChannel(ID, channelname, position, bitrate, limit, callback);
        }

        /// <summary> Creates an invite for this channel. </summary>
        /// <param name="maxAge">The age for this invite.</param>
        /// <param name="maxUses">The maximun amount of uses for this invite.</param>
        /// <param name="temporary">Whether this invite is temporary.</param>
        /// <param name="xkcdpass">The this invite should be in the form of a pass.</param>
        public void CreateInvite(int maxAge = 86400, int maxUses = 0, bool temporary = false, bool xkcdpass = false, DiscordInviteCallback callback = null)
        {
            client.CreateInvite(ID, maxAge, maxUses, temporary, xkcdpass, callback);
        }

        /// <summary> Gets the invites for this channel. </summary>
        public void GetInvites(DiscordInvitesCallback callback)
        {
            client.GetServerInvites(ID, callback);
        }

        /// <summary> Creates or edits custom permissions for a user. </summary>
        /// <param name="user">The user.</param>
        /// <param name="allowed">What this user is allowed to do.</param>
        /// <param name="denied">What this user is denied to do.</param>
        public void CreateEditPermission(DiscordUser user, DiscordPermission[] allowed, DiscordPermission[] denied, DiscordUserCallback callback)
        {
            client.CreateOrEditPermissionUser(ID, user.ID, allowed, denied, callback);
        }

        /// <summary> Creates or edits custom permissions for a role. </summary>
        /// <param name="role">The role.</param>
        /// <param name="allowed">What this role is allowed to do.</param>
        /// <param name="denied">What this role is denied to do.</param>
        public void CreateEditPermission(DiscordRole role, DiscordPermission[] allowed, DiscordPermission[] denied, DiscordRoleCallback callback)
        {
            client.CreateOrEditPermissionRole(ID, role.ID, allowed, denied, callback);
        }

        /// <summary> Deletes the custom permissions of a user. </summary>
        public void DeletePermission(DiscordUser user, DiscordCallback callback)
        {
            client.DeletePermission(ID, user.ID, callback);
        }

        /// <summary> Deletes the custom permissions of a role. </summary>
        public void DeletePermission(DiscordRole role, DiscordCallback callback)
        {
            client.DeletePermission(ID, role.ID, callback);
        }
    }

    public class DiscordPrivateChannel : DiscordTextChannelBase
    {
        /// <summary> The recipient of this private channel. </summary>
        public DiscordUser recipient { get; internal set; }

        internal string lastMessageID;

        internal DiscordPrivateChannel(DiscordClient parent, DiscordPrivateChannelJSON e)
        {
            pos = 0;
            ID = e.id;
            client = parent;
            recipient = new DiscordUser(client, e.recipient);
            lastMessageID = e.last_message_id;
        }
    }
}
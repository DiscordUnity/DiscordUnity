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
        public string name { get; internal set; }
        public string hostname { get; internal set; }
        public int port { get; internal set; }
        public bool vip { get; internal set; }
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
        public string code { get; internal set; }
        public string xkcdpass { get; internal set; }
        public int uses { get; internal set; }
        public int maxUses { get; internal set; }
        public int maxAge { get; internal set; }
        public bool temporary { get; internal set; }
        public bool revoked { get; internal set; }
        public DiscordUser inviter { get; internal set; }
        public DiscordServer server { get; internal set; }
        public DiscordChannel channel { get; internal set; }
        public DateTime createdAt { get; internal set; }

        internal DiscordInvite(DiscordClient parent, DiscordBasicInviteJSON invite)
        {
            code = invite.code;
            server = new DiscordServer(parent, invite.guild);
            channel = new DiscordChannel(parent, invite.channel);
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
        public string name { get; internal set; }
        public string url { get; internal set; }
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
        public string status { get; internal set; }
        public string body { get; internal set; }
        public DateTime createdAt { get; internal set; }
        public DateTime updatedAt { get; internal set; }
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
        public string name { get; internal set; }
        public string status { get; internal set; }
        public string link { get; internal set; }
        public string impact { get; internal set; }
        public DiscordIncident[] incidents { get; internal set; }
        public DateTime createdAt { get; internal set; }
        public DateTime updatedAt { get; internal set; }
        public DateTime monitoringAt { get; internal set; }
        public DateTime resolvedAt { get; internal set; }
        public DateTime scheduledFor { get; internal set; }
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
        public DiscordPage page { get; internal set; }
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
        public string name { get; internal set; }
    }
    
    public class DiscordPresence
    {
        public string status { get; internal set; }
        public DiscordRole[] roles { get { return _roles.Values.ToArray(); } }
        public string game { get; internal set; }

        internal string serverID;
        internal Dictionary<string, DiscordRole> _roles;

        internal DiscordPresence(DiscordUser user, DiscordPresenceJSON e)
        {
            serverID = e.guild_id;

            if (!string.IsNullOrEmpty(e.status))
            {
                status = e.status;
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
                _roles.Add(role.id, new DiscordRole(role));
            }
        }
    }
    
    public class DiscordServer
    {
        public DiscordUser[] members { get { return _members.Values.ToArray(); } }
        public DiscordUser[] bannedMembers { get { return _members.Values.Where(x => x.isBanned).ToArray(); } }
        public DiscordChannel[] channels { get { return _channels.Values.ToArray(); } }
        public DiscordEmoji[] emojis { get { return _emojis.Values.ToArray(); } }
        public DiscordRole[] roles { get { return _roles.Values.ToArray(); } }
        public string region { get; internal set; }
        public string name { get; internal set; }
        public DateTime joinedAt { get; internal set; }
        public Texture2D icon { get; internal set; }
        public Texture2D splash { get; internal set; }
        public TextureState iconState { get; internal set; }
        public TextureState splashState { get; internal set; }

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
        internal Dictionary<string, DiscordChannel> _channels;
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
            if(!string.IsNullOrEmpty(e.joined_at)) joinedAt = DateTime.Parse(e.joined_at);

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
            _channels = new Dictionary<string, DiscordChannel>();
            _emojis = new Dictionary<string, DiscordEmoji>();
            _roles = new Dictionary<string, DiscordRole>();

            if (e.members != null)
            {
                foreach (var member in e.members)
                {
                    _members.Add(member.user.id, new DiscordUser(client, member, ID));
                }
            }

            if (e.members != null)
            {
                foreach (var channel in e.channels)
                {
                    _channels.Add(channel.id, new DiscordChannel(client, channel, ID));
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
                    _roles.Add(role.id, new DiscordRole(role));
                }
            }
        }
        
        public void CreateChannel(string channelname, DiscordChannelType type)
        {
            client.CreateChannel(ID, channelname, type == DiscordChannelType.Text ? "text" : "voice");
        }

        public void Edit(string servername, string region, int? verificationLevel, DiscordChannel afkchannel, int? timeout, Texture2D icon, Texture2D splash)
        {
            client.EditServer(ID, servername, null, region, verificationLevel, afkchannel == null ? null : afkchannel.ID, timeout, icon, splash);
        }

        public void ChangeOwner(DiscordUser newOwner)
        {
            client.EditServer(ID, null, newOwner.ID, null, null, null, null, null, null);
        }

        public void Leave()
        {
            client.LeaveServer(ID);
        }

        public void Delete()
        {
            if (client.isBot) return;
            client.DeleteServer(ID);
        }

        public void CreateRole()
        {
            client.CreateRole(ID);
        }

        public void EditRole(DiscordRole role, Color color, bool hoist, string name, DiscordPermission[] permissions)
        {
            client.EditRole(ID, role.ID, Utils.GetIntFromColor(color), hoist, name, permissions);
        }

        public void ReorderRoles(DiscordRole[] roles)
        {
            client.ReorderRoles(ID, roles);
        }

        public void DeleteRole(DiscordRole role)
        {
            client.DeleteRole(ID, role.ID);
        }

        public void GetInvites()
        {
            client.GetServerInvites(ID);
        }

        public void GetOfflineMembers(string filter, int limit)
        {
            client.GetOfflineServerMembers(ID, filter, limit);
        }

        public IEnumerator GetIcon()
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

        public IEnumerator GetSplash()
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
        public string name { get; internal set; }
        public bool hoist { get; internal set; }
        public bool managed { get; internal set; }
        public Color color { get; internal set; }
        public DiscordPermission[] permissions { get; internal set; }

        internal string ID;
        internal int pos;

        internal DiscordRole(DiscordRoleJSON role)
        {
            ID = role.id;
            hoist = role.hoist;
            managed = role.managed;
            name = role.name;
            pos = role.position;
            color = Utils.GetColorFromInt(role.color);
            permissions = Utils.GetPermissions(role.permissions);
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
        public string name { get; internal set; }
        public Texture2D emoji { get; internal set; }
        public TextureState emojiState { get; internal set; }

        internal string ID;

        internal DiscordEmoji(DiscordEmojiJSON e)
        {
            ID = e.id;
            name = e.name;
            emojiState = TextureState.Unloaded;
        }

        public IEnumerator GetEmoji()
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
        public string name { get; internal set; }
        public bool verifiedEmail { get; internal set; }
        public string email { get; internal set; }
        public Texture2D avatar { get; internal set; }
        public TextureState avatarState { get; internal set; }

        public bool muted { get; internal set; }
        public bool deaf { get; internal set; }
        public bool isTyping { get; internal set; }
        public bool isBanned { get; internal set; }
        public DateTime joinedAt { get; internal set; }
        public string game { get; internal set; }
        public MemberStatus status { get; internal set; }

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
            if(profile.verified) email = profile.email;

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

        public IEnumerator GetAvatar()
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

        public void Edit(string nick, DiscordRole[] roles, bool? mute, bool? deaf, string channelID)
        {
            EditMemberArgs args = new EditMemberArgs() { nick = nick, roles = Utils.GetRoleIDs(roles), channel_id = channelID };
            if (mute != null) args.mute = mute.Value;
            if (deaf != null) args.deaf = deaf.Value;
            client.EditMember(serverID, ID, args);
        }

        public void Kick()
        {
            client.KickMember(serverID, ID);
        }

        public void Ban(int clearPreviousDays)
        {
            client.AddBan(serverID, ID, clearPreviousDays);
        }

        public void UnBan()
        {
            client.RemoveBan(serverID, ID);
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
        public string name { get; internal set; }
        public string url { get; internal set; }
    }
    
    public class DiscordThumbnail
    {
        public string url { get; internal set; }
        public string poxy_url { get; internal set; }
        public int height { get; internal set; }
        public int width { get; internal set; }
    }
    
    public class DiscordEmbed
    {
        public string title { get; internal set; }
        public string type { get; internal set; }
        public string description { get; internal set; }
        public string url { get; internal set; }
        public DiscordThumbnail thumbnail { get; internal set; }
        public DiscordProvider provider { get; internal set; }
    }
    
    public class DiscordMessage
    {
        public DiscordUser author { get; internal set; }
        public string content { get; internal set; }
        public List<DiscordUser> mentions { get; internal set; }
        public List<DiscordEmbed> embeds { get; internal set; }
        public DateTime createdAt { get; internal set; }
        public DateTime editedAt { get; internal set; }

        internal string ID;
        internal string channelID;
        internal DiscordClient client;

        internal DiscordMessage(DiscordClient parent, DiscordMessageJSON e)
        {
            ID = e.id;
            client = parent;
            channelID = e.channel_id;
            author = new DiscordUser(client, e.author);
            content = e.content;
            mentions = new List<DiscordUser>();
            if (e.embeds != null) embeds = new List<DiscordEmbed>(e.embeds);
            else embeds = new List<DiscordEmbed>();
            if (!string.IsNullOrEmpty(e.timestamp)) createdAt = DateTime.Parse(e.timestamp);
            else createdAt = DateTime.Now;
            if (!string.IsNullOrEmpty(e.edited_timestamp)) editedAt = DateTime.Parse(e.edited_timestamp);
            else editedAt = DateTime.Now;

            if (e.mentions != null)
            {
                foreach (var mention in e.mentions)
                {
                    mentions.Add(new DiscordUser(client, mention));
                }
            }
        }

        public void Edit(string content)
        {
            client.EditMessage(channelID, ID, content);
        }

        public void Delete()
        {
            client.DeleteMessage(channelID, ID);
        }

        public void Acknowledge()
        {
            client.AcknowledgeMessage(channelID, ID);
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
    
    public class DiscordChannelBase
    {
        internal string ID;
        internal int pos;
        internal DiscordClient client;

        public void Edit(string channelname, string topic)
        {
            client.EditChannel(ID, channelname, topic, pos);
        }

        public void Edit(string channelname, string topic, int position)
        {
            client.EditChannel(ID, channelname, topic, position);
        }

        public void Delete()
        {
            client.DeleteChannel(ID);
        }
        
        public void BroadcastTyping()
        {
            client.BroadcastTyping(ID);
        }

        public void GetMessages(int limit)
        {
            client.GetMessages(ID, limit);
        }

        public void GetMessages(int limit, DiscordMessage message, bool before)
        {
            client.GetMessages(ID, limit, message.ID, before);
        }

        public void SendMessage(string content, bool textToSpeech)
        {
            client.SendMessage(ID, content, 0, textToSpeech);
        }
        
        public void SendFile(string filePath)
        {
            client.SendFile(ID, filePath);
        }

        public override bool Equals(object o)
        {
            return base.Equals(o);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(DiscordChannelBase a, DiscordChannelBase b)
        {
            return a.ID == b.ID;
        }

        public static bool operator !=(DiscordChannelBase a, DiscordChannelBase b)
        {
            return a.ID != b.ID;
        }
    }
    
    public class DiscordChannel : DiscordChannelBase
    {
        public string name { get; internal set; }
        public DiscordChannelType type { get; internal set; }

        internal string serverID;
        internal string lastMessageID;
        internal int bitrate = -1;

        internal DiscordChannel(DiscordClient parent, DiscordChannelJSON e)
        {
            ID = e.id;
            client = parent;
            serverID = e.guild_id;
            name = e.name;
            pos = e.position;
            type = e.type == "text" ? DiscordChannelType.Text : DiscordChannelType.Voice;
            bitrate = type == DiscordChannelType.Voice ? e.bitrate : -1;
            if (e.last_message_id != null) lastMessageID = e.last_message_id;
        }

        internal DiscordChannel(DiscordClient parent, DiscordChannelJSON e, string guild_id)
        {
            ID = e.id;
            client = parent;
            serverID = guild_id;
            name = e.name;
            pos = e.position;
            type = e.type == "text" ? DiscordChannelType.Text : DiscordChannelType.Voice;
            if (e.last_message_id != null) lastMessageID = e.last_message_id;
        }

        public void CreateInvite(int maxAge = 86400, int maxUses = 0, bool temporary = false, bool xkcdpass = false)
        {
            client.CreateInvite(ID, maxAge, maxUses, temporary, xkcdpass);
        }

        public void GetInvites()
        {
            client.GetServerInvites(ID);
        }

        public void CreatePermission(DiscordUser user, DiscordPermission[] allowed, DiscordPermission[] denied, TargetType type)
        {
            client.CreateOrEditPermission(ID, user.ID, allowed, denied, type);
        }

        public void CreatePermission(DiscordRole role, DiscordPermission[] allowed, DiscordPermission[] denied, TargetType type)
        {
            client.CreateOrEditPermission(ID, role.ID, allowed, denied, type);
        }

        public void EditPermission(DiscordUser user, DiscordPermission[] allowed, DiscordPermission[] denied, TargetType type)
        {
            client.CreateOrEditPermission(ID, user.ID, allowed, denied, type);
        }

        public void EditPermission(DiscordRole role, DiscordPermission[] allowed, DiscordPermission[] denied, TargetType type)
        {
            client.CreateOrEditPermission(ID, role.ID, allowed, denied, type);
        }

        public void DeletePermission(DiscordUser user)
        {
            client.DeletePermission(ID, user.ID);
        }

        public void DeletePermission(DiscordRole role)
        {
            client.DeletePermission(ID, role.ID);
        }
    }
    
    public class DiscordPrivateChannel : DiscordChannelBase
    {
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
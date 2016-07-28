using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DiscordUnity
{
    public class DiscordServer
    {
        /// <summary> The collection of members this server has. </summary>
        public DiscordUser[] members { get { return _members.Values.ToArray(); } }
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
                    _members.Add(member.user.id, new DiscordUser(parent, member, ID));
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
                    _emojis.Add(emoji.id, new DiscordEmoji(client, emoji));
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
        public void CreateTextChannel(string channelname, DiscordTextChannelCallback callback)
        {
            client.CreateTextChannel(ID, channelname, callback);
        }

        /// <summary> Creates a channel. </summary>
        /// <param name="channelname">The name of the channel you want to create.</param>
        /// <param name="type">The type of channel you want to create.</param>
        public void CreateVoiceChannel(string channelname, DiscordVoiceChannelCallback callback)
        {
            client.CreateVoiceChannel(ID, channelname, callback);
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

        /// <summary> Gets all the banned members. </summary>
        public void GetBannedMembers(DiscordBansCallback callback)
        {
            client.GetBans(ID, callback);
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
                    client.unityInvoker.Enqueue(() => callback(client, null, new DiscordError("Failed to load texture.")));
                    yield break;
                }

                icon = www.texture;
                iconState = TextureState.Loaded;
                client.unityInvoker.Enqueue(() => callback(client, icon, new DiscordError()));
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
                client.unityInvoker.Enqueue(() => callback(client, splash, new DiscordError()));
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
            if ((object)a == null && (object)b == null) return true;
            if ((object)a == null && (object)b != null) return false;
            if ((object)a != null && (object)b == null) return false;
            return a.ID == b.ID;
        }

        public static bool operator !=(DiscordServer a, DiscordServer b)
        {
            return !(a == b);
        }
    }
}
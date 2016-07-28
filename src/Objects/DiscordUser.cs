using System;
using System.Collections;
using UnityEngine;

namespace DiscordUnity
{
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
                client.unityInvoker.Enqueue(() => callback(client, avatar, new DiscordError()));
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
            if ((object)a == null && (object)b == null) return true;
            if ((object)a == null && (object)b != null) return false;
            if ((object)a != null && (object)b == null) return false;
            return a.ID == b.ID;
        }

        public static bool operator !=(DiscordUser a, DiscordUser b)
        {
            return !(a == b);
        }
    }
}
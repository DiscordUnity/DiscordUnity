namespace DiscordUnity
{
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

        /// <summary> Deletes this channel. </summary>
        public void Delete(DiscordVoiceChannelCallback callback)
        {
            client.DeleteVoiceChannel(ID, callback);
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
        public void OverwritePermissions(DiscordUser user, DiscordPermission[] allowed, DiscordPermission[] denied, DiscordCallback callback)
        {
            client.CreateOrEditPermissionUser(ID, user.ID, allowed, denied, callback);
        }

        /// <summary> Creates or edits custom permissions for a role. </summary>
        /// <param name="role">The role.</param>
        /// <param name="allowed">What this role is allowed to do.</param>
        /// <param name="denied">What this role is denied to do.</param>
        public void OverwritePermissions(DiscordRole role, DiscordPermission[] allowed, DiscordPermission[] denied, DiscordCallback callback)
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
}
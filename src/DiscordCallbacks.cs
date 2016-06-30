using UnityEngine;

namespace DiscordUnity
{
    public class DiscordError
    {
        public bool failed;
        public string message;

        internal DiscordError()
        {
            failed = false;
        }

        internal DiscordError(string Message)
        {
            message = Message;
            failed = true;
        }
    }

    public delegate void DiscordCallback(DiscordClient client, string content, DiscordError error);

    public delegate void DiscordUserCallback(DiscordClient client, DiscordUser user, DiscordError error);

    public delegate void DiscordUsersCallback(DiscordClient client, DiscordUser[] users, DiscordError error);

    public delegate void DiscordRegionsCallback(DiscordClient client, DiscordRegion[] regions, DiscordError error);

    public delegate void DiscordServerCallback(DiscordClient client, DiscordServer server, DiscordError error);

    public delegate void DiscordServersCallback(DiscordClient client, DiscordServer[] servers, DiscordError error);

    public delegate void DiscordChannelCallback(DiscordClient client, DiscordChannel channel, DiscordError error);

    public delegate void DiscordChannelsCallback(DiscordClient client, DiscordChannel[] channels, DiscordError error);

    public delegate void DiscordTextChannelCallback(DiscordClient client, DiscordTextChannel channel, DiscordError error);

    public delegate void DiscordVoiceChannelCallback(DiscordClient client, DiscordVoiceChannel channel, DiscordError error);

    public delegate void DiscordPrivateChannelCallback(DiscordClient client, DiscordPrivateChannel channel, DiscordError error);

    public delegate void DiscordMessageCallback(DiscordClient client, DiscordMessage message, DiscordError error);

    public delegate void DiscordMessagesCallback(DiscordClient client, DiscordMessage[] messages, DiscordError error);

    public delegate void DiscordRoleCallback(DiscordClient client, DiscordRole role, DiscordError error);

    public delegate void DiscordRolesCallback(DiscordClient client, DiscordRole[] roles, DiscordError error);

    public delegate void DiscordInviteCallback(DiscordClient client, DiscordInvite invite, DiscordError error);

    public delegate void DiscordInvitesCallback(DiscordClient client, DiscordInvite[] invites, DiscordError error);

    public delegate void DiscordStatusCallback(DiscordClient client, DiscordStatusPacket status, DiscordError error);

    public delegate void DiscordTextureCallback(DiscordClient client, Texture2D texture, DiscordError error);


    public delegate void DiscordVoiceCallback(DiscordClient client, DiscordVoiceClient voiceclient, DiscordError error);

    public delegate void DiscordVoicePacketCallback(DiscordVoiceClient client, DiscordVoiceArgs content, DiscordError error);
}
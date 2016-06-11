using System;

namespace DiscordUnity
{
    public class DiscordEventArgs : EventArgs
    {
        public DiscordClient client;
    }

    public class DiscordChannelArgs : DiscordEventArgs
    {
        public DiscordChannel channel;
    }

    public class DiscordDMArgs : DiscordEventArgs
    {
        public DiscordPrivateChannel channel;
    }

    public class DiscordMessageArgs : DiscordEventArgs
    {
        public DiscordMessage message;
    }

    public class DiscordPresenceArgs : DiscordEventArgs
    {
        public DiscordPresence presence;
    }

    public class DiscordUserArgs : DiscordEventArgs
    {
        public DiscordUser user;
    }

    public class DiscordSendRateArgs : DiscordEventArgs
    {
        public int duration;
    }

    public class DiscordMemberArgs : DiscordEventArgs
    {
        public DiscordUser member;
    }

    public class DiscordServerArgs : DiscordEventArgs
    {
        public DiscordServer server;
    }

    public class DiscordRoleArgs : DiscordEventArgs
    {
        public DiscordRole role;
    }

    public class DiscordInviteArgs : DiscordEventArgs
    {
        public DiscordInvite invite;
    }

    public class DiscordStatusArgs : DiscordEventArgs
    {
        public DiscordStatusPacket packet;
    }

    public class DiscordRegionArgs : DiscordEventArgs
    {
        public DiscordRegion[] regions;
    }

    public class DiscordVoiceArgs : DiscordEventArgs
    {
        public DiscordChannel channel;
        public DiscordUser sender;
        public byte[] packet;
        public float[] unitypacket;
        public byte[] raw;
    }

    public class DiscordUserSpeakingArgs : DiscordEventArgs
    {
        public DiscordUser user;
        public bool speaking;
    }

    public class DiscordVoiceClientArgs : DiscordEventArgs
    {
        public DiscordVoiceClient voiceClient;
    }
}

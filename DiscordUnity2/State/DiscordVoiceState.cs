using DiscordUnity2.Models;

namespace DiscordUnity2.State
{
    public class DiscordVoiceState
    {
        public DiscordServer Server => string.IsNullOrEmpty(GuildId) ? null : DiscordAPI.Servers[GuildId];
        public DiscordChannel Channel => string.IsNullOrEmpty(GuildId) ? DiscordAPI.PrivateChannels[ChannelId] : Server.Channels[ChannelId];
        public DiscordUser User { get; internal set; }
        public DiscordServerMember Member { get; internal set; }
        public string SessionId { get; internal set; }
        public bool Deaf { get; internal set; }
        public bool Mute { get; internal set; }
        public bool SelfDeaf { get; internal set; }
        public bool SelfMute { get; internal set; }
        public bool? SelfStream { get; internal set; }
        public bool Suppress { get; internal set; }

        private readonly string GuildId;
        private readonly string ChannelId;

        internal DiscordVoiceState(VoiceStateModel model)
        {
            GuildId = model.GuildId;
            ChannelId = model.ChannelId;
            User = Channel.Recipients[model.UserId];
            if (model.Member != null) Member = new DiscordServerMember(model.Member);
            SessionId = model.SessionId;
            Deaf = model.Deaf;
            Mute = model.Mute;
            SelfDeaf = model.SelfDeaf;
            SelfMute = model.SelfMute;
            SelfStream = model.SelfStream;
            Suppress = model.Suppress;
        }
    }
}

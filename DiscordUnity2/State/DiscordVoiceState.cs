using DiscordUnity2.Models;

namespace DiscordUnity2.State
{
    public class DiscordVoiceState
    {
        public DiscordServer Server { get; internal set; }
        public DiscordChannel Channel { get; internal set; }
        public DiscordUser User { get; internal set; }
        public DiscordServerMember Member { get; internal set; }
        public string SessionId { get; internal set; }
        public bool Deaf { get; internal set; }
        public bool Mute { get; internal set; }
        public bool SelfDeaf { get; internal set; }
        public bool SelfMute { get; internal set; }
        public bool? SelfStream { get; internal set; }
        public bool Suppress { get; internal set; }

        internal DiscordVoiceState(VoiceStateModel model, DiscordServer server = null)
        {
            if (server != null) Server = server;

            else
            {
                if (string.IsNullOrEmpty(model.GuildId))
                {
                    Server = null;
                    Channel = DiscordAPI.PrivateChannels[model.ChannelId];
                }

                else
                {
                    Server = DiscordAPI.Servers[model.GuildId];
                    Channel = Server.Channels[model.ChannelId];
                }
            }

            Channel = Server.Channels[model.ChannelId];
            User = Channel.Recipients[model.UserId];
            if (model.Member != null) Member = new DiscordServerMember(model.Member, Server);
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

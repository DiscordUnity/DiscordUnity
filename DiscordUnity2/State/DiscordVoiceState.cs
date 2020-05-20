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

        internal DiscordVoiceState(VoiceStateModel model)
        {
            
        }
    }
}

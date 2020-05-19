namespace DiscordUnity2.Models
{
    public class VoiceStateModel
    {
        public string GuildId { get; set; }
        public string ChannelId { get; set; }
        public string UserId { get; set; }
        public GuildMemberModel Member { get; set; }
        public string SessionId { get; set; }
        public bool Deaf { get; set; }
        public bool Mute { get; set; }
        public bool SelfDeaf { get; set; }
        public bool SelfMute { get; set; }
        public bool? SelfStream { get; set; }
        public bool Suppress { get; set; }
    }

    public class VoiceServerModel
    {
        public string Token { get; set; }
        public string GuildId { get; set; }
        public string Endpoint { get; set; }
    }
}

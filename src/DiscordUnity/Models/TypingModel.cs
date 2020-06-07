namespace DiscordUnity.Models
{
    internal class TypingModel
    {
        public string ChannelId { get; set; }
        public string GuildId { get; set; }
        public string UserId { get; set; }
        public int Timestamp { get; set; }
        public GuildMemberModel Member { get; set; }
    }
}

namespace DiscordUnity
{
    public class DiscordEmbed
    {
        /// <summary> The title of this embed. </summary>
        public string title { get; internal set; }
        /// <summary> The type of this embed. </summary>
        public string type { get; internal set; }
        /// <summary> The description of this embed. </summary>
        public string description { get; internal set; }
        /// <summary> The url of this embed. </summary>
        public string url { get; internal set; }
        /// <summary> The thumbnail of this embed. </summary>
        public DiscordThumbnail thumbnail { get; internal set; }
        /// <summary> The provider of this embed. </summary>
        public DiscordProvider provider { get; internal set; }
    }
}
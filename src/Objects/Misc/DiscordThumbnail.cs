namespace DiscordUnity
{
    public class DiscordThumbnail
    {
        /// <summary> The url of this thumbnail. </summary>
        public string url { get; internal set; }
        /// <summary> The poxy_url of this thumbnail. </summary>
        public string poxy_url { get; internal set; }
        /// <summary> The height of this thumbnail. </summary>
        public int height { get; internal set; }
        /// <summary> The width of this thumbnail. </summary>
        public int width { get; internal set; }
    }
}
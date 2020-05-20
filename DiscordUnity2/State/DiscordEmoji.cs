using DiscordUnity2.Models;

namespace DiscordUnity2.State
{
    public class DiscordEmoji
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DiscordRole[] Roles { get; set; }
        public DiscordUser User { get; set; }
        public bool? RequireColons { get; set; }
        public bool? Managed { get; set; }
        public bool? Animated { get; set; }
        public bool? Available { get; set; }

        internal DiscordEmoji(EmojiModel model)
        {
            
        }
    }
}

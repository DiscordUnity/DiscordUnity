using DiscordUnity.Models;
using System.Linq;

namespace DiscordUnity.State
{
    public class DiscordEmoji
    {
        public string Id { get; internal set; }
        public string Name { get; internal set; }
        public DiscordRole[] Roles { get; internal set; }
        public DiscordUser User { get; internal set; }
        public bool? RequireColons { get; internal set; }
        public bool? Managed { get; internal set; }
        public bool? Animated { get; internal set; }
        public bool? Available { get; internal set; }

        internal DiscordEmoji(EmojiModel model)
        {
            Id = model.Id;
            Name = model.Name;
            Roles = model.Roles?.Select(x => new DiscordRole(x)).ToArray();
            if (model.User != null) User = new DiscordUser(model.User);
            RequireColons = model.RequireColons;
            Managed = model.Managed;
            Animated = model.Animated;
            Available = model.Available;
        }
    }
}

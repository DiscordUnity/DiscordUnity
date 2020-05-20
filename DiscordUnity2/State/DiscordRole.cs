using DiscordUnity2.Models;

namespace DiscordUnity2.State
{
    public class DiscordRole
    {
        public string Id { get; internal set; }
        public string Name { get; internal set; }
        public int Color { get; internal set; }
        public bool Hoist { get; internal set; }
        public int Position { get; internal set; }
        public int Permissions { get; internal set; }
        public bool Managed { get; internal set; }
        public bool Mentionable { get; internal set; }

        internal DiscordRole(RoleModel model)
        {
            
        }
    }
}

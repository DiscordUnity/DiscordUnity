namespace DiscordUnity2.Models
{
    public class RoleModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Color { get; set; }
        public bool Hoist { get; set; }
        public int Position { get; set; }
        public int Permissions { get; set; }
        public bool Managed { get; set; }
        public bool Mentionable { get; set; }
    }
}

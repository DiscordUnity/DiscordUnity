namespace DiscordUnity2.Models
{
    public class EmojiModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public RoleModel[] Roles { get; set; }
        public UserModel User { get; set; }
        public bool? RequireColons { get; set; }
        public bool? Managed { get; set; }
        public bool? Animated { get; set; }
        public bool? Available { get; set; }
    }
}

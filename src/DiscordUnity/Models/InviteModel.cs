using System;

namespace DiscordUnity.Models
{
    internal class InviteModel
    {
        public string ChannelId { get; set; }
        public string Code { get; set; }
        public DateTime CreatedAt { get; set; }
        public string GuildId { get; set; }
        public UserModel Inviter { get; set; }
        public int MaxAge { get; set; }
        public int MaxUses { get; set; }
        public UserModel TargetUser { get; set; }
        public int TargetUserType { get; set; }
        public bool Temporary { get; set; }
        public int Uses { get; set; }
        public int? ApproximatePresenceCount { get; set; }
        public int? ApproximateMemberCount { get; set; }
    }
}

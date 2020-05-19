namespace DiscordUnity2.Models
{
    public class UserModel
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Discriminator { get; set; }
        public string Avatar { get; set; }
        public bool? Bot { get; set; }
        public bool? System { get; set; }
        public bool? MfaEnabled { get; set; }
        public string Locale { get; set; }
        public bool? Verified { get; set; }
        public string Email { get; set; }
        public UserFlags? Flags { get; set; }
        public PremiumType? PremiumType { get; set; }
        public UserFlags? PublicFlags { get; set; }
    }

    public enum UserFlags
    {
        None = 0,
        DiscordEmployee = 1,
        DiscordPartner = 2,
        HypeSquadEvents = 4,
        BugHunterLevel1 = 8,
        HouseBravery = 16,
        HouseBrilliance = 32,
        HouseBalance = 64,
        EarlySupporter = 128,
        TeamUser = 256,
        System = 512,
        BugHunterLevel2 = 1024,
        VerifiedBot = 2048,
        VerifiedBotDeveloper = 4096
    }
}

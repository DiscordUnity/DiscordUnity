using DiscordUnity2.Models;

namespace DiscordUnity2.State
{
    public class DiscordUser
    {
        public string Id { get; internal set; }
        public string Username { get; internal set; }
        public string Discriminator { get; internal set; }
        public string Avatar { get; internal set; }
        public bool? Bot { get; internal set; }
        public bool? System { get; internal set; }
        public bool? MfaEnabled { get; internal set; }
        public string Locale { get; internal set; }
        public bool? Verified { get; internal set; }
        public string Email { get; internal set; }
        public UserFlags? Flags { get; internal set; }
        public PremiumType? PremiumType { get; internal set; }
        public UserFlags? PublicFlags { get; internal set; }

        internal DiscordUser(UserModel model)
        { 
        
        }
    }
}

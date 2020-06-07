using DiscordUnity.Models;

namespace DiscordUnity.State
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
            Id = model.Id;
            Username = model.Username;
            Discriminator = model.Discriminator;
            Avatar = model.Avatar;
            Bot = model.Bot;
            System = model.System;
            MfaEnabled = model.MfaEnabled;
            Locale = model.Locale;
            Verified = model.Verified;
            Email = model.Email;
            Flags = model.Flags;
            PremiumType = model.PremiumType;
            PublicFlags = model.PublicFlags;
        }
    }
}

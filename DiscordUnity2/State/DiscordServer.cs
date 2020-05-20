using DiscordUnity2.Models;
using System;

namespace DiscordUnity2.State
{
    public class DiscordServer
    {
        public string Id { get; internal set; }
        public string Name { get; internal set; }
        public string Icon { get; internal set; }
        public string Splash { get; internal set; }
        public string DiscoverySplash { get; internal set; }
        public bool IsOwner { get; internal set; }
        public DiscordServerMember Owner { get; internal set; }
        public int? Permissions { get; internal set; }
        public string Region { get; internal set; }
        public DiscordChannel AfkChannel { get; internal set; }
        public int AfkTimeout { get; internal set; }
        public bool EmbedEnabled { get; internal set; }
        public DiscordChannel EmbedChannel { get; internal set; }
        public int VerificationLevel { get; internal set; }
        public int DefaultMessageNotifications { get; internal set; }
        public int ExplicitContentFilter { get; internal set; }
        public DiscordRole[] Roles { get; internal set; }
        public DiscordEmoji[] Emojis { get; internal set; }
        public GuildFeature[] Features { get; internal set; }
        public int MfaLevel { get; internal set; }
        public string ApplicationId { get; internal set; }
        public bool WidgetEnabled { get; internal set; }
        public DiscordChannel WidgetChannel { get; internal set; }
        public DiscordChannel SystemChannel { get; internal set; }
        public int SystemChannelFlags { get; internal set; }
        public DiscordChannel RulesChannel { get; internal set; }
        public DateTime? JoinedAt { get; internal set; }
        public bool? Large { get; internal set; }
        public bool? Unavailable { get; internal set; }
        public int? MemberCount { get; internal set; }
        public DiscordVoiceState[] VoiceStates { get; internal set; }
        public DiscordServerMember[] Members { get; internal set; }
        public DiscordChannel[] Channels { get; internal set; }
        public DiscordPresence[] Presences { get; internal set; }
        public int? MaxPresences { get; internal set; }
        public int? MaxMembers { get; internal set; }
        public string VanityUrlCode { get; internal set; }
        public string Description { get; internal set; }
        public string Banner { get; internal set; }
        public int PremiumTier { get; internal set; }
        public int? PremiumSubscriptionCount { get; internal set; }
        public string PreferredLocale { get; internal set; }
        public DiscordChannel PublicUpdatesChannel { get; internal set; }
        public int MaxVideoChannelUsers { get; internal set; }
        public int ApproximateMemberCount { get; internal set; }
        public int ApproximatePresenceCount { get; internal set; }

        internal DiscordServer(GuildModel model)
        {
            
        }
    }

    public class DiscordServerMember
    {
        public DiscordUser User { get; internal set; }
        public string Nick { get; internal set; }
        public DiscordRole Roles { get; internal set; }
        public DateTime JoinedAt { get; internal set; }
        public DateTime? PremiumSince { get; internal set; }
        public bool Deaf { get; internal set; }
        public bool Mute { get; internal set; }

        internal DiscordServerMember(GuildMemberModel model)
        {
            
        }
    }
}

using System;

namespace DiscordUnity2.Models
{
    public class GuildModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Splash { get; set; }
        public string DiscoverySplash { get; set; }
        public bool? Owner { get; set; }
        public string OwnerId { get; set; }
        public int? Permissions { get; set; }
        public string Region { get; set; }
        public string AfkChannelId { get; set; }
        public int AfkTimeout { get; set; }
        public bool? EmbedEnabled { get; set; }
        public string EmbedEnabledId { get; set; }
        public int VerificationLevel { get; set; }
        public int DefaultMessageNotifications { get; set; }
        public int ExplicitContentFilter { get; set; }
        public RoleModel[] Roles { get; set; }
        public EmojiModel[] Emojis { get; set; }
        public GuildFeature[] Features { get; set; }
        public int MfaLevel { get; set; }
        public string ApplicationId { get; set; }
        public bool? WidgetEnabled { get; set; }
        public string WidgetChannelId { get; set; }
        public string SystemChannelId { get; set; }
        public int SystemChannelFlags { get; set; }
        public string RulesChannelId { get; set; }
        public DateTime? JoinedAt { get; set; }
        public bool? Large { get; set; }
        public bool? Unavailable { get; set; }
        public int? MemberCount { get; set; }
        public VoiceStateModel[] VoiceStates { get; set; }
        public GuildMemberModel[] Members { get; set; }
        public ChannelModel[] Channels { get; set; }
        public PresenceModel[] Presences { get; set; }
        public int? MaxPresences { get; set; }
        public int? MaxMembers { get; set; }
        public string VanityUrlCode { get; set; }
        public string Description { get; set; }
        public string Banner { get; set; }
        public int PremiumTier { get; set; }
        public int? PremiumSubscriptionCount { get; set; }
        public string PreferredLocale { get; set; }
        public string PublicUpdatesChannelId { get; set; }
        public int MaxVideoChannelUsers { get; set; }
        public int ApproximateMemberCount { get; set; }
        public int ApproximatePresenceCount { get; set; }
    }

    public enum GuildFeature
    {
        INVITE_SPLASH,          // Guild has access to set an invite splash background
        VIP_REGIONS,            // Guild has access to set 384kbps bitrate in voice (previously VIP voice servers)
        VANITY_URL,             // Guild has access to set a vanity URL
        VERIFIED,               // Guild is verified
        PARTNERED,              // Guild is partnered
        PUBLIC,                 // Guild is public
        COMMERCE,               // Guild has access to use commerce features (i.e. create store channels)
        NEWS,                   // Guild has access to create news channels
        DISCOVERABLE,           // Guild is able to be discovered in the directory
        FEATURABLE,             // Guild is able to be featured in the directory
        ANIMATED_ICON,          // Guild has access to set an animated guild icon
        BANNER,                 // Guild has access to set a guild banner image
        PUBLIC_DISABLED,        // Guild cannot be public
        WELCOME_SCREEN_ENABLED	// Guild has enabled the welcome screen
    }

    public class GuildMemberModel
    {
        public string GuildId { get; set; }
        public UserModel User { get; set; }
        public string Nick { get; set; }
        public string[] Roles { get; set; }
        public DateTime JoinedAt { get; set; }
        public DateTime PremiumSince { get; set; }
        public bool Deaf { get; set; }
        public bool Mute { get; set; }
    }

    public class GuildBanModel
    {
        public string GuildId { get; set; }
        public UserModel User { get; set; }
    }

    public class GuildEmojisModel
    {
        public string GuildId { get; set; }
        public EmojiModel[] Emojis { get; set; }
    }

    public class GuildRoleModel
    {
        public string GuildId { get; set; }
        public RoleModel Role { get; set; }
    }

    public class GuildRoleIdModel
    {
        public string GuildId { get; set; }
        public string RoleId { get; set; }
    }

    public class GuildIntegrationsModel
    {
        public string GuildId { get; set; }
    }

    public class GuildMembersChunkModel
    {
        public string GuildId { get; set; }
        public GuildMemberModel[] Members { get; set; }
        public int ChunkIndex { get; set; }
        public int ChunkCount { get; set; }
        public string[] NotFound { get; set; }
        public PresenceModel[] Presences { get; set; }
        public string Nonce { get; set; }
    }

    public class WebhookModel
    {
        public string Id { get; set; }
        public WebhookType Type { get; set; }
        public string GuildId { get; set; }
        public string ChannelId { get; set; }
        public UserModel User { get; set; }
        public string Name { get; set; }
        public string Avatar { get; set; }
        public string Token { get; set; }
    }

    public enum WebhookType
    {
        Incoming = 1,           // Incoming Webhooks can post messages to channels with a generated token
        ChannelFollower = 2     // Channel Follower Webhooks are internal webhooks used with Channel Following to post new messages into channels
    }

    public class ServerWebhookModel
    {
        public string GuildId { get; set; }
        public string ChannelId { get; set; }
    }
}

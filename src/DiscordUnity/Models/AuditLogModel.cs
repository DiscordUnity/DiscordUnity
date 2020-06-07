using Newtonsoft.Json;
using System;

namespace DiscordUnity.Models
{
    internal class AuditLogModel
    {
        public WebhookModel[] Webhooks { get; set; }
        public UserModel[] Users { get; set; }
        public AuditLogEntry[] Entries { get; set; }
        public IntegrationModel[] Integrations { get; set; }
    }

    internal class IntegrationModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public bool Enabled { get; set; }
        public bool Syncing { get; set; }
        public string RoleId { get; set; }
        public bool? EnabledEmoticons { get; set; }
        public IntegrationExpireBehavior ExpireBehavior { get; set; }
        public int ExpireGracePeriod { get; set; }
        public UserModel User { get; set; }
        public IntegrationAccountModel Account { get; set; }
        public DateTime SyncedAt { get; set; }
    }

    internal class IntegrationAccountModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public enum IntegrationExpireBehavior
    {
        RemoveRole = 0,
        Kick = 1
    }

    internal class AuditLogEntry
    {
        public string TargetId { get; set; }
        public AuditLogChange[] Changes { get; set; }
        public string UserId { get; set; }
        public string Id { get; set; }
        public AuditLogEvent ActionType { get; set; }
        public AuditEntryInfo Options { get; set; }
        public string Reason { get; set; }
    }

    public enum AuditLogEvent
    {
        GUILD_UPDATE = 1,
        CHANNEL_CREATE = 10,
        CHANNEL_UPDATE = 11,
        CHANNEL_DELETE = 12,
        CHANNEL_OVERWRITE_CREATE = 13,
        CHANNEL_OVERWRITE_UPDATE = 14,
        CHANNEL_OVERWRITE_DELETE = 15,
        MEMBER_KICK = 20,
        MEMBER_PRUNE = 21,
        MEMBER_BAN_ADD = 22,
        MEMBER_BAN_REMOVE = 23,
        MEMBER_UPDATE = 24,
        MEMBER_ROLE_UPDATE = 25,
        MEMBER_MOVE = 26,
        MEMBER_DISCONNECT = 27,
        BOT_ADD = 28,
        ROLE_CREATE = 30,
        ROLE_UPDATE = 31,
        ROLE_DELETE = 32,
        INVITE_CREATE = 40,
        INVITE_UPDATE = 41,
        INVITE_DELETE = 42,
        WEBHOOK_CREATE = 50,
        WEBHOOK_UPDATE = 51,
        WEBHOOK_DELETE = 52,
        EMOJI_CREATE = 60,
        EMOJI_UPDATE = 61,
        EMOJI_DELETE = 62,
        MESSAGE_DELETE = 72,
        MESSAGE_BULK_DELETE = 73,
        MESSAGE_PIN = 74,
        MESSAGE_UNPIN = 75,
        INTEGRATION_CREATE = 80,
        INTEGRATION_UPDATE = 81,
        INTEGRATION_DELETE = 82
    }

    internal class AuditEntryInfo
    {
        public string DeleteMemberDays { get; set; }
        public string MembersRemoved { get; set; }
        public string ChannelId { get; set; }
        public string MessageId { get; set; }
        public string Count { get; set; }
        public string Id { get; set; }
        public string Type { get; set; }
        public string RoleName { get; set; }
    }

    internal class AuditLogChange
    {
        public AuditLogChangeKey NewValue { get; set; }
        public AuditLogChangeKey OldValue { get; set; }
        public string Key { get; set; }
    }

    internal class AuditLogChangeKey
    {
        public string Name { get; set; }
        public string IconHash { get; set; }
        public string SplashHash { get; set; }
        public string OwnerId { get; set; }
        public string Region { get; set; }
        public string AfkChannelId { get; set; }
        public int? AfkTimeout { get; set; }
        public int? MfaLevel { get; set; }
        public int? VerficationLevel { get; set; }
        public int? ExplicitContentFilter { get; set; }
        public int? DefaultMessageNotifications { get; set; }
        public string VanityUrlCode { get; set; }
        [JsonProperty("$add")]
        public RoleModel[] Add { get; set; }
        [JsonProperty("remove")]
        public RoleModel[] Remove { get; set; }
        public int PruneDeleteDays { get; set; }
        public bool WidgetBoolean { get; set; }
        public string WidgetChannelId { get; set; }
        public string SystemChannelId { get; set; }
        public int Position { get; set; }
        public string Topic { get; set; }
        public int Bitrate { get; set; }
        public OverwriteModel[] PermissionOverwrites { get; set; }
        public bool Nsfw { get; set; }
        public string ApplicationId { get; set; }
        public int RateLimitPerUser { get; set; }
        public int Permissions { get; set; }
        public int Color { get; set; }
        public bool Hoist { get; set; }
        public bool Mentionable { get; set; }
        public int Allow { get; set; }
        public int Deny { get; set; }
        public string Code { get; set; }
        public string ChannelId { get; set; }
        public string InviterId { get; set; }
        public int MaxUses { get; set; }
        public int Uses { get; set; }
        public int MaxAge { get; set; }
        public bool Temporary { get; set; }
        public bool Deaf { get; set; }
        public bool Mute { get; set; }
        public string Nick { get; set; }
        public string AvatarHash { get; set; }
        public string Id { get; set; }
        public string Type { get; set; }
        public bool EnableEmoticons { get; set; }
        public int ExpireBehavior { get; set; }
        public int ExpireGracePeriod { get; set; }

    }
}
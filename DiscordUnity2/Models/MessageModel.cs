using System;

namespace DiscordUnity2.Models
{
    public class MessageModel
    {
        public string Id { get; set; }
        public string ChannelId { get; set; }
        public string GuildId { get; set; }
        public UserModel Author { get; set; }
        public GuildMemberModel Member { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
        public DateTime? EditedTimestamp { get; set; }
        public bool Tts { get; set; }
        public bool MentionEveryone { get; set; }
        public UserModel[] Mentions { get; set; }
        public RoleModel[] MentionRoles { get; set; }
        public ChannelMentionModel[] MentionChannels { get; set; }
        public AttachmentModel[] Attachments { get; set; }
        public EmbedModel[] Embeds { get; set; }
        public ReactionModel[] Reactions { get; set; }
        public object Nonce { get; set; }
        public bool Pinned { get; set; }
        public string WebhookId { get; set; }
        public MessageType Type { get; set; }
        public MessageActivityModel Activity { get; set; }
        public MessageApplicationModel Application { get; set; }
        public MessageReferenceModel MessageReference { get; set; }
        public MessageFlags Flags { get; set; }
    }

    public class ChannelMentionModel
    {
        public string Id { get; set; }
        public string GuildId { get; set; }
        public ChannelType Type { get; set; }
        public string Name { get; set; }
    }

    public class AttachmentModel
    {
        public string Id { get; set; }
        public string Filename { get; set; }
        public int Size { get; set; }
        public string Url { get; set; }
        public string ProxyUrl { get; set; }
        public int? Height { get; set; }
        public int? Width { get; set; }
    }

    public class EmbedModel
    {
        public string Title { get; set; }
        public EmbedType Type { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public DateTime? Timestamp { get; set; }
        public int Color { get; set; }
        public EmbedFooterModel Footer { get; set; }
        public EmbedImageModel Image { get; set; }
        public EmbedThumbnailModel Thumbnail { get; set; }
        public EmbedVideoModel Video { get; set; }
        public EmbedProviderModel Provider { get; set; }
        public EmbedAuthorModel Author { get; set; }
        public EmbedFieldModel[] Fields { get; set; }
    }

    public enum EmbedType
    {
        Rich,
        Image,
        Video,
        Gifv,
        Article,
        Link
    }

    public class EmbedFooterModel
    {
        public string Text { get; set; }
        public string IconUrl { get; set; }
        public string ProxyIconUrl { get; set; }
    }

    public class EmbedImageModel
    {
        public string Url { get; set; }
        public string proxyUrl { get; set; }
        public int? Height { get; set; }
        public int? Width { get; set; }
    }

    public class EmbedThumbnailModel
    {
        public string Url { get; set; }
        public string proxyUrl { get; set; }
        public int? Height { get; set; }
        public int? Width { get; set; }
    }

    public class EmbedVideoModel
    {
        public string Url { get; set; }
        public int? Height { get; set; }
        public int? Width { get; set; }
    }

    public class EmbedProviderModel
    {
        public string Name { get; set; }
        public string Url { get; set; }
    }

    public class EmbedAuthorModel
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string IconUrl { get; set; }
        public string ProxyIconUrl { get; set; }
    }

    public class EmbedFieldModel
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public bool? Inline { get; set; }
    }

    public class ReactionModel
    {
        public int Count { get; set; }
        public bool Me { get; set; }
        public EmojiModel Emoji { get; set; }
    }

    public class MessageReactionModel
    {
        public string UserId { get; set; }
        public string ChannelId { get; set; }
        public string MessageId { get; set; }
        public string GuildId { get; set; }
        public GuildMemberModel Member { get; set; }
        public EmojiModel Emoji { get; set; }
    }

    public enum MessageType
    {
        DEFAULT = 0,
        RECIPIENT_ADD = 1,
        RECIPIENT_REMOVE = 2,
        CALL = 3,
        CHANNEL_NAME_CHANGE = 4,
        CHANNEL_ICON_CHANGE = 5,
        CHANNEL_PINNED_MESSAGE = 6,
        GUILD_MEMBER_JOIN = 7,
        USER_PREMIUM_GUILD_SUBSCRIPTION = 8,
        USER_PREMIUM_GUILD_SUBSCRIPTION_TIER_1 = 9,
        USER_PREMIUM_GUILD_SUBSCRIPTION_TIER_2 = 10,
        USER_PREMIUM_GUILD_SUBSCRIPTION_TIER_3 = 11,
        CHANNEL_FOLLOW_ADD = 12,
        GUILD_DISCOVERY_DISQUALIFIED = 14,
        GUILD_DISCOVERY_REQUALIFIED = 15
    }

    public class MessageActivityModel
    {
        public MessageActivityType Type { get; set; }
        public string PartyId { get; set; }
    }

    public enum MessageActivityType
    {
        JOIN = 1,
        SPECTATE = 2,
        LISTEN = 3,
        JOIN_REQUEST = 5
    }

    public class MessageApplicationModel
    {
        public string Id { get; set; }
        public string CoverImage { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string Name { get; set; }
    }

    public class MessageReferenceModel
    {
        public string MessageId { get; set; }
        public string ChannelId { get; set; }
        public string GuildId { get; set; }
    }

    public enum MessageFlags
    {
        CROSSPOSTED = 1,            // this message has been published to subscribed channels (via Channel Following)
        IS_CROSSPOST = 2,           // this message originated from a message in another channel (via Channel Following)
        SUPPRESS_EMBEDS = 4,        // do not include any embeds when serializing this message
        SOURCE_MESSAGE_DELETED = 8, // the source message for this crosspost has been deleted (via Channel Following)
        URGENT = 16,				// this message came from the urgent message system
    }

    public class MessageBulkModel
    {
        public string[] Ids { get; set; }
        public string ChannelId { get; set; }
        public string GuildId { get; set; }
    }
}
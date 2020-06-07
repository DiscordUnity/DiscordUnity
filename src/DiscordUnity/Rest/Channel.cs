using DiscordUnity.Models;
using DiscordUnity.State;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordUnity
{
    public static partial class DiscordAPI
    {
        public static Task<RestResult<DiscordChannel>> GetChannel(string channelId)
            => SyncInherit(Get<ChannelModel>($"/channels/{channelId}"), r => new DiscordChannel(r));
        public static Task<RestResult<DiscordChannel>> ModifyChannel(string channelId, string name, int? type, int? position, string topic, bool? nsfw, int? rate_limit_per_user, int? bitrate, int? user_limit, object permission_overwrites, string parent_id)
            => SyncInherit(Patch<ChannelModel>($"/channels/{channelId}", new { name, type, position, topic, nsfw, rate_limit_per_user, bitrate, user_limit, permission_overwrites, parent_id }), r => new DiscordChannel(r));
        public static Task<RestResult<DiscordChannel>> DeleteChannel(string channelId)
            => SyncInherit(Delete<ChannelModel>($"/channels/{channelId}"), r => new DiscordChannel(r));

        public static Task<RestResult<bool>> EditChannelPermissions(string channelId, string overwriteId, bool allow, bool deny, string type)
            => SyncInherit(Put<object>($"/channels/{channelId}/permissions/{overwriteId}", new { allow, deny, type }), r => true);
        public static Task<RestResult<bool>> DeleteChannelPermissions(string channelId, string overwriteId)
            => SyncInherit(Delete<object>($"/channels/{channelId}/permissions/{overwriteId}"), r => true);
        public static Task<RestResult<bool>> TriggerTypingIndicator(string channelId)
            => SyncInherit(Post<object>($"/channels/{channelId}/typing", null), r => true);

        public static Task<RestResult<DiscordMessage[]>> GetPinnedMessages(string channelId)
            => SyncInherit(Get<MessageModel[]>($"/channels/{channelId}/pins"), r => r.Select(x => new DiscordMessage(x)).ToArray());
        public static Task<RestResult<bool>> AddPinnedMessages(string channelId, string messageId)
            => SyncInherit(Put<object>($"/channels/{channelId}/pins/{messageId}", null), r => true);
        public static Task<RestResult<bool>> DeletePinnedMessages(string channelId, string messageId)
            => SyncInherit(Delete<object>($"/channels/{channelId}/pins/{messageId}"), r => true);

        public static Task<RestResult<bool>> AddRecipientToGroupDM(string channelId, string userId, string accessToken, string nick)
            => SyncInherit(Put<object>($"/channels/{channelId}/recipients/{userId}", new { accessToken, nick }), r => true);
        public static Task<RestResult<bool>> RemoveRecipientFromGroupDM(string channelId, string userId)
            => SyncInherit(Delete<object>($"/channels/{channelId}/recipients/{userId}"), r => true);

        public static Task<RestResult<DiscordInvite[]>> GetChannelInvites(string channelId)
            => SyncInherit(Get<InviteModel[]>($"/channels/{channelId}/invites"), r => r.Select(x => new DiscordInvite(x)).ToArray());
        public static Task<RestResult<DiscordInvite>> CreateChannelInvite(string channelId, int maxAge, int maxUses, bool temporary, bool unique, string targetUser, int targetUserType)
            => SyncInherit(Post<InviteModel>($"/channels/{channelId}/invites", new { maxAge, maxUses, temporary, unique, targetUser, targetUserType }), r => new DiscordInvite(r));

        public static Task<RestResult<DiscordMessage[]>> GetChannelMessages(string channelId, string around, string before, string after, int? limit)
            => SyncInherit(Get<MessageModel[]>($"/channels/{channelId}/messages", new { around, before, after, limit }), r => r.Select(x => new DiscordMessage(x)).ToArray());
        public static Task<RestResult<DiscordMessage>> GetChannelMessages(string channelId, string messageId)
            => SyncInherit(Get<MessageModel>($"/channels/{channelId}/messages/{messageId}"), r => new DiscordMessage(r));
        public static Task<RestResult<DiscordMessage>> CreateMessage(string channelId, string content, string nonce, bool? tts, object file, object embed, string payload_json, object allowed_mentions)
            => SyncInherit(Post<MessageModel>($"/channels/{channelId}/messages", new { content, nonce, tts, file, embed, payload_json, allowed_mentions }), r => new DiscordMessage(r));
        public static Task<RestResult<DiscordMessage>> EditMessage(string channelId, string messageId, string content, object embed, int flags)
            => SyncInherit(Patch<MessageModel>($"/channels/{channelId}/messages/{messageId}", new { content, embed, flags }), r => new DiscordMessage(r));
        public static Task<RestResult<bool>> DeleteMessage(string channelId, string messageId)
            => SyncInherit(Delete<object>($"/channels/{channelId}/messages/{messageId}"), r => true);
        public static Task<RestResult<bool>> BulkDeleteMessages(string channelId, params string[] messageIds)
            => SyncInherit(Post<object>($"/channels/{channelId}/messages/bulk-delete", new { messages = messageIds }), r => true);

        public static Task<RestResult<bool>> CreateReaction(string channelId, string messageId, string emoji /* URL Encoded */)
            => SyncInherit(Put<object>($"/channels/{channelId}/messages/{messageId}/reactions/{emoji}/@me", null), r => true);
        public static Task<RestResult<bool>> DeleteReaction(string channelId, string messageId, string emoji /* URL Encoded */, string userId = null)
            => SyncInherit(Delete<object>($"/channels/{channelId}/messages/{messageId}/reactions/{emoji}/{userId ?? "@me"}"), r => true);
        public static Task<RestResult<DiscordUser[]>> GetReactions(string channelId, string messageId, string emoji /* URL Encoded */, string before = null, string after = null, int? limit = null)
            => SyncInherit(Get<UserModel[]>($"/channels/{channelId}/messages/{messageId}/reactions/{emoji}", new { before, after, limit }), r => r.Select(x => new DiscordUser(x)).ToArray());
        public static Task<RestResult<bool>> DeleteAllReactionsForEmoji(string channelId, string messageId, string emoji /* URL Encoded */)
            => SyncInherit(Delete<object>($"/channels/{channelId}/messages/{messageId}/reactions/{emoji}"), r => true);
    }
}

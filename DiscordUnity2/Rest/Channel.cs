using DiscordUnity2.Models;
using DiscordUnity2.State;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordUnity2
{
    public static partial class DiscordAPI
    {
        public static Task<RestResult<DiscordChannel>> GetChannel(string channelId)
            => SyncInherit(Get<ChannelModel>($"/channels/{channelId}"), r => new DiscordChannel(r));
        public static Task<RestResult<DiscordChannel>> ModifyChannel(string channelId, string name, int? type, int? position, string topic, bool? nsfw, int? rate_limit_per_user, int? bitrate, int? user_limit, object permission_overwrites, string parent_id)
            => SyncInherit(Patch<ChannelModel>($"/channels/{channelId}", new { name, type, position, topic, nsfw, rate_limit_per_user, bitrate, user_limit, permission_overwrites, parent_id }), r => new DiscordChannel(r));
        public static Task<RestResult<DiscordChannel>> DeleteChannel(string channelId)
            => SyncInherit(Delete<ChannelModel>($"/channels/{channelId}"), r => new DiscordChannel(r));

        public static Task<RestResult<DiscordMessage[]>> GetChannelMessages(string channelId, string around, string before, string after, int? limit)
            => SyncInherit(Get<MessageModel[]>($"/channels/{channelId}/messages", new { around, before, after, limit }), r => r.Select(x => new DiscordMessage(x)).ToArray());
        public static Task<RestResult<DiscordMessage>> GetChannelMessages(string channelId, string messageId)
            => SyncInherit(Get<MessageModel>($"/channels/{channelId}/messages/{messageId}"), r => new DiscordMessage(r));
        public static Task<RestResult<DiscordMessage>> CreateMessage(string channelId, string content, string nonce, bool? tts, object file, object embed, string payload_json, object allowed_mentions)
            => SyncInherit(Post<MessageModel>($"/channels/{channelId}/messages", new { content, nonce, tts, file, embed, payload_json, allowed_mentions }), r => new DiscordMessage(r));
    }
}

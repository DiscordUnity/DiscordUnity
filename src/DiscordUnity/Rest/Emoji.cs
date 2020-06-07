using DiscordUnity.Models;
using DiscordUnity.State;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordUnity
{
    public static partial class DiscordAPI
    {
        public static Task<RestResult<DiscordEmoji[]>> ListServerEmojis(string serverId)
            => SyncInherit(Get<EmojiModel[]>($"/guilds/{serverId}/emojis"), r => r.Select(x => new DiscordEmoji(x)).ToArray());
        public static Task<RestResult<DiscordEmoji>> GetServerEmoji(string serverId, string emojiId)
            => SyncInherit(Get<EmojiModel>($"/guilds/{serverId}/emojis/{emojiId}"), r => new DiscordEmoji(r));
        public static Task<RestResult<DiscordEmoji>> CreateServerEmoji(string serverId, string name, object image, params string[] roles)
            => SyncInherit(Post<EmojiModel>($"/guilds/{serverId}/emojis", new { name, image, roles }), r => new DiscordEmoji(r));
        public static Task<RestResult<DiscordEmoji>> ModifyServerEmoji(string serverId, string emojiId, string name, params string[] roles)
            => SyncInherit(Patch<EmojiModel>($"/guilds/{serverId}/emojis/{emojiId}", new { name, roles }), r => new DiscordEmoji(r));
        public static Task<RestResult<bool>> DeleteServerEmoji(string serverId, string emojiId)
            => SyncInherit(Delete<object>($"/guilds/{serverId}/emojis/{emojiId}"), r => true);
    }
}

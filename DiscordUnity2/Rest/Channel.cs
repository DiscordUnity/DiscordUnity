using DiscordUnity2.Models;
using DiscordUnity2.State;
using System.Threading.Tasks;

namespace DiscordUnity2
{
    public static partial class DiscordAPI
    {
        public static async Task<RestResult<DiscordMessage>> CreateMessage(string channelId, string content, string nonce = null, bool tts = false)
        {
            var model = await Post<MessageModel>($"/channels/{channelId}/messages", new { content, nonce, tts });
            if (model) return RestResult<DiscordMessage>.FromResult(new DiscordMessage(model.Data));
            return RestResult<DiscordMessage>.FromException(model.Exception);
        }
    }
}

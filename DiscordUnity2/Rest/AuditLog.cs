using DiscordUnity2.Models;
using System.Threading.Tasks;

namespace DiscordUnity2
{
    public static partial class DiscordAPI
    {
        // TODO: edit to DiscordAuditLog
        public static Task<RestResult<object>> GetServerAuditLog(string serverId, string userId, string content, string nonce = null, bool tts = false)
            => SyncInherit(Post<AuditLogModel>($"/guildids/{serverId}/audit-logs", new { content, nonce, tts }), r => (object)r);
    }
}

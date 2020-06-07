using DiscordUnity2.Models;
using DiscordUnity2.State;
using System.Threading.Tasks;

namespace DiscordUnity2
{
    public static partial class DiscordAPI
    {
        public static Task<RestResult<DiscordInvite>> GetInvite(string inviteCode, bool? withCounts = null)
            => SyncInherit(Get<InviteModel>($"/invites/{inviteCode}", new { withCounts }), r => new DiscordInvite(r));
        public static Task<RestResult<DiscordInvite>> DeleteInvite(string inviteCode)
            => SyncInherit(Delete<InviteModel>($"/invites/{inviteCode}"), r => new DiscordInvite(r));
    }
}

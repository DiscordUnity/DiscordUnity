using DiscordUnity2.Models;
using DiscordUnity2.State;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordUnity2
{
    public static partial class DiscordAPI
    {
        public static Task<RestResult<DiscordUser>> GetCurrentUser()
            => GetUser(null);
        public static Task<RestResult<DiscordUser>> GetUser(string userId = null)
            => SyncInherit(Get<UserModel>($"/users/{userId ?? "@me"}"), r => new DiscordUser(r));
        public static Task<RestResult<DiscordUser>> ModifyUser(string username, object avatar)
            => SyncInherit(Patch<UserModel>($"/users/@me", new { username, avatar }), r => new DiscordUser(r));
        public static Task<RestResult<DiscordServer[]>> GeUserServers(string before, string after, int? limit)
            => SyncInherit(Get<GuildModel[]>($"/users/@me/guilds", new { before, after, limit }), r => r.Select(x => new DiscordServer(x)).ToArray());
        public static Task<RestResult<bool>> LeaveServer(string serverId)
            => SyncInherit(Delete<object>($"/users/@me/guilds/{serverId}"), r => true);
        public static Task<RestResult<DiscordChannel[]>> GetUserDMs()
            => SyncInherit(Get<ChannelModel[]>($"/users/@me/channels"), r => r.Select(x => new DiscordChannel(x)).ToArray());
        public static Task<RestResult<DiscordChannel>> CreateDM(string recipientId)
            => SyncInherit(Post<ChannelModel>($"/users/@me/channels", new { recipientId }), r => new DiscordChannel(r));
        public static Task<RestResult<DiscordChannel>> CreateGroupDM(string accessTokens, Dictionary<string, string> nicks)
            => SyncInherit(Post<ChannelModel>($"/users/@me/channels", new { accessTokens, nicks }), r => new DiscordChannel(r));
        public static Task<RestResult<object[]>> GetUserConnections()
            => SyncInherit(Get<object[]>($"/users/@me/connections"), r => r.Select(x => x).ToArray());
    }
}

using DiscordUnity2.Models;
using DiscordUnity2.State;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordUnity2
{
    public static partial class DiscordAPI
    {
        public static Task<RestResult<DiscordServer>> GetServer(string guildId, bool? withcounts = null)
            => SyncInherit(Get<GuildModel>($"/guilds/{guildId}", new { withcounts }), r => new DiscordServer(r));
        public static Task<RestResult<DiscordServer>> GetServerPreview(string guildId)
            => SyncInherit(Get<GuildModel>($"/guilds/{guildId}/preview"), r => new DiscordServer(r));
        // TODO Edits
        public static Task<RestResult<DiscordServer>> ModifyServer(string guildId, object edits)
            => SyncInherit(Patch<GuildModel>($"/guilds/{guildId}", edits), r => new DiscordServer(r));
        public static Task<RestResult<bool>> DeleteServer(string guildId)
            => SyncInherit(Delete<object>($"/guilds/{guildId}"), r => true);

        public static Task<RestResult<DiscordChannel[]>> GetServerChannels(string guildId)
            => SyncInherit(Get<ChannelModel[]>($"/guilds/{guildId}/channels"), r => r.Select(x => new DiscordChannel(x)).ToArray());
        public static Task<RestResult<DiscordChannel>> CreateServerChannel(string guildId, string name, int? type, string topic, int? bitrate, int? userLimit, int? rateLimitPerUser, int? position, 
            object[] permissionOverwrites, string parentid, bool? nsfw)
            => SyncInherit(Post<ChannelModel>($"/guilds/{guildId}/channels", new { name, type, topic, bitrate, userLimit, rateLimitPerUser, position, permissionOverwrites, parentid, nsfw }), r => new DiscordChannel(r));
        public static Task<RestResult<bool>> ModifyServerChannelPositions(string guildId, string channelId, int? position)
            => SyncInherit(Patch<object>($"/guilds/{guildId}/channels", new { id = channelId, position }), r => true);

        public static Task<RestResult<DiscordServerMember>> GetServerMember(string guildId, string userId)
            => SyncInherit(Get<GuildMemberModel>($"/guilds/{guildId}/mebmers/{userId}"), r => new DiscordServerMember(r));
        public static Task<RestResult<DiscordServerMember[]>> GetServerMembers(string guildId, int? limit, string after)
            => SyncInherit(Get<GuildMemberModel[]>($"/guilds/{guildId}/mebmers", new { limit, after }), r => r.Select(x => new DiscordServerMember(x)).ToArray());
        public static Task<RestResult<DiscordServerMember>> AddServerMember(string guildId, string userId, string accessToken, string nick, string[] roles, bool? mute, bool? deaf)
            => SyncInherit(Put<GuildMemberModel>($"/guilds/{guildId}/mebmers/{userId}", new { accessToken, nick, roles, mute, deaf }), r => new DiscordServerMember(r));
        public static Task<RestResult<DiscordServerMember>> ModifyServerMember(string guildId, string userId, string nick, string[] roles, bool? mute, bool? deaf, string channelId)
            => SyncInherit(Patch<GuildMemberModel>($"/guilds/{guildId}/mebmers/{userId}", new { nick, roles, mute, deaf, channelId }), r => new DiscordServerMember(r));
        public static Task<RestResult<bool>> ModifyUserNick(string guildId, string nick)
            => SyncInherit(Patch<object>($"/guilds/{guildId}/mebmers/@me/nick", new { nick }), r => true);
        public static Task<RestResult<bool>> RemoveServerMember(string guildId, string userId)
            => SyncInherit(Delete<object>($"/guilds/{guildId}/mebmers/{userId}"), r => true);

        public static Task<RestResult<bool>> AddServerMemberRole(string guildId, string userId, string roleId)
            => SyncInherit(Put<object>($"/guilds/{guildId}/mebmers/{userId}/roles/{roleId}", null), r => true);
        public static Task<RestResult<bool>> RemoveServerMemberRole(string guildId, string userId, string roleId)
            => SyncInherit(Delete<object>($"/guilds/{guildId}/mebmers/{userId}/roles/{roleId}", null), r => true);

        public static Task<RestResult<object[]>> GetServerBans(string guildId)
            => SyncInherit(Get<GuildBanModel[]>($"/guilds/{guildId}/bans"), r => (object[])r);
        public static Task<RestResult<object>> GetServerBan(string guildId, string userId)
            => SyncInherit(Get<GuildBanModel>($"/guilds/{guildId}/bans/{userId}"), r => (object)r);
        public static Task<RestResult<bool>> CreateServerBan(string guildId, string userId, int? deleteMessageDays, string reason)
            => SyncInherit(Put<object>($"/guilds/{guildId}/bans/{userId}", null, new { deleteMessageDays, reason }), r => true);
        public static Task<RestResult<bool>> RemoveServerBan(string guildId, string userId)
            => SyncInherit(Delete<object>($"/guilds/{guildId}/bans/{userId}"), r => true);

        public static Task<RestResult<DiscordRole[]>> GetServerRoles(string guildId)
            => SyncInherit(Get<RoleModel[]>($"/guilds/{guildId}/roles"), r => r.Select(x => new DiscordRole(x)).ToArray());
        public static Task<RestResult<DiscordRole>> CreateServerRole(string guildId, string name, int? permissions, int? color, bool? hoist, bool? mentionable)
            => SyncInherit(Post<RoleModel>($"/guilds/{guildId}/roles", new { name, permissions, color, hoist, mentionable }), r => new DiscordRole(r));
        public static Task<RestResult<DiscordRole>> ModifyServerRole(string guildId, string roleId, string name, int? permissions, int? color, bool? hoist, bool? mentionable)
            => SyncInherit(Patch<RoleModel>($"/guilds/{guildId}/roles/{roleId}", new { name, permissions, color, hoist, mentionable }), r => new DiscordRole(r));
        public static Task<RestResult<DiscordRole[]>> ModifyServerRolePositions(string guildId, string roleId, int? position)
            => SyncInherit(Patch<RoleModel[]>($"/guilds/{guildId}/roles", new { id = roleId, position }), r => r.Select(x => new DiscordRole(x)).ToArray());
        public static Task<RestResult<bool>> DeleteServerRole(string guildId, string roleId)
            => SyncInherit(Delete<object>($"/guilds/{guildId}/roles/{roleId}"), r => true);

        // TODO From https://discord.com/developers/docs/resources/guild#get-guild-prune-count
    }
}

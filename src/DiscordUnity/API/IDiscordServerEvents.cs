using DiscordUnity.State;

namespace DiscordUnity.API
{
    public interface IDiscordServerEvents : IDiscordInterface
    {
        void OnServerJoined(DiscordServer server);
        void OnServerUpdated(DiscordServer server);
        void OnServerLeft(DiscordServer server);

        void OnServerBan(DiscordServer server, DiscordUser user);
        void OnServerUnban(DiscordServer server, DiscordUser user);

        void OnServerEmojisUpdated(DiscordServer server, DiscordEmoji[] emojis);

        void OnServerMemberJoined(DiscordServer server, DiscordServerMember member);
        void OnServerMemberUpdated(DiscordServer server, DiscordServerMember member);
        void OnServerMemberLeft(DiscordServer server, DiscordServerMember member);
        void OnServerMembersChunk(DiscordServer server, DiscordServerMember[] members, string[] notFound, DiscordPresence[] presences);

        void OnServerRoleCreated(DiscordServer server, DiscordRole role);
        void OnServerRoleUpdated(DiscordServer server, DiscordRole role);
        void OnServerRoleRemove(DiscordServer server, DiscordRole role);
    }
}

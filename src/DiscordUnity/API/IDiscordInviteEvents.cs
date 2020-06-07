using DiscordUnity.State;

namespace DiscordUnity.API
{
    public interface IDiscordInviteEvents : IDiscordInterface
    {
        void InviteCreated(DiscordServer server, DiscordInvite invite);
        void InviteDeleted(DiscordServer server, DiscordInvite invite);
    }
}

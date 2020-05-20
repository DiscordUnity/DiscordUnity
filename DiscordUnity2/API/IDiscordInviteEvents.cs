using DiscordUnity2.State;

namespace DiscordUnity2.API
{
    public interface IDiscordInviteEvents : IDiscordInterface
    {
        void InviteCreated(DiscordServer server, DiscordInvite invite);
        void InviteDeleted(DiscordServer server, DiscordInvite invite);
    }
}

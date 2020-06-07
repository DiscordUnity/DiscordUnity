using DiscordUnity.State;

namespace DiscordUnity.API
{
    public interface IDiscordWebhookEvents : IDiscordInterface
    {
        void OnWebhooksUpdated(DiscordChannel channel);
    }
}

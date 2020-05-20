using DiscordUnity2.State;

namespace DiscordUnity2.API
{
    public interface IDiscordWebhookEvents : IDiscordInterface
    {
        void OnWebhooksUpdated(DiscordChannel channel);
    }
}

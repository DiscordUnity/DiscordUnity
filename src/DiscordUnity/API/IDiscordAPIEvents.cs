namespace DiscordUnity.API
{
    public interface IDiscordAPIEvents : IDiscordInterface
    {
        void OnDiscordAPIOpen();
        void OnDiscordAPIResumed();
        void OnDiscordAPIClosed();
    }
}

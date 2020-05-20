namespace DiscordUnity2.API
{
    public interface IDiscordAPIEvents : IDiscordInterface
    {
        void OnDiscordAPIOpen();
        void OnDiscordAPIResumed();
        void OnDiscordAPIClosed();
    }
}

using DiscordUnity.State;

namespace DiscordUnity.API
{
    public interface IDiscordVoiceEvents : IDiscordInterface
    {
        void OnVoiceStateUpdated(DiscordVoiceState voiceState);
        void OnVoiceServerUpdated(DiscordServer server, string token, string endpoint);
    }
}

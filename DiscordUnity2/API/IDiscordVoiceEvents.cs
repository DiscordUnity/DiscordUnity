using DiscordUnity2.State;

namespace DiscordUnity2.API
{
    public interface IDiscordVoiceEvents : IDiscordInterface
    {
        void OnVoiceStateUpdated(DiscordVoiceState voiceState);
        void OnVoiceServerUpdated(DiscordServer server, string token, string endpoint);
    }
}

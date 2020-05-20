using DiscordUnity2.State;
using System;

namespace DiscordUnity2.API
{
    public interface IDiscordChannelEvents : IDiscordInterface
    {
        void OnChannelCreated(DiscordChannel channel);
        void OnChannelUpdated(DiscordChannel channel);
        void OnChannelDeleted(DiscordChannel channel);

        void OnChannelPinsUpdated(DiscordChannel channel, DateTime? lastPinTimestamp);
    }
}

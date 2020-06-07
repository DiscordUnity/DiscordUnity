using DiscordUnity.State;
using System;

namespace DiscordUnity.API
{
    public interface IDiscordChannelEvents : IDiscordInterface
    {
        void OnChannelCreated(DiscordChannel channel);
        void OnChannelUpdated(DiscordChannel channel);
        void OnChannelDeleted(DiscordChannel channel);

        void OnChannelPinsUpdated(DiscordChannel channel, DateTime? lastPinTimestamp);
    }
}

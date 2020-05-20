using DiscordUnity2.Models;
using System;

namespace DiscordUnity2.State
{
    public class DiscordPresence
    {
        public DiscordUser User { get; internal set; }
        public string[] Roles { get; internal set; }
        public DiscordActivity Game { get; internal set; }
        public string GuildId { get; internal set; }
        public PresenceStatus Status { get; internal set; }
        public DiscordActivity[] Activities { get; internal set; }
        public DiscordClientStatus ClientStatus { get; internal set; }
        public DateTime? PremiumSince { get; internal set; }
        public string Nick { get; internal set; }

        internal DiscordPresence(PresenceModel model)
        {
            
        }
    }

    public class DiscordClientStatus
    {
        public string Desktop { get; internal set; }
        public string Mobile { get; internal set; }
        public string Web { get; internal set; }

        internal DiscordClientStatus(PresenceModel model)
        {
            
        }
    }
}

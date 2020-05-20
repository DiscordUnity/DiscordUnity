using System;

namespace DiscordUnity2.Models
{
    internal class PresenceModel
    {
        public UserModel User { get; set; }
        public string[] Roles { get; set; }
        public ActivityModel Game { get; set; }
        public string GuildId { get; set; }
        public PresenceStatus Status { get; set; }
        public ActivityModel[] Activities { get; set; }
        public ClientStatusModel ClientStatus { get; set; }
        public DateTime? PremiumSince { get; set; }
        public string Nick { get; set; }
    }

    public enum PresenceStatus
    {
        Idle,
        Dnd,
        Online,
        Offline
    }

    internal class ClientStatusModel
    {
        public string Desktop { get; set; }
        public string Mobile { get; set; }
        public string Web { get; set; }
    }
}

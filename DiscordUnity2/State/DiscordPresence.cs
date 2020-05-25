using DiscordUnity2.Models;
using System;
using System.Linq;

namespace DiscordUnity2.State
{
    public class DiscordPresence
    {
        public DiscordUser User { get; internal set; }
        public DiscordRole[] Roles => RoleIds?.Select(x => Server.Roles[x]).ToArray();
        public DiscordActivity Game { get; internal set; }
        public DiscordServer Server => DiscordAPI.Servers[GuildId];
        public PresenceStatus Status { get; internal set; }
        public DiscordActivity[] Activities { get; internal set; }
        public DiscordClientStatus ClientStatus { get; internal set; }
        public DateTime? PremiumSince { get; internal set; }
        public string Nick { get; internal set; }

        private readonly string GuildId;
        private readonly string[] RoleIds;

        internal DiscordPresence(PresenceModel model)
        {
            User = new DiscordUser(model.User);
            GuildId = model.GuildId;
            RoleIds = model.Roles;
            if (model.Game != null) Game = new DiscordActivity(model.Game);
            Status = model.Status;
            Activities = model.Activities?.Select(x => new DiscordActivity(x)).ToArray();
            if (model.ClientStatus != null) ClientStatus = new DiscordClientStatus(model.ClientStatus);
            PremiumSince = model.PremiumSince;
            Nick = model.Nick;
        }
    }

    public class DiscordClientStatus
    {
        public string Desktop { get; internal set; }
        public string Mobile { get; internal set; }
        public string Web { get; internal set; }

        internal DiscordClientStatus(ClientStatusModel model)
        {
            Desktop = model.Desktop;
            Mobile = model.Mobile;
            Web = model.Web;
        }
    }
}

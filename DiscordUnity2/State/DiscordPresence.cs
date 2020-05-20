using DiscordUnity2.Models;
using System;
using System.Linq;

namespace DiscordUnity2.State
{
    public class DiscordPresence
    {
        public DiscordUser User { get; internal set; }
        public DiscordRole[] Roles { get; internal set; }
        public DiscordActivity Game { get; internal set; }
        public DiscordServer Server { get; internal set; }
        public PresenceStatus Status { get; internal set; }
        public DiscordActivity[] Activities { get; internal set; }
        public DiscordClientStatus ClientStatus { get; internal set; }
        public DateTime? PremiumSince { get; internal set; }
        public string Nick { get; internal set; }

        internal DiscordPresence(PresenceModel model)
        {
            User = new DiscordUser(model.User);
            Server = DiscordAPI.Servers[model.GuildId];
            Roles = model.Roles?.Select(x => Server.Roles[x]).ToArray();
            if (model.Game != null) Game = new DiscordActivity(model.Game);
            Status = model.Status;
            Activities = model.Activities?.Select(x => new DiscordActivity(x)).ToArray();
            if (model.ClientStatus != null) ClientStatus = new DiscordClientStatus(model.ClientStatus);
            PremiumSince = model.PremiumSince;
            Nick = model.Nick;
        }

        internal DiscordPresence(PresenceModel model, DiscordServer server)
        {
            User = new DiscordUser(model.User);
            Server = server;
            Roles = model.Roles?.Select(x => Server.Roles[x]).ToArray();
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

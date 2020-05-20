using DiscordUnity2.Models;
using System;

namespace DiscordUnity2.State
{
    public class DiscordInvite
    {
        public DiscordChannel Channel { get; internal set; }
        public string Code { get; internal set; }
        public DateTime CreatedAt { get; internal set; }
        public DiscordServer Server { get; internal set; }
        public DiscordUser Inviter { get; internal set; }
        public int MaxAge { get; internal set; }
        public int MaxUses { get; internal set; }
        public DiscordUser TargetUser { get; internal set; }
        public int TargetUserType { get; internal set; }
        public bool Temporary { get; internal set; }
        public int Uses { get; internal set; }

        internal DiscordInvite(InviteModel model)
        {
            Server = DiscordAPI.Servers[model.GuildId];
            Channel = Server.Channels[model.ChannelId];
            Code = model.Code;
            CreatedAt = model.CreatedAt;
            if (model.Inviter != null) Inviter = new DiscordUser(model.Inviter);
            MaxAge = model.MaxAge;
            MaxUses = model.MaxUses;
            if (model.TargetUser != null) TargetUser = new DiscordUser(model.TargetUser);
            TargetUserType = model.TargetUserType;
            Temporary = model.Temporary;
            Uses = model.Uses;
        }
    }
}

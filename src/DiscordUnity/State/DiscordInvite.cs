using DiscordUnity.Models;
using System;

namespace DiscordUnity.State
{
    public class DiscordInvite
    {
        public string Code { get; internal set; }
        public DateTime CreatedAt { get; internal set; }
        public DiscordServer Server => string.IsNullOrEmpty(GuildId) ? null : DiscordAPI.Servers[GuildId];
        public DiscordChannel Channel => string.IsNullOrEmpty(GuildId) ? DiscordAPI.PrivateChannels[ChannelId] : Server.Channels[ChannelId];
        public DiscordUser Inviter { get; internal set; }
        public int MaxAge { get; internal set; }
        public int MaxUses { get; internal set; }
        public DiscordUser TargetUser { get; internal set; }
        public int TargetUserType { get; internal set; }
        public bool Temporary { get; internal set; }
        public int Uses { get; internal set; }

        private readonly string GuildId;
        private readonly string ChannelId;

        internal DiscordInvite(InviteModel model)
        {
            GuildId = model.GuildId;
            ChannelId = model.ChannelId;
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

using DiscordUnity2.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DiscordUnity2.State
{
    public class DiscordServer
    {
        public string Id { get; internal set; }
        public string Name { get; internal set; }
        public string Icon { get; internal set; }
        public string Splash { get; internal set; }
        public string DiscoverySplash { get; internal set; }
        public bool IsOwner { get; internal set; }
        public DiscordServerMember Owner { get; internal set; }
        public int? Permissions { get; internal set; }
        public string Region { get; internal set; }
        public DiscordChannel AfkChannel { get; internal set; }
        public int AfkTimeout { get; internal set; }
        public bool EmbedEnabled { get; internal set; }
        public DiscordChannel EmbedChannel { get; internal set; }
        public int VerificationLevel { get; internal set; }
        public int DefaultMessageNotifications { get; internal set; }
        public int ExplicitContentFilter { get; internal set; }
        public Dictionary<string, DiscordRole> Roles { get; internal set; }
        public Dictionary<string, DiscordEmoji> Emojis { get; internal set; }
        public GuildFeature[] Features { get; internal set; }
        public int MfaLevel { get; internal set; }
        public string ApplicationId { get; internal set; }
        public bool WidgetEnabled { get; internal set; }
        public DiscordChannel WidgetChannel { get; internal set; }
        public DiscordChannel SystemChannel { get; internal set; }
        public int SystemChannelFlags { get; internal set; }
        public DiscordChannel RulesChannel { get; internal set; }
        public DateTime? JoinedAt { get; internal set; }
        public bool? Large { get; internal set; }
        public bool? Unavailable { get; internal set; }
        public int? MemberCount { get; internal set; }
        public Dictionary<string, DiscordVoiceState> VoiceStates { get; internal set; }
        public Dictionary<string, DiscordServerMember> Members { get; internal set; }
        public Dictionary<string, DiscordChannel> Channels { get; internal set; }
        public Dictionary<string, DiscordPresence> Presences { get; internal set; }
        public int? MaxPresences { get; internal set; }
        public int? MaxMembers { get; internal set; }
        public string VanityUrlCode { get; internal set; }
        public string Description { get; internal set; }
        public string Banner { get; internal set; }
        public int PremiumTier { get; internal set; }
        public int? PremiumSubscriptionCount { get; internal set; }
        public string PreferredLocale { get; internal set; }
        public DiscordChannel PublicUpdatesChannel { get; internal set; }
        public int MaxVideoChannelUsers { get; internal set; }
        public int ApproximateMemberCount { get; internal set; }
        public int ApproximatePresenceCount { get; internal set; }
        public Dictionary<string, DiscordInvite> Invites { get; internal set; }
        public Dictionary<string, DiscordUser> Bans { get; internal set; }

        internal DiscordServer(GuildModel model)
        {
            Id = model.Id;
            Name = model.Name;
            Icon = model.Icon;
            Splash = model.Splash;
            DiscoverySplash = model.DiscoverySplash;
            IsOwner = model.Owner ?? false;
            Roles = model.Roles?.ToDictionary(x => x.Id, x => new DiscordRole(x));
            Members = model.Members?.ToDictionary(x => x.User.Id, x => new DiscordServerMember(x));
            if (!string.IsNullOrEmpty(model.OwnerId)) Owner = Members[model.OwnerId];
            Permissions = model.Permissions;
            Region = model.Region;
            Channels = model.Channels?.ToDictionary(x => x.Id, x => new DiscordChannel(x));
            if (!string.IsNullOrEmpty(model.AfkChannelId)) AfkChannel = Channels[model.AfkChannelId];
            AfkTimeout = model.AfkTimeout;
            EmbedEnabled = model.EmbedEnabled ?? false;
            if (!string.IsNullOrEmpty(model.EmbedChannelId)) EmbedChannel = Channels[model.EmbedChannelId];
            VerificationLevel = model.VerificationLevel;
            DefaultMessageNotifications = model.DefaultMessageNotifications;
            ExplicitContentFilter = model.ExplicitContentFilter;
            Emojis = model.Emojis?.ToDictionary(x => x.Id, x => new DiscordEmoji(x));
            Features = model.Features;
            MfaLevel = model.MfaLevel;
            ApplicationId = model.ApplicationId;
            WidgetEnabled = model.WidgetEnabled ?? false;
            if (!string.IsNullOrEmpty(model.WidgetChannelId)) WidgetChannel = Channels[model.WidgetChannelId];
            if (!string.IsNullOrEmpty(model.SystemChannelId)) SystemChannel = Channels[model.SystemChannelId];
            SystemChannelFlags = model.SystemChannelFlags;
            if (!string.IsNullOrEmpty(model.RulesChannelId)) RulesChannel = Channels[model.RulesChannelId];
            JoinedAt = model.JoinedAt;
            Large = model.Large ?? false;
            Unavailable = model.Unavailable ?? false;
            MemberCount = model.MemberCount;
            VoiceStates = model.VoiceStates?.ToDictionary(x => x.Member.User.Id, x => new DiscordVoiceState(x));
            Presences = model.Presences?.ToDictionary(x => x.User.Id, x => new DiscordPresence(x));
            MaxPresences = model.MaxPresences;
            MaxMembers = model.MaxMembers;
            VanityUrlCode = model.VanityUrlCode;
            Description = model.Description;
            Banner = model.Banner;
            PremiumTier = model.PremiumTier;
            PremiumSubscriptionCount = model.PremiumSubscriptionCount;
            PreferredLocale = model.PreferredLocale;
            if (!string.IsNullOrEmpty(model.PublicUpdatesChannelId)) PublicUpdatesChannel = Channels[model.PublicUpdatesChannelId];
            MaxVideoChannelUsers = model.MaxVideoChannelUsers;
            ApproximateMemberCount = model.ApproximateMemberCount;
            ApproximatePresenceCount = model.ApproximatePresenceCount;
            Invites = new Dictionary<string, DiscordInvite>();
            Bans = new Dictionary<string, DiscordUser>();

            if (model.Channels != null)
                foreach (var channel in model.Channels)
                    if (!string.IsNullOrEmpty(channel.ParentId))
                        Channels[channel.Id].Parent = Channels[channel.ParentId];
        }
    }

    public class DiscordServerMember
    {
        public DiscordUser User { get; internal set; }
        public string Nick { get; internal set; }
        public DiscordServer Server => string.IsNullOrEmpty(GuildId) ? null : DiscordAPI.Servers[GuildId];
        public DiscordRole[] Roles => RoleIds?.Select(x => Server.Roles[x]).ToArray();
        public DateTime JoinedAt { get; internal set; }
        public DateTime? PremiumSince { get; internal set; }
        public bool Deaf { get; internal set; }
        public bool Mute { get; internal set; }

        private readonly string GuildId;
        private readonly string[] RoleIds;

        internal DiscordServerMember(GuildMemberModel model)
        {
            GuildId = model.GuildId;
            User = new DiscordUser(model.User);
            Nick = model.Nick;
            RoleIds = model.Roles;
            JoinedAt = model.JoinedAt;
            PremiumSince = model.PremiumSince;
            Deaf = model.Deaf;
            Mute = model.Mute;
        }
    }
}

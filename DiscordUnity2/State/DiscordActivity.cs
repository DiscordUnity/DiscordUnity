using DiscordUnity2.Models;

namespace DiscordUnity2.State
{
    public class DiscordActivity
    {
        public string Name { get; internal set; }
        public ActivityType Type { get; internal set; }
        public string Url { get; internal set; }
        public int CreatedAt { get; internal set; }
        public DiscordActivityTimestamps Timestamps { get; internal set; }
        public string ApplicationId { get; internal set; }
        public string Details { get; internal set; }
        public string State { get; internal set; }
        public DiscordActivityEmoji Emoji { get; internal set; }
        public DiscordActivityParty Party { get; internal set; }
        public DiscordActivityAssets Assets { get; internal set; }
        public DiscordActivitySecrets Secrets { get; internal set; }
        public bool Instance { get; internal set; }
        public ActivityFlags Flags { get; internal set; }

        internal DiscordActivity(ActivityModel model)
        {
            Name = model.Name;
            Type = model.Type;
            Url = model.Url;
            CreatedAt = model.CreatedAt;
            if (model.Timestamps != null) Timestamps = new DiscordActivityTimestamps(model.Timestamps);
            ApplicationId = model.ApplicationId;
            Details = model.Details;
            State = model.State;
            if (model.Emoji != null) Emoji = new DiscordActivityEmoji(model.Emoji);
            if (model.Party != null) Party = new DiscordActivityParty(model.Party);
            if (model.Assets != null) Assets = new DiscordActivityAssets(model.Assets);
            if (model.Secrets != null) Secrets = new DiscordActivitySecrets(model.Secrets);
            Instance = model.Instance;
            Flags = model.Flags;
        }
    }

    public class DiscordActivityTimestamps
    {
        public int Start { get; internal set; }
        public int End { get; internal set; }

        internal DiscordActivityTimestamps(ActivityTimestampsModel model)
        {
            Start = model.Start;
            End = model.End;
        }
    }

    public class DiscordActivityEmoji
    {
        public string Name { get; internal set; }
        public string Id { get; internal set; }
        public bool? Animated { get; internal set; }

        internal DiscordActivityEmoji(ActivityEmojiModel model)
        {
            Name = model.Name;
            Id = model.Id;
            Animated = model.Animated;
        }
    }

    public class DiscordActivityParty
    {
        public string Id { get; internal set; }
        public int? Size { get; internal set; }
        public int? MaxSize { get; internal set; }

        internal DiscordActivityParty(ActivityPartyModel model)
        {
            Id = model.Id;

            if (model.Size.Length == 2)
            {
                Size = model.Size[0];
                MaxSize = model.Size[1];
            }
        }
    }

    public class DiscordActivityAssets
    {
        public string LargeImage { get; internal set; }
        public string LargeText { get; internal set; }
        public string SmallImage { get; internal set; }
        public string SmallText { get; internal set; }

        internal DiscordActivityAssets(ActivityAssetsModel model)
        {
            LargeImage = model.LargeImage;
            LargeText = model.LargeText;
            SmallImage = model.SmallImage;
            SmallText = model.SmallText;
        }
    }

    public class DiscordActivitySecrets
    {
        public string Join { get; internal set; }
        public string Spectate { get; internal set; }
        public string Match { get; internal set; }

        internal DiscordActivitySecrets(ActivitySecretsModel model)
        {
            Join = model.Join;
            Spectate = model.Spectate;
            Match = model.Match;
        }
    }
}

using System;

namespace DiscordUnity2.Models
{
    public class ActivityModel
    {
        public string Name { get; set; }
        public ActivityType Type { get; set; }
        public string Url { get; set; }
        public int CreatedAt { get; set; }
        public ActivityTimestampsModel Timestamps { get; set; }
        public string ApplicationId { get; set; }
        public string Details { get; set; }
        public string State { get; set; }
        public ActivityEmojiModel Emoji { get; set; }
        public ActivityPartyModel Party { get; set; }
        public ActivityAssetsModel Assets { get; set; }
        public ActivitySecretsModel Secrets { get; set; }
        public bool Instance { get; set; }
        public ActivityFlags Flags { get; set; }
    }

    public enum ActivityType
    {
        Game = 0,       // Playing {name}	"Playing Rocket League"
        Streaming = 1,  // Streaming {details}	"Streaming Rocket League"
        Listening = 2,  // Listening to {name}	"Listening to Spotify"
        Custom = 4      // {emoji} {name}	":smiley: I am cool"
    }

    [Flags]
    public enum ActivityFlags
    {
        INSTANCE = 1,
        JOIN = 2,
        SPECTATE = 4,
        JOIN_REQUEST = 8,
        SYNC = 16,
        PLAY = 32
    }

    public class ActivityTimestampsModel
    {
        public int Start { get; set; }
        public int End { get; set; }
    }

    public class ActivityEmojiModel
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public bool? Animated { get; set; }
    }

    public class ActivityPartyModel
    {
        public string Id { get; set; }
        public int[] Size { get; set; }
    }

    public class ActivityAssetsModel
    {
        public string LargeImage { get; set; }
        public string LargeText { get; set; }
        public string SmallImage { get; set; }
        public string SmallText { get; set; }
    }

    public class ActivitySecretsModel
    {
        public string Join { get; set; }
        public string Spectate { get; set; }
        public string Match { get; set; }
    }
}

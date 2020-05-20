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
            
        }
    }

    public class DiscordActivityTimestamps
    {
        internal DiscordActivityTimestamps(ActivityTimestampsModel model)
        {
            
        }
    }

    public class DiscordActivityEmoji
    {
        internal DiscordActivityEmoji(ActivityEmojiModel model)
        {
            
        }
    }

    public class DiscordActivityParty
    {
        internal DiscordActivityParty(ActivityPartyModel model)
        {
            
        }
    }

    public class DiscordActivityAssets
    {
        internal DiscordActivityAssets(ActivityAssetsModel model)
        {
            
        }
    }

    public class DiscordActivitySecrets
    {
        internal DiscordActivitySecrets(ActivitySecretsModel model)
        {
            
        }
    }
}

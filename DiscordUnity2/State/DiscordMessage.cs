using DiscordUnity2.Models;
using System;

namespace DiscordUnity2.State
{
    public class DiscordMessage
    {
        public string Id { get; internal set; }
        public DiscordChannel Channel { get; internal set; }
        public DiscordServer Server { get; internal set; }
        public DiscordUser Author { get; internal set; }
        public string Content { get; internal set; }
        public DateTime Timestamp { get; internal set; }
        public DateTime? EditedTimestamp { get; internal set; }
        public bool Tts { get; internal set; }
        public bool MentionEveryone { get; internal set; }
        public MessageType Type { get; internal set; }

        internal DiscordMessage(MessageModel model)
        {
            Id = model.Id;

            if (string.IsNullOrEmpty(model.GuildId))
            {
                Server = null;
                Channel = DiscordAPI.PrivateChannels[model.ChannelId];
            }

            else
            {
                Server = DiscordAPI.Servers[model.GuildId];
                Channel = Server.Channels[model.ChannelId];
            }

            if (model.Author != null) Author = new DiscordUser(model.Author);
            Content = model.Content;
            Timestamp = model.Timestamp;
            EditedTimestamp = model.EditedTimestamp;
            Tts = model.Tts;
            MentionEveryone = model.MentionEveryone;
            Type = model.Type;
        }
    }

    public class DiscordReaction
    {
        public int Count { get; internal set; }
        public bool Me { get; internal set; }
        public DiscordEmoji Emoji { get; internal set; }

        internal DiscordReaction(ReactionModel model)
        {
            Count = model.Count;
            Me = model.Me;
            if (model.Emoji != null) Emoji = new DiscordEmoji(model.Emoji);
        }
    }
}

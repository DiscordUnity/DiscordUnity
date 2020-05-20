using DiscordUnity2.State;

namespace DiscordUnity2.API
{
    public interface IDiscordMessageEvents : IDiscordInterface
    {
        void OnMessageCreated(DiscordMessage message);
        void OnMessageUpdated(DiscordMessage message);
        void OnMessageDeleted(DiscordMessage message);

        void OnMessageDeletedBulk(string[] messageIds);

        void OnMessageReactionAdded(DiscordMessage message, DiscordReaction reaction);
        void OnMessageReactionRemoved(DiscordMessage message, DiscordReaction reaction);

        void OnMessageAllReactionsRemoved(DiscordMessage message, DiscordReaction reaction);
        void OnMessageEmojiReactionRemoved(DiscordMessage message, DiscordReaction reaction);
    }
}

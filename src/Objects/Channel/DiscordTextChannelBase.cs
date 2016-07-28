namespace DiscordUnity
{

    public class DiscordTextChannelBase : DiscordChannel
    {
        /// <summary> Deletes this channel. </summary>
        public void Delete(DiscordTextChannelCallback callback)
        {
            client.DeleteTextChannel(ID, callback);
        }

        /// <summary> Broadcasts for when you start typing. </summary>
        public void BroadcastTyping(DiscordCallback callback)
        {
            client.BroadcastTyping(ID, callback);
        }

        /// <summary> Gets the messages in this channel. </summary>
        /// <param name="limit">The limit of the amount of messages.</param>
        public void GetMessages(int limit, DiscordMessagesCallback callback)
        {
            client.GetMessages(ID, limit, callback);
        }

        /// <summary> Gets the messages in this channel. </summary>
        /// <param name="limit">The limit of the amount of messages.</param>
        /// <param name="message">A message to focus on.</param>
        /// <param name="before">Whether we should get all the message before or after the message that is focused.</param>
        public void GetMessages(int limit, DiscordMessage message, bool before, DiscordMessagesCallback callback)
        {
            client.GetMessages(ID, limit, message.ID, before, callback);
        }

        /// <summary> Sends a message to this channel. </summary>
        /// <param name="content">The content of this message.</param>
        /// <param name="textToSpeech">Whether this message should be used by text to speech.</param>
        public void SendMessage(string content, bool textToSpeech, DiscordMessageCallback callback)
        {
            client.SendMessage(ID, content, 0, textToSpeech, callback);
        }

        /// <summary> Sends a file to this channel. </summary>
        /// <param name="filePath">The path of this file.</param>
        public void SendFile(string filePath, DiscordCallback callback)
        {
            client.SendFile(ID, filePath, callback);
        }
    }
}
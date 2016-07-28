namespace DiscordUnity
{
    public class DiscordPrivateChannel : DiscordTextChannelBase
    {
        /// <summary> The recipient of this private channel. </summary>
        public DiscordUser recipient { get; internal set; }

        internal string lastMessageID;

        internal DiscordPrivateChannel(DiscordClient parent, DiscordPrivateChannelJSON e)
        {
            pos = 0;
            ID = e.id;
            client = parent;
            recipient = new DiscordUser(client, e.recipient);
            lastMessageID = e.last_message_id;
        }
    }
}
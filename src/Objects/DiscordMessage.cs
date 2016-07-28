using System;
using System.Collections.Generic;

namespace DiscordUnity
{
    public class DiscordMessage
    {
        /// <summary> The author of this message. </summary>
        public DiscordUser author { get; internal set; }
        /// <summary> The content of this message. </summary>
        public string content { get; internal set; }
        /// <summary> The collection this message has. </summary>
        public DiscordUser[] mentions { get { return _mentions.ToArray(); } }
        /// <summary> The collection of embeds this message has. </summary>
        public DiscordEmbed[] embeds { get { return _embeds.ToArray(); } }
        /// <summary> When is this message created? </summary>
        public DateTime createdAt { get; internal set; }
        /// <summary> When is this message edited? </summary>
        public DateTime editedAt { get; internal set; }
        /// <summary> The channel this message is created in. </summary>
        public DiscordTextChannel channel { get { return client._channels[channelID]; } }

        internal string ID;
        internal string channelID;
        internal DiscordClient client;
        internal List<DiscordUser> _mentions;
        internal List<DiscordEmbed> _embeds;

        internal DiscordMessage(DiscordClient parent, DiscordMessageJSON e)
        {
            ID = e.id;
            client = parent;
            channelID = e.channel_id;
            author = new DiscordUser(client, e.author);
            content = e.content;
            _mentions = new List<DiscordUser>();
            if (e.embeds != null) _embeds = new List<DiscordEmbed>(e.embeds);
            else _embeds = new List<DiscordEmbed>();
            if (!string.IsNullOrEmpty(e.timestamp)) createdAt = DateTime.Parse(e.timestamp);
            else createdAt = DateTime.Now;
            if (!string.IsNullOrEmpty(e.edited_timestamp)) editedAt = DateTime.Parse(e.edited_timestamp);
            else editedAt = DateTime.Now;

            if (e.mentions != null)
            {
                foreach (var mention in e.mentions)
                {
                    _mentions.Add(new DiscordUser(client, mention));
                }
            }
        }

        /// <summary> Edits this message. </summary>
        /// <param name="content">The content of this message.</param>
        public void Edit(string content, DiscordMessageCallback callback)
        {
            client.EditMessage(channelID, ID, content, callback);
        }

        /// <summary> Deletes this message. </summary>
        public void Delete(DiscordMessageCallback callback)
        {
            client.DeleteMessage(channelID, ID, callback);
        }

        /// <summary> Sets this message as read. </summary>
        public void Acknowledge(DiscordMessageCallback callback)
        {
            client.AcknowledgeMessage(channelID, ID, callback);
        }

        public override bool Equals(object o)
        {
            return base.Equals(o);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(DiscordMessage a, DiscordMessage b)
        {
            if ((object)a == null && (object)b == null) return true;
            if ((object)a == null && (object)b != null) return false;
            if ((object)a != null && (object)b == null) return false;
            return a.ID == b.ID;
        }

        public static bool operator !=(DiscordMessage a, DiscordMessage b)
        {
            return !(a == b);
        }
    }
}
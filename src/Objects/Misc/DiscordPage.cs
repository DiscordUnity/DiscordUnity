using System;

namespace DiscordUnity
{
    public class DiscordPage
    {
        /// <summary> The name of this page. </summary>
        public string name { get; internal set; }
        /// <summary> The url of this page. </summary>
        public string url { get; internal set; }
        /// <summary> When is this page updated? </summary>
        public DateTime updatedAt { get; internal set; }

        internal string ID;

        internal DiscordPage(DiscordPageJSON e)
        {
            name = e.name;
            url = e.url;
            ID = e.id;
            updatedAt = DateTime.Parse(e.updated_at);
        }

        public override bool Equals(object o)
        {
            return base.Equals(o);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(DiscordPage a, DiscordPage b)
        {
            if ((object)a == null && (object)b == null) return true;
            if ((object)a == null && (object)b != null) return false;
            if ((object)a != null && (object)b == null) return false;
            return a.ID == b.ID;
        }

        public static bool operator !=(DiscordPage a, DiscordPage b)
        {
            return !(a == b);
        }
    }
}
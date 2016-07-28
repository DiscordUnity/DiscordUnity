namespace DiscordUnity
{
    public class DiscordChannel
    {
        public int position { get { return pos; } }

        internal string ID;
        internal int pos;
        internal DiscordClient client;

        public override bool Equals(object o)
        {
            return base.Equals(o);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(DiscordChannel a, DiscordChannel b)
        {
            if ((object)a == null && (object)b == null) return true;
            if ((object)a == null && (object)b != null) return false;
            if ((object)a != null && (object)b == null) return false;
            return a.ID == b.ID;
        }

        public static bool operator !=(DiscordChannel a, DiscordChannel b)
        {
            return !(a == b);
        }
    }
}
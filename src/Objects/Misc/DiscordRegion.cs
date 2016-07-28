namespace DiscordUnity
{
    public class DiscordRegion
    {
        /// <summary> The name of this region. </summary>
        public string name { get; internal set; }
        /// <summary> The host of this region. </summary>
        public string hostname { get; internal set; }
        /// <summary> The port of this region used by voiceClient. </summary>
        public int port { get; internal set; }
        /// <summary> Is this region for vips? </summary>
        public bool vip { get; internal set; }
        /// <summary> Is this the best server? </summary>
        public bool optimal { get; internal set; }

        internal string ID;

        internal DiscordRegion(DiscordRegionJSON e)
        {
            name = e.name;
            hostname = e.sample_hostname;
            port = e.sample_port;
            ID = e.id;
            vip = e.vip;
            optimal = e.optimal;
        }

        public override bool Equals(object o)
        {
            return base.Equals(o);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(DiscordRegion a, DiscordRegion b)
        {
            if ((object)a == null && (object)b == null) return true;
            if ((object)a == null && (object)b != null) return false;
            if ((object)a != null && (object)b == null) return false;
            return a.ID == b.ID;
        }

        public static bool operator !=(DiscordRegion a, DiscordRegion b)
        {
            return !(a == b);
        }
    }
}
using System;

namespace DiscordUnity
{

    public class DiscordIncident
    {
        /// <summary> The status of this incident. </summary>
        public string status { get; internal set; }
        /// <summary> The content of this incident. </summary>
        public string body { get; internal set; }
        /// <summary> When is this incident-information created? </summary>
        public DateTime createdAt { get; internal set; }
        /// <summary> When is this incident-information updated? </summary>
        public DateTime updatedAt { get; internal set; }
        /// <summary> When is this incident-information displayed? </summary>
        public DateTime displayedAt { get; internal set; }

        internal string ID;
        internal string incidentID;

        internal DiscordIncident(DiscordIncidentJSON e)
        {
            status = e.status;
            body = e.body;
            createdAt = DateTime.Parse(e.created_at);
            updatedAt = DateTime.Parse(e.updated_at);
            displayedAt = DateTime.Parse(e.display_at);
            ID = e.id;
            incidentID = e.incident_id;
        }

        public override bool Equals(object o)
        {
            return base.Equals(o);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(DiscordIncident a, DiscordIncident b)
        {
            if ((object)a == null && (object)b == null) return true;
            if ((object)a == null && (object)b != null) return false;
            if ((object)a != null && (object)b == null) return false;
            return a.ID == b.ID;
        }

        public static bool operator !=(DiscordIncident a, DiscordIncident b)
        {
            return !(a == b);
        }
    }
}
using System;
using System.Collections.Generic;

namespace DiscordUnity
{
    public class DiscordMaintenance
    {
        /// <summary> The name of this maintenance. </summary>
        public string name { get; internal set; }
        /// <summary> The status of this maintenance. </summary>
        public string status { get; internal set; }
        /// <summary> The link to this maintenance in the browser. </summary>
        public string link { get; internal set; }
        /// <summary> The impact of this maintenance. </summary>
        public string impact { get; internal set; }
        /// <summary> The collection of incidents of this maintenance. </summary>
        public DiscordIncident[] incidents { get; internal set; }
        /// <summary> When is this maintenance-information created? </summary>
        public DateTime createdAt { get; internal set; }
        /// <summary> When is this maintenance-information updated? </summary>
        public DateTime updatedAt { get; internal set; }
        /// <summary> When will this maintenance be monitored? </summary>
        public DateTime monitoringAt { get; internal set; }
        /// <summary> When is this maintenance resolved? </summary>
        public DateTime resolvedAt { get; internal set; }
        /// <summary> When is this maintenance scheduled for? </summary>
        public DateTime scheduledFor { get; internal set; }
        /// <summary> When is this maintenance scheduled until? </summary>
        public DateTime scheduledUntil { get; internal set; }

        internal string ID;
        internal string pageID;

        internal DiscordMaintenance(DiscordMaintenanceJSON e)
        {
            name = e.name;
            status = e.status;
            link = e.shortlink;
            impact = e.impact;
            createdAt = DateTime.Parse(e.created_at);
            updatedAt = DateTime.Parse(e.updated_at);
            monitoringAt = DateTime.Parse(e.monitoring_at);
            resolvedAt = DateTime.Parse(e.resolved_at);
            scheduledFor = DateTime.Parse(e.scheduled_for);
            scheduledUntil = DateTime.Parse(e.scheduled_until);
            ID = e.id;
            pageID = e.page_id;
            List<DiscordIncident> incidentsList = new List<DiscordIncident>();

            foreach (var incident in e.incident_updates)
            {
                incidentsList.Add(new DiscordIncident(incident));
            }

            incidents = incidentsList.ToArray();
        }

        public override bool Equals(object o)
        {
            return base.Equals(o);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(DiscordMaintenance a, DiscordMaintenance b)
        {
            if ((object)a == null && (object)b == null) return true;
            if ((object)a == null && (object)b != null) return false;
            if ((object)a != null && (object)b == null) return false;
            return a.ID == b.ID;
        }

        public static bool operator !=(DiscordMaintenance a, DiscordMaintenance b)
        {
            return !(a == b);
        }
    }
}
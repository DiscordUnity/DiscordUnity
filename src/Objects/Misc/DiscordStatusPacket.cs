using System.Collections.Generic;

namespace DiscordUnity
{
    public class DiscordStatusPacket
    {
        /// <summary> The page with information. </summary>
        public DiscordPage page { get; internal set; }
        /// <summary> The collection of maintenances. </summary>
        public DiscordMaintenance[] maintenances { get; internal set; }

        internal DiscordStatusPacket(DiscordStatusPacketJSON e)
        {
            page = new DiscordPage(e.page);
            List<DiscordMaintenance> maintenancesList = new List<DiscordMaintenance>();

            foreach (var maintenance in e.scheduled_maintenances)
            {
                maintenancesList.Add(new DiscordMaintenance(maintenance));
            }

            maintenances = maintenancesList.ToArray();
        }
    }
}

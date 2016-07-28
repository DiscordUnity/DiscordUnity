using System.Linq;
using System.Collections.Generic;

namespace DiscordUnity
{
    public class DiscordPresence
    {
        /// <summary> The status of this user. </summary>
        public MemberStatus status { get; internal set; }
        /// <summary> The collections of roles this user has. </summary>
        public DiscordRole[] roles { get { return _roles.Values.ToArray(); } }
        /// <summary> The name of the game this user is playing. </summary>
        public string game { get; internal set; }

        internal string serverID;
        internal Dictionary<string, DiscordRole> _roles;

        internal DiscordPresence(DiscordClient parent, DiscordUser user, DiscordPresenceJSON e)
        {
            serverID = e.guild_id;

            if (!string.IsNullOrEmpty(e.status))
            {
                status = e.status == "online" ? MemberStatus.Online : e.status == "idle" ? MemberStatus.Idle : MemberStatus.Offline;
                user.status = e.status == "online" ? MemberStatus.Online : e.status == "idle" ? MemberStatus.Idle : MemberStatus.Offline;
            }

            if (e.game != null)
            {
                user.game = e.game.name;
                game = e.game.name;
            }

            _roles = new Dictionary<string, DiscordRole>();

            foreach (var role in e.roles)
            {
                if (role.id == null) continue;
                _roles.Add(role.id, new DiscordRole(parent, role, serverID));
            }
        }
    }
}
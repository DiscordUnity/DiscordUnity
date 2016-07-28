using UnityEngine;

namespace DiscordUnity
{
    public class DiscordRole
    {
        /// <summary> The name of this role. </summary>
        public string name { get; internal set; }
        /// <summary> Is this role hoisted?. </summary>
        public bool hoist { get; internal set; }
        /// <summary> Is this role managed? </summary>
        public bool managed { get; internal set; }
        /// <summary> The color of this role. </summary>
        public Color color { get; internal set; }
        /// <summary> The collection of permissions for this role. </summary>
        public DiscordPermission[] permissions { get; internal set; }
        /// <summary> The server this role is created in. </summary>
        public DiscordServer server { get { return client._servers[serverID]; } }
        /// <summary> The position of this role. </summary>
        public int position { get { return pos; } }

        internal string ID;
        internal string serverID;
        internal int pos;
        internal DiscordClient client;

        internal DiscordRole(DiscordClient parent, DiscordRoleJSON role, string serverid)
        {
            ID = role.id;
            client = parent;
            serverID = serverid;
            hoist = role.hoist;
            managed = role.managed;
            name = role.name;
            pos = role.position;
            color = Utils.GetColorFromInt(role.color);
            permissions = Utils.GetPermissions(role.permissions);
        }

        /// <summary> Edits the role. </summary>
        /// <param name="color">The color of this server.</param>
        /// <param name="hoist">Whether this should be hoisted.</param>
        /// <param name="name">The name of this role.</param>
        /// <param name="permissions">The permissions for this role.</param>
        public void EditRole(Color color, bool hoist, string name, DiscordPermission[] permissions, DiscordRoleCallback callback)
        {
            client.EditRole(serverID, ID, Utils.GetIntFromColor(color), hoist, name, permissions, callback);
        }

        /// <summary> Deletes this role if you have the permission. </summary>
        public void DeleteRole(DiscordRoleCallback callback)
        {
            client.DeleteRole(serverID, ID, callback);
        }

        public override bool Equals(object o)
        {
            return base.Equals(o);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(DiscordRole a, DiscordRole b)
        {
            if ((object)a == null && (object)b == null) return true;
            if ((object)a == null && (object)b != null) return false;
            if ((object)a != null && (object)b == null) return false;
            return a.ID == b.ID;
        }

        public static bool operator !=(DiscordRole a, DiscordRole b)
        {
            return !(a == b);
        }
    }
}
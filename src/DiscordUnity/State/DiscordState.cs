using DiscordUnity.State;
using System.Collections.Generic;

namespace DiscordUnity
{
    public static partial class DiscordAPI
    {
        public static int Version { get; private set; }
        public static DiscordUser User { get; private set; }
        public static Dictionary<string, DiscordServer> Servers { get; private set; }
        public static Dictionary<string, DiscordChannel> PrivateChannels { get; private set; }
        internal static Dictionary<string, DiscordUser> Users { get; private set; }

        internal static void InitializeState()
        {
            Version = -1;
            User = null;
            Servers = new Dictionary<string, DiscordServer>();
            PrivateChannels = new Dictionary<string, DiscordChannel>();
            Users = new Dictionary<string, DiscordUser>();
        }
    }
}

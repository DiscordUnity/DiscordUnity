using DiscordUnity2.State;
using System.Collections.Generic;

namespace DiscordUnity2
{
    public static partial class DiscordAPI
    {
        public static string Version { get; private set; }
        public static DiscordUser User { get; private set; }
        public static Dictionary<string, DiscordServer> Server { get; private set; }
        public static Dictionary<string, DiscordChannel> PrivateChannels { get; private set; }

        internal static void InitializeState()
        {
            Version = null;
            User = null;
            Server = new Dictionary<string, DiscordServer>();
            PrivateChannels = new Dictionary<string, DiscordChannel>();
        }
    }
}

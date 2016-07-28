using System;
using System.IO;

namespace DiscordUnity
{
    internal static class Debugger
    {
        // Temporarily disabled -> this is mostly what I'm using for creating DiscordUnity
        public static void WriteLine(string content)
        {
            /*using (StreamWriter writer = new StreamWriter("DiscordLog.txt", true))
            {
                writer.WriteLine("[{0}]: {1}", DateTime.Now, content);
                writer.Close();
            }*/
        }
    }
}

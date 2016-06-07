using System;
using System.IO;

namespace DiscordUnity
{
    internal static class Debugger
    {
        public static void WriteLine(string content)
        {
            using (StreamWriter writer = new StreamWriter("DiscordLog.txt", true))
            {
                writer.WriteLine("[{0}]: {1}", DateTime.Now, content);
                writer.Close();
            }
        }
    }
}

using System;

namespace DiscordUnity2
{
    public interface ILogger
    {
        void Log(string log);
        void LogWarning(string log);
        void LogError(string log, Exception exception = null);
    }

    internal class Logger : ILogger
    {
        public void Log(string log)
        {

        }

        public void LogError(string log, Exception exception = null)
        {

        }

        public void LogWarning(string log)
        {

        }
    }
}

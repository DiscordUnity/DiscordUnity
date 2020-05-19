using DiscordUnity2;
using System;
using System.Threading;

namespace DiscordUnity2Tests
{
    class Program
    {
        private const string Token = "NzEyMDIzMDkzODk5NjkwMDE1.XsLhTw.yZxTgImP0NOdAwvuTt9Tf8aO-ws";

        private static Thread thread;

        static void Main(string[] args)
        {
            Console.Title = "DiscordUnity";
            Console.ForegroundColor = ConsoleColor.Green;

            Console.WriteLine("Starting up DiscordUnity!");
            thread = Thread.CurrentThread;

            DiscordUnity.Logger = new Logger();

            static async void Start()
            {
                await DiscordUnity.StartWithBot(Token);
                Console.WriteLine("DiscordUnity Started: " + (Thread.CurrentThread == thread));
            }

            Start();

            while (Console.ReadLine() != "exit")
                DiscordUnity.Update();

            DiscordUnity.Stop();

            Console.ReadKey();
        }

        class Logger : ILogger
        {
            public void Log(string log)
            {
                Console.WriteLine(log);
            }

            public void LogError(string log, Exception exception = null)
            {
                Console.WriteLine(log);
                if (exception != null) Console.WriteLine(exception);
            }

            public void LogWarning(string log)
            {
                Console.WriteLine(log);
            }
        }
    }
}

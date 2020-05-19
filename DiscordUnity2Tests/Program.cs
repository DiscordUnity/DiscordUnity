using DiscordUnity2;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace DiscordUnity2Tests
{
    class Program
    {
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
                string token;

                using (StreamReader r = new StreamReader("config.json"))
                {
                    string json = r.ReadToEnd();
                    var config = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                    token = config["token"];
                }

                await DiscordUnity.StartWithBot(token);
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

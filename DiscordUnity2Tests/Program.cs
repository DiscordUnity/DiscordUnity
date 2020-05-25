using DiscordUnity2;
using DiscordUnity2.API;
using DiscordUnity2.State;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

            DiscordAPI.Logger = new Logger();

            static async void Start()
            {
                string token;

                using (StreamReader r = new StreamReader("config.json"))
                {
                    string json = r.ReadToEnd();
                    var config = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                    token = config["token"];
                }

                await DiscordAPI.StartWithBot(token);
                DiscordAPI.RegisterEventsHandler(new Handler());
                Console.WriteLine("DiscordUnity Started: " + (Thread.CurrentThread == thread));
            }

            Start();

            while (Console.ReadLine() != "exit")
                DiscordAPI.Update();

            DiscordAPI.Stop();

            Console.ReadKey();
        }

        class Handler : IDiscordServerEvents
        {
            public void OnServerBan(DiscordServer server, DiscordUser user)
            {

            }

            public void OnServerEmojisUpdated(DiscordServer server, DiscordEmoji[] emojis)
            {

            }

            public void OnServerJoined(DiscordServer server)
            {
                server.Channels.Values.FirstOrDefault(x => x.Type == DiscordUnity2.Models.ChannelType.GUILD_TEXT)?.CreateMessage("Hello World!", null, null, null, null, null, null);
            }

            public void OnServerLeft(DiscordServer server)
            {

            }

            public void OnServerMemberJoined(DiscordServer server, DiscordServerMember member)
            {

            }

            public void OnServerMemberLeft(DiscordServer server, DiscordServerMember member)
            {

            }

            public void OnServerMembersChunk(DiscordServer server, DiscordServerMember[] members, string[] notFound, DiscordPresence[] presences)
            {

            }

            public void OnServerMemberUpdated(DiscordServer server, DiscordServerMember member)
            {

            }

            public void OnServerRoleCreated(DiscordServer server, DiscordRole role)
            {

            }

            public void OnServerRoleRemove(DiscordServer server, DiscordRole role)
            {

            }

            public void OnServerRoleUpdated(DiscordServer server, DiscordRole role)
            {

            }

            public void OnServerUnban(DiscordServer server, DiscordUser user)
            {

            }

            public void OnServerUpdated(DiscordServer server)
            {

            }
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

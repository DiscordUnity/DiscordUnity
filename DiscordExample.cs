using UnityEngine;
using DiscordUnity;
using System.Linq;

public class DiscordExample : MonoBehaviour
{
    private bool bot;
    private string email;
    private string password;
    private string token;
    private string serverName;
    private string channelName;
    private string content;

    private DiscordClient client;
    private DiscordChannel randomChannel;
    private System.Diagnostics.Stopwatch timer;

    void Start()
    {
        bot = false;
        email = "email";
        password = "password";
        token = "token";
        serverName = "server name";
        channelName = "channel name";
        content = "Hello World!";

        client = new DiscordClient();
        timer = new System.Diagnostics.Stopwatch();

        client.OnClientOpened += ClientOpened;
        client.OnClientClosed += ClientClosed;
        client.OnMessageCreated += MessageCreated;
    }

    private void ClientOpened(object s, DiscordEventArgs e)
    {
        Debug.Log("Client opened.");
        timer.Start();
    }

    private void ClientClosed(object s, DiscordEventArgs e)
    {
        timer.Stop();
        Debug.Log("Client closed: " + timer.Elapsed.Seconds);
    }

    private void MessageCreated(object s, DiscordMessageArgs e)
    {
        if(e.message.author == client.user)
        {
            Debug.Log("We send: " + e.message.content);
        }
    }

    void Update()
    {
        if (client.isOnline)
        {
            client.Update();
        }
    }

    void OnGUI()
    {
        if (!client.isOnline)
        {
            bot = GUILayout.Toggle(bot, "Bot");

            if (!bot)
            {
                email = GUILayout.TextField(email);
                password = GUILayout.PasswordField(password, '*');
            }

            else
            {
                token = GUILayout.TextField(token);
            }

            if (GUILayout.Button("Start"))
            {
                if (!bot) client.Start(email, password);
                else client.StartBot(token);
            }
        }

        else
        {
            serverName = GUILayout.TextField(serverName);
            channelName = GUILayout.TextField(channelName);
            content = GUILayout.TextField(content);

            if (GUILayout.Button("Send Message"))
            {
                DiscordServer server = client.servers.Where(x => x.name == serverName).FirstOrDefault();
                randomChannel = server.channels.Where(x => x.name == channelName).FirstOrDefault();
                randomChannel.SendMessage(content, false);
            }

            if (GUILayout.Button("Stop"))
            {
                client.Stop();
            }
        }
    }
}
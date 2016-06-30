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
    private string filepath;

    private DiscordClient client;
    private DiscordTextChannel randomChannel;
    private System.Diagnostics.Stopwatch timer;

    void Start()
    {
        bot = false;
        email = "email";
        password = "password";
        token = "token";
        serverName = "server name";
        channelName = "channel name";
        filepath = "D://Documents//Pictures//MountainBackground.jpg";

        client = new DiscordClient();
        timer = new System.Diagnostics.Stopwatch();
        client.OnMessageCreated += MessageCreated;
    }

    private void MessageCreated(object s, DiscordMessageArgs e)
    {
        if (e.message.author == client.user)
        {
            Debug.Log("We send: " + e.message.content);
        }
    }

    private void ClientOpened(DiscordClient client, string result, DiscordError error)
    {
        if (error.failed)
        {
            Debug.LogError("Client failed to open: " + error.message);
            return;
        }

        Debug.Log("Client opened.");
        timer.Start();
    }

    private void ClientClosed(DiscordClient client, string result, DiscordError error)
    {
        if (error.failed)
        {
            Debug.LogError("Client failed to close: " + error.message);
            return;
        }
        
        timer.Start();
        timer.Stop();
        Debug.Log("Client closed: " + timer.Elapsed.Seconds);
    }

    private void FileSend(DiscordClient client, string result, DiscordError error)
    {
        if (error.failed)
        {
            Debug.LogError("Client failed to close: " + error.message);
            return;
        }

        Debug.Log("File send.");
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
                if (!bot) client.Start(email, password, ClientOpened);
                else client.StartBot(token, ClientOpened);
            }
        }

        else
        {
            serverName = GUILayout.TextField(serverName);
            channelName = GUILayout.TextField(channelName);
            filepath = GUILayout.TextField(filepath);

            if (GUILayout.Button("Send File"))
            {
                DiscordServer server = client.servers.Where(x => x.name == serverName).FirstOrDefault();
                randomChannel = server.channels.Where(x => x.name == channelName).FirstOrDefault();
                randomChannel.SendFile(filepath, FileSend);
            }

            if (GUILayout.Button("Stop"))
            {
                client.Stop(ClientClosed);
            }
        }
    }
}
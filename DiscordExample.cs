using UnityEngine;
using DiscordUnity;
using System.Linq;

public class DiscordExample : MonoBehaviour
{
    private bool bot;
    private bool editRole;
    private string email;
    private string password;
    private string token;
    private string serverName;
    private string roleName;
    private string memberName;
    private string channelName;
    //private string filepath;

    private DiscordClient client;
    private DiscordServer server;
    private DiscordTextChannel channel;
    private DiscordUser member;
    private DiscordRole role;
    private DiscordTextChannel randomChannel;
    private System.Diagnostics.Stopwatch timer;

    void Start()
    {
        bot = false;
        editRole = false;
        email = "email";
        password = "password";
        token = "token";
        serverName = "server name";
        memberName = "member name";
        roleName = "role name";
        channelName = "channel name";
        //filepath = "D://Documents//Pictures//MountainBackground.jpg";

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
            memberName = GUILayout.TextField(memberName);
            roleName = GUILayout.TextField(roleName);

            editRole = GUILayout.Toggle(editRole, "Role");

            if (GUILayout.Button("Give Permission for specific channel"))
            {
                if (server == null) server = client.servers.Where(x => x.name == serverName).FirstOrDefault();
                else if (server.name != serverName) server = client.servers.Where(x => x.name == serverName).FirstOrDefault();
                if (channel == null) channel = server.channels.Where(x => x.name == channelName).FirstOrDefault();
                else if (channel.name != channelName) channel = server.channels.Where(x => x.name == channelName).FirstOrDefault();

                if (editRole)
                {
                    if (role == null) role = server.roles.Where(x => x.name == roleName).FirstOrDefault();
                    else if (role.name != roleName) role = server.roles.Where(x => x.name == roleName).FirstOrDefault();
                    Debug.Log((role == null));
                    Debug.Log((channel == null));
                    channel.OverwritePermissions(role, new DiscordPermission[1] { DiscordPermission.SendMessages }, new DiscordPermission[0], OnPermissionChanged);
                }

                else
                {
                    if (member == null) member = server.members.Where(x => x.name == memberName).FirstOrDefault();
                    else if (member.name != memberName) member = server.members.Where(x => x.name == memberName).FirstOrDefault();
                    channel.OverwritePermissions(member, new DiscordPermission[1] { DiscordPermission.SendMessages }, new DiscordPermission[0], OnPermissionChanged);
                }
            }

            if (GUILayout.Button("Remove Permission for specific channel"))
            {
                if (server == null) server = client.servers.Where(x => x.name == serverName).FirstOrDefault();
                else if (server.name != serverName) server = client.servers.Where(x => x.name == serverName).FirstOrDefault();
                if (channel == null) channel = server.channels.Where(x => x.name == channelName).FirstOrDefault();
                else if (channel.name != channelName) channel = server.channels.Where(x => x.name == channelName).FirstOrDefault();

                if (editRole)
                {
                    if (role == null) role = server.roles.Where(x => x.name == roleName).FirstOrDefault();
                    else if (role.name != roleName) role = server.roles.Where(x => x.name == roleName).FirstOrDefault();
                    channel.OverwritePermissions(role, new DiscordPermission[0], new DiscordPermission[1] { DiscordPermission.SendMessages }, OnPermissionChanged);
                }

                else
                {
                    if (member == null) member = server.members.Where(x => x.name == memberName).FirstOrDefault();
                    else if (member.name != memberName) member = server.members.Where(x => x.name == memberName).FirstOrDefault();
                    channel.OverwritePermissions(member, new DiscordPermission[0], new DiscordPermission[1] { DiscordPermission.SendMessages }, OnPermissionChanged);
                }
            }

            if (GUILayout.Button("Stop"))
            {
                client.Stop(ClientClosed);
            }
        }
    }

    private void OnPermissionChanged(DiscordClient client, string result, DiscordError error)
    {
        if (error.failed)
        {
            Debug.Log("Failed to change permissions!");
        }

        Debug.Log("Permissions changed!");
    }
}
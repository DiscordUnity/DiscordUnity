using UnityEngine;
using DiscordUnity;
using System.Linq;
using System.Collections.Generic;

public class DiscordVoiceExample : MonoBehaviour
{
    public string servername;
    public string channelname;

    private string email;
    private string password;
    private DiscordClient client;
    private DiscordVoiceClient audioClient;
    private DiscordChannel channel;
    private List<float[]> buffer;

    private AudioSource source;
    private AudioClip sendClip;
    private AudioClip receivedClip;
    private int delayed;
    private int sendinterval;
    private bool sending;

    void Start()
    {
        email = "";
        password = "";
        audioClient = null;
        delayed = 0;
        sendinterval = 0;
        sending = false;
        buffer = new List<float[]>();
        source = GetComponent<AudioSource>();
        receivedClip = AudioClip.Create("ReceivedAudio", 960, 1, 48000, false);
        source.clip = receivedClip;
        client = new DiscordClient();

        client.OnClientOpened += ClientStarted;
        client.OnClientClosed += ClientStopped;

        client.OnVoiceClientOpened += AudioClientStarted;
        client.OnVoiceClientClosed += AudioClientStopped;
    }

    void FixedUpdate()
    {
        if (audioClient == null)
            return;

        if (audioClient.isOnline)
        {
            if (sending)
            {
                float[] sendbuffer = new float[960];
                sendClip.GetData(sendbuffer, sendinterval * 960);
                audioClient.SendVoice(sendbuffer);
                sendinterval = sendinterval >= 49 ? 0 : sendinterval + 1;
            }

            if (buffer.Count > 0)
            {
                if (delayed > 4)
                {
                    float[] packet = buffer[0];
                    buffer.RemoveAt(0);

                    if (source.isPlaying) source.Stop();
                    receivedClip.SetData(packet, 0);
                    source.Play();
                }

                else
                {
                    delayed++;
                }
            }

            else
            {
                delayed = 0;
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            sendClip = Microphone.Start("", true, 1, 48000);
            sending = true;
        }

        if (Input.GetKeyUp(KeyCode.V))
        {
            sending = false;
            Microphone.End("");
        }

        if (client != null)
        {
            client.Update();
        }
    }

    void OnGUI()
    {
        if (client.isOnline)
        {
            if (audioClient == null)
            {
                if (GUILayout.Button("Start Audio"))
                {
                    client.GetVoiceClient(channel);
                }
            }

            else
            {
                if (GUILayout.Button("Stop Audio"))
                {
                    audioClient.Stop();
                }
            }

            if (GUILayout.Button("Stop Client"))
            {
                client.Stop();
            }
        }

        else
        {
            email = GUILayout.TextField(email);
            password = GUILayout.PasswordField(password, '*');

            if (GUILayout.Button("Start Client"))
            {
                client.Start(email, password);
            }
        }
    }

    private void ClientStarted(object s, DiscordEventArgs e)
    {
        Debug.Log("Client started.");
        channel = client.servers.Where(x => x.name == servername).FirstOrDefault().channels.Where(x => x.name == channelname).FirstOrDefault();
    }

    private void ClientStopped(object s, DiscordEventArgs e)
    {
        Debug.Log("Client stopped.");
    }

    private void AudioClientStarted(object s, DiscordVoiceClientArgs e)
    {
        Debug.Log("Audio started.");
        audioClient = e.voiceClient;
        audioClient.OnVoicePacketSend += PacketSend;
        audioClient.OnVoicePacketReceived += PacketReceived;
    }

    private void AudioClientStopped(object s, DiscordVoiceClientArgs e)
    {
        if (audioClient == e.voiceClient)
        {
            Debug.Log("Audio stopped.");
            audioClient = null;
        }
    }

    private void PacketSend(object s, DiscordVoiceArgs e)
    {
        Debug.Log("Send packet: " + e.packet.Length);
    }

    private void PacketReceived(object s, DiscordVoiceArgs e)
    {
        Debug.Log("Received packet: " + e.unitypacket.Length);
        buffer.Add(e.unitypacket);
    }
}
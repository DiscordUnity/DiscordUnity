using UnityEngine;
using DiscordUnity;
using System;
using System.Net;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

public class Buffer<T>
{
    private T[] buffer;

    public Buffer()
    {
        buffer = new T[0];
    }

    public T[] this[int amount]
    {
        set
        {
            int length = buffer.Length;
            Array.Resize<T>(ref buffer, length + amount);
            Array.Copy(value, 0, buffer, length, amount);
        }

        get
        {
            T[] result = buffer.Take(amount).ToArray();
            buffer = buffer.Skip(amount).Take(buffer.Length - amount).ToArray();
            return result;
        }
    }

    public int Count
    {
        get
        {
            return buffer.Length;
        }
    }
}

public class DiscordVoiceExample : MonoBehaviour
{
    public string servername;
    public string channelname;

    private string email;
    private string password;
    private DiscordClient client;
    private DiscordVoiceClient voiceClient;
    private DiscordVoiceChannel channel;
    private Buffer<float> buffer;

    private AudioSource source;
    private AudioClip sendClip;
    private AudioClip receivedClip;
    private int delayed;
    private int sendinterval;
    private bool sending;

    void Start()
    {
        email = "email";
        password = "password";
        voiceClient = null;
        delayed = 0;
        sendinterval = 0;
        sending = false;
        buffer = new Buffer<float>();
        source = GetComponent<AudioSource>();
        receivedClip = AudioClip.Create("ReceivedAudio", 9600, 1, 48000, false);
        source.clip = receivedClip;
        ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;
        client = new DiscordClient();
    }

    void FixedUpdate()
    {
        if (voiceClient == null)
            return;

        if (voiceClient.isOnline)
        {
            if (sending)
            {
                float[] sendbuffer = new float[960];
                sendClip.GetData(sendbuffer, sendinterval * 960);
                voiceClient.SendVoice(sendbuffer);
                sendinterval = sendinterval >= 49 ? 0 : sendinterval + 1;
            }

            if (buffer.Count > 0)
            {
                if (delayed > 9)
                {
                    if (buffer.Count > 9600)
                    {
                        delayed = 0;
                        float[] packet = buffer[9600];
                        if (source.isPlaying) source.Stop();
                        receivedClip.SetData(packet, 0);
                        source.Play();
                    }

                    else
                    {
                        float[] packet = buffer[buffer.Count];
                        if (source.isPlaying) source.Stop();
                        receivedClip.SetData(packet, 0);
                        source.Play();
                    }
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
            sendClip = Microphone.Start("Microphone (3- Logitech USB Headset)", true, 1, 48000);
            sending = true;
        }

        if (Input.GetKeyUp(KeyCode.V))
        {
            sending = false;
            Microphone.End("Microphone (3- Logitech USB Headset)");
        }

        if (client.isUpdatable)
        {
            client.Update();
        }
    }

    void OnGUI()
    {
        if (client.isOnline)
        {
            if (voiceClient == null)
            {
                if (GUILayout.Button("Start Audio"))
                {
                    client.GetVoiceClient(channel, false, false, VoiceClientStarted);
                }
            }

            else
            {
                if (GUILayout.Button("Stop Audio"))
                {
                    voiceClient.Stop(VoiceClientStopped);
                }
            }

            if (GUILayout.Button("Stop Client"))
            {
                client.Stop(ClientStopped);
            }
        }

        else
        {
            email = GUILayout.TextField(email);
            password = GUILayout.PasswordField(password, '*');

            if (GUILayout.Button("Start Client"))
            {
                client.Start(email, password, ClientStarted);
            }
        }
    }

    private void ClientStarted(DiscordClient client, string message, DiscordError error)
    {
        if (error.failed)
        {
            Debug.LogError("Start failed: " + error.message);
            return;
        }

        Debug.Log("Client started.");
        channel = client.servers.Where(x => x.name == servername).FirstOrDefault().voicechannels.Where(x => x.name == channelname).FirstOrDefault();
    }

    private void ClientStopped(DiscordClient client, string message, DiscordError error)
    {
        if (error.failed)
        {
            Debug.LogError("Stop failed: " + error.message);
            return;
        }

        Debug.Log("Client stopped.");
    }

    private void VoiceClientStarted(DiscordClient client, DiscordVoiceClient vc, DiscordError error)
    {
        if (error.failed)
        {
            Debug.LogError("Start failed: " + error.message);
            return;
        }

        Debug.Log("Audio started.");
        voiceClient = vc;
        voiceClient.OnVoicePacketSend += PacketSend;
        voiceClient.OnVoicePacketReceived += PacketReceived;
    }

    private void VoiceClientStopped(DiscordClient client, DiscordVoiceClient vc, DiscordError error)
    {
        if (error.failed)
        {
            Debug.LogError("Stop failed: " + error.message);
            return;
        }

        if (voiceClient == vc)
        {
            Debug.Log("VoiceClient stopped.");
            voiceClient = null;
        }
    }

    private void PacketSend(object s, DiscordVoiceArgs e)
    {
        Debug.Log("Send packet: " + e.packet.Length);
    }

    private void PacketReceived(object s, DiscordVoiceArgs e)
    {
        Debug.Log("Received packet: " + e.unitypacket.Length);
        buffer[e.unitypacket.Length] = e.unitypacket;
    }

    public bool MyRemoteCertificateValidationCallback(System.Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        bool isOk = true;
        // If there are errors in the certificate chain, look at each error to determine the cause.
        if (sslPolicyErrors != SslPolicyErrors.None)
        {
            for (int i = 0; i < chain.ChainStatus.Length; i++)
            {
                if (chain.ChainStatus[i].Status != X509ChainStatusFlags.RevocationStatusUnknown)
                {
                    chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                    chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                    chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
                    chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
                    bool chainIsValid = chain.Build((X509Certificate2)certificate);
                    if (!chainIsValid)
                    {
                        isOk = false;
                    }
                }
            }
        }
        return isOk;
    }
}
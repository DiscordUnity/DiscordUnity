using System.Collections.Generic;

namespace DiscordUnity2.API
{
    internal class DiscordInterfaces : IDiscordAPIEvents
    {
        private readonly HashSet<IDiscordAPIEvents> api;
        private readonly HashSet<IDiscordChannelEvents> channel;
        private readonly HashSet<IDiscordServerEvents> server;
        private readonly HashSet<IDiscordInviteEvents> invite;
        private readonly HashSet<IDiscordMessageEvents> message;
        private readonly HashSet<IDiscordVoiceEvents> voice;
        private readonly HashSet<IDiscordWebhookEvents> webhook;

        internal DiscordInterfaces()
        {
            api = new HashSet<IDiscordAPIEvents>();
            channel = new HashSet<IDiscordChannelEvents>();
            server = new HashSet<IDiscordServerEvents>();
            invite = new HashSet<IDiscordInviteEvents>();
            message = new HashSet<IDiscordMessageEvents>();
            voice = new HashSet<IDiscordVoiceEvents>();
            webhook = new HashSet<IDiscordWebhookEvents>();
        }

        internal void AddEventHandler(IDiscordInterface e)
        {
            if (e is IDiscordAPIEvents a) api.Add(a);
            if (e is IDiscordChannelEvents c) channel.Add(c);
            if (e is IDiscordServerEvents s) server.Add(s);
            if (e is IDiscordInviteEvents i) invite.Add(i);
            if (e is IDiscordMessageEvents m) message.Add(m);
            if (e is IDiscordVoiceEvents v) voice.Add(v);
            if (e is IDiscordWebhookEvents w) webhook.Add(w);
        }

        internal void RemoveEventHandler(IDiscordInterface e)
        {
            if (e is IDiscordAPIEvents a) api.Remove(a);
            if (e is IDiscordChannelEvents c) channel.Remove(c);
            if (e is IDiscordServerEvents s) server.Remove(s);
            if (e is IDiscordInviteEvents i) invite.Remove(i);
            if (e is IDiscordMessageEvents m) message.Remove(m);
            if (e is IDiscordVoiceEvents v) voice.Remove(v);
            if (e is IDiscordWebhookEvents w) webhook.Remove(w);
        }

        public void OnDiscordAPIClosed()
        {
            foreach (var e in api)
                e.OnDiscordAPIClosed();
        }

        public void OnDiscordAPIOpen()
        {
            foreach (var e in api)
                e.OnDiscordAPIOpen();
        }

        public void OnDiscordAPIResumed()
        {
            foreach (var e in api)
                e.OnDiscordAPIResumed();
        }
    }
}

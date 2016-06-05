using System;

namespace DiscordUnity
{
    public partial class DiscordClient
    {

        /// <summary> SenderObject is the client.</summary>
        public EventHandler<DiscordEventArgs> OnClientOpened = delegate { };
        /// <summary> SenderObject is the client.</summary>
        public EventHandler<DiscordEventArgs> OnClientClosed = delegate { };
        /// <summary> SenderObject is the client.</summary>
        public EventHandler<DiscordRegionArgs> OnRegionsReceived = delegate { };

        /// <summary> SenderObject is the client.</summary>
        public EventHandler<DiscordDMArgs> OnDMCreated = delegate { };
        /// <summary> SenderObject is the client.</summary>
        public EventHandler<DiscordDMArgs> OnDMDeleted = delegate { };

        /// <summary> SenderObject is the server.</summary>
        public EventHandler<DiscordChannelArgs> OnChannelCreated = delegate { };
        /// <summary> SenderObject is the server.</summary>
        public EventHandler<DiscordChannelArgs> OnChannelUpdated = delegate { };
        /// <summary> SenderObject is the server.</summary>
        public EventHandler<DiscordChannelArgs> OnChannelDeleted = delegate { };

        /// <summary> SenderObject is the channel.</summary>
        public EventHandler<DiscordMessageArgs> OnMessageCreated = delegate { };
        /// <summary> SenderObject is the channel.</summary>
        public EventHandler<DiscordMessageArgs> OnMessageUpdated = delegate { };
        /// <summary> SenderObject is the channel.</summary>
        public EventHandler<DiscordMessageArgs> OnMessageDeleted = delegate { };

        /// <summary> SenderObject is the user.</summary>
        public EventHandler<DiscordPresenceArgs> OnPresenceUpdated = delegate { };
        /// <summary> SenderObject is the channel.</summary>
        public EventHandler<DiscordMemberArgs> OnTypingStarted = delegate { };
        /// <summary> SenderObject is the channel.</summary>
        public EventHandler<DiscordMemberArgs> OnTypingStopped = delegate { };

        /// <summary> SenderObject is the client.</summary>
        public EventHandler<DiscordServerArgs> OnServerCreated = delegate { };
        /// <summary> SenderObject is the client.</summary>
        public EventHandler<DiscordServerArgs> OnServerUpdated = delegate { };
        /// <summary> SenderObject is the client.</summary>
        public EventHandler<DiscordServerArgs> OnServerDeleted = delegate { };

        /// <summary> SenderObject is the server.</summary>
        public EventHandler<DiscordMemberArgs> OnMemberJoined = delegate { };
        /// <summary> SenderObject is the server.</summary>
        public EventHandler<DiscordMemberArgs> OnMemberUpdated = delegate { };
        /// <summary> SenderObject is the server.</summary>
        public EventHandler<DiscordMemberArgs> OnMemberLeft = delegate { };

        /// <summary> SenderObject is the server.</summary>
        public EventHandler<DiscordMemberArgs> OnMemberBanned = delegate { };
        /// <summary> SenderObject is the server.</summary>
        public EventHandler<DiscordUserArgs> OnMemberUnbanned = delegate { };

        /// <summary> SenderObject is the server.</summary>
        public EventHandler<DiscordRoleArgs> OnRoleCreated = delegate { };
        /// <summary> SenderObject is the server.</summary>
        public EventHandler<DiscordRoleArgs> OnRoleUpdated = delegate { };
        /// <summary> SenderObject is the server.</summary>
        public EventHandler<DiscordRoleArgs> OnRoleDeleted = delegate { };

        /// <summary> SenderObject is the client.</summary>
        public EventHandler<DiscordInviteArgs> OnInviteCreated = delegate { };
        /// <summary> SenderObject is the client.</summary>
        public EventHandler<DiscordInviteArgs> OnInviteAccepted = delegate { };
        /// <summary> SenderObject is the client.</summary>
        public EventHandler<DiscordInviteArgs> OnInviteUpdated = delegate { };
        /// <summary> SenderObject is the client.</summary>
        public EventHandler<DiscordInviteArgs> OnInviteDeleted = delegate { };

        /// <summary> SenderObject is the client.</summary>
        public EventHandler<DiscordStatusArgs> OnStatusReceived = delegate { };
        /// <summary> SenderObject is the client.</summary>
        public EventHandler<DiscordUserArgs> OnUserUpdated = delegate { };
        /// <summary> SenderObject is the client.</summary>
        public EventHandler<DiscordUserArgs> OnProfileUpdated = delegate { };
        
        [Obsolete("AudioClient is work in progress.", false)]
        public EventHandler<DiscordAudioClientArgs> OnAudioClientOpened = delegate { };
        [Obsolete("AudioClient is work in progress.", false)]
        public EventHandler<DiscordAudioClientArgs> OnAudioClientClosed = delegate { };
    }
}

using System;

namespace DiscordUnity
{
    public class DiscordInvite
    {
        /// <summary> The code of this invite. </summary>
        public string code { get; internal set; }
        /// <summary> The passcode of this invite. </summary>
        public string xkcdpass { get; internal set; }
        /// <summary> The amount of uses left. </summary>
        public int uses { get; internal set; }
        /// <summary> The maximun amount of uses. </summary>
        public int maxUses { get; internal set; }
        /// <summary> The age. </summary>
        public int maxAge { get; internal set; }
        /// <summary> Is this a temporary invite? </summary>
        public bool temporary { get; internal set; }
        /// <summary> Is this invite revoked? </summary>
        public bool revoked { get; internal set; }
        /// <summary> Who created this invite. </summary>
        public DiscordUser inviter { get; internal set; }
        /// <summary> For what server is this invite. </summary>
        public DiscordServer server { get; internal set; }
        /// <summary> For what channel is this invite. </summary>
        public DiscordChannel channel { get; internal set; }
        /// <summary> When is this invite created? </summary>
        public DateTime createdAt { get; internal set; }

        internal DiscordInvite(DiscordClient parent, DiscordBasicInviteJSON invite)
        {
            code = invite.code;
            server = new DiscordServer(parent, invite.guild);
            channel = (invite.channel.type == "text") ? new DiscordTextChannel(parent, invite.channel) as DiscordChannel : new DiscordVoiceChannel(parent, invite.channel) as DiscordChannel;
            xkcdpass = invite.xkcdpass;
        }

        internal DiscordInvite(DiscordClient parent, DiscordRichInviteJSON invite) : this(parent, (DiscordBasicInviteJSON)invite)
        {
            uses = invite.uses;
            maxUses = invite.max_uses;
            maxAge = invite.max_age;
            temporary = invite.temporary;
            revoked = invite.revoked;
            inviter = new DiscordUser(parent, invite.inviter);
            createdAt = DateTime.Parse(invite.created_at);
        }
    }
}
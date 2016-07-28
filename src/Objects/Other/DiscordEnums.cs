namespace DiscordUnity
{
    public enum MemberStatus
    {
        Idle,
        Online,
        Offline
    }

    public enum TextureState
    {
        NoTexture,
        Unloaded,
        Loading,
        Loaded
    }

    public enum TargetType
    {
        Member,
        Role
    }

    public enum DiscordChannelType
    {
        Text,
        Voice
    }

    public enum DiscordPermission : byte
    {
        CreateInstanceInvite = 0,
        KickMembers = 1,
        BanMembers = 2,
        ManageRoles = 3,
        ManageChannels = 4,
        ManageServer = 5,

        ReadMessages = 10,
        SendMessages = 11,
        SendTTSMessages = 12,
        ManageMessages = 13,
        EmbedLinks = 14,
        AttachFiles = 15,
        ReadMessageHistory = 16,
        MentionEveryone = 17,

        VoiceConnect = 20,
        VoiceSpeak = 21,
        VoiceMuteMembers = 22,
        VoiceDeafenMembers = 23,
        VoiceMoveMembers = 24,
        VoiceUseActivationDetection = 25
    }
}
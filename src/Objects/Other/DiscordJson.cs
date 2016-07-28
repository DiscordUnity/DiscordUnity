using System;

namespace DiscordUnity
{

    [Serializable]
    internal struct DiscordEventJSON
    {
        public string t;
        public int s;
        public int op;
    }

    [Serializable]
    internal struct ReadyJSON
    {
        public int v;
        public DiscordUserJSON user;
        public DiscordPrivateChannelJSON[] private_channels;
        public DiscordServerJSON[] guilds;
        public DiscordReadStateJSON[] read_state;
        public string session_id;
        public int heartbeat_interval;
    }

    [Serializable]
    internal struct DiscordReadStateJSON
    {
        public string id;
        public int mention_count;
        public string last_message_id;
    }

    [Serializable]
    internal struct DiscordRegionJSON
    {
        public string sample_hostname;
        public int sample_port;
        public string id;
        public string name;
        public bool vip;
        public bool optimal;
    }

    [Serializable]
    internal struct DiscordRegionJSONWrapper
    {
        public DiscordRegionJSON[] regions;
    }

    [Serializable]
    internal struct DiscordPageJSON
    {
        public string id;
        public string name;
        public string url;
        public string updated_at;
    }

    [Serializable]
    internal struct DiscordIncidentJSON
    {
        public string status;
        public string body;
        public string created_at;
        public string updated_at;
        public string display_at;
        public string id;
        public string incident_id;
    }

    [Serializable]
    internal struct DiscordMaintenanceJSON
    {
        public string name;
        public string status;
        public string created_at;
        public string updated_at;
        public string monitoring_at;
        public string resolved_at;
        public string shortlink;
        public string scheduled_for;
        public string scheduled_until;
        public string id;
        public string page_id;
        public DiscordIncidentJSON[] incident_updates;
        public string impact;
    }

    [Serializable]
    internal struct DiscordStatusPacketJSON
    {
        public DiscordPageJSON page;
        public DiscordMaintenanceJSON[] scheduled_maintenances;
    }

    [Serializable]
    internal struct DiscordPrivateChannelJSON
    {
        public string id;
        public bool is_private;
        public DiscordUserJSON recipient;
        public string last_message_id;
    }

    [Serializable]
    internal class DiscordRichInviteJSON : DiscordBasicInviteJSON
    {
        public DiscordUserJSON inviter;
        public int uses;
        public int max_uses;
        public int max_age;
        public bool temporary;
        public string created_at;
        public bool revoked;
    }

    [Serializable]
    internal class DiscordRichInviteJSONWrapper
    {
        public DiscordRichInviteJSON[] invites;
    }

    [Serializable]
    internal class DiscordBasicInviteJSON
    {
        public string code;
        public DiscordServerJSON guild;
        public DiscordChannelJSON channel;
        public string xkcdpass;
    }

    [Serializable]
    internal class DiscordInviteJSON
    {
        public int max_age;
        public int max_uses;
        public bool temporary;
        public bool xkcdpass;
    }

    [Serializable]
    internal struct DiscordRoleJSONWrapper
    {
        public DiscordRoleJSON[] roles;
    }

    [Serializable]
    internal struct DiscordRoleJSON
    {
        public string id;
        public string name;
        public uint color;
        public bool hoist;
        public int position;
        public uint permissions;
        public bool managed;
    }

    [Serializable]
    internal class DiscordProfileJSON : DiscordUserJSON
    {
        public string token;
    }

    [Serializable]
    internal class DiscordUserBanJSONWrapper
    {
        public DiscordUserBanJSON[] bans;
    }

    [Serializable]
    internal class DiscordUserBanJSON
    {
        public string reason;
        public DiscordUserJSON user;
    }

    [Serializable]
    internal class DiscordUserJSON
    {
        public string id;
        public string username;
        public string email;
        public string discriminator;
        public bool verified;
        public bool bot = false;
        public string avatar;
    }

    [Serializable]
    internal struct DiscordMemberJSONWrapper
    {
        public DiscordMemberJSON[] members;
    }

    [Serializable]
    internal struct DiscordMemberJSON
    {
        public string guild_id;
        public DiscordUserJSON user;
        public DiscordRoleJSON[] roles;
        public bool mute;
        public string joined_at;
        public bool deaf;
    }

    [Serializable]
    internal struct DiscordMembersJSON
    {
        public string guild_id;
        public DiscordMemberJSON[] members;
    }

    [Serializable]
    internal struct DiscordEmojiJSON
    {
        public string id;
        public string name;
        public DiscordRoleJSON[] roles;
        public bool require_colons;
        public bool managed;
    }

    [Serializable]
    internal struct DiscordVoiceStateJSON
    {
        public string guild_id;
        public string channel_id;
        public string user_id;
        public string session_id;
        public bool deaf;
        public bool mute;
        public bool self_deaf;
        public bool self_mute;
        public bool suppress;
    }

    [Serializable]
    internal struct DiscordVoiceServerStateJSON
    {
        public string token;
        public string guild_id;
        public string endpoint;
    }

    [Serializable]
    internal struct DiscordRoleEventJSON
    {
        public string guild_id;
        public string role_id;
        public DiscordRoleJSON role;
    }

    [Serializable]
    internal struct DiscordBanJSON
    {
        public string guild_id;
        public DiscordUserJSON user;
    }

    [Serializable]
    internal struct DiscordServerEmojisJSON
    {
        public string guild_id;
        public DiscordEmojiJSON[] emojis;
    }

    [Serializable]
    internal struct DiscordServerIntegrationJSON
    {
        public string guild_id;
    }

    [Serializable]
    internal class DiscordServerJSONWrapper
    {
        public DiscordServerJSON[] servers;
    }

    [Serializable]
    internal class DiscordServerJSON
    {
        public string id;
        public string name;
        public string icon;
        public string splash;
        public string owner_id;
        public string region;
        public string afk_channel_id;
        public int afk_timeout;
        public bool embed_enabled;
        public bool unavailable;
        public string embed_channel_id;
        public int verification_level;
        public int member_count;
        public DiscordMemberJSON[] members;
        public DiscordPresenceJSON[] presences;
        public DiscordVoiceStateJSON[] voice_states;
        public DiscordRoleJSON[] roles;
        public DiscordEmojiJSON[] emojis;
        public bool large;
        public string joined_at;
        public DiscordChannelJSON[] channels;
        public DiscordFeatureJSON[] features;
    }

    [Serializable]
    internal struct DiscordFeatureJSON
    {

    }

    [Serializable]
    internal struct DiscordPresenceJSON
    {
        public DiscordUserJSON user;
        public DiscordRoleJSON[] roles;
        public DiscordGame game;
        public string guild_id;
        public string status;
    }

    [Serializable]
    internal struct DiscordAttachmentJSON
    {
        public string id;
        public string filename;
        public int size;
        public string url;
        public string proxy_url;
        public int height;
        public int width;
    }

    [Serializable]
    internal struct DiscordTypingJSON
    {
        public string channel_id;
        public string user_id;
        public string timestamp;
    }

    [Serializable]
    internal struct DiscordMessageAckJSON
    {
        public string message_id;
        public string channel_id;
    }

    [Serializable]
    internal struct DiscordMessageJSON
    {
        public string id;
        public string channel_id;
        public DiscordUserJSON author;
        public string content;
        public string timestamp;
        public string edited_timestamp;
        public bool tts;
        public bool mention_everyone;
        public DiscordUserJSON[] mentions;
        public DiscordAttachmentJSON[] attachments;
        public DiscordEmbed[] embeds;
        public int nonce;
        public bool pinned;
    }

    [Serializable]
    internal struct DiscordMessageJSONWrapper
    {
        public DiscordMessageJSON[] messages;
    }

    [Serializable]
    internal struct DiscordChannelJSON
    {
        public string id;
        public string guild_id;
        public string name;
        public string type;
        public int position;
        public bool is_private;
        public CreateOrEditPermissionArgs[] permission_overwrites;
        public string topic;
        public string last_message_id;
        public int bitrate;
    }

    [Serializable]
    internal struct DiscordChannelJSONWrapper
    {
        public DiscordChannelJSON[] channels;
    }

    [Serializable]
    internal struct GatewayJSON
    {
        public string url;
    }

    [Serializable]
    internal struct DiscordTokenJSON
    {
        public string id;
        public string token;
    }

    [Serializable]
    internal struct LoginArgs
    {
        public string email;
        public string password;
    }

    [Serializable]
    internal struct KeepAliveArgs
    {
        public int op;
        public int d;
    }

    [Serializable]
    internal struct CreateChannelArgs
    {
        public string name;
        public string type;
    }

    [Serializable]
    internal struct EditChannelArgs
    {
        public string name;
        public int position;
        public string topic;
    }

    [Serializable]
    internal struct EditVoiceChannelArgs
    {
        public string name;
        public int position;
        public int bitrate;
        public int user_limit;
    }

    [Serializable]
    internal struct SendMessageArgs
    {
        public string content;
        public int nonce;
        public bool tts;
    }

    [Serializable]
    internal struct EditMessageArgs
    {
        public string content;
    }

    [Serializable]
    internal struct CreateOrEditPermissionArgs
    {
        public string id;
        public string type;
        public uint allow;
        public uint deny;
    }

    [Serializable]
    internal struct CreateServerArgs
    {
        public string name;
        public string region;
        public string icon;
    }

    [Serializable]
    internal struct EditServerArgs
    {
        public string name;
        public string region;
        public int verification_level;
        public string afk_channel_id;
        public int afk_timeout;
        public string icon;
        public string owner_id;
        public string splash;
    }

    [Serializable]
    internal struct EditMemberArgs
    {
        public string nick;
        public string[] roles;
        public bool mute;
        public bool deaf;
        public string channel_id;
    }

    [Serializable]
    internal struct EditRoleArgs
    {
        public uint color;
        public bool hoist;
        public string name;
        public uint permissions;
    }

    [Serializable]
    internal struct CreatePrivateChannelArgs
    {
        public string recipient_id;
    }

    [Serializable]
    internal struct EditProfileArgs
    {
        public string avatar;
        public string email;
        public string new_password;
        public string password;
        public string username;
    }

    [Serializable]
    internal struct MoveMemberArgs
    {
        public string channel_id;
    }

    [Serializable]
    internal struct MemberChunkArgs
    {
        public string guild_id;
        public string query;
        public int limit;
    }

    [Serializable]
    internal struct JoinVoiceArgs
    {
        public string guild_id;
        public string channel_id;
        public bool self_mute;
        public bool self_deaf;
    }

    [Serializable]
    internal struct DiscordIdentifyArgs
    {
        public string server_id;
        public string user_id;
        public string session_id;
        public string token;
    }

    [Serializable]
    internal struct PayloadArgs<T>
    {
        public int op;
        public T d;
    }

    [Serializable]
    internal struct DiscordIdentityArgs
    {
        public string token;
        public int v;
        public DiscordPropertiesArgs properties;
    }
    
    [Serializable]
    internal struct DiscordPropertiesArgs
    {
        public string os;
        public string browser;
        public string device;
        public string referrer;
        public string referring_domain;
    }

    [Serializable]
    internal struct VoiceSpeakingArgs
    {
        public bool speaking;
        public int delay;
    }

    [Serializable]
    internal struct VoiceSendIPDataArgs
    {
        public string address;
        public int port;
        public string mode;
    }

    [Serializable]
    internal struct VoiceSendIPArgs
    {
        public string protocol;
        public VoiceSendIPDataArgs data;
    }

    [Serializable]
    internal struct PayloadJSON
    {
        public int op;
    }

    [Serializable]
    internal struct VoiceKeyJSON
    {
        public byte[] secret_key;
    }

    [Serializable]
    internal struct VoiceSpeakingJSON
    {
        public string user_id;
        public bool speaking;
        public uint ssrc;
    }

    [Serializable]
    internal struct VoiceConnectionJSON
    {
        public uint ssrc;
        public int port;
        public string[] modes;
        public int heartbeat_interval;
    }

    [Serializable]
    internal struct VoiceDisconnectArgs
    {
        public string guild_id;
        public string channel_id;
        public bool self_mute;
        public bool self_deaf;
    }

    [Serializable]
    internal struct RateLimitJSON
    {
        public string message;
        public int retry_after;
    }
}
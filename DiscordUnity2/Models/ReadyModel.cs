using Newtonsoft.Json;

namespace DiscordUnity2.Models
{
    internal class ReadyModel
    {
        [JsonProperty("v")]
        public int Version { get; set; }
        public UserModel User { get; set; }
        public object[] PrivateChannels { get; set; }
        public GuildModel[] Servers { get; set; }
        public string SessionId { get; set; }
        public int[] Shard { get; set; }
    }
}

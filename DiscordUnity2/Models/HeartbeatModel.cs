using Newtonsoft.Json;

namespace DiscordUnity2.Models
{
    public class HeartbeatModel
    {
        [JsonProperty("heartbeat_interval")]
        public int HeartbeatInterval { get; set; }
    }
}

using Newtonsoft.Json;

namespace DiscordUnity2.Models
{
    public class GatewayModel
    {
        public string Url { get; set; }
        public int Shards { get; set; }
        [JsonProperty("session_start_limit")]
        public SessionStartLimit SessionStartLimit { get; set; }
    }

    public class SessionStartLimit
    {
        public int Total { get; set; }
        public int Remaining { get; set; }
        [JsonProperty("reset_after")]
        public int ResetAfter { get; set; }
    }
}

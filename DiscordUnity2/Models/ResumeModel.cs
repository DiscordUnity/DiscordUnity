using Newtonsoft.Json;

namespace DiscordUnity2.Models
{
    internal class ResumeModel
    {
        public string Token { get; set; }
        public string SessionId { get; set; }
        [JsonProperty("seq")]
        public int Sequence { get; set; }
    }
}

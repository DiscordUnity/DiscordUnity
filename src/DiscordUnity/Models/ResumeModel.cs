using Newtonsoft.Json;

namespace DiscordUnity.Models
{
    internal class ResumeModel
    {
        public string Token { get; set; }
        public string SessionId { get; set; }
        [JsonProperty("seq")]
        public int Sequence { get; set; }
    }
}

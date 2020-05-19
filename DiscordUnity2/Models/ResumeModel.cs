﻿using Newtonsoft.Json;

namespace DiscordUnity2.Models
{
    public class ResumeModel
    {
        public string Token { get; set; }
        [JsonProperty("session_id")]
        public string SessionId { get; set; }
        [JsonProperty("seq")]
        public int Sequence { get; set; }
    }
}
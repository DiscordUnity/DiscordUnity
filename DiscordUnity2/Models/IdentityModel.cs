using System.Collections.Generic;

namespace DiscordUnity2.Models
{
    public class IdentityModel
    {
        public string Token { get; set; }
        public Dictionary<string, string> Properties { get; set; }
    }
}

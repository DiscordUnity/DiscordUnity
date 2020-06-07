using System.Collections.Generic;

namespace DiscordUnity.Models
{
    internal class IdentityModel
    {
        public string Token { get; set; }
        public Dictionary<string, string> Properties { get; set; }
    }
}

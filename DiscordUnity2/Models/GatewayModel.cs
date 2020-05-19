namespace DiscordUnity2.Models
{
    public class GatewayModel
    {
        public string Url { get; set; }
        public int Shards { get; set; }
        public SessionStartLimit SessionStartLimit { get; set; }
    }

    public class SessionStartLimit
    {
        public int Total { get; set; }
        public int Remaining { get; set; }
        public int ResetAfter { get; set; }
    }
}

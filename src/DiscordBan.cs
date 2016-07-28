namespace DiscordUnity
{
    public class DiscordBan
    {
        public string reason;
        public DiscordUser user;

        internal DiscordBan(DiscordClient parent, DiscordUserBanJSON e)
        {
            reason = e.reason;
            user = new DiscordUser(parent, e.user);
        }
    }
}
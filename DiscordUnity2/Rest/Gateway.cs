using DiscordUnity2.Models;
using System.Threading.Tasks;

namespace DiscordUnity2.Rest
{
    public static partial class DiscordRest
    {
        public static Task<RestResult<GatewayModel>> GetBotGateway()
            => Get<GatewayModel>("/gateway/bot");
    }
}

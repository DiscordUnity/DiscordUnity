using DiscordUnity2.Models;
using System.Threading.Tasks;

namespace DiscordUnity2
{
    public static partial class DiscordAPI
    {
        internal static Task<RestResult<GatewayModel>> GetBotGateway()
            => Get<GatewayModel>("/gateway/bot");
    }
}

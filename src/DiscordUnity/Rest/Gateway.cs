using DiscordUnity.Models;
using System.Threading.Tasks;

namespace DiscordUnity
{
    public static partial class DiscordAPI
    {
        internal static Task<RestResult<GatewayModel>> GetBotGateway()
            => Get<GatewayModel>("/gateway/bot");
    }
}

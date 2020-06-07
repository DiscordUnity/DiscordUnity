using System.Threading.Tasks;

namespace DiscordUnity2
{
    public static partial class DiscordAPI
    {
        // TODO: Fill in Voice Region Object
        public static Task<RestResult<object[]>> ListVoiceRegions()
            => SyncInherit(Get<object[]>($"/voice/regions"), r => r);
    }
}

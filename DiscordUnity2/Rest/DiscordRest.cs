using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace DiscordUnity2
{
    public static partial class DiscordAPI
    {
        internal const string API = "https://discord.com/api";
        internal static HttpClient Client;

        private static async Task<RestResult<T>> Get<T>(string endpoint)
        {
            try
            {
                return RestResult<T>.FromResult(JsonConvert.DeserializeObject<T>(await Client.GetStringAsync(API + endpoint)));
            }

            catch (Exception e)
            {
                return RestResult<T>.FromException(e);
            }
        }
    }

    public class RestResult<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
        public Exception Exception { get; set; }

        private RestResult() { }

        internal static RestResult<T> FromResult(T data) => new RestResult<T> { Success = true, Data = data };
        internal static RestResult<T> FromException(Exception exception) => new RestResult<T> { Success = false, Exception = exception };

        public static implicit operator bool(RestResult<T> restResult) => restResult.Success;
    }
}

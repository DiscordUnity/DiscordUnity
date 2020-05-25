using Newtonsoft.Json;
using System;
using System.Web;
using System.Text;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace DiscordUnity2
{
    public static partial class DiscordAPI
    {
        internal const string API = "https://discord.com/api";
        internal static HttpClient Client;

        private static async Task<RestResult<T>> Http<T>(HttpMethod method, string endpoint, object obj = null, object query = null)
        {
            try
            {
                string q = null;

                if (query != null)
                {
                    var properties = from p in query.GetType().GetProperties()
                                     where p.GetValue(query, null) != null
                                     select p.Name + "=" + HttpUtility.UrlEncode(p.GetValue(query, null).ToString());

                    q = string.Join("&", properties.ToArray());
                }

                var request = new HttpRequestMessage(method, API + endpoint + (string.IsNullOrEmpty(q) ? "" : "?" + q));
                if (obj != null) request.Content = new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");
                var result = await Client.SendAsync(request);
                if (!result.IsSuccessStatusCode) throw new Exception(result.ReasonPhrase);
                return RestResult<T>.FromResult(JsonConvert.DeserializeObject<T>(await result.Content.ReadAsStringAsync()));
            }

            catch (Exception e)
            {
                return RestResult<T>.FromException(e);
            }
        }

        private static Task<RestResult<T>> Get<T>(string endpoint, object query = null) => Http<T>(HttpMethod.Get, endpoint, null, query);
        private static Task<RestResult<T>> Patch<T>(string endpoint, object obj, object query = null) => Http<T>(new HttpMethod("PATCH"), endpoint, obj, query);
        private static Task<RestResult<T>> Post<T>(string endpoint, object obj, object query = null) => Http<T>(HttpMethod.Post, endpoint, obj, query);
        private static Task<RestResult<T>> Delete<T>(string endpoint, object query = null) => Http<T>(HttpMethod.Delete, endpoint, null, query);
        
        private static async Task<RestResult<R>> SyncInherit<T, R>(Task<RestResult<T>> call, Func<T, R> transform)
        {
            var task = new TaskCompletionSource<RestResult<R>>();
            var result = await call;

            if (result) Sync(() => task.SetResult(RestResult<R>.FromResult(transform(result.Data))));
            else Sync(() => task.SetResult(RestResult<R>.FromException(result.Exception)));

            return await task.Task;
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

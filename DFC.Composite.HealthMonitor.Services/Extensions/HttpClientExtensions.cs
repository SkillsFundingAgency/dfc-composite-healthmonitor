using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;

namespace DFC.Composite.HealthMonitor.Services.Extensions
{
    public static class HttpClientExtensions
    {
        public static Task<HttpResponseMessage> PatchAsJsonAsync<T>(this HttpClient client, string requestUri, T value)
        {
            var content = new ObjectContent<T>(value, new JsonMediaTypeFormatter());
            using var request = new HttpRequestMessage(HttpMethod.Patch, requestUri) { Content = content };
            return client?.SendAsync(request);
        }
    }
}
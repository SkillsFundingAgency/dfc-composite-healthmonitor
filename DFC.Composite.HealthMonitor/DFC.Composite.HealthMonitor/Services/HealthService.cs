using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace DFC.Composite.HealthMonitor.Services
{
    public class HealthService : IHealthService
    {
        public async Task<bool> IsHealthy(string url)
        {
            var result = false;

            using (var httpClient = new HttpClient())
            {
                try
                {
                    var response = await httpClient.GetAsync(url);
                    result = response != null && response.StatusCode == HttpStatusCode.OK;
                }
                catch { }
            }

            return result;
        }
    }
}

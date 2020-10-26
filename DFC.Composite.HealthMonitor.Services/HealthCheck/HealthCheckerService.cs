using DFC.Composite.HealthMonitor.Data.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace DFC.Composite.HealthMonitor.Services.HealthCheck
{
    public class HealthCheckerService : IHealthCheckerService
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly ILogger<HealthCheckerService> logger;

        public HealthCheckerService(IHttpClientFactory httpClientFactory, ILogger<HealthCheckerService> logger)
        {
            this.httpClientFactory = httpClientFactory;
            this.logger = logger;
        }

        public async Task<bool> IsHealthy(Uri url, bool treatNotFoundAsSuccessCode, string mediaTypeName)
        {
            try
            {
                var client = httpClientFactory.CreateClient(HttpClientName.Health);
                client.DefaultRequestHeaders.Add(HeaderNames.Accept, mediaTypeName);

                var response = await client.GetAsync(url).ConfigureAwait(false);
                return ValidateResponse(url, response, treatNotFoundAsSuccessCode);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error from Health checking url '{url}'");
                return false;
            }
        }

        private bool ValidateResponse(Uri url, HttpResponseMessage response, bool treatNotFoundAsSuccessCode)
        {
            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound && treatNotFoundAsSuccessCode)
            {
                logger.LogWarning($"Response from Health checking url '{url}' returned {response?.StatusCode.ToString()}, but for this region is treated as a success code");
                return true;
            }

            logger.LogWarning($"Response from Health checking url '{url}' returned unsuccessfully. Status code received: {response?.StatusCode.ToString()}");

            return false;
        }
    }
}
using DFC.Composite.HealthMonitor.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Threading.Tasks;

namespace DFC.Composite.HealthMonitor.Services.HealthCheck
{
    public class HealthChecker : IHealthChecker
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<HealthChecker> _logger;

        public HealthChecker(IHttpClientFactory httpClientFactory, ILogger<HealthChecker> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<bool> IsHealthy(string url)
        {
            var result = false;
            try
            {
                var client = _httpClientFactory.CreateClient(HttpClientName.Health);
                client.DefaultRequestHeaders.Add(HeaderNames.Accept, MediaTypeNames.Text.Html);
                var response = await client.GetAsync(url);
                result = response != null && response.StatusCode == HttpStatusCode.OK;
                if (!result)
                {
                    _logger.LogInformation($"Response from {url} was {result.ToString()}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error from {url}");
                throw;
            }

            return result;
        }
    }
}

using DFC.Composite.HealthMonitor.Data.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
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

        public async Task<bool> IsHealthy(Uri url)
        {
            try
            {
                var client = httpClientFactory.CreateClient(HttpClientName.Health);
                client.DefaultRequestHeaders.Add(HeaderNames.Accept, MediaTypeNames.Text.Html);

                var response = await client.GetAsync(url).ConfigureAwait(false);
                return ValidateResponse(url, response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error from Health url '{url}'");
                throw;
            }
        }

        private bool ValidateResponse(Uri url, HttpResponseMessage response)
        {
            var okResult = response?.StatusCode == HttpStatusCode.OK;
            if (!okResult)
            {
                logger.LogInformation($"Response from Health url '{url}' returned unsuccessfully. Status code received: {response?.StatusCode.ToString()}");
            }

            return okResult;
        }
    }
}
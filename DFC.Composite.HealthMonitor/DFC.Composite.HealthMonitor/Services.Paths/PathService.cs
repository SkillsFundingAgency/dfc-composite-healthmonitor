using DFC.Composite.HealthMonitor.Common;
using DFC.Composite.HealthMonitor.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace DFC.Composite.HealthMonitor.Services.Paths
{
    public class PathService : IPathService
    {
        private readonly IHttpClientFactory _httpClientFactory;
   
        public PathService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IEnumerable<PathModel>> GetPaths()
        {
            var response = await _httpClientFactory.CreateClient(HttpClientName.Paths).GetAsync("/api/paths");
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsAsync<IEnumerable<PathModel>>();
            return result;
        }
    }
}

using DFC.Composite.HealthMonitor.Data.Common;
using DFC.Composite.HealthMonitor.Data.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace DFC.Composite.HealthMonitor.Services.Paths
{
    public class PathService : IPathService
    {
        private readonly IHttpClientFactory httpClientFactory;

        public PathService(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }

        public async Task<IEnumerable<PathModel>> GetPaths()
        {
            var response = await httpClientFactory.CreateClient(HttpClientName.Paths).GetAsync(new Uri("api/paths", UriKind.Relative)).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsAsync<IEnumerable<PathModel>>().ConfigureAwait(false);
        }
    }
}
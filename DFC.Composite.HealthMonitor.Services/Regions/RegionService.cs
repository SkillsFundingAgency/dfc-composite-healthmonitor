using DFC.Composite.HealthMonitor.Data.Common;
using DFC.Composite.HealthMonitor.Data.Enums;
using DFC.Composite.HealthMonitor.Data.Models;
using DFC.Composite.HealthMonitor.Services.Extensions;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace DFC.Composite.HealthMonitor.Services.Regions
{
    public class RegionService : IRegionService
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly ILogger<RegionService> logger;

        public RegionService(IHttpClientFactory httpClientFactory, ILogger<RegionService> logger)
        {
            this.httpClientFactory = httpClientFactory;
            this.logger = logger;
        }

        public async Task<IEnumerable<RegionModel>> GetRegions(string path)
        {
            var response = await httpClientFactory.CreateClient(HttpClientName.Regions).GetAsync(new Uri($"api/paths/{path}/regions", UriKind.Relative)).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsAsync<IEnumerable<RegionModel>>().ConfigureAwait(false);
        }

        public async Task<bool> MarkAsHealthy(string path, PageRegion pageRegion)
        {
            var url = $"api/paths/{path}/regions/{(int)pageRegion}";

            try
            {
                var model = new JsonPatchDocument<RegionPatch>().Add(x => x.IsHealthy, true);
                var response = await httpClientFactory.CreateClient(HttpClientName.Regions).PatchAsJsonAsync(url, model).ConfigureAwait(false);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error from Health checking url '{url}'");
                return false;
            }
        }
    }
}
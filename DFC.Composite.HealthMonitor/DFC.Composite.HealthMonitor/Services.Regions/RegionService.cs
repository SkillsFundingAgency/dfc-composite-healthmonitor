using DFC.Composite.HealthMonitor.Common;
using DFC.Composite.HealthMonitor.Extensions;
using DFC.Composite.HealthMonitor.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using static DFC.Composite.HealthMonitor.Models.RegionUnhealthyModel;

namespace DFC.Composite.HealthMonitor.Services.Regions
{
    public class RegionService : IRegionService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public RegionService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IEnumerable<RegionModel>> GetRegions(string path)
        {
            var response = await _httpClientFactory.CreateClient(HttpClientName.Regions).GetAsync($"/api/paths/{path}/regions");
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsAsync<IEnumerable<RegionModel>>();
            return result;
        }

        public async Task MarkAsHealthy(string path, PageRegion pageRegion)
        {
            var model = new RegionPatch() { IsHealthy = true };
            var url = $"/api/paths/{path}/regions/{(int)pageRegion}";

            var response = await _httpClientFactory.CreateClient(HttpClientName.Regions).PatchAsJsonAsync(url, model);
            response.EnsureSuccessStatusCode();
        }
    }
}

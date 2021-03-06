﻿using DFC.Composite.HealthMonitor.Data.Common;
using DFC.Composite.HealthMonitor.Data.Enums;
using DFC.Composite.HealthMonitor.Data.Models;
using DFC.Composite.HealthMonitor.Services.Extensions;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace DFC.Composite.HealthMonitor.Services.AppRegistry
{
    public class AppRegistryService : IAppRegistryService
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly ILogger<AppRegistryService> logger;

        public AppRegistryService(IHttpClientFactory httpClientFactory, ILogger<AppRegistryService> logger)
        {
            this.httpClientFactory = httpClientFactory;
            this.logger = logger;
        }

        public async Task<IEnumerable<AppRegistryModel>> GetPathsAndRegions()
        {
            var response = await httpClientFactory.CreateClient(HttpClientName.AppRegistry).GetAsync(new Uri("appregistry", UriKind.Relative)).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsAsync<IEnumerable<AppRegistryModel>>().ConfigureAwait(false);
        }

        public async Task<bool> MarkRegionAsHealthy(string path, PageRegion pageRegion)
        {
            var url = $"appregistry/{path}/regions/{(int)pageRegion}";

            try
            {
                var model = new JsonPatchDocument<PatchModel>().Add(x => x.IsHealthy, true);
                var response = await httpClientFactory.CreateClient(HttpClientName.AppRegistry).PatchAsJsonAsync(url, model).ConfigureAwait(false);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error from Health checking url '{url}'");
                return false;
            }
        }

        public async Task<bool> MarkAjaxRequestAsHealthy(string path, string name)
        {
            var url = $"appregistry/{path}/ajaxrequests/{name}";

            try
            {
                var model = new JsonPatchDocument<PatchModel>().Add(x => x.IsHealthy, true);
                var response = await httpClientFactory.CreateClient(HttpClientName.AppRegistry).PatchAsJsonAsync(url, model).ConfigureAwait(false);
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

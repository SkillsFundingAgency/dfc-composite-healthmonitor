using DFC.Composite.HealthMonitor.Data.Models;
using DFC.Composite.HealthMonitor.Services.AppRegistry;
using DFC.Composite.HealthMonitor.Services.Extensions;
using DFC.Composite.HealthMonitor.Services.HealthCheck;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;

namespace DFC.Composite.HealthMonitor.Services.HealthMonitoring
{
    public class HealthMonitoringProcessor : IHealthMonitoringProcessor
    {
        private readonly IHealthCheckerService healthCheckerService;
        private readonly ILogger<HealthMonitoringProcessor> logger;
        private readonly IAppRegistryService appRegistryService;

        public HealthMonitoringProcessor(
            IAppRegistryService appRegistryService,
            IHealthCheckerService healthCheckerService,
            ILogger<HealthMonitoringProcessor> logger)
        {
            this.appRegistryService = appRegistryService;
            this.healthCheckerService = healthCheckerService;
            this.logger = logger;
        }

        public async Task Process()
        {
            var appRegistry = await appRegistryService.GetPathsAndRegions().ConfigureAwait(false);

            foreach (var path in appRegistry.Where(p => string.IsNullOrWhiteSpace(p.ExternalURL?.ToString())))
            {
                if (path.Regions != null)
                {
                    await ProcessRegions(path.Path, path.Regions.Where(r => r.RequiresHealthCheck())).ConfigureAwait(false);
                }

                if (path.AjaxRequests != null)
                {
                    await ProcessAjaxRequests(path.Path, path.AjaxRequests.Where(a => a.RequiresHealthCheck())).ConfigureAwait(false);
                }
            }
        }

        private async Task ProcessRegions(string path, IEnumerable<RegionModel> regions)
        {
            foreach (var region in regions)
            {
                var regionHealthEndpoint = new Uri(region.RegionEndpoint.Replace("{0}", $"{{nameof(HealthMonitoringProcessor)}.{nameof(ProcessRegions)}.{Guid.NewGuid().ToString()}}", StringComparison.OrdinalIgnoreCase));

                var isHealthy = await healthCheckerService.IsHealthy(regionHealthEndpoint, region.PageRegion == Data.Enums.PageRegion.Body, MediaTypeNames.Text.Html).ConfigureAwait(false);
                if (isHealthy)
                {
                    logger.LogInformation($"Starting to mark {regionHealthEndpoint} as healthy");
                    await appRegistryService.MarkRegionAsHealthy(path, region.PageRegion).ConfigureAwait(false);
                    logger.LogInformation($"Completed marking {regionHealthEndpoint} as healthy");
                }
            }
        }

        private async Task ProcessAjaxRequests(string path, IEnumerable<AjaxRequestModel> ajaxRequests)
        {
            foreach (var ajaxRequest in ajaxRequests)
            {
                var ajaxRequestHealthEndpoint = new Uri(ajaxRequest.AjaxEndpoint.Replace("{0}", $"{{}}", StringComparison.OrdinalIgnoreCase));

                var isHealthy = await healthCheckerService.IsHealthy(ajaxRequestHealthEndpoint, true, MediaTypeNames.Application.Json).ConfigureAwait(false);
                if (isHealthy)
                {
                    logger.LogInformation($"Starting to mark {ajaxRequestHealthEndpoint} as healthy");
                    await appRegistryService.MarkAjaxRequestAsHealthy(path, ajaxRequest.Name).ConfigureAwait(false);
                    logger.LogInformation($"Completed marking {ajaxRequestHealthEndpoint} as healthy");
                }
            }
        }
    }
}
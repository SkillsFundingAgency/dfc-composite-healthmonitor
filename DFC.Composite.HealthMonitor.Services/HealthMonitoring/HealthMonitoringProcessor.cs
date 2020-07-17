using DFC.Composite.HealthMonitor.Services.AppRegistry;
using DFC.Composite.HealthMonitor.Services.Extensions;
using DFC.Composite.HealthMonitor.Services.HealthCheck;
using DFC.Composite.HealthMonitor.Services.Paths;
using DFC.Composite.HealthMonitor.Services.Regions;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DFC.Composite.HealthMonitor.Services.HealthMonitoring
{
    public class HealthMonitoringProcessor : IHealthMonitoringProcessor
    {
        private readonly IPathService pathService; // TODO - REMOVE
        private readonly IRegionService regionService; // TODO - REMOVE
        private readonly IHealthCheckerService healthCheckerService;
        private readonly ILogger<HealthMonitoringProcessor> logger;
        private readonly IAppRegistryService appRegistryService;

        public HealthMonitoringProcessor(
            IPathService pathService, // TODO - REMOVE
            IRegionService regionService, // TODO - REMOVE
            IAppRegistryService appRegistryService,
            IHealthCheckerService healthCheckerService,
            ILogger<HealthMonitoringProcessor> logger)
        {
            this.pathService = pathService; // TODO - REMOVE
            this.regionService = regionService; // TODO - REMOVE
            this.appRegistryService = appRegistryService;
            this.healthCheckerService = healthCheckerService;
            this.logger = logger;
        }

        public async Task Process()
        {
            var appRegistry = await appRegistryService.GetPathsAndRegions().ConfigureAwait(false);

            foreach (var path in appRegistry.Where(p => string.IsNullOrWhiteSpace(p.ExternalURL?.ToString()))) {
                foreach (var region in path.Regions.Where(r => r.RequiresHealthCheck()))
                {
                    var regionHealthEndpoint = new Uri(region.RegionEndpoint.Replace("{0}", $"{nameof(HealthMonitoringProcessor)}.{nameof(Process)}.{Guid.NewGuid().ToString()}", StringComparison.OrdinalIgnoreCase));

                    var isHealthy = await healthCheckerService.IsHealthy(regionHealthEndpoint, region.PageRegion == Data.Enums.PageRegion.Body).ConfigureAwait(false);
                    if (isHealthy)
                    {
                        logger.LogInformation($"Starting to mark {regionHealthEndpoint} as healthy");
                        await appRegistryService.MarkAsHealthy(path.Path, region.PageRegion).ConfigureAwait(false);
                        logger.LogInformation($"Completed marking {regionHealthEndpoint} as healthy");
                    }
                }
            }
        }
    }
}
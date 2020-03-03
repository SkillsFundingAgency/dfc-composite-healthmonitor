using DFC.Composite.HealthMonitor.Data.Common;
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
        private readonly IPathService pathService;
        private readonly IRegionService regionService;
        private readonly IHealthCheckerService healthCheckerService;
        private readonly ILogger<HealthMonitoringProcessor> logger;

        public HealthMonitoringProcessor(
            IPathService pathService,
            IRegionService regionService,
            IHealthCheckerService healthCheckerService,
            ILogger<HealthMonitoringProcessor> logger)
        {
            this.pathService = pathService;
            this.regionService = regionService;
            this.healthCheckerService = healthCheckerService;
            this.logger = logger;
        }

        public async Task Process()
        {
            var paths = await pathService.GetPaths().ConfigureAwait(false);

            foreach (var path in paths.Where(p => string.IsNullOrWhiteSpace(p.ExternalURL?.ToString())))
            {
                var regions = await regionService.GetRegions(path.Path).ConfigureAwait(false);

                foreach (var region in regions.Where(r => r.RequiresHealthCheck()))
                {
                    var regionHealthEndpoint = GenerateHealthUri(region.RegionEndpoint);

                    var isHealthy = await healthCheckerService.IsHealthy(regionHealthEndpoint).ConfigureAwait(false);
                    if (isHealthy)
                    {
                        logger.LogInformation($"Starting to mark {regionHealthEndpoint} as healthy");
                        await regionService.MarkAsHealthy(region.Path, region.PageRegion).ConfigureAwait(false);
                        logger.LogInformation($"Completed marking {regionHealthEndpoint} as healthy");
                    }
                }
            }
        }

        private static Uri GenerateHealthUri(string regionEndpoint)
        {
            var endpointUri = new Uri(regionEndpoint);
            return new Uri($"{endpointUri.Scheme}://{endpointUri.Authority}/{UrlSegment.Health.TrimStart('/')}");
        }
    }
}
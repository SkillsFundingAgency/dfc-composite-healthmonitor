using DFC.Composite.HealthMonitor.Common;
using DFC.Composite.HealthMonitor.Models;
using DFC.Composite.HealthMonitor.Services.HealthCheck;
using DFC.Composite.HealthMonitor.Services.HealthMonitoringFilter;
using DFC.Composite.HealthMonitor.Services.Paths;
using DFC.Composite.HealthMonitor.Services.Regions;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.Composite.HealthMonitor.Services.HealthMonitoring
{
    public class HealthMonitoring : IHealthMonitoring
    {
        private readonly IPathService _pathService;
        private readonly IRegionService _regionService;
        private readonly IHealthMonitoringFilter _healthMonitoringFilter;
        private readonly IHealthChecker _healthChecker;
        private readonly ILogger<HealthMonitoring> _logger;

        public HealthMonitoring(
            IPathService pathService,
            IRegionService regionService,
            IHealthMonitoringFilter healthMonitoringFilter,
            IHealthChecker healthChecker,
            ILogger<HealthMonitoring> logger)
        {
            _pathService = pathService;
            _regionService = regionService;
            _healthMonitoringFilter = healthMonitoringFilter;
            _healthChecker = healthChecker;
            _logger = logger;
        }

        public async Task Monitor()
        {
            var allRegions = new List<RegionModel>();

            var paths = await _pathService.GetPaths();

            foreach (var path in paths)
            {
                var regions = await _regionService.GetRegions(path.Path);

                foreach (var region in regions)
                {
                    if (_healthMonitoringFilter.Filter(region))
                    {
                        string strippedPlaceholderEndpoint = region.RegionEndpoint.Replace("/{0}", string.Empty);
                        var regionHealthEndpoint = string.Concat(strippedPlaceholderEndpoint, UrlSegment.Health);
                        var isHealthy = await _healthChecker.IsHealthy(regionHealthEndpoint);

                        if (isHealthy)
                        {
                            _logger.LogInformation($"Starting to mark {regionHealthEndpoint} as healthy");

                            await _regionService.MarkAsHealthy(region.Path, region.PageRegion);

                            _logger.LogInformation($"Completed marking {regionHealthEndpoint} as healthy");
                        }
                    }
                }

            }

        }
    }
}

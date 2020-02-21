using DFC.Composite.HealthMonitor.Data.Enums;
using DFC.Composite.HealthMonitor.Data.Models;
using DFC.Composite.HealthMonitor.Services.HealthCheck;
using DFC.Composite.HealthMonitor.Services.HealthMonitoring;
using DFC.Composite.HealthMonitor.Services.Paths;
using DFC.Composite.HealthMonitor.Services.Regions;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Composite.HealthMonitor.Tests.ServiceTests
{
    public class HealthMonitoringProcessorTests
    {
        private readonly IPathService pathService;
        private readonly IRegionService regionService;
        private readonly IHealthCheckerService healthCheckerService;
        private readonly ILogger<HealthMonitoringProcessor> logger;
        private readonly IHealthMonitoringProcessor healthMonitoringProcessor;

        public HealthMonitoringProcessorTests()
        {
            pathService = A.Fake<IPathService>();
            regionService = A.Fake<IRegionService>();
            healthCheckerService = A.Fake<IHealthCheckerService>();
            logger = A.Fake<ILogger<HealthMonitoringProcessor>>();

            healthMonitoringProcessor = new HealthMonitoringProcessor(pathService, regionService, healthCheckerService, logger);
        }

        public static IEnumerable<object[]> HealthCheckRequiredChecks => new List<object[]>
        {
            new object[] { true, true, false },
            new object[] { true, false, false },
            new object[] { false, false, false },
            new object[] { false, true, true },
        };

        [Theory]
        [MemberData(nameof(HealthCheckRequiredChecks))]
        public async Task ProcessWhenUnhealthyEndpointThenMarkedAsHealthy(bool isHealthy, bool healthCheckRequired, bool markAsHealthyCalled)
        {
            // Arrange
            const string expectedRegionEndpoint = "https://expectedHost/regionEndpoint";
            var paths = new List<PathModel> { new PathModel { Path = "path1" } };
            var regions = new List<RegionModel>
            {
                new RegionModel
                {
                    Path = paths.First().Path,
                    RegionEndpoint = expectedRegionEndpoint,
                    HealthCheckRequired = healthCheckRequired,
                    IsHealthy = isHealthy,
                    PageRegion = PageRegion.Body,
                },
            };

            A.CallTo(() => pathService.GetPaths()).Returns(paths);
            A.CallTo(() => regionService.GetRegions(A<string>.Ignored)).Returns(regions);
            A.CallTo(() => healthCheckerService.IsHealthy(A<Uri>.Ignored)).Returns(true);

            // Act
            await healthMonitoringProcessor.Process().ConfigureAwait(false);

            // Assert
            if (markAsHealthyCalled)
            {
                var expectedRegionUri = new Uri($"https://expectedHost/health");
                A.CallTo(() => healthCheckerService.IsHealthy(expectedRegionUri)).MustHaveHappenedOnceExactly();
                A.CallTo(() => regionService.MarkAsHealthy(regions.First().Path, regions.First().PageRegion)).MustHaveHappenedOnceExactly();
            }
            else
            {
                A.CallTo(() => healthCheckerService.IsHealthy(A<Uri>.Ignored)).MustNotHaveHappened();
                A.CallTo(() => regionService.MarkAsHealthy(A<string>.Ignored, A<PageRegion>.Ignored)).MustNotHaveHappened();
            }
        }
    }
}
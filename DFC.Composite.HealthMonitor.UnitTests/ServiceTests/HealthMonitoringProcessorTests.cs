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

        public static IEnumerable<object[]> PathExternalUrls => new List<object[]>
        {
            new object[] { string.Empty, false },
            new object[] { "http://SomeExternalPathUrl", true },
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
            A.CallTo(() => healthCheckerService.IsHealthy(A<Uri>.Ignored, A<bool>.Ignored)).Returns(true);

            // Act
            await healthMonitoringProcessor.Process().ConfigureAwait(false);

            // Assert
            if (markAsHealthyCalled)
            {
                var expectedRegionUri = new Uri(expectedRegionEndpoint);
                A.CallTo(() => healthCheckerService.IsHealthy(expectedRegionUri, A<bool>.Ignored)).MustHaveHappenedOnceExactly();
                A.CallTo(() => regionService.MarkAsHealthy(regions.First().Path, regions.First().PageRegion)).MustHaveHappenedOnceExactly();
            }
            else
            {
                A.CallTo(() => healthCheckerService.IsHealthy(A<Uri>.Ignored, A<bool>.Ignored)).MustNotHaveHappened();
                A.CallTo(() => regionService.MarkAsHealthy(A<string>.Ignored, A<PageRegion>.Ignored)).MustNotHaveHappened();
            }
        }

        [Theory]
        [MemberData(nameof(PathExternalUrls))]
        public async Task ProcessWhenHasExternalUrlThenRegionsNotMarkedAsHealthy(string externalUrl, bool pathHasExternalUrl)
        {
            const string expectedRegionEndpoint = "https://expectedHost/regionEndpoint";
            var paths = new List<PathModel> { new PathModel { Path = "path1", ExternalURL = new Uri(externalUrl, UriKind.RelativeOrAbsolute) } };
            var regions = new List<RegionModel>
            {
                new RegionModel
                {
                    Path = paths.First().Path,
                    RegionEndpoint = expectedRegionEndpoint,
                    HealthCheckRequired = true,
                    IsHealthy = false,
                    PageRegion = PageRegion.Body,
                },
            };

            A.CallTo(() => pathService.GetPaths()).Returns(paths);
            A.CallTo(() => regionService.GetRegions(A<string>.Ignored)).Returns(regions);
            A.CallTo(() => healthCheckerService.IsHealthy(A<Uri>.Ignored, A<bool>.Ignored)).Returns(true);

            // Act
            await healthMonitoringProcessor.Process().ConfigureAwait(false);

            // Assert
            if (pathHasExternalUrl)
            {
                A.CallTo(() => regionService.GetRegions(A<string>.Ignored)).MustNotHaveHappened();
                A.CallTo(() => regionService.MarkAsHealthy(A<string>.Ignored, A<PageRegion>.Ignored)).MustNotHaveHappened();
                A.CallTo(() => healthCheckerService.IsHealthy(A<Uri>.Ignored, A<bool>.Ignored)).MustNotHaveHappened();
            }
            else
            {
                var expectedRegionUri = new Uri(expectedRegionEndpoint);
                A.CallTo(() => healthCheckerService.IsHealthy(expectedRegionUri, A<bool>.Ignored)).MustHaveHappenedOnceExactly();
                A.CallTo(() => regionService.MarkAsHealthy(regions.First().Path, regions.First().PageRegion)).MustHaveHappenedOnceExactly();
            }
        }
    }
}
using DFC.Composite.HealthMonitor.Data.Enums;
using DFC.Composite.HealthMonitor.Data.Models;
using DFC.Composite.HealthMonitor.Services.AppRegistry;
using DFC.Composite.HealthMonitor.Services.HealthCheck;
using DFC.Composite.HealthMonitor.Services.HealthMonitoring;
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
        private readonly IAppRegistryService appRegistryService;
        private readonly IHealthCheckerService healthCheckerService;
        private readonly ILogger<HealthMonitoringProcessor> logger;
        private readonly IHealthMonitoringProcessor healthMonitoringProcessor;

        public HealthMonitoringProcessorTests()
        {
            appRegistryService = A.Fake<IAppRegistryService>();
            healthCheckerService = A.Fake<IHealthCheckerService>();
            logger = A.Fake<ILogger<HealthMonitoringProcessor>>();

            healthMonitoringProcessor = new HealthMonitoringProcessor(appRegistryService, healthCheckerService, logger);
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

            var listOfPaths = new List<AppRegistryModel>
            {
                new AppRegistryModel
                {
                    Path = "Path1",
                    Regions = new List<RegionModel>()
                    {
                        new RegionModel()
                        {
                            RegionEndpoint = expectedRegionEndpoint,
                            HealthCheckRequired = healthCheckRequired,
                            IsHealthy = isHealthy,
                            PageRegion = PageRegion.Body,
                        },
                        new RegionModel
                        {
                            RegionEndpoint = expectedRegionEndpoint,
                            HealthCheckRequired = healthCheckRequired,
                            IsHealthy = isHealthy,
                            PageRegion = PageRegion.Body,
                        },
                    },
                    AjaxRequests = new List<AjaxRequestModel>()
                    {
                        new AjaxRequestModel()
                        {
                            AjaxEndpoint = expectedRegionEndpoint,
                            HealthCheckRequired = healthCheckRequired,
                            IsHealthy = isHealthy,
                            Name = "ajax-1",
                        },
                        new AjaxRequestModel
                        {
                            AjaxEndpoint = expectedRegionEndpoint,
                            HealthCheckRequired = healthCheckRequired,
                            IsHealthy = isHealthy,
                            Name = "ajax-2",
                        },
                    },
                },
            };

            A.CallTo(() => appRegistryService.GetPathsAndRegions()).Returns(listOfPaths);
            A.CallTo(() => healthCheckerService.IsHealthy(A<Uri>.Ignored, A<string>.Ignored)).Returns(true);

            // Act
            await healthMonitoringProcessor.Process().ConfigureAwait(false);

            // Assert
            if (markAsHealthyCalled)
            {
                var expectedRegionUri = new Uri(expectedRegionEndpoint);
                var firstItem = listOfPaths.First();
                var pageRegion = firstItem.Regions.First();
                A.CallTo(() => healthCheckerService.IsHealthy(expectedRegionUri, A<string>.Ignored)).MustHaveHappened();
                A.CallTo(() => appRegistryService.MarkRegionAsHealthy(firstItem.Path, pageRegion.PageRegion)).MustHaveHappened();
            }
            else
            {
                A.CallTo(() => healthCheckerService.IsHealthy(A<Uri>.Ignored, A<string>.Ignored)).MustNotHaveHappened();
                A.CallTo(() => appRegistryService.MarkRegionAsHealthy(A<string>.Ignored, A<PageRegion>.Ignored)).MustNotHaveHappened();
            }
        }

        [Theory]
        [MemberData(nameof(PathExternalUrls))]
        public async Task ProcessWhenHasExternalUrlThenRegionsNotMarkedAsHealthy(string externalUrl, bool pathHasExternalUrl)
        {
            // Arrange
            const string expectedRegionEndpoint = "https://expectedHost/regionEndpoint";
            var listOfPaths = new List<AppRegistryModel>
            {
                new AppRegistryModel
                {
                    Path = "Path1",
                    ExternalURL = new Uri(externalUrl, UriKind.RelativeOrAbsolute),
                    Regions = new List<RegionModel>()
                    {
                        new RegionModel()
                        {
                            RegionEndpoint = expectedRegionEndpoint,
                            HealthCheckRequired = true,
                            IsHealthy = false,
                            PageRegion = PageRegion.Body,
                        },
                    },
                    AjaxRequests = new List<AjaxRequestModel>()
                    {
                        new AjaxRequestModel()
                        {
                            AjaxEndpoint = expectedRegionEndpoint,
                            HealthCheckRequired = true,
                            IsHealthy = false,
                            Name = "ajax-1",
                        },
                    },
                },
            };

            A.CallTo(() => appRegistryService.GetPathsAndRegions()).Returns(listOfPaths);
            A.CallTo(() => healthCheckerService.IsHealthy(A<Uri>.Ignored, A<string>.Ignored)).Returns(true);

            // Act
            await healthMonitoringProcessor.Process().ConfigureAwait(false);

            // Assert
            if (pathHasExternalUrl)
            {
                A.CallTo(() => appRegistryService.MarkRegionAsHealthy(A<string>.Ignored, A<PageRegion>.Ignored)).MustNotHaveHappened();
                A.CallTo(() => healthCheckerService.IsHealthy(A<Uri>.Ignored, A<string>.Ignored)).MustNotHaveHappened();
            }
            else
            {
                var expectedRegionUri = new Uri(expectedRegionEndpoint);
                var firstItem = listOfPaths.First();
                var pageRegion = firstItem.Regions.First();
                var ajaxRequest = firstItem.AjaxRequests.First();
                A.CallTo(() => healthCheckerService.IsHealthy(expectedRegionUri, A<string>.Ignored)).MustHaveHappenedTwiceExactly();
                A.CallTo(() => appRegistryService.MarkRegionAsHealthy(firstItem.Path, pageRegion.PageRegion)).MustHaveHappenedOnceExactly();
                A.CallTo(() => appRegistryService.MarkAjaxRequestAsHealthy(firstItem.Path, ajaxRequest.Name)).MustHaveHappenedOnceExactly();
            }
        }
    }
}
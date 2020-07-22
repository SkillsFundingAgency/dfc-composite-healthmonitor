﻿using DFC.Composite.HealthMonitor.Data.Enums;
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
                    Regions = new List<RegionModel>() {
                        new RegionModel() {
                            Path = "find-a-course",
                            RegionEndpoint = expectedRegionEndpoint,
                            HealthCheckRequired = healthCheckRequired,
                            IsHealthy = isHealthy,
                            PageRegion = PageRegion.Body,
                        },
                        new RegionModel
                        {
                            Path = "find-a-course",
                            RegionEndpoint = expectedRegionEndpoint,
                            HealthCheckRequired = healthCheckRequired,
                            IsHealthy = isHealthy,
                            PageRegion = PageRegion.Body,
                        },
                    },
                },
            };

            A.CallTo(() => appRegistryService.GetPathsAndRegions()).Returns(listOfPaths);
            A.CallTo(() => healthCheckerService.IsHealthy(A<Uri>.Ignored, A<bool>.Ignored)).Returns(true);

            // Act
            await healthMonitoringProcessor.Process().ConfigureAwait(false);

            // Assert
            if (markAsHealthyCalled)
            {
                var expectedRegionUri = new Uri(expectedRegionEndpoint);
                var firstItem = listOfPaths.First();
                var pageRegion = firstItem.Regions.First();
                A.CallTo(() => healthCheckerService.IsHealthy(expectedRegionUri, A<bool>.Ignored)).MustHaveHappened();
                A.CallTo(() => appRegistryService.MarkAsHealthy(firstItem.Path, pageRegion.PageRegion)).MustHaveHappened();
            }
            else
            {
                A.CallTo(() => healthCheckerService.IsHealthy(A<Uri>.Ignored, A<bool>.Ignored)).MustNotHaveHappened();
                A.CallTo(() => appRegistryService.MarkAsHealthy(A<string>.Ignored, A<PageRegion>.Ignored)).MustNotHaveHappened();
            }
        }

        [Theory]
        [MemberData(nameof(PathExternalUrls))]
        public async Task ProcessWhenHasExternalUrlThenRegionsNotMarkedAsHealthy(string externalUrl, bool pathHasExternalUrl)
        {
            const string expectedRegionEndpoint = "https://expectedHost/regionEndpoint";
            // Arrange
            var listOfPaths = new List<AppRegistryModel>
            {
                new AppRegistryModel
                {
                    Path = "Path1",
                    ExternalURL = new Uri(externalUrl, UriKind.RelativeOrAbsolute),
                    Regions = new List<RegionModel>() {
                        new RegionModel() {
                            Path = "find-a-course",
                            RegionEndpoint = expectedRegionEndpoint,
                            HealthCheckRequired = true,
                            IsHealthy = false,
                            PageRegion = PageRegion.Body,
                        },
                    },
                },
            };

            A.CallTo(() => appRegistryService.GetPathsAndRegions()).Returns(listOfPaths);
            A.CallTo(() => healthCheckerService.IsHealthy(A<Uri>.Ignored, A<bool>.Ignored)).Returns(true);

            // Act
            await healthMonitoringProcessor.Process().ConfigureAwait(false);

            // Assert
            if (pathHasExternalUrl)
            {
                A.CallTo(() => appRegistryService.MarkAsHealthy(A<string>.Ignored, A<PageRegion>.Ignored)).MustNotHaveHappened();
                A.CallTo(() => healthCheckerService.IsHealthy(A<Uri>.Ignored, A<bool>.Ignored)).MustNotHaveHappened();
            }
            else
            {
                var expectedRegionUri = new Uri(expectedRegionEndpoint);
                var firstItem = listOfPaths.First();
                var pageRegion = firstItem.Regions.First();
                A.CallTo(() => healthCheckerService.IsHealthy(expectedRegionUri, A<bool>.Ignored)).MustHaveHappenedOnceExactly();
                A.CallTo(() => appRegistryService.MarkAsHealthy(firstItem.Path, pageRegion.PageRegion)).MustHaveHappenedOnceExactly();
            }
        }
    }
}
using DFC.Composite.HealthMonitor.Common;
using DFC.Composite.HealthMonitor.Models;
using DFC.Composite.HealthMonitor.Services.HealthCheck;
using DFC.Composite.HealthMonitor.Services.HealthMonitoring;
using DFC.Composite.HealthMonitor.Services.HealthMonitoringFilter;
using DFC.Composite.HealthMonitor.Services.Paths;
using DFC.Composite.HealthMonitor.Services.Regions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Composite.HealthMonitor.Tests.ServiceTests
{
    public class HealthMonitoringTests
    {
        private readonly Mock<IPathService> _pathService;
        private readonly Mock<IRegionService> _regionService;
        private readonly Mock<IHealthMonitoringFilter> _healthMonitoringFilter;
        private readonly Mock<IHealthChecker> _healthChecker;
        private readonly Mock<ILogger<HealthMonitoring>> _logger;
        private readonly IHealthMonitoring _healthMonitoring;

        public HealthMonitoringTests()
        {
            _pathService = new Mock<IPathService>();
            _regionService = new Mock<IRegionService>();
            _healthMonitoringFilter = new Mock<IHealthMonitoringFilter>();
            _healthChecker = new Mock<IHealthChecker>();
            _logger = new Mock<ILogger<HealthMonitoring>>();

            _healthMonitoring = new HealthMonitoring(_pathService.Object, _regionService.Object, _healthMonitoringFilter.Object, _healthChecker.Object, _logger.Object);
        }

        [Fact]
        public async Task Can_MarkUnHealthyEndPoints_AsHealthy()
        {
            //Arrange
            var path1 = new PathModel() { Path = "path1" };

            var region1a = new RegionModel() { Path = path1.Path, RegionEndpoint = "https://localhost/region1a", HeathCheckRequired = false, PageRegion = PageRegion.Body };
            var region1b = new RegionModel() { Path = path1.Path, RegionEndpoint = "https://localhost/region1b", IsHealthy = false, HeathCheckRequired = true, PageRegion = PageRegion.BodyFooter };
            var region1c = new RegionModel() { Path = path1.Path, RegionEndpoint = "https://localhost/region1c", IsHealthy = false, HeathCheckRequired = true, PageRegion = PageRegion.BodyTop };

            var paths = new List<PathModel>() { path1 };
            _pathService.Setup(x => x.GetPaths()).ReturnsAsync(paths);

            var regions = new List<RegionModel>() { region1a, region1b, region1c };
            _regionService.Setup(x => x.GetRegions(path1.Path)).ReturnsAsync(regions);

            _healthMonitoringFilter.Setup(x => x.Filter(region1a)).Returns(false);
            _healthMonitoringFilter.Setup(x => x.Filter(region1b)).Returns(true);
            _healthMonitoringFilter.Setup(x => x.Filter(region1c)).Returns(true);

            _healthChecker.Setup(x => x.IsHealthy(region1c.RegionEndpoint + UrlSegment.Health)).ReturnsAsync(true);

            //Act
            await _healthMonitoring.Monitor();

            //Assert
            _regionService.Verify(x => x.MarkAsHealthy(path1.Path, region1a.PageRegion), Times.Never());
            _regionService.Verify(x => x.MarkAsHealthy(path1.Path, region1b.PageRegion), Times.Never());
            _regionService.Verify(x => x.MarkAsHealthy(path1.Path, region1c.PageRegion), Times.Once());
        }

    }
}

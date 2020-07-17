using DFC.Composite.HealthMonitor.Data.Enums;
using DFC.Composite.HealthMonitor.Data.Models;
using DFC.Composite.HealthMonitor.Services.AppRegistry;
using DFC.Composite.HealthMonitor.Tests.FakeHttpHandlers;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Composite.HealthMonitor.UnitTests.ServiceTests
{
    public class AppRegistryServiceTests
    {
        [Fact]
        public async Task GetPathsAndRegionsReturnsListOfPaths()
        {
            var regionHead = new RegionModel() { IsHealthy = true, HealthCheckRequired = true, PageRegion = Data.Enums.PageRegion.Head };
            var regionBody = new RegionModel() { IsHealthy = true, HealthCheckRequired = true, PageRegion = Data.Enums.PageRegion.Body };

            // Arrange
            var listOfPaths = new List<AppRegistryModel>
            {
                new AppRegistryModel
                {
                    Path = "Path1",
                    Regions = new List<RegionModel>() { regionBody },
                },
                new AppRegistryModel
                {
                    Path = "Path2",
                    Regions = new List<RegionModel>() { regionHead},
                },
            };
            var content = new StringContent(JsonConvert.SerializeObject(listOfPaths), Encoding.Default, "application/json");
            var httpClientFactory = A.Fake<IHttpClientFactory>();
            var fakeILogger = A.Fake<ILogger<AppRegistryService>>();
            using var httpResponse = new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = content };
            var fakeHttpRequestSender = A.Fake<IFakeHttpRequestSender>();
            using var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);
            using var httpClient = new HttpClient(fakeHttpMessageHandler) { BaseAddress = new Uri("http://baseaddress.com") };
            A.CallTo(() => fakeHttpRequestSender.Send(A<HttpRequestMessage>.Ignored)).Returns(httpResponse);
            A.CallTo(() => httpClientFactory.CreateClient(A<string>.Ignored)).Returns(httpClient);

            var appRegistryService = new AppRegistryService(httpClientFactory, fakeILogger);

            // Act
            var result = await appRegistryService.GetPathsAndRegions().ConfigureAwait(false);

            // Assert
            Assert.True(result.ToList().Count == listOfPaths.Count);
        }

        [Fact]
        public async Task GetPathsAndRegionsThrowsExceptionWhenNotOkFromPathsApi()
        {
            // Arrange
            var httpClientFactory = A.Fake<IHttpClientFactory>();
            using var httpResponse = new HttpResponseMessage { StatusCode = HttpStatusCode.BadRequest };
            var fakeHttpRequestSender = A.Fake<IFakeHttpRequestSender>();
            var fakeILogger = A.Fake<ILogger<AppRegistryService>>();
            using var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);
            using var httpClient = new HttpClient(fakeHttpMessageHandler) { BaseAddress = new Uri("http://baseaddress.com") };
            A.CallTo(() => fakeHttpRequestSender.Send(A<HttpRequestMessage>.Ignored)).Returns(httpResponse);
            A.CallTo(() => httpClientFactory.CreateClient(A<string>.Ignored)).Returns(httpClient);

            var appRegistryService = new AppRegistryService(httpClientFactory, fakeILogger);

            // Assert
            await Assert.ThrowsAsync<HttpRequestException>(async () => await appRegistryService.GetPathsAndRegions().ConfigureAwait(false)).ConfigureAwait(false);
        }

        [Theory]
        [InlineData(true, HttpStatusCode.OK)]
        [InlineData(false, HttpStatusCode.BadRequest)]
        public async Task MarkAsHealthyReturnsAppropriateResponseWhenPatchMessageSent(bool success, HttpStatusCode statusCode)
        {
            // Arrange
            var httpClientFactory = A.Fake<IHttpClientFactory>();
            using var httpResponse = new HttpResponseMessage { StatusCode = statusCode };
            var fakeHttpRequestSender = A.Fake<IFakeHttpRequestSender>();
            var fakeILogger = A.Fake<ILogger<AppRegistryService>>();
            using var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);
            using var httpClient = new HttpClient(fakeHttpMessageHandler) { BaseAddress = new Uri("http://baseaddress.com") };

            A.CallTo(() => fakeHttpRequestSender.Send(A<HttpRequestMessage>.Ignored)).Returns(httpResponse);
            A.CallTo(() => httpClientFactory.CreateClient(A<string>.Ignored)).Returns(httpClient);

            var appRegistryService = new AppRegistryService(httpClientFactory, fakeILogger);

            // Act
            var response = await appRegistryService.MarkAsHealthy("path1", PageRegion.Body).ConfigureAwait(false);

            // Assert
            Assert.Equal(success, response);
        }

        [Fact]
        public async Task MarkAsHealthyCatchesExceoptionTest()
        {
            // Arrange
            var httpClientFactory = A.Fake<IHttpClientFactory>();
            var fakeILogger = A.Fake<ILogger<AppRegistryService>>();

            A.CallTo(() => httpClientFactory.CreateClient(A<string>.Ignored)).Throws<HttpRequestException>();

            var appRegistryService = new AppRegistryService(httpClientFactory, fakeILogger);

            // Act
            var response = await appRegistryService.MarkAsHealthy("path1", PageRegion.Body).ConfigureAwait(false);

            // Assert
            Assert.False(response);
        }
    }
}

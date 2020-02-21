using DFC.Composite.HealthMonitor.Data.Enums;
using DFC.Composite.HealthMonitor.Data.Models;
using DFC.Composite.HealthMonitor.Services.Regions;
using DFC.Composite.HealthMonitor.Tests.FakeHttpHandlers;
using FakeItEasy;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Composite.HealthMonitor.Tests.ServiceTests
{
    public class RegionServiceTests
    {
        [Fact]
        public async Task GetRegionsReturnsListOfRegions()
        {
            // Arrange
            var listOfRegions = new List<RegionModel>
            {
                new RegionModel
                {
                    DocumentId = Guid.NewGuid(),
                    PageRegion = PageRegion.Body,
                    RegionEndpoint = "region1endpoint",
                },
                new RegionModel
                {
                    DocumentId = Guid.NewGuid(),
                    PageRegion = PageRegion.BodyFooter,
                    RegionEndpoint = "region2endpoint",
                },
            };
            var content = new StringContent(JsonConvert.SerializeObject(listOfRegions), Encoding.Default, "application/json");
            var httpClientFactory = A.Fake<IHttpClientFactory>();
            using var httpResponse = new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = content };
            var fakeHttpRequestSender = A.Fake<IFakeHttpRequestSender>();
            using var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);
            using var httpClient = new HttpClient(fakeHttpMessageHandler) { BaseAddress = new Uri("http://baseaddress.com") };
            A.CallTo(() => fakeHttpRequestSender.Send(A<HttpRequestMessage>.Ignored)).Returns(httpResponse);
            A.CallTo(() => httpClientFactory.CreateClient(A<string>.Ignored)).Returns(httpClient);

            var regionService = new RegionService(httpClientFactory);

            // Act
            var result = await regionService.GetRegions("path1").ConfigureAwait(false);

            // Assert
            Assert.True(result.Count() == listOfRegions.Count);
        }

        [Fact]
        public async Task GetRegionsThrowsExceptionWhenNotOkFromRegionsApi()
        {
            // Arrange
            var httpClientFactory = A.Fake<IHttpClientFactory>();
            using var httpResponse = new HttpResponseMessage { StatusCode = HttpStatusCode.BadRequest };
            var fakeHttpRequestSender = A.Fake<IFakeHttpRequestSender>();
            using var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);
            using var httpClient = new HttpClient(fakeHttpMessageHandler) { BaseAddress = new Uri("http://baseaddress.com") };
            A.CallTo(() => fakeHttpRequestSender.Send(A<HttpRequestMessage>.Ignored)).Returns(httpResponse);
            A.CallTo(() => httpClientFactory.CreateClient(A<string>.Ignored)).Returns(httpClient);

            var regionService = new RegionService(httpClientFactory);

            // Assert
            await Assert.ThrowsAsync<HttpRequestException>(async () => await regionService.GetRegions("path1").ConfigureAwait(false)).ConfigureAwait(false);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task MarkAsHealthyReturnsAppropriateResponseWhenPatchMessageSent(bool success)
        {
            // Arrange
            var httpClientFactory = A.Fake<IHttpClientFactory>();
            using var httpResponse = new HttpResponseMessage { StatusCode = success ? HttpStatusCode.OK : HttpStatusCode.BadRequest };
            var fakeHttpRequestSender = A.Fake<IFakeHttpRequestSender>();
            using var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);
            using var httpClient = new HttpClient(fakeHttpMessageHandler) { BaseAddress = new Uri("http://baseaddress.com") };

            A.CallTo(() => fakeHttpRequestSender.Send(A<HttpRequestMessage>.Ignored)).Returns(httpResponse);
            A.CallTo(() => httpClientFactory.CreateClient(A<string>.Ignored)).Returns(httpClient);

            var regionService = new RegionService(httpClientFactory);

            // Act
            var response = await regionService.MarkAsHealthy("path1", PageRegion.Body).ConfigureAwait(false);

            // Assert
            Assert.Equal(success, response);
        }
    }
}
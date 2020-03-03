using DFC.Composite.HealthMonitor.Data.Models;
using DFC.Composite.HealthMonitor.Services.Paths;
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
    public class PathServiceTests
    {
        [Fact]
        public async Task GetPathsReturnsListOfPaths()
        {
            // Arrange
            var listOfPaths = new List<PathModel>
            {
                new PathModel
                {
                    Path = "Path1",
                },
                new PathModel
                {
                    Path = "Path2",
                },
            };
            var content = new StringContent(JsonConvert.SerializeObject(listOfPaths), Encoding.Default, "application/json");
            var httpClientFactory = A.Fake<IHttpClientFactory>();
            using var httpResponse = new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = content };
            var fakeHttpRequestSender = A.Fake<IFakeHttpRequestSender>();
            using var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);
            using var httpClient = new HttpClient(fakeHttpMessageHandler) { BaseAddress = new Uri("http://baseaddress.com") };
            A.CallTo(() => fakeHttpRequestSender.Send(A<HttpRequestMessage>.Ignored)).Returns(httpResponse);
            A.CallTo(() => httpClientFactory.CreateClient(A<string>.Ignored)).Returns(httpClient);

            var pathService = new PathService(httpClientFactory);

            // Act
            var result = await pathService.GetPaths().ConfigureAwait(false);

            // Assert
            Assert.True(result.Count() == listOfPaths.Count);
        }

        [Fact]
        public async Task GetPathsThrowsExceptionWhenNotOkFromPathsApi()
        {
            // Arrange
            var httpClientFactory = A.Fake<IHttpClientFactory>();
            using var httpResponse = new HttpResponseMessage { StatusCode = HttpStatusCode.BadRequest };
            var fakeHttpRequestSender = A.Fake<IFakeHttpRequestSender>();
            using var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);
            using var httpClient = new HttpClient(fakeHttpMessageHandler) { BaseAddress = new Uri("http://baseaddress.com") };
            A.CallTo(() => fakeHttpRequestSender.Send(A<HttpRequestMessage>.Ignored)).Returns(httpResponse);
            A.CallTo(() => httpClientFactory.CreateClient(A<string>.Ignored)).Returns(httpClient);

            var pathService = new PathService(httpClientFactory);

            // Assert
            await Assert.ThrowsAsync<HttpRequestException>(async () => await pathService.GetPaths().ConfigureAwait(false)).ConfigureAwait(false);
        }
    }
}
using DFC.Composite.HealthMonitor.Services.HealthCheck;
using DFC.Composite.HealthMonitor.Tests.FakeHttpHandlers;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Composite.HealthMonitor.Tests.ServiceTests
{
    public class HealthCheckerServiceTests
    {
        private readonly ILogger<HealthCheckerService> defaultLogger;

        public HealthCheckerServiceTests()
        {
            defaultLogger = A.Fake<ILogger<HealthCheckerService>>();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task IsHealthyReturnsTrueWhenOkResultFromChildApp(bool isHealthy)
        {
            // Arrange
            var httpClientFactory = A.Fake<IHttpClientFactory>();
            using var httpResponse = new HttpResponseMessage { StatusCode = isHealthy ? HttpStatusCode.OK : HttpStatusCode.BadRequest };
            var fakeHttpRequestSender = A.Fake<IFakeHttpRequestSender>();
            using var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);
            using var httpClient = new HttpClient(fakeHttpMessageHandler);
            A.CallTo(() => fakeHttpRequestSender.Send(A<HttpRequestMessage>.Ignored)).Returns(httpResponse);
            A.CallTo(() => httpClientFactory.CreateClient(A<string>.Ignored)).Returns(httpClient);
            var service = new HealthCheckerService(httpClientFactory, defaultLogger);

            // Act
            var result = await service.IsHealthy(new Uri("http://someRegionEndpoint")).ConfigureAwait(false);

            // Assert
            Assert.Equal(isHealthy, result);
        }

        [Fact]
        public async Task IsHealthyThrowsExceptionWhenExceptionThrownByChildApp()
        {
            // Arrange
            var httpClientFactory = A.Fake<IHttpClientFactory>();
            A.CallTo(() => httpClientFactory.CreateClient(A<string>.Ignored)).Throws<Exception>();
            var service = new HealthCheckerService(httpClientFactory, defaultLogger);

            // Assert
            await Assert.ThrowsAsync<Exception>(async () => await service.IsHealthy(new Uri("http://someRegionEndpoint")).ConfigureAwait(false)).ConfigureAwait(false);
        }
    }
}
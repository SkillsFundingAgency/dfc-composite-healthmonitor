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
        [InlineData(true, HttpStatusCode.OK, false)]
        [InlineData(true, HttpStatusCode.NoContent, false)]
        [InlineData(false, HttpStatusCode.NotFound, false)]
        [InlineData(true, HttpStatusCode.NotFound, true)]
        [InlineData(false, HttpStatusCode.BadRequest, false)]
        public async Task IsHealthyReturnsTrueWhenSuccessResultFromChildApp(bool isHealthy, HttpStatusCode statusCode, bool treatNotFoundAsSuccessCode)
        {
            // Arrange
            var httpClientFactory = A.Fake<IHttpClientFactory>();
            using var httpResponse = new HttpResponseMessage { StatusCode = statusCode };
            var fakeHttpRequestSender = A.Fake<IFakeHttpRequestSender>();
            using var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);
            using var httpClient = new HttpClient(fakeHttpMessageHandler);
            A.CallTo(() => fakeHttpRequestSender.Send(A<HttpRequestMessage>.Ignored)).Returns(httpResponse);
            A.CallTo(() => httpClientFactory.CreateClient(A<string>.Ignored)).Returns(httpClient);
            var service = new HealthCheckerService(httpClientFactory, defaultLogger);

            // Act
            var result = await service.IsHealthy(new Uri("http://someRegionEndpoint"), treatNotFoundAsSuccessCode).ConfigureAwait(false);

            // Assert
            Assert.Equal(isHealthy, result);
        }

        [Fact]
        public async Task IsHealthyCatchesExceptionWhenExceptionThrownByChildApp()
        {
            // Arrange
            var httpClientFactory = A.Fake<IHttpClientFactory>();
            A.CallTo(() => httpClientFactory.CreateClient(A<string>.Ignored)).Throws<Exception>();
            var service = new HealthCheckerService(httpClientFactory, defaultLogger);

            var result = await service.IsHealthy(new Uri("http://someRegionEndpoint"), false).ConfigureAwait(false);

            // Assert
            Assert.False(result);
        }
    }
}
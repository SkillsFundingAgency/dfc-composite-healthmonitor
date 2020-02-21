using System.Net.Http;

namespace DFC.Composite.HealthMonitor.Tests.FakeHttpHandlers
{
    public interface IFakeHttpRequestSender
    {
        HttpResponseMessage Send(HttpRequestMessage request);
    }
}
using System;
using System.Threading.Tasks;

namespace DFC.Composite.HealthMonitor.Services.HealthCheck
{
    public interface IHealthCheckerService
    {
        Task<bool> IsHealthy(Uri url, string mediaTypeName);
    }
}
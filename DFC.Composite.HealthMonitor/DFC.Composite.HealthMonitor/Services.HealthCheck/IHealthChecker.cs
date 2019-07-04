using System.Threading.Tasks;

namespace DFC.Composite.HealthMonitor.Services.HealthCheck
{
    public interface IHealthChecker
    {
        Task<bool> IsHealthy(string url);
    }
}

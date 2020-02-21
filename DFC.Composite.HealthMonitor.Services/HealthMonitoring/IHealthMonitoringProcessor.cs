using System.Threading.Tasks;

namespace DFC.Composite.HealthMonitor.Services.HealthMonitoring
{
    public interface IHealthMonitoringProcessor
    {
        Task Process();
    }
}
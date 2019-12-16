using System.Threading.Tasks;

namespace DFC.Composite.HealthMonitor.Services.HealthMonitoring
{
    public interface IHealthMonitoring
    {
        Task Monitor();
    }
}

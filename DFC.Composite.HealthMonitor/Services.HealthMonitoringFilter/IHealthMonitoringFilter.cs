using DFC.Composite.HealthMonitor.Models;

namespace DFC.Composite.HealthMonitor.Services.HealthMonitoringFilter
{
    public interface IHealthMonitoringFilter
    {
        bool Filter(RegionModel regionModel);

    }
}

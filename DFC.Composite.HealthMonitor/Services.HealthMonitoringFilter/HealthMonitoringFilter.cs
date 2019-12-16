using DFC.Composite.HealthMonitor.Models;

namespace DFC.Composite.HealthMonitor.Services.HealthMonitoringFilter
{
    public class HealthMonitoringFilter : IHealthMonitoringFilter
    {
        public bool Filter(RegionModel regionModel)
        {
            return !regionModel.IsHealthy && regionModel.HeathCheckRequired;
        }
    }
}

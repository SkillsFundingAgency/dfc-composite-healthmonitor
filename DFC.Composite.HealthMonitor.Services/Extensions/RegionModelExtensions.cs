using DFC.Composite.HealthMonitor.Data.Models;

namespace DFC.Composite.HealthMonitor.Services.Extensions
{
    public static class RegionModelExtensions
    {
        public static bool RequiresHealthCheck(this RegionModel regionModel)
        {
            return !(regionModel?.IsHealthy).GetValueOrDefault() && (regionModel?.HealthCheckRequired).GetValueOrDefault();
        }
    }
}
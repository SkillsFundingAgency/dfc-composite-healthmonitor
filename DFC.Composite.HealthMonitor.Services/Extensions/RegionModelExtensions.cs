using DFC.Composite.HealthMonitor.Data.Models;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Composite.HealthMonitor.Services.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class RegionModelExtensions
    {
        public static bool RequiresHealthCheck(this RegionModel regionModel)
        {
            return !(regionModel?.IsHealthy).GetValueOrDefault() && (regionModel?.HealthCheckRequired).GetValueOrDefault();
        }
    }
}
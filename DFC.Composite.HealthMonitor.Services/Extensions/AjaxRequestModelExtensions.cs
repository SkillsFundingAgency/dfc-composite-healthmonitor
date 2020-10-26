using DFC.Composite.HealthMonitor.Data.Models;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Composite.HealthMonitor.Services.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class AjaxRequestModelExtensions
    {
        public static bool RequiresHealthCheck(this AjaxRequestModel ajaxRequestModel)
        {
            return !(ajaxRequestModel?.IsHealthy).GetValueOrDefault() && (ajaxRequestModel?.HealthCheckRequired).GetValueOrDefault();
        }
    }
}
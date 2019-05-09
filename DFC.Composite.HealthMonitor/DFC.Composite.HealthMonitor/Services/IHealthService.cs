using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DFC.Composite.HealthMonitor.Services
{
    public interface IHealthService
    {
        Task<bool> IsHealthy(string url);
    }
}

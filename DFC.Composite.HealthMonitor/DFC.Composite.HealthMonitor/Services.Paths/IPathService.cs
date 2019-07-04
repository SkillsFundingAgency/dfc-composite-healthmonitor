using DFC.Composite.HealthMonitor.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.Composite.HealthMonitor.Services.Paths
{
    public interface IPathService
    {
        Task<IEnumerable<PathModel>> GetPaths();
    }
}

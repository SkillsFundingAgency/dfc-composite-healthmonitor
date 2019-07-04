using DFC.Composite.HealthMonitor.Common;
using DFC.Composite.HealthMonitor.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.Composite.HealthMonitor.Services.Regions
{
    public interface IRegionService
    {
        Task<IEnumerable<RegionModel>> GetRegions(string path);

        Task MarkAsHealthy(string path, PageRegion pageRegion);
    }
}

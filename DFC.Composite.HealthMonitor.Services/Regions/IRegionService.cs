using DFC.Composite.HealthMonitor.Data.Enums;
using DFC.Composite.HealthMonitor.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.Composite.HealthMonitor.Services.Regions
{
    public interface IRegionService
    {
        Task<IEnumerable<RegionModel>> GetRegions(string path);

        Task<bool> MarkAsHealthy(string path, PageRegion pageRegion);
    }
}
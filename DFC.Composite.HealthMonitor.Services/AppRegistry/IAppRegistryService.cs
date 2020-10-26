using DFC.Composite.HealthMonitor.Data.Enums;
using DFC.Composite.HealthMonitor.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.Composite.HealthMonitor.Services.AppRegistry
{
    public interface IAppRegistryService
    {
        Task<IEnumerable<AppRegistryModel>> GetPathsAndRegions();

        Task<bool> MarkRegionAsHealthy(string path, PageRegion pageRegion);

        Task<bool> MarkAjaxRequestAsHealthy(string path, string name);
    }
}

using System;
using System.Collections.Generic;

namespace DFC.Composite.HealthMonitor.Data.Models
{
    public class AppRegistryModel
    {
        public string Path { get; set; }

        public Uri ExternalURL { get; set; }

        public IList<RegionModel> Regions { get; set; }
    }
}

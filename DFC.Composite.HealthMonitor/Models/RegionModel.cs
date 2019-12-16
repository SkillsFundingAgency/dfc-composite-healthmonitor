using DFC.Composite.HealthMonitor.Common;
using System;

namespace DFC.Composite.HealthMonitor.Models
{
    public class RegionModel
    {
        public Guid? DocumentId { get; set; }

        public string Path { get; set; }

        public PageRegion PageRegion { get; set; }

        public bool IsHealthy { get; set; }

        public string RegionEndpoint { get; set; }

        public bool HeathCheckRequired { get; set; }

        public string OfflineHtml { get; set; }

        public DateTime DateOfRegistration { get; set; }

        public DateTime LastModifiedDate { get; set; }
    }
}

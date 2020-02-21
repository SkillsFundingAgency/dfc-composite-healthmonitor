using DFC.Composite.HealthMonitor.Data.Enums;
using System;

namespace DFC.Composite.HealthMonitor.Data.Models
{
    public class RegionModel
    {
        public Guid? DocumentId { get; set; }

        public string Path { get; set; }

        public PageRegion PageRegion { get; set; }

        public bool IsHealthy { get; set; }

        public string RegionEndpoint { get; set; }

        public bool HealthCheckRequired { get; set; }

        public string OfflineHtml { get; set; }

        public DateTime DateOfRegistration { get; set; }

        public DateTime LastModifiedDate { get; set; }
    }
}
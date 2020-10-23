using System;

namespace DFC.Composite.HealthMonitor.Data.Models
{
    public class AjaxRequestModel
    {
        public string Name { get; set; }

        public bool IsHealthy { get; set; } = true;

        public string AjaxEndpoint { get; set; }

        public bool HealthCheckRequired { get; set; }

        public string OfflineHtml { get; set; }

        public DateTime? DateOfRegistration { get; set; }

        public DateTime? LastModifiedDate { get; set; }
    }
}

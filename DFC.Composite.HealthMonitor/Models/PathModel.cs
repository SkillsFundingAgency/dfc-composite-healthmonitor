using DFC.Composite.HealthMonitor.Common;
using System;

namespace DFC.Composite.HealthMonitor.Models
{
    public class PathModel
    {
        public Guid DocumentId { get; set; }

        public string Path { get; set; }

        public string TopNavigationText { get; set; }

        public int TopNavigationOrder { get; set; }

        public Layout Layout { get; set; }

        public bool IsOnline { get; set; }

        public string OfflineHtml { get; set; }

        public string PhaseBannerHtml { get; set; }

        public string SitemapURL { get; set; }

        public string ExternalURL { get; set; }

        public string RobotsURL { get; set; }

        public DateTime DateOfRegistration { get; set; }

        public DateTime LastModifiedDate { get; set; }

    }
}

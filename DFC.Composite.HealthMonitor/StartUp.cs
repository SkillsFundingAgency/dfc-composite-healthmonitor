using DFC.Common.Standard.Logging;
using DFC.Composite.HealthMonitor;
using DFC.Composite.HealthMonitor.Common;
using DFC.Composite.HealthMonitor.Services.HealthCheck;
using DFC.Composite.HealthMonitor.Services.HealthMonitoring;
using DFC.Composite.HealthMonitor.Services.HealthMonitoringFilter;
using DFC.Composite.HealthMonitor.Services.Paths;
using DFC.Composite.HealthMonitor.Services.Regions;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

[assembly: WebJobsStartup(typeof(StartUp))]
namespace DFC.Composite.HealthMonitor
{
    public class StartUp : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            var config = CreateConfiguration(builder);

            RegisterServices(builder.Services, config);
        }

        private IConfiguration CreateConfiguration(IWebJobsBuilder builder)
        {
            var configurationBuilder = new ConfigurationBuilder();
            var descriptor = builder.Services.FirstOrDefault(d => d.ServiceType == typeof(IConfiguration));
            if (descriptor?.ImplementationInstance is IConfigurationRoot configuration)
            {
                configurationBuilder.AddConfiguration(configuration);
            }

            return configurationBuilder.Build();
        }
        private void RegisterServices(IServiceCollection services, IConfiguration config)
        {
            services.AddHttpClient(HttpClientName.Paths, x => x.BaseAddress = new Uri(config["Paths.BaseUrl"]));
            services.AddHttpClient(HttpClientName.Regions, x => x.BaseAddress = new Uri(config["Regions.BaseUrl"]));
            services.AddHttpClient(HttpClientName.Health);

            services.AddTransient<IHealthChecker, HealthChecker>();
            services.AddTransient<IHealthMonitoring, HealthMonitoring>();
            services.AddTransient<IHealthMonitoringFilter, HealthMonitoringFilter>();
            services.AddTransient<IPathService, PathService>();
            services.AddTransient<IRegionService, RegionService>();
            services.AddTransient<ILoggerHelper, LoggerHelper>();
        }
    }
}

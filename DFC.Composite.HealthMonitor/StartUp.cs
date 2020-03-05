using DFC.Composite.HealthMonitor;
using DFC.Composite.HealthMonitor.Data.Common;
using DFC.Composite.HealthMonitor.Services.HealthCheck;
using DFC.Composite.HealthMonitor.Services.HealthMonitoring;
using DFC.Composite.HealthMonitor.Services.Paths;
using DFC.Composite.HealthMonitor.Services.Regions;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

[assembly: WebJobsStartup(typeof(StartUp))]

namespace DFC.Composite.HealthMonitor
{
    [ExcludeFromCodeCoverage]
    public class StartUp : IWebJobsStartup
    {
        private const string PathsBaseUrlSetting = "PathsBaseUrl";
        private const string RegionsBaseUrlSetting = "RegionsBaseUrl";
        private const string ApimSubscriptionKeyHeaderName = "Ocp-Apim-Subscription-Key";
        private const string ApimSubscriptionKeySetting = "ApimSubscriptionKey";

        public void Configure(IWebJobsBuilder builder)
        {
            if (builder == null)
            {
                return;
            }

            var config = CreateConfiguration(builder);
            RegisterServices(builder.Services, config);
        }

        private static IConfiguration CreateConfiguration(IWebJobsBuilder builder)
        {
            var configurationBuilder = new ConfigurationBuilder();
            var descriptor = builder.Services.FirstOrDefault(d => d.ServiceType == typeof(IConfiguration));
            if (descriptor?.ImplementationInstance is IConfigurationRoot configuration)
            {
                configurationBuilder.AddConfiguration(configuration);
            }

            return configurationBuilder.Build();
        }

        private static void RegisterServices(IServiceCollection services, IConfiguration config)
        {
            services.AddHttpClient(HttpClientName.Paths, httpClient =>
            {
                httpClient.BaseAddress = new Uri($"{config[PathsBaseUrlSetting].TrimEnd('/')}/");
                httpClient.DefaultRequestHeaders.Add(ApimSubscriptionKeyHeaderName, config[ApimSubscriptionKeySetting]);
            });
            services.AddHttpClient(HttpClientName.Regions, httpClient =>
            {
                httpClient.BaseAddress = new Uri($"{config[RegionsBaseUrlSetting].TrimEnd('/')}/");
                httpClient.DefaultRequestHeaders.Add(ApimSubscriptionKeyHeaderName, config[ApimSubscriptionKeySetting]);
            });
            services.AddHttpClient(HttpClientName.Health);

            services.AddTransient<IHealthCheckerService, HealthCheckerService>();
            services.AddTransient<IHealthMonitoringProcessor, HealthMonitoringProcessor>();
            services.AddTransient<IPathService, PathService>();
            services.AddTransient<IRegionService, RegionService>();
        }
    }
}
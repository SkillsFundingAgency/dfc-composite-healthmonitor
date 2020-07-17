using DFC.Composite.HealthMonitor;
using DFC.Composite.HealthMonitor.Data.Common;
using DFC.Composite.HealthMonitor.Services.AppRegistry;
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

[assembly: WebJobsStartup(typeof(StartUp))]

namespace DFC.Composite.HealthMonitor
{
    [ExcludeFromCodeCoverage]
    public class StartUp : IWebJobsStartup
    {
        private const string AppRegistryBaseUrlSetting = "AppRegistryBaseUrl";
        private const string ApimSubscriptionKeyHeaderName = "Ocp-Apim-Subscription-Key";
        private const string ApimSubscriptionKeySetting = "ApimSubscriptionKey";

        public void Configure(IWebJobsBuilder builder)
        {
            if (builder == null)
            {
                return;
            }

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            RegisterServices(builder.Services, configuration);
        }

        private static void RegisterServices(IServiceCollection services, IConfiguration config)
        {
            services.AddHttpClient(HttpClientName.AppRegistry, httpClient =>
            {
                httpClient.BaseAddress = new Uri($"{config[AppRegistryBaseUrlSetting]}");
                httpClient.DefaultRequestHeaders.Add(ApimSubscriptionKeyHeaderName, config[ApimSubscriptionKeySetting]);
            });
            services.AddHttpClient(HttpClientName.Health);

            services.AddTransient<IHealthCheckerService, HealthCheckerService>();
            services.AddTransient<IHealthMonitoringProcessor, HealthMonitoringProcessor>();
            services.AddTransient<IAppRegistryService, AppRegistryService>();
        }
    }
}
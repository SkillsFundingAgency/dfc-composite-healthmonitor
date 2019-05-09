using DFC.Common.Standard.Logging;
using DFC.Composite.HealthMonitor;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.DependencyInjection;

[assembly: WebJobsStartup(typeof(StartUp))]
namespace DFC.Composite.HealthMonitor
{
    public class StartUp : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            RegisterServices(builder.Services);
        }

        private void RegisterServices(IServiceCollection services)
        {
            services.AddTransient<ILoggerHelper, LoggerHelper>();
        }
    }
}

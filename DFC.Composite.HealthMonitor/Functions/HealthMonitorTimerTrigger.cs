using DFC.Common.Standard.Logging;
using DFC.Composite.HealthMonitor.Services.HealthMonitoring;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace DFC.Composite.HealthMonitor.Functions
{
    public class HealthMonitorTimerTrigger
    {
        private readonly ILogger<HealthMonitorTimerTrigger> logger;
        private readonly ILoggerHelper loggerHelper;
        private readonly IHealthMonitoringProcessor healthMonitoringProcessor;

        public HealthMonitorTimerTrigger(
            ILogger<HealthMonitorTimerTrigger> logger,
            ILoggerHelper loggerHelper,
            IHealthMonitoringProcessor healthMonitoringProcessor)
        {
            this.logger = logger;
            this.loggerHelper = loggerHelper;
            this.healthMonitoringProcessor = healthMonitoringProcessor;
        }

        [FunctionName("HealthMonitorTimerTrigger")]
        public async Task Run([TimerTrigger("%HealthMonitorTimerTriggerSchedule%")]TimerInfo myTimer)
        {
            loggerHelper.LogMethodEnter(logger);

            await healthMonitoringProcessor.Process().ConfigureAwait(false);
            logger.LogTrace($"Next run of health check is {myTimer?.ScheduleStatus.Next}");
            loggerHelper.LogMethodExit(logger);
        }
    }
}
using DFC.Common.Standard.Logging;
using DFC.Composite.HealthMonitor.Services.HealthMonitoring;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace DFC.Composite.HealthMonitor.Functions
{
    public class HealthMonitorTimerTrigger
    {
        private readonly ILogger<HealthMonitorTimerTrigger> _logger;
        private readonly ILoggerHelper _loggerHelper;
        private readonly IHealthMonitoring _healthMonitoring;

        public HealthMonitorTimerTrigger(
            ILogger<HealthMonitorTimerTrigger> logger, 
            ILoggerHelper loggerHelper, 
            IHealthMonitoring healthMonitoring)
        {
            _logger = logger;
            _loggerHelper = loggerHelper;
            _healthMonitoring = healthMonitoring;
        }

        [FunctionName("HealthMonitorTimerTrigger")]
        public void Run([TimerTrigger("%HealthMonitorTimerTriggerSchedule%")]TimerInfo myTimer)
        {
            _loggerHelper.LogMethodEnter(_logger);

            _healthMonitoring.Monitor();

            _loggerHelper.LogMethodExit(_logger);
        }
    }
}

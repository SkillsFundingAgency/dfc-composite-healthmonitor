using DFC.Common.Standard.Logging;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;

namespace DFC.Composite.HealthMonitor.Functions
{
    public class HealthMonitorTimerTrigger
    {
        private readonly ILogger<HealthMonitorTimerTrigger> _logger;
        private readonly ILoggerHelper _loggerHelper;

        public HealthMonitorTimerTrigger(ILogger<HealthMonitorTimerTrigger> logger, ILoggerHelper loggerHelper)
        {
            _logger = logger;
            _loggerHelper = loggerHelper;
        }

        [FunctionName("HealthMonitorTimerTrigger")]
        public void Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer)
        {
            _loggerHelper.LogMethodEnter(_logger);

            _loggerHelper.LogMethodExit(_logger);
        }
    }
}

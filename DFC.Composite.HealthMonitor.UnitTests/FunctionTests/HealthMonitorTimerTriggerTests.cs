﻿using DFC.Composite.HealthMonitor.Functions;
using DFC.Composite.HealthMonitor.Services.HealthMonitoring;
using FakeItEasy;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Timers;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Composite.HealthMonitor.Tests.FunctionTests
{
    public class HealthMonitorTimerTriggerTests
    {
        [Fact]
        public async Task RunFunctionCallsHealthMonitoringProcessor()
        {
            // Arrange
            var logger = A.Fake<ILogger<HealthMonitorTimerTrigger>>();
            var healthMonitoringProcessor = A.Fake<IHealthMonitoringProcessor>();
            var function = new HealthMonitorTimerTrigger(logger, healthMonitoringProcessor);
            var timerInfo = new TimerInfo(new ConstantSchedule(new TimeSpan(1)), new ScheduleStatus());

            // Act
            await function.Run(timerInfo).ConfigureAwait(false);

            // Assert
            A.CallTo(() => healthMonitoringProcessor.Process()).MustHaveHappenedOnceExactly();
        }
    }
}
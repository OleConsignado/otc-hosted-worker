using Microsoft.Extensions.Logging;
using Otc.HostedWorker.Abstractions;
using System;
using System.Collections.Generic;

namespace Otc.HostedWorker
{
    public class DiagnosticsService : IDiagnosticsProducer, IDiagnosticsConsumer
    {
        private static List<string> LastErrors = new List<string>();
        private static Status Status = Status.Unknow;

        private static readonly DiagnosticsModel diagnostics = new DiagnosticsModel()
        {
            ContainerName = Environment.MachineName,
            ExecutionCount = 0,
            LastErrors = LastErrors
        };

        private readonly ILogger logger;
        private readonly IHostedWorkerHealth hostedWorkerHealth;

        public DiagnosticsService(ILoggerFactory loggerFactory, IHostedWorkerHealth hostedWorkerHealth)
        {
            logger = loggerFactory?.CreateLogger<DiagnosticsService>() ?? throw new ArgumentNullException(nameof(loggerFactory));
            this.hostedWorkerHealth = hostedWorkerHealth ?? throw new ArgumentNullException(nameof(hostedWorkerHealth));
        }

        public void Started()
        {
            logger.LogInformation("HostedWorker started.");
            diagnostics.StartTime = DateTimeOffset.Now;
        }

        public void WorkerExecutionStarted()
        {
            diagnostics.ExecutionCount++;
            logger.LogDebug("Start running new work #{ExecutionIndex}.", diagnostics.ExecutionCount);
            Status = Status.Running;
            diagnostics.LastExecutionStartTime = DateTimeOffset.Now;
            diagnostics.LastExecutionEndTime = null;
        }

        public void WorkerExecutionFinished()
        {
            logger.LogDebug("Finished work #{ExecutionIndex}.", diagnostics.ExecutionCount);
            Status = Status.Unknow;
            diagnostics.LastExecutionEndTime = DateTimeOffset.Now;
        }

        public void WorkerSuccessfulFinished()
        {
            logger.LogDebug("Successful finished #{ExecutionIndex}.", diagnostics.ExecutionCount);
            diagnostics.SuccessfulExecutionCount++;
        }


        public void WorkerFail(Exception e)
        {
            logger.LogError(e, "Finished with error #{ExecutionIndex}.", diagnostics.ExecutionCount);
            LastErrors.Add(e.Message);
            diagnostics.FailedExecutionCount++;
        }

        public void CancelationRequestDueTimeoutWorked()
        {
            logger.LogInformation("Worker has successful stoped after timeout's cancelation request.");
            Status = Status.Unknow;
        }

        public void ExternalCancelationRequested()
        {
            logger.LogWarning("External cancelation requested.");
            Status = Status.ExternalCancelationRequested;
        }

        public void ExecutionTimeout()
        {
            var message = "Execution timeout.";
            logger.LogError(message);
            LastErrors.Add(message);
            Status = Status.CancelationRequestedDueTimeout;
        }

        public void SetIdle()
        {
            Status = Status.Idle;
        }

        public void SetUnknow()
        {
            Status = Status.Unknow;
        }

        public void SetTerminating()
        {
            logger.LogInformation("Hosted Worker is terminating.");
            Status = Status.Terminating;
        }

        public void SetPanic()
        {
            logger.LogCritical("PANIC!!!");

            Status = Status.Panic;
        }

        public DiagnosticsModel GetDiagnostics()
        {
            if (hostedWorkerHealth.Healthy)
            {
                diagnostics.Status = Status;
            }
            else
            {
                diagnostics.Status = Status.Unhealthy;
            }

            return diagnostics;
        }
    }
}

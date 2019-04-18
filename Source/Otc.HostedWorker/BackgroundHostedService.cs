using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Otc.HostedWorker.Abstractions;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Otc.HostedWorker
{
    internal class BackgroundHostedService : BackgroundService
    {
        private readonly ILogger logger;
        private readonly IServiceProvider serviceProvider;
        private readonly HostedWorkerConfiguration configuration;
        private readonly IDiagnosticsProducer diagnosticsProducer;
        public const int TimeoutPanicExitCode = 127;
        public const int MaxConsecutiveErrorsReachedPanicExitCode = 234;

        public BackgroundHostedService(ILoggerFactory loggerFactory, IServiceProvider serviceProvider, HostedWorkerConfiguration configuration, IDiagnosticsProducer diagnosticsProducer)
        {
            logger = loggerFactory?.CreateLogger<BackgroundHostedService>() ??
                throw new ArgumentNullException(nameof(loggerFactory));
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.diagnosticsProducer = diagnosticsProducer ?? throw new ArgumentNullException(nameof(diagnosticsProducer));
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("BackgroundHostedService: Stop fired.");

            return base.StopAsync(cancellationToken);
        }

        private CancellationTokenSource workerCancellationTokenSource;

        private void RequestWorkerCancellation()
        {
            logger.LogInformation($"Cancellation for {nameof(workerCancellationTokenSource)} was requested.");

            if (workerCancellationTokenSource != null)
            {
                workerCancellationTokenSource.Cancel();
            }
            else
            {
                logger.LogWarning($"{nameof(workerCancellationTokenSource)} is null, could not request cancellation.");
            }
        }

        private async Task LogCriticalAndTerminateProcessAsync(int exitCode, string message, params object[] args)
        {
            diagnosticsProducer.SetPanic();

            logger.LogCritical($"PANIC!!! {message} **THE PROCESS IS BEING TERMINATED (GRACEFULLY) IN 1 SECOND.**", args);
            await Task.Delay(1000); // give a chance to log properly

            // Terminate process gracefully
            Environment.Exit(exitCode);

            // Termination fallback, it will abrubtely killed after 5 seconds if process don't terminate gracefully
            await Task.Delay(5000);
            logger.LogCritical("PANIC!!! FAIL TO ABORT PROCESS, TRYING TO **ABRUBTELY KILL IT** IN 1 SECOND.");
            await Task.Delay(1000);
            Process.GetCurrentProcess().Kill();
        }

        private bool working = false;

        private async Task ExecuteHelperAsync(CancellationToken stoppingToken)
        {
            diagnosticsProducer.Started();

            stoppingToken.Register(() => 
            {
                diagnosticsProducer.ExternalCancelationRequested();

                if (working)
                {
                    RequestWorkerCancellation();
                }
            });

            int consecutiveErrors = 0;
            bool hasPendingWork = false;

            while (!stoppingToken.IsCancellationRequested)
            {
                diagnosticsProducer.SetUnknow();

                if (hasPendingWork)
                {
                    logger.LogDebug("Pending work! So, working ...");

                    hasPendingWork = false;

                    try
                    {
                        workerCancellationTokenSource = new CancellationTokenSource();
                        var scopeId = Guid.NewGuid();

                        var workerTask = Task.Run(async () =>
                        {
                            using (var scope = serviceProvider.CreateScope())
                            using (logger.BeginScope(scopeId))
                            {
                                var worker = scope.ServiceProvider.GetService<IHostedWorker>();
                                worker.HasPendingWork = false;

                                try
                                {  
                                    diagnosticsProducer.WorkerExecutionStarted();    
                                    working = true;
                                    HostedWorkerHealth.CurrentWorkStartedAt = DateTimeOffset.Now;
                                    await worker.WorkAsync(workerCancellationTokenSource.Token);
                                    diagnosticsProducer.WorkerSuccessfulFinished();
                                }
                                finally
                                {
                                    working = false;
                                    HostedWorkerHealth.CurrentWorkStartedAt = null;
                                    hasPendingWork = worker.HasPendingWork;
                                    diagnosticsProducer.WorkerExecutionFinished();
                                }
                            }
                        });

                        int index = Task.WaitAny(new Task[] { workerTask },
                            TimeSpan.FromSeconds(configuration.WorkerTimeoutInSeconds));

                        if (index == -1)
                        {
                            workerCancellationTokenSource.Cancel();
                            diagnosticsProducer.ExecutionTimeout();

                            // Wait for more [WorkerTerminationTorerationTimeoutInSeconds] seconds
                            index = Task.WaitAny(new Task[] { workerTask }, TimeSpan.FromSeconds(configuration.WorkerTerminationTorerationTimeoutInSeconds));

                            if (index == -1)
                            {
                                await LogCriticalAndTerminateProcessAsync(TimeoutPanicExitCode, "Worker has timedout then it didn't stoped after " +
                                    "{WorkerTerminationTorerationTimeoutInSeconds} from token cancellation request.", configuration.WorkerTerminationTorerationTimeoutInSeconds);
                            }
                            else
                            {
                                diagnosticsProducer.CancelationRequestDueTimeoutWorked();
                            }
                        }

                        await workerTask;
                        consecutiveErrors = 0;
                    }
                    catch (Exception e)
                    {
                        consecutiveErrors++;
                        diagnosticsProducer.WorkerFail(e);

                        if (consecutiveErrors > configuration.MaxConsecutiveErrors)
                        {
                            await LogCriticalAndTerminateProcessAsync(MaxConsecutiveErrorsReachedPanicExitCode, $"{nameof(configuration.MaxConsecutiveErrors)} reached! " +
                                $"Current consecutive errors count is {{ConsecutiveErrors}}.", consecutiveErrors);
                        }
                    }
                }

                diagnosticsProducer.SetUnknow();

                if (!hasPendingWork)
                {
                    bool pulled = Pulled();

                    while (!pulled && !stoppingToken.IsCancellationRequested)
                    {
                        await Task.Delay(50);
                        pulled = Pulled();
                        diagnosticsProducer.SetIdle();
                    }

                    hasPendingWork = pulled;
                }
            }

            diagnosticsProducer.SetTerminating();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Starting ExecuteAsync ...");
            await Task.Factory.StartNew(async () => await ExecuteHelperAsync(stoppingToken), TaskCreationOptions.LongRunning);
            logger.LogInformation("ExecuteAsync done.");
        }

        private bool Pulled()
        {
            return InternalHostedWorkerTrigger.Pulled();
        }
    }
}

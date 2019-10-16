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

        public const int TimeoutPanicExitCode = 127;
        public const int MaxConsecutiveErrorsReachedPanicExitCode = 234;

        public BackgroundHostedService(ILoggerFactory loggerFactory, 
            IServiceProvider serviceProvider, 
            HostedWorkerConfiguration configuration)
        {
            logger = loggerFactory?.CreateLogger<BackgroundHostedService>() ??
                throw new ArgumentNullException(nameof(loggerFactory));
            this.serviceProvider = serviceProvider ?? 
                throw new ArgumentNullException(nameof(serviceProvider));
            this.configuration = configuration ?? 
                throw new ArgumentNullException(nameof(configuration));
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation($"{nameof(StartAsync)}: Start fired.");

            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation($"{nameof(StopAsync)}: Stop fired.");

            return base.StopAsync(cancellationToken);
        }

        private CancellationTokenSource workerCancellationTokenSource;

        private void RequestWorkerCancellation()
        {
            logger.LogInformation($"Cancellation for {nameof(RequestWorkerCancellation)} " +
                $"was requested.");

            if (workerCancellationTokenSource != null)
            {
                workerCancellationTokenSource.Cancel();
            }
            else
            {
                logger.LogWarning($"{nameof(RequestWorkerCancellation)} is null, could " +
                    $"not request cancellation.");
            }
        }

        private async Task LogCriticalAndTerminateProcessAsync(int exitCode, 
            string message, params object[] args)
        {
            logger.LogCritical("PANIC!!! {Message} **THE PROCESS IS BEING " +
                "TERMINATED (GRACEFULLY) IN 1 SECOND.**", message, args);
            await Task.Delay(1000); // give a chance to log properly

            // Terminate process gracefully
            Environment.Exit(exitCode);

            // Termination fallback, it will abrubtely killed after 5 seconds 
            // if process don't terminate gracefully
            await Task.Delay(5000);
            logger.LogCritical("PANIC!!! FAIL TO ABORT PROCESS, TRYING TO " +
                "**ABRUBTELY KILL IT** IN 1 SECOND.");
            await Task.Delay(1000);
            Process.GetCurrentProcess().Kill();
        }

        private bool working = false;

        private async Task ExecuteHelperAsync(CancellationToken stoppingToken)
        {
            stoppingToken.Register(() => 
            {
                if (working)
                {
                    RequestWorkerCancellation();
                }
            });

            int consecutiveErrors = 0;

            if(configuration.WorkOnStartup)
            {
                logger.LogInformation("{WorkOnStartup} is true, so executing " +
                    $"{nameof(ExecuteHelperAsync)} right now.", 
                    nameof(HostedWorkerConfiguration.WorkOnStartup));
            }

            bool hasPendingWork = configuration.WorkOnStartup;

            while (!stoppingToken.IsCancellationRequested)
            {
                if (hasPendingWork)
                {
                    logger.LogInformation("Pending work! So, working ...");

                    hasPendingWork = false;

                    try
                    {
                        workerCancellationTokenSource = new CancellationTokenSource();

                        var workerTask = Task.Run(async () =>
                        {
                            using (var scope = serviceProvider.CreateScope())
                            using (logger.BeginScope(Guid.NewGuid()))
                            {
                                var worker = scope.ServiceProvider.GetService<IHostedWorker>();
                                worker.HasPendingWork = false;
                                working = true;
                                HostedWorkerHealth.CurrentWorkStartedAt = DateTimeOffset.Now;
                                await worker.WorkAsync(workerCancellationTokenSource.Token);
                                working = false;
                                HostedWorkerHealth.CurrentWorkStartedAt = null;
                                hasPendingWork = worker.HasPendingWork;
                            }
                        });

                        int index = Task.WaitAny(new Task[] { workerTask },
                            TimeSpan.FromSeconds(configuration.WorkerTimeoutInSeconds));

                        if (index == -1)
                        {
                            logger.LogError("Worker has timedout. Asking it to stop ...");
                            workerCancellationTokenSource.Cancel();

                            // Wait for more [WorkerTerminationTorerationTimeoutInSeconds] seconds
                            index = Task.WaitAny(new Task[] { workerTask }, 
                                TimeSpan.FromSeconds(configuration.WorkerTerminationTorerationTimeoutInSeconds));

                            if (index == -1)
                            {
                                await LogCriticalAndTerminateProcessAsync(TimeoutPanicExitCode, 
                                    "Worker has timedout then it didn't stoped after " +
                                    "{WorkerTerminationTorerationTimeoutInSeconds} from " +
                                    "token cancellation request.", 
                                    configuration.WorkerTerminationTorerationTimeoutInSeconds);
                            }
                            else
                            {
                                logger.LogWarning("Worker sucessfully stoped " +
                                    "after asked to stop due the timeout ...");
                            }
                        }

                        await workerTask;
                        consecutiveErrors = 0;
                    }
                    catch (Exception e)
                    {
                        consecutiveErrors++;
                        logger.LogError(e, "BackgroundHostedService worker failed.");

                        if (consecutiveErrors > configuration.MaxConsecutiveErrors)
                        {
                            await LogCriticalAndTerminateProcessAsync(MaxConsecutiveErrorsReachedPanicExitCode, 
                                $"{nameof(configuration.MaxConsecutiveErrors)} reached! " +
                                $"Current consecutive errors count is {{ConsecutiveErrors}}.", 
                                consecutiveErrors);
                        }
                    }
                }

                if (!hasPendingWork)
                {
                    bool pulled = Pulled();

                    while (!pulled && !stoppingToken.IsCancellationRequested)
                    {
                        await Task.Delay(50);
                        pulled = Pulled();
                    }

                    hasPendingWork = pulled;
                }
            }

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation($"Starting {nameof(ExecuteAsync)} ...");
            await Task.Factory.StartNew(async () => await ExecuteHelperAsync(stoppingToken), 
                TaskCreationOptions.LongRunning);
            logger.LogInformation($"{nameof(ExecuteAsync)} done.");
        }

        private bool Pulled()
        {
            return InternalHostedWorkerTrigger.Pulled();
        }
    }
}

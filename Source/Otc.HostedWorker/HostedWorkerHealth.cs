using Otc.HostedWorker.Abstractions;
using System;

namespace Otc.HostedWorker
{
    internal class HostedWorkerHealth : IHostedWorkerHealth
    {
        private readonly HostedWorkerConfiguration hostedWorkerConfiguration;

        internal static DateTimeOffset? CurrentWorkStartedAt { get; set; }

        public HostedWorkerHealth(HostedWorkerConfiguration hostedWorkerConfiguration)
        {
            this.hostedWorkerConfiguration = hostedWorkerConfiguration ?? 
                throw new ArgumentNullException(nameof(hostedWorkerConfiguration));
        }

        public bool Healthy
        {
            get
            {
                if (CurrentWorkStartedAt?
                    .AddSeconds(hostedWorkerConfiguration.WorkerTimeoutInSeconds) < DateTimeOffset.Now)
                {
                    return false;
                }

                return true;
            }
        }
    }
}

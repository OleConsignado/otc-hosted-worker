namespace Otc.HostedWorker
{
    public class HostedWorkerConfiguration
    {
        public int WorkerTimeoutInSeconds { get; set; } = 60;
        public int WorkerTerminationTorerationTimeoutInSeconds { get; set; } = 15;
        public int MaxConsecutiveErrors { get; set; } = 5;

        /// <summary>
        /// If true, <see cref="Abstractions.IHostedWorker.WorkAsync" /> will 
        /// be fired at startup.
        /// </summary>
        public bool WorkOnStartup { get; set; } = false;
    }
}

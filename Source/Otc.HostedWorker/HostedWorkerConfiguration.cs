namespace Otc.HostedWorker
{
    public class HostedWorkerConfiguration
    {
        public int WorkerTimeoutInSeconds { get; set; } = 60;
        public int WorkerTerminationTorerationTimeoutInSeconds { get; set; } = 15;
        public int MaxConsecutiveErrors { get; set; } = 5;
    }
}

namespace Otc.HostedWorker.Abstractions
{
    public interface IHostedWorkerHealth
    {
        bool Healthy { get; }
    }
}

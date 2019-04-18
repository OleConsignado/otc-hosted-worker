namespace Otc.HostedWorker.Abstractions
{
    public interface IHostedWorkerTrigger
    {
        void Pull();
    }
}

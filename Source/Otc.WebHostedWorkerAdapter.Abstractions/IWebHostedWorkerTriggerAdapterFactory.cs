namespace Otc.WebHostedWorkerAdapter.Abstractions
{
    public interface IWebHostedWorkerTriggerAdapterFactory
    {
        IWebHostedWorkerTriggerAdapter Create(string baseUrl);
    }
}

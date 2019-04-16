using Otc.HostedWorker.Abstractions;

namespace Otc.HostedWorker
{
    internal class HostedWorkerTrigger : IHostedWorkerTrigger
    {
        public void Pull()
        {
            InternalHostedWorkerTrigger.Pull();
        }
    }
}

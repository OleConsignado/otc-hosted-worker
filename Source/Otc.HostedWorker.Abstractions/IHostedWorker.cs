using System.Threading;
using System.Threading.Tasks;

namespace Otc.HostedWorker.Abstractions
{
    public interface IHostedWorker
    {
        Task WorkAsync(CancellationToken cancellationToken);

        bool HasPendingWork { get; set; }
    }
}

using Otc.HostedWorker.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Otc.HostedWorker.Tests
{
    public class TestHostedWorker : IHostedWorker
    {
        private readonly Func<CancellationToken, Task> testWorker;

        public TestHostedWorker(Func<CancellationToken, Task> testWorker)
        {
            this.testWorker = testWorker ?? throw new ArgumentNullException(nameof(testWorker));
        }

        public bool HasPendingWork { get; set; }

        public Task WorkAsync(CancellationToken cancellationToken)
        {
            return testWorker.Invoke(cancellationToken);
        }
    }
}

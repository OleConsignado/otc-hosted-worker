using System;
using System.Collections.Generic;
using System.Text;

namespace Otc.HostedWorker.Abstractions
{
    public interface IHostedWorkerTrigger
    {
        void Pull();
    }
}

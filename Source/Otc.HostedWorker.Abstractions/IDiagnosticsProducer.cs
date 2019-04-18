using System;
using System.Collections.Generic;
using System.Text;

namespace Otc.HostedWorker.Abstractions
{
    public interface IDiagnosticsProducer
    {
        void Started();
        void WorkerExecutionStarted();
        void WorkerExecutionFinished();
        void ExecutionTimeout();
        void CancelationRequestDueTimeoutWorked();
        void WorkerSuccessfulFinished();
        void WorkerFail(Exception e);
        void ExternalCancelationRequested();
        void SetIdle();
        void SetUnknow();
        void SetTerminating();
        void SetPanic();
    }
}

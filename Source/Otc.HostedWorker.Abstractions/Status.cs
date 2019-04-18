namespace Otc.HostedWorker.Abstractions
{
    public enum Status
    {
        Unknow = 0,
        Idle,
        Running,
        ExternalCancelationRequested,
        CancelationRequestedDueTimeout,
        Unhealthy,
        Suspended,
        Terminating,
        Panic
    }
}

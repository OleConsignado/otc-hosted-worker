using System;
using System.Collections.Generic;
using System.Text;

namespace Otc.HostedWorker.Abstractions
{
    public class DiagnosticsModel
    {
        public string ContainerName { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset? LastExecutionStartTime { get; set; }
        public DateTimeOffset? LastExecutionEndTime { get; set; }
        public TimeSpan ExecutionDurationAverage { get; set; }
        public TimeSpan ExecutionDurationStdDeviation { get; set; }
        public Status Status { get; set; }
        public int ExecutionCount { get; set; }
        public int SuccessfulExecutionCount { get; set; }
        public int FailedExecutionCount { get; set; }
        public IEnumerable<string> LastErrors { get; set; }
    }
}

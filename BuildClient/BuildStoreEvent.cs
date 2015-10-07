using System;
using System.Collections.Generic;
using Microsoft.TeamFoundation.Build.Client;

namespace BuildClient
{
    public enum BuildStoreEventType
    {
        Build,
        QualityChanged
    }

    public class BuildData
    {

        public string BuildName { get; set; }

        public BuildStoreEventType EventType { get; set; }

        public string BuildRequestedFor { get; set; }

        public BuildExecutionStatus Status { get; set; }
        public string Quality { get; set; }
    }

    public enum BuildExecutionStatus
    {
        Succeeded,
        Failed,
        Stopped,
        InProgress,
        PartiallySucceeded,
        Unknown
    }

    public class BuildStoreEventArgs : EventArgs
    {
        public BuildStoreEventType Type { get; set; }
        public BuildData Data { get; set; }
    }

    public delegate void BuildWatcherEventHandler(object sender, BuildStoreEventArgs buildWatcherEventArgs);

    public delegate void BuildWatcherInitializingHandler(
        object sender, IEnumerable<BuildStoreEventArgs> buildWatcherEventArgs);

    public delegate void BuildWatcherStoppingHandler(object sender, EventArgs eventArgs);
}
using System;
using System.Diagnostics;

namespace BuildClient
{
    public static class Tracing
    {


        /// <summary>
        /// Logs an exception using Trace functionality
        /// </summary>
        /// <param name="source">The tracesource to use.</param>
        /// <param name="exception">Exception to log</param>
        public static void LogException(this TraceSource source, Exception exception)
        {
            if (source == null) return;
            source.TraceEvent(TraceEventType.Error, 0, "{0} {1}", DateTime.UtcNow, exception);
        }

        public static void TraceError(this TraceSource source, string message)
        {
            if (source == null) return;
            source.TraceEvent(TraceEventType.Error, 0, message);
        }

        public static void TraceError(this TraceSource source, string message, string arg1)
        {
            if (source == null) return;

            if (String.IsNullOrEmpty(arg1))
            {
                source.TraceEvent(TraceEventType.Error, 0, message);
            }
            else
            {
                source.TraceEvent(TraceEventType.Error, 0, string.Format(message, arg1));
            }


        }

        public static void TraceError(this TraceSource source, string message, string arg1, string arg2)
        {
            if (source == null) return;

            if (String.IsNullOrEmpty(arg1) || String.IsNullOrEmpty(arg2))
            {
                source.TraceEvent(TraceEventType.Error, 0, message);
            }
            else
            {
                source.TraceEvent(TraceEventType.Error, 0, String.Format(message, arg1, arg2));
            }


        }

        public static void TraceError(this TraceSource source, string message, string arg1, string arg2, string arg3)
        {
            if (source == null) return;

            if (String.IsNullOrEmpty(arg1) || String.IsNullOrEmpty(arg2) || String.IsNullOrEmpty(arg3))
            {
                source.TraceEvent(TraceEventType.Error, 0, message);
            }

            else
            {
                source.TraceEvent(TraceEventType.Error, 0, string.Format(message, arg1, arg2, arg3));
            }

        }

        public static void TraceBeginActivity(this TraceSource source, string activityName, ref Guid oldActivityId)
        {
            if (source == null)
            {
                return;
            }
            oldActivityId = Trace.CorrelationManager.ActivityId;
            Guid traceID = Guid.NewGuid();
            source.TraceTransfer(0, "transfer", traceID);
            Trace.CorrelationManager.ActivityId = traceID; // Trace is static
            source.TraceEvent(TraceEventType.Start, 0, activityName);

        }

        public static void TraceEndActivity(this TraceSource source, string activityName, Guid oldActivityId)
        {
            source.TraceTransfer(0, "transfer", oldActivityId);
            source.TraceEvent(TraceEventType.Stop, 0, activityName);
            Trace.CorrelationManager.ActivityId = oldActivityId;

        }

        public static void TraceInformation(this TraceSource source, string message)
        {
            if (source == null) return;
            source.TraceEvent(TraceEventType.Information, 0, message);
        }

        public static void TraceInformation(this TraceSource source, string message, string arg1)
        {
            if (source == null) return;

            //if arg1 is not supplied
            source.TraceEvent(TraceEventType.Information, 0,
                              String.IsNullOrEmpty(arg1) ? message : string.Format(message, arg1));
        }

        public static void TraceInformation(this TraceSource source, string message, string arg1, string arg2)
        {
            if (source == null) return;

            if (String.IsNullOrEmpty(arg1) || String.IsNullOrEmpty(arg2))
            {
                source.TraceEvent(TraceEventType.Information, 0, message);
            }
            else
            {
                source.TraceEvent(TraceEventType.Information, 0, String.Format(message, arg1, arg2));
            }

        }

        public static void TraceInformation(this TraceSource source, string message, string arg1, string arg2, string arg3)
        {
            if (source == null) return;

            if (String.IsNullOrEmpty(arg1) || String.IsNullOrEmpty(arg2) || String.IsNullOrEmpty(arg3))
            {
                source.TraceEvent(TraceEventType.Information, 0, message);
            }
            else
            {
                source.TraceEvent(TraceEventType.Information, 0, string.Format(message, arg1, arg2, arg3));
            }

        }


        public static readonly TraceSource Server = new TraceSource("BuildStateServer.Server");
        public static readonly TraceSource Client = new TraceSource("BuildClient.Client");
        public static readonly TraceSource ErrorHandler = new TraceSource("BuildCommon.Error");


        
    }
}
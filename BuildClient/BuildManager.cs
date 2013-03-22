using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BuildClient.Configuration;
using BuildCommon;
using Microsoft.TeamFoundation.Build.Client;

namespace BuildClient
{
    public class BuildManager
    {
        private static Timer _lockExpiryTimer;
        private readonly IBuildConfigurationManager _buildConfigurationManager;
        private readonly IBuildStoreEventSource _eventSource;
        private readonly INotifier _notifier;
        private AutoResetEvent _autoResetEvent;
        private static readonly object EventLocker=new object();

        public BuildManager(IBuildConfigurationManager buildConfigurationManager,
                            INotifier notifier,IBuildStoreEventSource eventSource)
        {
            _buildConfigurationManager = buildConfigurationManager;
            _notifier = notifier;
            _eventSource = eventSource;
        }

        //Method that will be called periodically
        public void StartProcessing(object stateInfo)
        {
            Tracing.Server.TraceInformation("Server: StartProcessing (loop)");

            lock (EventLocker)
            {
                // timer callback.
                _autoResetEvent = new AutoResetEvent(false);
                var tcb = new TimerCallback(PollBuildServer);
                _lockExpiryTimer = new Timer(tcb, _autoResetEvent, 1000, Int32.Parse(_buildConfigurationManager.PollPeriod) * 1000);
                _autoResetEvent.WaitOne();
            }
           
        }

        private string HandleAggregateExcetion(AggregateException aggregateException)
        {
            var stringBuilder = new StringBuilder();
            
            var innerExceptions = aggregateException.Flatten().InnerExceptions;
            foreach (var vr in innerExceptions)
            {
                stringBuilder.AppendLine(vr.ToString());
            }
            return stringBuilder.ToString();
        }

        private void PollBuildServer(object state)
        {
            IEnumerable<BuildStoreEventArgs> buildStoreEvents;

            try
            {
                buildStoreEvents = _eventSource.GetListOfBuildStoreEvents();
            }
            catch (AggregateException exception)
            {
                var message = HandleAggregateExcetion(exception);
                Tracing.Client.TraceError("An Exception Occured while connecting TfsServer" + message);
                return;
            }
            catch (Exception exception)
            {
                Tracing.Client.TraceError("An Unhandled Exception Occured while connecting TfsServer" + exception);
                throw;
            }

            try
            {
                Parallel.ForEach(buildStoreEvents, ProcessBuildEvent);
            }
            catch (AggregateException exception)
            {
                var message = HandleAggregateExcetion(exception);
                Tracing.Client.TraceError("An Exception while processing build events" + message);
            }
        }

        private void ProcessBuildEvent(BuildStoreEventArgs buildEvent)
        {
            Tracing.Client.TraceInformation("Build was requested for " + buildEvent.Data.RequestedFor);

            switch (buildEvent.Type)
            {
                case BuildStoreEventType.Build:
                    Tracing.Client.TraceInformation("Build Event");
                    HandleEvent(buildEvent);
                    break;
                case BuildStoreEventType.QualityChanged:
                    Tracing.Client.TraceInformation("Quality Change Event");
                    HandleEvent(buildEvent);
                    break;
                default:
                    throw new Exception("Event was not recognised.");
            }
        }


        private void HandleEvent(BuildStoreEventArgs buildStoreEventArgs)
        {
            BuildStatus status = buildStoreEventArgs.Data.Status;
            try
            {
                var notificationChannel = CreateNotificationChannel(buildStoreEventArgs);
                switch (status)
                {
                    case BuildStatus.Succeeded:
                        notificationChannel.OnBuildSuceeded();
                        Tracing.Client.TraceInformation("Build Succeeded");
                        break;
                    case BuildStatus.Failed:
                        notificationChannel.OnBuildFailed();
                        Tracing.Client.TraceInformation("Build Failed");
                        break;
                    case BuildStatus.Stopped:
                        notificationChannel.OnBuildStopped();
                        Tracing.Client.TraceInformation("Build Stopped");
                        break;
                    case BuildStatus.InProgress:
                        notificationChannel.OnBuildStarted();
                        Tracing.Client.TraceInformation("Build Started");
                        break;
                    case BuildStatus.PartiallySucceeded:
                        notificationChannel.OnBuildPartiallySucceeded();
                        Tracing.Client.TraceInformation("Build Partially Succeeded");
                        break;
                }
            }
            catch (EndpointNotFoundException)
            {
                Tracing.Client.TraceError(
                    String.Format("No service is listening at address [{0}] to accept build light commands",
                                  _notifier.GetNotificationAddress(buildStoreEventArgs.Data.BuildDefinition)));
            }

            catch (CommunicationException exception)
            {
                Tracing.Client.TraceError(
                    String.Format(
                        "There was a problem with communication while communicating with notifier at address {0} ",
                        _notifier.GetNotificationAddress(buildStoreEventArgs.Data.BuildDefinition)));
                Tracing.Client.TraceError(exception.ToString());
            }
            catch (System.Net.WebException webEx)
            {
                Tracing.Client.TraceError(
                    String.Format(
                        "There was a problem with communication while communicating with notifier at address {0} ",
                        _notifier.GetNotificationAddress(buildStoreEventArgs.Data.BuildDefinition)));
                Tracing.Client.TraceError(webEx.ToString());
                
            }
        }

        private IBuildStatusChange CreateNotificationChannel(BuildStoreEventArgs buildStoreEventArgs)
        {
            return _notifier.GetNotificationChannel(buildStoreEventArgs.Data.BuildDefinition);
        }

        public void StopProcessing()
        {
            lock (EventLocker)
            {
                if (_lockExpiryTimer!=null)
                {
                    _lockExpiryTimer.Dispose();
                }

                if (_autoResetEvent!=null)
                {
                    _autoResetEvent.Set();
                }
                
            }
            
        }
    }
}
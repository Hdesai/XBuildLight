using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BuildClient.Configuration;
using BuildCommon;
using Microsoft.TeamFoundation.Build.Client;

namespace BuildClient
{
    public class BuildManager:IDisposable
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

        
        public void StartProcessing(object stateInfo)
        {
            Tracing.Server.TraceInformation("Server: StartProcessing (loop)");

            lock (EventLocker)
            {
                // timer callback.

                //If we are restarting
                if (_autoResetEvent!=null)
                {
                    _autoResetEvent.Dispose();
                }
                
                _autoResetEvent = new AutoResetEvent(true);

                var tcb = new TimerCallback(PollBuildServer);
                
                _lockExpiryTimer = 
                    new Timer(tcb, _autoResetEvent, 1000, Int32.Parse(_buildConfigurationManager.PollPeriod) * 1000);
                
            }
           
        }

        private string UnWrapAggregateExcetion(AggregateException aggregateException)
        {
            var stringBuilder = new StringBuilder();
            
            var innerExceptions = aggregateException.Flatten().InnerExceptions;
            foreach (var vr in innerExceptions)
            {
                stringBuilder.AppendLine(vr.ToString());
            }
            return stringBuilder.ToString();
        }

        //Method that will be called periodically
        private void PollBuildServer(object state)
        {
            _autoResetEvent.WaitOne();

            try
            {
                Tracing.Client.TraceInformation("Getting list of build store events");
                IEnumerable<BuildStoreEventArgs> buildStoreEvents = _eventSource.GetListOfBuildStoreEvents();
                Parallel.ForEach(buildStoreEvents, ProcessBuildEvent);
                
            }
            catch (WebException exception)
            {
                Tracing.Client.TraceError(String.Format("An Exception Occured while connecting TfsServer {0}", exception));
            }
            catch (AggregateException exception)
            {
                var message = UnWrapAggregateExcetion(exception);
                Tracing.Client.TraceError(String.Format("An Exception Occured while connecting TfsServer {0} ", message));
            }
            catch (Exception exception)
            {
                Tracing.Client.TraceError(String.Format(
                    "An Unhandled Exception Occured while connecting TfsServer {0} ", exception));
                throw;
            }
            finally
            {
                _autoResetEvent.Set();
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
                if (_autoResetEvent != null)
                {
                    _autoResetEvent.Set();
                }

            }
            
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this); 
        }

        private bool _disposed;

        protected virtual void Dispose(bool disposing)
        {
            // If you need thread safety, use a lock around these  
            // operations, as well as in your methods that use the resource. 
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_lockExpiryTimer != null)
                        _lockExpiryTimer.Dispose();

                    if (_autoResetEvent!=null)
                    {
                        _autoResetEvent.Dispose();
                    }
                
                }

                // Indicate that the instance has been disposed.
                _autoResetEvent = null;
                _lockExpiryTimer = null;
                _disposed = true;
            }
        }
    }
}
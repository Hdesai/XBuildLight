using System;
using System.Configuration;
using System.Linq;
using System.Threading;
using BuildClient.Configuration;
using BuildCommon;

namespace BuildClient
{
    public class BuildManager : IDisposable
    {
        private const int DueTime = 1000;
        private static Timer _lockExpiryTimer;
        private readonly IBuildConfigurationManager _buildConfigurationManager;
        private readonly IBuildEventPublisher _buildEventPublisher;
        private readonly IBuildStoreEventSource _eventSource;
        private bool _disposed;

        public BuildManager(IBuildConfigurationManager buildConfigurationManager,
            IBuildStoreEventSource eventSource, IBuildEventPublisher buildEventPublisher)
        {
            _buildConfigurationManager = buildConfigurationManager;
            _eventSource = eventSource;
            _buildEventPublisher = buildEventPublisher;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void StartProcessing(object stateInfo)
        {
            Tracing.Server.TraceInformation("Server: StartProcessing (loop)");

            _lockExpiryTimer =
                new Timer(PollBuildServer, null, Timeout.Infinite, Timeout.Infinite);
            _lockExpiryTimer.Change(0, Timeout.Infinite);
        }

        private int GetPollingPeriod()
        {
            return Int32.Parse(_buildConfigurationManager.PollPeriod)*1000;
        }

        //Method that will be called periodically
        private void PollBuildServer(object state)
        {
            try
            {
                BuildManagerExceptionHelper.WithExceptionHandling(() => _eventSource
                    .GetBuildStoreEvents().ToList()
                    .ForEach(ProcessBuildEvent),
                    () =>
                        Tracing.Client.TraceInformation(
                            "Getting list of build store events"),
                    () => _lockExpiryTimer.Change(DueTime, GetPollingPeriod())
                    );
            }
            catch (Exception exception)
            {
                Tracing.Client.TraceError(String.Format(
                    "An Exception Occured while connecting TfsServer {0} ", exception));
            }
        }

        private void ProcessBuildEvent(BuildStoreEventArgs buildEvent)
        {
            Tracing.Client.TraceInformation("Build was requested for " + buildEvent.Data.BuildRequestedFor);

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
            //if key exists and turned on then dont send the notification
            if (ShouldDisablePublish())
            {
                DisplayPublishOnScreen(buildStoreEventArgs);
            }
            else
            {
                _buildEventPublisher.Publish(buildStoreEventArgs.Data.BuildName, buildStoreEventArgs.Data.Status);
            }
        }

        private static void DisplayPublishOnScreen(BuildStoreEventArgs buildStoreEventArgs)
        {
            if (buildStoreEventArgs.Type == BuildStoreEventType.Build)
            {
                if (buildStoreEventArgs.Data.Status == BuildExecutionStatus.Failed)
                {
                    ConsoleColor currentForegroundColor = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Build Failed!");
                    Console.ForegroundColor = currentForegroundColor;
                }

                else
                {
                    Console.WriteLine(buildStoreEventArgs.Data.Status.ToString());
                }
            }

            Tracing.Client.TraceInformation("Supressing Publish Event");
        }

        private bool ShouldDisablePublish()
        {
            string shouldDisablePublish =
                Convert.ToString(ConfigurationManager.AppSettings["DisablePublishNotification"]);

            if (shouldDisablePublish != null && String.CompareOrdinal(shouldDisablePublish, "true") == 0)
            {
                return true;
            }
            return false;
        }

        public void StopProcessing()
        {
        }

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
                }

                // Indicate that the instance has been disposed.
                _lockExpiryTimer = null;
                _disposed = true;
            }
        }
    }
}
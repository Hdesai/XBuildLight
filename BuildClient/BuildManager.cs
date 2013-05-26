using System;
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
        private static readonly object EventLocker = new object();
        private readonly IBuildConfigurationManager _buildConfigurationManager;
        private readonly IBuildEventPublisher _buildEventPublisher;
        private readonly IBuildStoreEventSource _eventSource;
        private AutoResetEvent _autoResetEvent;
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

            lock (EventLocker)
            {
                //If we are restarting
                if (_autoResetEvent != null)
                {
                    _autoResetEvent.Dispose();
                }

                _autoResetEvent = new AutoResetEvent(true);
                _lockExpiryTimer =
                    new Timer(PollBuildServer, _autoResetEvent, DueTime, GetPollingPeriod());
            }
        }

        private int GetPollingPeriod()
        {
            return Int32.Parse(_buildConfigurationManager.PollPeriod)*1000;
        }

        //Method that will be called periodically
        private void PollBuildServer(object state)
        {
            ExecuteOnSingleThread(() =>
                {
                    Tracing.Client.TraceInformation("Getting list of build store events");
                    _eventSource
                        .GetListOfBuildStoreEvents().ToList()
                        .ForEach(ProcessBuildEvent);
                });
        }

        private void ExecuteOnSingleThread(Action tAction)
        {
            BuildManagerExceptionHelper.WithExceptionHandling(
                tAction,
                () => _autoResetEvent.WaitOne(),
                () => _autoResetEvent.Set()
                );
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
            ThreadPool.QueueUserWorkItem(callback => _buildEventPublisher.Publish(buildStoreEventArgs));
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

                    if (_autoResetEvent != null)
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
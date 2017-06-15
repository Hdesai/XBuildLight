using System;
using BuildCommon;

namespace BuildClient
{
    public class ChannelFactoryBuildPublisher : BuildEventPublisher
    {
        private readonly ICachedChannelManager<IBuildStatusChange> _cachedChannelManager;
        private readonly INotifier _notifier;

        public ChannelFactoryBuildPublisher(INotifier notifier,
                                            ICachedChannelManager<IBuildStatusChange> cachedChannelManager)
        {
            _notifier = notifier;
            _cachedChannelManager = cachedChannelManager;
        }

        private IBuildStatusChange GetBuildStatusChangeChannel(string notificationAddress)
        {
            return _cachedChannelManager.CreateChannel(notificationAddress);
        }

        private IBuildStatusChange CreateChannel(string notificationAddress)
        {
            if (String.IsNullOrEmpty(notificationAddress))
            {
                throw new ArgumentException("Notification Address is not provided in configuration");
            }

            return GetBuildStatusChangeChannel(notificationAddress);
        }

        public override void Publish(int buildId,string buildName,BuildExecutionStatus buildStatus)
        {
            foreach (string serviceAddress in _notifier.GetNotificationAddress(buildId))
            {
                BuildManagerExceptionHelper.With(serviceAddress,
                    () =>
                        CreateChannel(serviceAddress)
                            .ExecuteOneWayCall(channel => Process(channel, buildStatus)),
                    () => Tracing.Client.TraceInformation("About to Publish... "),
                    () => Tracing.Client.TraceInformation("Sent to Publisher Target"));
            }
        }

        public override void PublishQualityChange(int buildId, string buildName,string buildQuality)
        {
            foreach (string serviceAddress in _notifier.GetNotificationAddress(buildId))
            {
                BuildManagerExceptionHelper.With(serviceAddress,
                    () =>
                        CreateChannel(serviceAddress)
                            .ExecuteOneWayCall(channel => Process(channel, buildQuality)),
                    () => Tracing.Client.TraceInformation("About to Publish... "),
                    () => Tracing.Client.TraceInformation("Sent to Publisher Target"));
            }
        }

        private static void Process(IBuildStatusChange notificationChannel, string buildQuality)
        {
            notificationChannel.OnBuildQualityChange(buildQuality);
            Tracing.Client.TraceInformation("Build Quality change to '{0}'",buildQuality);
        }

        private static void Process(IBuildStatusChange notificationChannel, BuildExecutionStatus buildStatus)
        {
            switch (buildStatus)
            {
                case BuildExecutionStatus.Succeeded:
                    notificationChannel.OnBuildSuceeded();
                    Tracing.Client.TraceInformation("Build Succeeded");
                    break;

                case BuildExecutionStatus.Failed:
                    notificationChannel.OnBuildFailed();
                    Tracing.Client.TraceInformation("Build Failed");
                    break;

                case BuildExecutionStatus.Stopped:
                    notificationChannel.OnBuildStopped();
                    Tracing.Client.TraceInformation("Build Stopped");
                    break;

                case BuildExecutionStatus.InProgress:
                    notificationChannel.OnBuildStarted();
                    Tracing.Client.TraceInformation("Build Started");
                    break;

                case
                    BuildExecutionStatus.PartiallySucceeded:
                    notificationChannel.OnBuildPartiallySucceeded();
                    Tracing.Client.TraceInformation("Build Partially Succeeded");
                    break;
            }
        }
    }
}
using System;
using BuildCommon;
using Microsoft.TeamFoundation.Build.Client;

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

        private IBuildStatusChange CreateChannel(string buildName)
        {
            string notificationAddress = _notifier.GetNotificationAddress(buildName);

            if (String.IsNullOrEmpty(notificationAddress))
            {
                throw new ArgumentException("Notification Address is not provided in configuration");
            }

            return GetBuildStatusChangeChannel(notificationAddress);
        }

        public override void Publish(string buildName,BuildExecutionStatus buildStatus)
        {
            string serviceAddress = _notifier.GetNotificationAddress(buildName);

            BuildManagerExceptionHelper.With(serviceAddress,
                                             () =>
                                             CreateChannel(buildName)
                                                 .ExecuteOneWayCall(channel => Process(channel, buildStatus)),
                                             () => Tracing.Client.TraceInformation("About to Publish... "),
                                             () => Tracing.Client.TraceInformation("Sent to Publisher Target"));
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
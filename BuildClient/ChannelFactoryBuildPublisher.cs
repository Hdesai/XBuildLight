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

        private IBuildStatusChange CreateChannel(BuildStoreEventArgs buildStoreEventArgs)
        {
            string notificationAddress = _notifier.GetNotificationAddress(buildStoreEventArgs.Data.BuildDefinition);

            if (String.IsNullOrEmpty(notificationAddress))
            {
                throw new ArgumentException("Notification Address is not provided in configuration");
            }

            return GetBuildStatusChangeChannel(notificationAddress);
        }

        public override void Publish(BuildStoreEventArgs buildStoreEventArgs)
        {
            string serviceAddress = _notifier.GetNotificationAddress(buildStoreEventArgs.Data.BuildDefinition);

            BuildManagerExceptionHelper.With(serviceAddress,
                                             () =>
                                             CreateChannel(buildStoreEventArgs)
                                                 .ExecuteOneWayCall(channel => Process(channel, buildStoreEventArgs)),
                                             () => Tracing.Client.TraceInformation("About to Publish... "),
                                             () => Tracing.Client.TraceInformation("Sent to Publisher Target"));
        }

        private static void Process(IBuildStatusChange notificationChannel, BuildStoreEventArgs buildStoreEventArgs)
        {
            switch (buildStoreEventArgs.Data.Status)
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

                case
                    BuildStatus.PartiallySucceeded:
                    notificationChannel.OnBuildPartiallySucceeded();
                    Tracing.Client.TraceInformation("Build Partially Succeeded");
                    break;
            }
        }
    }
}
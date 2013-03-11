using System;
using System.Linq;
using System.ServiceModel;
using BuildClient.Configuration;
using BuildCommon;
using Microsoft.TeamFoundation.Build.Client;

namespace BuildClient
{
    public class Notifier : INotifier
    {
        private readonly IBuildConfigurationManager _buildConfigurationManager;
        public Notifier(IBuildConfigurationManager buildConfigurationManager)
        {
            _buildConfigurationManager = buildConfigurationManager;
        }

        public IBuildStatusChange GetNotificationChannel(IBuildDefinition definition)
        {
            var notificationAddress = GetNotificationAddress(definition);


            if (notificationAddress!=null && !String.IsNullOrEmpty(notificationAddress))
            {
                var cf =
                new ChannelFactory<IBuildStatusChange>(new NetTcpBinding
                {
                    Security = new NetTcpSecurity() { Mode = SecurityMode.None }
                });

                return cf.CreateChannel(
                                new EndpointAddress(notificationAddress));
            }

            return null;
        }

        public string GetNotificationAddress(IBuildDefinition definition)
        {
            var buildName = definition.Name;
            var element = _buildConfigurationManager
                .BuildMappers
                .OfType<BuildMapperElement>()
                .FirstOrDefault(x => x.TfsBuildToMonitor == buildName);

            if (element != null) return element.NotificationAddress;

            return null;
        }
    }
}
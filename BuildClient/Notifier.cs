using System.Linq;
using BuildClient.Configuration;
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

        public string GetNotificationAddress(IBuildDefinition definition)
        {
            string buildName = definition.Name;
            BuildMapperElement element = _buildConfigurationManager
                .BuildMappers
                .OfType<BuildMapperElement>()
                .FirstOrDefault(x => x.TfsBuildToMonitor == buildName);

            if (element != null) return element.NotificationAddress;

            return null;
        }
    }
}
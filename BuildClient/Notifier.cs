using System.Collections.Generic;
using System.Linq;
using BuildClient.Configuration;

namespace BuildClient
{
    public class Notifier : INotifier
    {
        private readonly IBuildConfigurationManager _buildConfigurationManager;


        public Notifier(IBuildConfigurationManager buildConfigurationManager)
        {
            _buildConfigurationManager = buildConfigurationManager;
        }

        public IEnumerable<string> GetNotificationAddress(string buildName)
        {
            return _buildConfigurationManager
                .BuildMappers
                .Where(x => x.TfsBuildToMonitor == buildName)
                .Select(x => x.NotificationAddress);
        }
    }
}
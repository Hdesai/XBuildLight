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

        public IEnumerable<string> GetNotificationAddress(int buildId)
        {
            return _buildConfigurationManager
                .BuildMappers
                .Where(x => x.BuildId == buildId)
                .Select(x => x.NotificationAddress);
        }
    }
}
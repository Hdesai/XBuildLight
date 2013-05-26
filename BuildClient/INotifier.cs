using Microsoft.TeamFoundation.Build.Client;

namespace BuildClient
{
    public interface INotifier
    {
        //IBuildStatusChange GetNotificationChannel(IBuildDefinition definition);
        string GetNotificationAddress(IBuildDefinition definition);
    }
}
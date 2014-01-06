using Microsoft.TeamFoundation.Build.Client;

namespace BuildClient
{
    public interface INotifier
    {
        string GetNotificationAddress(string buildName);
    }
}
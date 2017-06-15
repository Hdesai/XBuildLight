using System.Collections.Generic;

namespace BuildClient
{
    public interface INotifier
    {
        IEnumerable<string> GetNotificationAddress(int buildId);
    }
}
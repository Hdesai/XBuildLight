using Microsoft.TeamFoundation.Build.Client;

namespace BuildClient
{
    public interface IBuildEventPublisher
    {
        void Publish(string buildName,BuildExecutionStatus buildStatus);
    }
}
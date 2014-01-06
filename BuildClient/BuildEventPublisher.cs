using Microsoft.TeamFoundation.Build.Client;

namespace BuildClient
{
    public abstract class BuildEventPublisher : IBuildEventPublisher
    {
        public abstract void Publish(string buildName,BuildExecutionStatus buildStatus);
    }
}
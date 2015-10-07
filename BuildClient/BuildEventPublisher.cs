namespace BuildClient
{
    public abstract class BuildEventPublisher : IBuildEventPublisher
    {
        public abstract void Publish(string buildName,BuildExecutionStatus buildStatus);
        public abstract void PublishQualityChange(string buildName, string buildQuality);
    }
}
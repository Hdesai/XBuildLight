namespace BuildClient
{
    public abstract class BuildEventPublisher : IBuildEventPublisher
    {
        public abstract void Publish(int buildId,string buildName,BuildExecutionStatus buildStatus);
        public abstract void PublishQualityChange(int buildId,string buildName, string buildQuality);
    }
}
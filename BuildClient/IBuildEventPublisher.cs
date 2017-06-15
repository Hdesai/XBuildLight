namespace BuildClient
{
    public interface IBuildEventPublisher
    {
        void Publish(int buildId,string buildName,BuildExecutionStatus buildStatus);
        void PublishQualityChange(int buildId,string buildName, string buildQuality);
    }
}
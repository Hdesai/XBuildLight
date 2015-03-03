namespace BuildClient
{
    public interface IBuildEventPublisher
    {
        void Publish(string buildName,BuildExecutionStatus buildStatus);
        void PublishQualityChange(string buildName, string buildQuality);
    }
}
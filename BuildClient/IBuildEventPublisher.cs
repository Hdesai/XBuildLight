namespace BuildClient
{
    public interface IBuildEventPublisher
    {
        void Publish(BuildStoreEventArgs buildStoreEventArgs);
    }
}
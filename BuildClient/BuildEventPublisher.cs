namespace BuildClient
{
    public abstract class BuildEventPublisher : IBuildEventPublisher
    {
        public abstract void Publish(BuildStoreEventArgs buildStoreEventArgs);
    }
}
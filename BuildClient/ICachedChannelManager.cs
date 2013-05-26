namespace BuildClient
{
    public interface ICachedChannelManager<T>
    {
        T CreateChannel(string address);
    }
}
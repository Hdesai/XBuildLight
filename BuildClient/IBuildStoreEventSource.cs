using System.Collections.Generic;

namespace BuildClient
{
    public interface IBuildStoreEventSource
    {
        IEnumerable<BuildStoreEventArgs> GetListOfBuildStoreEvents();
    }
}
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BuildClient
{
    public interface IBuildStoreEventSource
    {
        Task<IEnumerable<BuildStoreEventArgs>> GetBuildStoreEvents();
    }
}
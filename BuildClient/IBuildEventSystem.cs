using System.Collections.Generic;
using System.Threading.Tasks;

namespace BuildClient
{
    public interface IBuildEventSystem
    {
        Task<IEnumerable<BuildStoreEventArgs>> GetBuildStoreEvents();
    }
}
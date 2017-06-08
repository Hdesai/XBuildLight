using System.Collections.Generic;
using System.Threading.Tasks;

namespace BuildClient
{
    public class BuildStoreEventSource : IBuildStoreEventSource
    {
        private readonly IBuildEventSystem _buildEventSystem;
        public BuildStoreEventSource(IBuildEventSystem buildEventSystem)
        {
            _buildEventSystem = buildEventSystem;
        }
        
        public async Task<IEnumerable<BuildStoreEventArgs>> GetBuildStoreEvents()
        {
            return await _buildEventSystem.GetBuildStoreEvents();
        }
    }
}
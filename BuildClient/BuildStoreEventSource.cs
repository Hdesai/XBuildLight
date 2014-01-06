using System.Collections.Generic;

namespace BuildClient
{
    public class BuildStoreEventSource : IBuildStoreEventSource
    {
        private readonly IBuildEventSystem _buildEventSystem;
        public BuildStoreEventSource(IBuildEventSystem buildEventSystem)
        {
            _buildEventSystem = buildEventSystem;
        }
        
        public IEnumerable<BuildStoreEventArgs> GetBuildStoreEvents()
        {
            return _buildEventSystem.GetBuildStoreEvents();
        }
    }
}
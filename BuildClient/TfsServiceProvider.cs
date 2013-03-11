using System;
using Microsoft.TeamFoundation.Client;

namespace BuildClient
{
    public class TfsServiceProvider : IServiceProvider
    {
        private readonly Uri _projectCollectionUri;

        public TfsServiceProvider(string projectCollectionUri)
        {
            _projectCollectionUri = new Uri(projectCollectionUri);
        }

        public object GetService(Type serviceType)
        {
            var collection = new TfsTeamProjectCollection(_projectCollectionUri);
            object service = null;

            try
            {
                collection.EnsureAuthenticated();
                service = collection.GetService(serviceType);
            }
            catch (Exception ex)
            {
                Tracing.Client.TraceError(
                    String.Format("Error communication with TFS server: {0} detail error message {1} ",
                                  _projectCollectionUri, ex));
            }
            Tracing.Client.TraceInformation("Connection to TFS established.");
            return service;
        }
    }
}
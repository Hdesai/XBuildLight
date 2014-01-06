using System;
using System.Net;
using BuildClient.Configuration;
using BuildCommon;
using Microsoft.TeamFoundation.Client;

namespace BuildClient
{
    public class TfsServiceProvider : IServiceProvider
    {
        private readonly Uri _projectCollectionUri;
        private readonly IBuildConfigurationManager _buildConfigurationManager;
        public TfsServiceProvider(string projectCollectionUri,IBuildConfigurationManager buildConfigurationManager)
        {
            _projectCollectionUri = new Uri(projectCollectionUri);
            _buildConfigurationManager = buildConfigurationManager;
        }

        public object GetService(Type serviceType)
        {
            var collection = new TfsTeamProjectCollection(_projectCollectionUri);

            object service;

            try
            {

                if (_buildConfigurationManager.UseCredentialToAuthenticate)
                {
                    collection.Credentials = new NetworkCredential(_buildConfigurationManager.TfsAccountUserName,
                        _buildConfigurationManager.TfsAccountPassword, _buildConfigurationManager.TfsAccountDomain);
                }

                
                collection.EnsureAuthenticated();
                service = collection.GetService(serviceType);
            }
            catch (Exception ex)
            {
                Tracing.Client.TraceError(
                    String.Format("Error communication with TFS server: {0} detail error message {1} ",
                                  _projectCollectionUri, ex));
                throw;
            }
            Tracing.Client.TraceInformation("Connection to TFS established.");
            return service;
        }
    }
}
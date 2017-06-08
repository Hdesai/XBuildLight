using System;
using System.Configuration;

namespace BuildClient.Configuration
{
    public class BuildConfigurationManager : IBuildConfigurationManager
    {
        public string PollPeriod
        {
            get { return ConfigurationManager.AppSettings["PollPeriod"]; }
        }

        public string BuildDefinitionNameExclusionPattern
        {
            get { return ConfigurationManager.AppSettings["BuildDefinitionNameExclusionPattern"]; }
        }

        public string TeamFoundationUrl
        {
            get { return ConfigurationManager.AppSettings["teamFoundationUrl"]; }
        }


        public BuildMapperGroupElementCollection BuildMappers
        {
            get { return BuildMapperConfigSection.Current.BuildMappers; }
        }

        public bool UseCredentialToAuthenticate
        {
            get
            {
                bool output;
                if (bool.TryParse(ConfigurationManager.AppSettings["UseCredentialToAuthenticate"], out output))
                {
                    return output;
                }

                return false;
            }

        }

        public string TfsAccountDomain
        {
            get { return ConfigurationManager.AppSettings["TFSAccountDomain"]; }
        }

        public string TfsAccountUserName
        {
            get { return ConfigurationManager.AppSettings["TFSAccountUserName"]; }
        }

        public string TfsAccountPassword
        {
            get { return ConfigurationManager.AppSettings["TFSAccountPassword"]; }
        }

        public string PAFToken => ConfigurationManager.AppSettings["PAFToken"];
    }
}
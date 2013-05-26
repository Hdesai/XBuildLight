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
    }
}
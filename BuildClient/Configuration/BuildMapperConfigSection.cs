using System.Configuration;

namespace BuildClient.Configuration
{
    public class BuildMapperConfigSection : ConfigurationSection
    {
        [ConfigurationProperty("buildMappers", IsDefaultCollection = true)]
        public BuildMapperGroupElementCollection BuildMappers
        {
            get { return (BuildMapperGroupElementCollection) base["buildMappers"]; }
        }

        public static readonly BuildMapperConfigSection Current =
            (BuildMapperConfigSection)ConfigurationManager.GetSection("buildMapperConfigSection");
    }
}
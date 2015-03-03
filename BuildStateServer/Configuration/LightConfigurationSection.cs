using System.Configuration;

namespace BuildStateServer.Configuration
{
    public class LightConfigurationSection : ConfigurationSection
    {
        public static readonly LightConfigurationSection Current =
            (LightConfigurationSection)ConfigurationManager.GetSection("lightConfiguration");

        [ConfigurationProperty("rules")]
        public LightRuleCollection Rules
        {
            get { return (LightRuleCollection)base["rules"]; }
        }
    }
}

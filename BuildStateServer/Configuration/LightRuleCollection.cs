using System.Configuration;
using BuildCommon;

namespace BuildStateServer.Configuration
{
    [ConfigurationCollection(typeof(LightRuleElement))]
    public class LightRuleCollection : ElementCollection<LightRuleElement>
    {
        protected override string ElementName
        {
            get { return "add"; }
        }
    }
}

using System.Configuration;

namespace BuildClient.Configuration
{
    public class BuildMapperElement : ConfigurationElement, ICollectionElement
    {
        [ConfigurationProperty("TfsProjectToMonitor", IsKey = false, IsRequired = true)]
        public string TfsProjectToMonitor
        {
            get { return (string) this["TfsProjectToMonitor"]; }
            set { this["TfsProjectToMonitor"] = value; }
        }

        [ConfigurationProperty("TfsBuildToMonitor", IsKey = false, IsRequired = true)]
        public string TfsBuildToMonitor
        {
            get { return (string) this["TfsBuildToMonitor"]; }
            set { this["TfsBuildToMonitor"] = value; }
        }

        [ConfigurationProperty("NotificationAddress", IsKey = false, IsRequired = true)]
        public string NotificationAddress
        {
            get { return (string) this["NotificationAddress"]; }
            set { this["NotificationAddress"] = value; }
        }

        [ConfigurationProperty("name", IsKey = true, IsRequired = true)]
        public string Name
        {
            get { return (string) this["name"]; }
            set { this["name"] = value; }
        }
    }

    public class BuildMapperElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new BuildMapperElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((BuildMapperElement) element).Name;
        }
    }
}
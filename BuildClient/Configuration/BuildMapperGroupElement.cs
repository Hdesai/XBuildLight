using System.Configuration;
using BuildCommon;

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

    public class BuildMapperElementV2 : ConfigurationElement, ICollectionElement
    {
        [ConfigurationProperty("TfsProjectToMonitor", IsKey = false, IsRequired = true)]
        public string TfsProjectToMonitor
        {
            get { return (string)this["TfsProjectToMonitor"]; }
            set { this["TfsProjectToMonitor"] = value; }
        }

        [ConfigurationProperty("TfsBuildToMonitor", IsKey = false, IsRequired = true)]
        public string TfsBuildToMonitor
        {
            get { return (string)this["TfsBuildToMonitor"]; }
            set { this["TfsBuildToMonitor"] = value; }
        }

        [ConfigurationProperty("NotificationAddress", IsKey = false, IsRequired = true)]
        public string NotificationAddress
        {
            get { return (string)this["NotificationAddress"]; }
            set { this["NotificationAddress"] = value; }
        }

        [ConfigurationProperty("name", IsKey = false, IsRequired = true)]
        public string Name
        {
            get { return (string)this["name"]; }
            set { this["name"] = value; }
        }

        [ConfigurationProperty("buildId", IsKey = true, IsRequired = true)]
        public int BuildId
        {
            get { return (int)this["buildId"]; }
            set { this["buildId"] = value; }
        }
    }


    public class BuildMapperElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new BuildMapperElementV2();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((BuildMapperElementV2) element).Name;
        }
    }


}
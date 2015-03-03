using System;
using System.Configuration;
using BuildCommon;

namespace BuildStateServer.Configuration
{
    public class LightRuleElement: ConfigurationElement, ICollectionElement
    {
        [ConfigurationProperty("name", IsKey = true, IsRequired = true)]
        public string Name
        {
            get { return (string)this["name"]; }
            set { this["name"] = value; }
        }

        [ConfigurationProperty("red", IsKey = false, IsRequired = false, DefaultValue = false)]
        public bool Red
        {
            get { return Convert.ToBoolean(this["red"]); }
            set { this["red"] = value.ToString(); }
        }

        [ConfigurationProperty("flashRed", IsKey = false, IsRequired = false, DefaultValue = false)]
        public bool FlashRed
        {
            get { return Convert.ToBoolean(this["flashRed"]); }
            set { this["flashRed"] = value.ToString(); }
        }

        [ConfigurationProperty("blue", IsKey = false, IsRequired = false, DefaultValue = false)]
        public bool Blue
        {
            get { return Convert.ToBoolean(this["blue"]); }
            set { this["blue"] = value.ToString(); }
        }

        [ConfigurationProperty("flashBlue", IsKey = false, IsRequired = false, DefaultValue = false)]
        public bool FlashBlue
        {
            get { return Convert.ToBoolean(this["flashBlue"]); }
            set { this["flashBlue"] = value.ToString(); }
        }

        [ConfigurationProperty("green", IsKey = false, IsRequired = false, DefaultValue = false)]
        public bool Green
        {
            get { return Convert.ToBoolean(this["green"]); }
            set { this["green"] = value.ToString(); }
        }

        [ConfigurationProperty("flashGreen", IsKey = false, IsRequired = false, DefaultValue = false)]
        public bool FlashGreen
        {
            get { return Convert.ToBoolean(this["flashGreen"]); }
            set { this["flashGreen"] = value.ToString(); }
        }
    }
}
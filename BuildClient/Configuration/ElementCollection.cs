using System.Configuration;

namespace BuildClient.Configuration
{
    public class ElementCollection<TElement> : ConfigurationElementCollection
        where TElement : ConfigurationElement, ICollectionElement, new()
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new TElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((TElement)element).Name;
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        public TElement this[int index]
        {
            get { return (TElement)BaseGet(index); }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }
    }
}
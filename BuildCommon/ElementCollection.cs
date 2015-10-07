using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace BuildCommon
{
    public class ElementCollection<TElement> : ConfigurationElementCollection, IEnumerable<TElement> where TElement : ConfigurationElement, ICollectionElement, new()
    {
        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        public TElement this[int index]
        {
            get { return (TElement) BaseGet(index); }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new TElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((TElement) element).Name;
        }

        IEnumerator<TElement> IEnumerable<TElement>.GetEnumerator()
        {
            return (from i in Enumerable.Range(0, this.Count)
                    select this[i])
                .GetEnumerator();
        }
    }
}
namespace BuildClient.Configuration
{
    public sealed class BuildMapperGroupElementCollection : ElementCollection<BuildMapperElement>
    {
        /// <summary>
        /// Gets the name used to identify this collection of elements in the configuration file when overridden in a derived class.
        /// </summary>
        /// <value></value>
        /// <returns>The name of the collection; otherwise, an empty string. The default is an empty string.</returns>
        protected override string ElementName
        {
            get { return "buildMapper"; }
        }
    }

    public interface ICollectionElement
    {
        string Name { get; }
    }

   

}
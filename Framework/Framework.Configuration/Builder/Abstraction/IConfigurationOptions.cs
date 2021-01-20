namespace ZTR.Framework.Configuration.Builder.Abstraction
{
    #pragma warning disable CS1591
    public interface IConfigurationOptions
    {
        /// <summary>
        /// Gets the name of the section.
        /// </summary>
        /// <value>
        /// The name of the section.
        /// </value>
        string SectionName { get; }
    }
    #pragma warning restore CS1591
}

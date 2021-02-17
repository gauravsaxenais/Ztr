namespace ZTR.Framework.Configuration.Builder.Abstraction
{
    /// <summary>
    /// This interface maps with appsettings file.
    /// </summary>
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
}

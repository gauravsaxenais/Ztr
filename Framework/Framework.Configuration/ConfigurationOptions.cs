namespace ZTR.Framework.Configuration
{
    using System;
    using Builder.Abstraction;

    /// <summary>
    /// ConfigurationOptions
    /// </summary>
    /// <seealso cref="IConfigurationOptions" />
    public abstract class ConfigurationOptions : IConfigurationOptions
    {
        private const string Suffix = "Options";
        private string _sectionName;

        /// <summary>
        /// Gets or sets the name of the section.
        /// </summary>
        /// <value>
        /// The name of the section.
        /// </value>
        public string SectionName
        {
            get => GetSectionName();

            set => _sectionName = value;
        }

        private string GetSectionName()
        {
            if (string.IsNullOrWhiteSpace(_sectionName))
            {
                var className = GetType().Name;

                if (className.EndsWith(Suffix, StringComparison.OrdinalIgnoreCase))
                {
                    _sectionName = className.Substring(0, className.Length - Suffix.Length);
                }
            }

            return _sectionName;
        }
    }
}

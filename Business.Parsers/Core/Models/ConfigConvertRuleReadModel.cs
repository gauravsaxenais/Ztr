using System;

namespace Business.Parsers.Core.Models
{
    /// <summary>
    /// Config Convert Model
    /// </summary>
    public class ConfigConvertRuleReadModel
    {
        /// <summary>
        /// Gets or sets the property.
        /// </summary>
        /// <value>
        /// The property.
        /// </value>
        public string Property { get; set; }
        public string Value { get; set; }
        /// <summary>
        /// Gets or sets the schema.
        /// </summary>
        /// <value>
        /// The schema.
        /// </value>
        public ConversionScheme Schema { get; set; }
        public static ConversionScheme TryScheme(string value)
        {
            return (ConversionScheme)Enum.Parse(typeof(ConversionScheme), value, true);
        }
    }
}

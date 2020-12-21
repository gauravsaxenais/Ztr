namespace Business.Models
{
    using System.Collections.Generic;

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

        /// <summary>
        /// Gets or sets the schema.
        /// </summary>
        /// <value>
        /// The schema.
        /// </value>
        public IList<ConfigConvertObjectReadModel> Schema { get; set; }
    }
}

using System;

namespace Business.Models
{
    using Newtonsoft.Json;

    // name = "A_low", label = "12V Low", description = "12V min value.", type = "integer", min = "0", max = "200" },
    /// <summary>
    ///   <br />
    /// </summary>
    public class NetworkArgumentReadModel : ICloneable
    {
        /// <summary>Gets or sets the identifier.</summary>
        /// <value>The identifier.</value>
        public int Id { get; set; }
        /// <summary>Gets or sets the name.</summary>
        /// <value>The name.</value>
        public string Name { get; set; }
        /// <summary>Gets or sets the label.</summary>
        /// <value>The label.</value>
        public string Label { get; set; }
        /// <summary>Gets or sets the description.</summary>
        /// <value>The description.</value>
        public string Description { get; set; }
        /// <summary>Gets or sets the type of the data.</summary>
        /// <value>The type of the data.</value>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }
        /// <summary>
        /// Gets or sets the type of the data.
        /// </summary>
        /// <value>
        /// The type of the data.
        /// </value>
        [JsonProperty(PropertyName = "datatype")]
        public string DataType { get; set; }
        /// <summary>Determines the minimum of the parameters.</summary>
        /// <value>The minimum.</value>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object Min { get; set; }
        /// <summary>Determines the maximum of the parameters.</summary>
        /// <value>The maximum.</value>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object Max { get; set; }
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public string Value { get; set; }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public object Clone()
        {
            var other = (NetworkArgumentReadModel)MemberwiseClone();

            DeepCopy(other);

            return other;
        }

        private void DeepCopy(NetworkArgumentReadModel other)
        {
            other.Name = Name;
            other.Label = Label;
            other.Description = Description;
            other.Type = Type;
            other.Value = Value;
            other.Min = Min;
            other.Max = Max;
            other.DataType = DataType;
        }
    }
}

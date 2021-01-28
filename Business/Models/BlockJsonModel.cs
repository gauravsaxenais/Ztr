using System.Linq;

namespace Business.Models
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///   Returns list of arguments for block as a json.
    /// </summary>
    public class BlockJsonModel : ICloneable
    {
        /// <summary>Gets or sets the identifier.</summary>
        /// <value>The identifier.</value>
        public int Id { get; set; }
        /// <summary>Gets or sets the type.</summary>
        /// <value>The type.</value>
        public string Type { get; set; }
        /// <summary>Gets or sets the tag.</summary>
        /// <value>The tag.</value>
        public string Tag { get; set; }
        /// <summary>Gets the arguments.</summary>
        /// <value>The arguments.</value>
        public List<NetworkArgumentReadModel> Args { get; } = new List<NetworkArgumentReadModel>();
        /// <summary>
        /// Gets the modules.
        /// </summary>
        /// <value>
        /// The modules.
        /// </value>
        public List<string> Modules { get; } = new List<string>();

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public object Clone()
        {
            var other = new BlockJsonModel {Id = Id};
            other.Modules.AddRange(Modules);

            DeepCopy(other);

            return other;
        }

        private void DeepCopy(BlockJsonModel other)
        {
            other.Type = Type;
            other.Tag = Tag;

            var args = Args.Select(x => (NetworkArgumentReadModel)x.Clone()).ToList();
            other.Args.AddRange(args);
        }
    }
}

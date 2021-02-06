namespace Business.Models
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    /// <summary>
    ///   Returns list of arguments for block as a json.
    /// </summary>
    public class BlockJsonModel : ICloneable, IEquatable<BlockJsonModel>
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
            var other = new BlockJsonModel { Id = Id };
            other.Modules.AddRange(Modules);

            DeepCopy(other);

            return other;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.
        /// </returns>
        public bool Equals([AllowNull] BlockJsonModel other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return string.Equals(Type, other.Type);
        }
        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((BlockJsonModel)obj);
        }
        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(BlockJsonModel left, BlockJsonModel right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(BlockJsonModel left, BlockJsonModel right)
        {
            return !Equals(left, right);
        }
        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Type != null ? Type.GetHashCode() : 0);
                return hashCode;
            }
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

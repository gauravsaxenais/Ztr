namespace Business.Models
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Checks whether the two modulereadmodels are equal or not.
    /// </summary>
    public class ModuleReadModelComparer : IEqualityComparer<ModuleReadModel>
    {
        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <param name="x">The first object of type <paramref name="x" /> to compare.</param>
        /// <param name="y">The second object of type <paramref name="y" /> to compare.</param>
        /// <returns>
        ///   <see langword="true" /> if the specified objects are equal; otherwise, <see langword="false" />.
        /// </returns>
        public bool Equals([AllowNull] ModuleReadModel x, [AllowNull] ModuleReadModel y)
        {
            if (x == null || y == null)
            {
                return false;
            }

            return x.UUID == y.UUID &&
                  x.Name == y.Name;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public int GetHashCode([DisallowNull] ModuleReadModel obj)
        {
            return HashCode.Combine(obj.Name, obj.UUID);
        }
    }
}

namespace ZTR.Framework.Business
{
    using System.Collections.Generic;
    using System.Linq;
    using EnsureThat;

    /// <summary>
    /// Extension class for EnsureArg.
    /// </summary>
    public static class EnsureArgExtensions
    {
        /// <summary>
        /// Determines whether the specified items has items.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items">The items.</param>
        /// <param name="paramName">Name of the parameter.</param>
        public static void HasItems<T>(IEnumerable<T> items, string paramName = null)
        {
            EnsureArg.IsNotNull(items, nameof(items));
            EnsureArg.IsTrue(items.Any(), paramName, options => options.WithMessage("Empty collection is not allowed."));
        }
    }
}

namespace ZTR.Framework.Business
{
    /// <summary>
    /// IIndexedItem
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IIndexedItem<out T>
    {
        /// <summary>
        /// Gets the ordinal position.
        /// </summary>
        /// <value>
        /// The ordinal position.
        /// </value>
        int OrdinalPosition { get; }

        /// <summary>
        /// Gets the item.
        /// </summary>
        /// <value>
        /// The item.
        /// </value>
        T Item { get; }
    }
}

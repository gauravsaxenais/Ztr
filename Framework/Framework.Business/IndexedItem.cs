namespace ZTR.Framework.Business
{
    public sealed class IndexedItem<T> : IIndexedItem<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IndexedItem{T}"/> class.
        /// </summary>
        /// <param name="ordinalPosition">The ordinal position.</param>
        /// <param name="item">The item.</param>
        public IndexedItem(int ordinalPosition, T item)
        {
            OrdinalPosition = ordinalPosition;
            Item = item;
        }

        /// <summary>
        /// Gets the ordinal position.
        /// </summary>
        /// <value>
        /// The ordinal position.
        /// </value>
        public int OrdinalPosition { get; }

        /// <summary>
        /// Gets the item.
        /// </summary>
        /// <value>
        /// The item.
        /// </value>
        public T Item { get; }
    }
}

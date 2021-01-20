namespace ZTR.Framework.Business
{
    using System.Collections.Generic;

    /// <summary>
    /// GroupedItem
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    public sealed class GroupedItem<TItem, TKey>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupedItem{TItem, TKey}"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="items">The items.</param>
        public GroupedItem(TKey key, IEnumerable<TItem> items)
        {
            Key = key;
            Items = items;
        }

        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public TKey Key { get; set; }

        /// <summary>
        /// Gets or sets the items.
        /// </summary>
        /// <value>
        /// The items.
        /// </value>
        public IEnumerable<TItem> Items { get; set; }
    }
}

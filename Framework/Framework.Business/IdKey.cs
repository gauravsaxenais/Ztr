namespace ZTR.Framework.Business
{
    /// <summary>
    /// IdKey
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class IdKey<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IdKey{T}"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="key">The key.</param>
        public IdKey(long id, T key)
        {
            Id = id;
            Key = key;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdKey{T}"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        public IdKey(T key)
        {
            Id = null;
            Key = key;
        }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public long? Id { get; set; }

        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public T Key { get; set; }
    }
}

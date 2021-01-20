namespace ZTR.Framework.Business
{
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// WrapperObject
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="System.Collections.Generic.IEnumerable{T}" />
    public class WrapperObject<T> : IEnumerable<T>
        where T : class
    {
        private readonly List<T> _models = new List<T>();

        /// <summary>
        /// Initializes a new instance of the <see cref="WrapperObject{T}"/> class.
        /// </summary>
        /// <param name="models">The models.</param>
        public WrapperObject(IEnumerable<T> models)
        {
            _models.AddRange(models);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WrapperObject{T}"/> class.
        /// </summary>
        protected WrapperObject()
        {
        }

        /// <summary>
        /// Adds the specified model.
        /// </summary>
        /// <param name="model">The model.</param>
        public void Add(T model)
        {
            _models.Add(model);
        }

        /// <summary>
        /// Adds the range.
        /// </summary>
        /// <param name="models">The models.</param>
        public void AddRange(IEnumerable<T> models)
        {
            _models.AddRange(models);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            return _models.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

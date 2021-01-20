namespace ZTR.Framework.DataAccess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// IQueryableExtensions
    /// </summary>
    public static class IQueryableExtensions
    {
        /// <summary>
        /// Returns an IQueryable of the chosen type
        /// </summary>
        /// <typeparam name="TModel">The type of the models</typeparam>
        /// <typeparam name="TEntity">The type of IQueryable</typeparam>
        /// <typeparam name="TReturnType">The type of the returned list</typeparam>
        /// <param name="entities">the query to reorder</param>
        /// <param name="models">the entities in the correct order</param>
        /// <param name="entityKey">the func choosing your entity key</param>
        /// <param name="entityValue">the func choosing your value to be returned</param>
        /// <param name="modelKey">the func choosing your model key (this should be the same type as your entityKey)</param>
        /// <returns>A list in the order of the given entities</returns>
        public static List<TReturnType> OrderEntitiesByModelsOrder<TModel, TEntity, TReturnType>(this IQueryable<TEntity> entities, IEnumerable<TModel> models, Func<TEntity, object> entityKey, Func<TEntity, TReturnType> entityValue, Func<TModel, object> modelKey)
        {
            var returnedModels = new List<TReturnType>();
            var entityDictionary = entities.ToDictionary(e => entityKey(e), e => entityValue(e));

            foreach (TModel model in models)
            {
                TReturnType entity = entityDictionary[modelKey(model)];
                returnedModels.Add(entity);
            }

            return returnedModels;
        }
    }
}

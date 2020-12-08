namespace ZTR.Framework.Business
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using EnsureThat;
    using FluentValidation;
    using Microsoft.Extensions.Logging;

    public abstract class BaseCommandManager<TErrorCode, TEntity> : Manager
        where TErrorCode : struct, Enum
        where TEntity : class
    {
        public BaseCommandManager(ILogger<BaseCommandManager<TErrorCode, TEntity>> logger)
            : base(logger)
        {
        }

        protected virtual async Task<ManagerResponse<TErrorCode>> DeleteByExpressionAsync<T>(IEnumerable<T> keys, Expression<Func<TEntity, bool>> predicate)
        {
            try
            {
                EnsureArg.IsNotNull(predicate, nameof(predicate));

                var entities = new List<TEntity>();

                var errorRecords = await DeleteValidationAsync(keys, entities).ConfigureAwait(false);

                return new ManagerResponse<TErrorCode>(errorRecords);
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, nameof(DeleteByExpressionAsync));
                return new ManagerResponse<TErrorCode>(ex);
            }
        }

        protected virtual async Task<ErrorRecords<TErrorCode>> DeleteValidationAsync<T>(IEnumerable<T> keys, IList<TEntity> entities)
        {
            return await Task.FromResult(new ErrorRecords<TErrorCode>()).ConfigureAwait(false);
        }
    }
}

namespace ZTR.Framework.Business
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using EnsureThat;
    using Microsoft.Extensions.Logging;

    public abstract class CommandManager<TDbContext, TErrorCode, TEntity, TCreateModel, TUpdateModel> : BaseCommandManager<TErrorCode, TEntity>
        where TErrorCode : struct, Enum
        where TEntity : class
        where TCreateModel : class, IModel
        where TUpdateModel : class, TCreateModel, IModelWithId
    {
        protected CommandManager(ModelValidator<TCreateModel> createModelValidator, ModelValidator<TUpdateModel> updateModelValidator, ILogger<CommandManager<TDbContext, TErrorCode, TEntity, TCreateModel, TUpdateModel>> logger, IMapper mapper, TErrorCode idDoesNotExist, TErrorCode idNotUnique)
            : base(logger)
        {
            EnsureArg.IsNotNull(createModelValidator, nameof(createModelValidator));
            EnsureArg.IsNotNull(updateModelValidator, nameof(updateModelValidator));
            EnsureArg.IsNotNull(mapper, nameof(mapper));

            CreateModelValidator = createModelValidator;
            UpdateModelValidator = updateModelValidator;
            Mapper = mapper;
            IdDoesNotExist = idDoesNotExist;
            IdNotUnique = idNotUnique;
        }

        protected TErrorCode IdDoesNotExist { get; }

        protected TErrorCode IdNotUnique { get; }

        protected IMapper Mapper { get; }

        protected ModelValidator<TCreateModel> CreateModelValidator { get; }

        protected ModelValidator<TUpdateModel> UpdateModelValidator { get; }

        public async Task<ManagerResponse<TErrorCode>> CreateAsync(TCreateModel model, params TCreateModel[] models)
        {
            try
            {
                EnsureArg.IsNotNull(model, nameof(model));

                return await CreateAsync(models.Prepend(model)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, nameof(CreateAsync));
                return new ManagerResponse<TErrorCode>(ex);
            }
        }

        public virtual async Task<ManagerResponse<TErrorCode>> CreateAsync(IEnumerable<TCreateModel> models)
        {
            try
            {
                ValidateModel(models);
                var indexedModels = models.ToIndexedItems().ToList();
                var errorRecords = CreateModelValidator.ExecuteCreateValidation<TErrorCode, TCreateModel>(indexedModels);
                var customErrorRecords = await CreateValidationAsync(indexedModels).ConfigureAwait(false);

                return new ManagerResponse<TErrorCode>(customErrorRecords);
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, nameof(CreateAsync));
                return new ManagerResponse<TErrorCode>(ex);
            }
        }

        public async Task<ManagerResponse<TErrorCode>> UpdateAsync(TUpdateModel model, params TUpdateModel[] models)
        {
            try
            {
                EnsureArg.IsNotNull(model, nameof(model));

                return await UpdateAsync(models.Prepend(model).ToArray()).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, nameof(UpdateAsync));
                return new ManagerResponse<TErrorCode>(ex);
            }
        }

        public virtual async Task<ManagerResponse<TErrorCode>> UpdateAsync(IEnumerable<TUpdateModel> models)
        {
            try
            {
                ValidateModel(models);
                var indexedModels = models.ToIndexedItems().ToList();
                var errorRecords = UpdateModelValidator.ExecuteUpdateValidation<TErrorCode, TUpdateModel>(indexedModels);

                return new ManagerResponse<TErrorCode>(errorRecords);
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, nameof(UpdateAsync));
                return new ManagerResponse<TErrorCode>(ex);
            }
        }

        protected virtual async Task<ErrorRecords<TErrorCode>> CreateValidationAsync(IList<IIndexedItem<TCreateModel>> indexedModels)
        {
            Logger.LogDebug($"Calling {nameof(CreateValidationAsync)}");

            return await Task.FromResult(new ErrorRecords<TErrorCode>()).ConfigureAwait(false);
        }

        protected virtual Task CreateAfterMapAsync(IList<IIndexedItem<TCreateModel>> indexedItems, IList<TEntity> entities)
        {
            return Task.CompletedTask;
        }

        protected virtual Task UpdateAfterMapAsync(IList<IIndexedItem<TUpdateModel>> indexedItems, IList<TEntity> entities)
        {
            return Task.CompletedTask;
        }

        private void ValidateModel<TModel>(IEnumerable<TModel> models)
        {
            EnsureArg.IsNotNull(models, nameof(models));
            EnsureArgExtensions.HasItems(models, nameof(models));
        }
    }
}

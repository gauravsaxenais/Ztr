namespace ZTR.Framework.Business
{
    using FluentValidation;

    /// <summary>
    /// ModelValidator.
    /// </summary>
    /// <typeparam name="TModel">The type of the model.</typeparam>
    /// <seealso cref="AbstractValidator{TModel}" />
    public class ModelValidator<TModel> : AbstractValidator<TModel>
        where TModel : IModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModelValidator{TModel}"/> class.
        /// </summary>
        public ModelValidator()
        {
        }
    }
}

namespace ZTR.Framework.Business
{
    using FluentValidation;

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

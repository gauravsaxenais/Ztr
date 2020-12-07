namespace ZTR.Framework.Business
{
    using System;
    using ZTR.Framework.DataAccess;
    using FluentValidation;

    public static class RuleBuilderExtensions
    {
        public static IRuleBuilderOptions<TModel, long> IdValidation<TModel>(this IRuleBuilder<TModel, long> ruleBuilder, Enum idMustBeGreaterThanZero)
            where TModel : class, IModelWithId
        {
            return ruleBuilder
                .GreaterThan(0).WithErrorEnum(idMustBeGreaterThanZero);
        }

        public static IRuleBuilderOptions<TModel, string> UrlValidation<TModel>(this IRuleBuilder<TModel, string> ruleBuilder, Enum urlNotWellFormed, UriKind uriKind = UriKind.Absolute)
            where TModel : class
        {
            return ruleBuilder
                .Must(x => Uri.IsWellFormedUriString(x, uriKind)).WithMessage("The format of '{PropertyName}' must be a well formed URL.").WithErrorEnum(urlNotWellFormed);
        }
    }
}

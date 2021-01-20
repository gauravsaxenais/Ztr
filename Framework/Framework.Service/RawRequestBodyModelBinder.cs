namespace ZTR.Framework.Service
{
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc.ModelBinding;

    /// <summary>
    /// RawRequestBodyModelBinder
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.ModelBinding.IModelBinder" />
    public class RawRequestBodyModelBinder : IModelBinder
    {
        /// <summary>
        /// Attempts to bind a model.
        /// </summary>
        /// <param name="bindingContext">The <see cref="T:Microsoft.AspNetCore.Mvc.ModelBinding.ModelBindingContext" />.</param>
        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var request = bindingContext.HttpContext.Request;

            using var reader = new StreamReader(request.Body);
            var content = await reader.ReadToEndAsync().ConfigureAwait(false);
            bindingContext.Result = ModelBindingResult.Success(content);
        }
    }
}

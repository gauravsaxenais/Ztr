namespace ZTR.Framework.Service
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNetCore.Http;
    using Microsoft.OpenApi.Models;
    using Swashbuckle.AspNetCore.SwaggerGen;

    /// <summary>
    /// FileUploadOperationFilter
    /// </summary>
    /// <seealso cref="Swashbuckle.AspNetCore.SwaggerGen.IOperationFilter" />
    public class FileUploadOperationFilter : IOperationFilter
    {
        /// <summary>
        /// Applies the specified operation.
        /// </summary>
        /// <param name="operation">The operation.</param>
        /// <param name="context">The context.</param>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (context.ApiDescription.ParameterDescriptions.Any(x => x.ModelMetadata.ContainerType == typeof(IFormFile)))
            {
                var formFileParameterName = context
                    .ApiDescription
                    .ActionDescriptor
                    .Parameters
                    .Where(x => x.ParameterType == typeof(IFormFile))
                    .Select(x => x.Name)
                    .First();

                var uploadFileMediaType = new OpenApiMediaType()
                {
                    Schema = new OpenApiSchema()
                    {
                        Type = "object",
                        Properties =
                    {
                        ["UploadedFile"] = new OpenApiSchema()
                        {
                            Description = "Upload File",
                            Type = "file",
                            Format = "binary"
                        }
                    },
                        Required = new HashSet<string>()
                    {
                        "UploadedFile"
                    }
                    }
                };

                operation.RequestBody = new OpenApiRequestBody
                {
                    Content =
                {
                    [SupportedContentTypes.MultipartFormData] = uploadFileMediaType
                }
                };
            }
        }
    }
}

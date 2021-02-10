namespace ZTR.Framework.Service
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Configuration;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Rewrite;
    using Swashbuckle.AspNetCore.SwaggerUI;

    /// <summary>
    /// SwaggerConfigurationExtension
    /// </summary>
    public static class SwaggerConfigurationExtension
    {
        /// <summary>
        /// Uses the swagger.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <param name="apiVersion">The API version.</param>
        /// <param name="apiName">Name of the API.</param>
        /// <param name="alwaysShowInSwaggerUI">if set to <c>true</c> [always show in swagger UI].</param>
        /// <param name="isOpenAuth">if set to <c>true</c> [is open authentication].</param>
        public static void UseSwagger(this IApplicationBuilder app, string apiVersion, string apiName, bool alwaysShowInSwaggerUI = false, bool isOpenAuth = false)
        {
            ConfigureSwaggerUI(app, (swaggerUIOptions) =>
            {
                AddSwaggerEndpointToUi(swaggerUIOptions, new[] { new SwaggerConfigurationModel(apiVersion, apiName, alwaysShowInSwaggerUI) });
            }, isOpenAuth: isOpenAuth);
        }

        /// <summary>
        /// Uses the swagger.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <param name="swaggerConfigurationModels">The swagger configuration models.</param>
        /// <param name="isOpenAuth">if set to <c>true</c> [is open authentication].</param>
        public static void UseSwagger(this IApplicationBuilder app, IEnumerable<SwaggerConfigurationModel> swaggerConfigurationModels, bool isOpenAuth = false)
        {
            ConfigureSwaggerUI(app, (swaggerUIOptions) =>
            {
                if (ApplicationConfiguration.IsDevelopment)
                {
                    // add all endpoints in dev
                    AddSwaggerEndpointToUi(swaggerUIOptions, swaggerConfigurationModels);
                }
                else
                {
                    // add only items to the swagger ui that are listed to always show
                    var swaggerItemsToShow = swaggerConfigurationModels.Where(item => item.AlwaysShowInSwaggerUI);
                    AddSwaggerEndpointToUi(swaggerUIOptions, swaggerItemsToShow);
                }
            }, isOpenAuth);
        }

        private static void ConfigureSwaggerUI(this IApplicationBuilder app, Action<SwaggerUIOptions> configureEndPoints, bool isOpenAuth)
        {
            app.UseSwagger();
            app.UseSwaggerUI(swaggerUIOptions =>
            {
                configureEndPoints(swaggerUIOptions);
                swaggerUIOptions.DisplayOperationId();
                swaggerUIOptions.DocExpansion(DocExpansion.None);
            });

            app.UseRewriter(new RewriteOptions().AddRedirect("^$", "swagger"));
        }

        private static void AddSwaggerEndpointToUi(SwaggerUIOptions swaggerUIOptions, IEnumerable<SwaggerConfigurationModel> swaggerConfigurations)
        {
            foreach (var swaggerConfiguration in swaggerConfigurations)
            {
                swaggerUIOptions.SwaggerEndpoint($"/swagger/{swaggerConfiguration.ApiVersion}/swagger.json", $"{swaggerConfiguration.ApiName} {swaggerConfiguration.ApiVersion}");
            }
        }
    }
}

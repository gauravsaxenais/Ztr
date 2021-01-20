namespace ZTR.Framework.Service
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using EnsureThat;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.OpenApi.Models;

    /// <summary>
    /// SwaggerServicesExtensions
    /// </summary>
    public static class SwaggerServicesExtensions
    {
        private const string FileSchemaType = "file";

        /// <summary>
        /// Adds the swagger with comments.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="apiName">Name of the API.</param>
        /// <param name="apiVersion">The API version.</param>
        /// <param name="apiDescription">The API description.</param>
        /// <param name="assemblies">The assemblies.</param>
        /// <returns></returns>
        public static IServiceCollection AddSwaggerWithComments(this IServiceCollection services, string apiName, string apiVersion, string apiDescription, IEnumerable<Assembly> assemblies)
        {
            EnsureArg.IsNotNull(assemblies, nameof(assemblies));
            EnsureArg.IsNotNullOrWhiteSpace(apiName, nameof(apiName));
            EnsureArg.IsNotNullOrWhiteSpace(apiVersion, nameof(apiVersion));
            EnsureArg.IsNotNullOrWhiteSpace(apiDescription, nameof(apiDescription));

            services.AddSwaggerGen(swaggerOptions =>
            {
#pragma warning disable CS0618 // Type or member is obsolete
                swaggerOptions.DescribeAllEnumsAsStrings();
#pragma warning restore CS0618 // Type or member is obsolete

                var swaggerInfo = new OpenApiInfo
                {
                    Title = apiName,
                    Version = apiVersion,
                    Description = apiDescription,
                    Contact = new OpenApiContact(),
                    License = new OpenApiLicense()
                };

                swaggerOptions.SwaggerDoc(apiVersion, swaggerInfo);

                foreach (var assembly in assemblies)
                {
                    var xmlPath = Path.Combine(AppContext.BaseDirectory, $"{assembly.GetName().Name}.xml");
                    if (File.Exists(xmlPath))
                    {
                        swaggerOptions.IncludeXmlComments(xmlPath);
                        swaggerOptions.CustomOperationIds(e => $"{e.ActionDescriptor.RouteValues["controller"]}{e.ActionDescriptor.RouteValues["action"]}");
                    }
                }

                // use full name of classes in case two classes with same name are being
                // used 
                swaggerOptions.CustomSchemaIds(item => item.FullName);

                swaggerOptions.IgnoreObsoleteProperties();

                if (!swaggerOptions.SchemaGeneratorOptions.CustomTypeMappings.ContainsKey(typeof(FileContentResult)))
                {
                    // Swagger Configurations
                    swaggerOptions.MapType<FileContentResult>(() => new OpenApiSchema
                    {
                        Type = FileSchemaType
                    });
                }
            });

            return services;
        }

        /// <summary>
        /// Adds the swagger with comments.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="apiName">Name of the API.</param>
        /// <param name="apiVersion">The API version.</param>
        /// <param name="apiDescription">The API description.</param>
        /// <param name="assembly">The assembly.</param>
        /// <param name="assemblies">The assemblies.</param>
        /// <returns></returns>
        public static IServiceCollection AddSwaggerWithComments(this IServiceCollection services, string apiName, string apiVersion, string apiDescription, Assembly assembly, params Assembly[] assemblies)
        {
            EnsureArg.IsNotNull(assembly, nameof(assembly));

            return AddSwaggerWithComments(services, apiName, apiVersion, apiDescription, assemblies.Prepend(assembly));
        }
    }
}

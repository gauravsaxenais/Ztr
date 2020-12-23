using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Business.Parsers.Core.Converter
{
    public static class Inject
    {
        public static void AddConverters(this IServiceCollection services)
        {
            services.AddScoped<ConverterService>();
            services.AddScoped<IJsonConverter, JsonConverter>();
            services.AddSingleton<IBuilder<IDictionary<string, object>>, TomlBuilder>();

            services.AddScoped<ConvertConfig>();
           
        }
    }
}

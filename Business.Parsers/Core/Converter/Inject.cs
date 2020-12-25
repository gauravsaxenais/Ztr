namespace Business.Parsers.Core.Converter
{
    using Microsoft.Extensions.DependencyInjection;
    using System.Collections.Generic;

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

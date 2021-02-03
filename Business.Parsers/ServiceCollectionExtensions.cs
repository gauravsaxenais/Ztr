namespace Business.Parsers
{
    using Core.Converter;
    using Microsoft.Extensions.DependencyInjection;
    using ProtoParser.Parser;
    using System.Collections.Generic;

    public static class ServiceCollectionExtensions
    {
        public static void AddConverters(this IServiceCollection services)
        {
            services.AddScoped<ConverterService>();
            services.AddScoped<IJsonConverter, DictionaryConverter>();
            services.AddScoped<IHTMLConverter, HTMLConverter>();
            services.AddScoped<IBuilder<ITree>, TomlBuilder>();
            services.AddScoped<IProtoMessageParser, ProtoMessageParser>();
            services.AddScoped<ICustomMessageParser, CustomMessageParser>();
            services.AddScoped<IModuleParser, ModuleParser>();
            services.AddScoped<IProtoFileCompiler, ProtoFileCompiler>();

            services.AddScoped<ConvertConfig>();
        }
    }
}

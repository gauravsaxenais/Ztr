namespace Business.Parsers.TomlParser.Core.Converter
{
    using Business.Parsers.Core.Models;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class ConverterService
    {
        private readonly ConvertConfig _config;
        private readonly IJsonConverter _parser;
        private readonly IBuilder<IDictionary<string, object>> _builder;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigGeneratorManager"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public ConverterService(IJsonConverter parser, IBuilder<IDictionary<string, object>> builder, ConvertConfig config)
        {
            _config = config;
            _parser = parser;
            _builder = builder;
        }

        public async Task<string> CreateConfigTomlAsync(ConfigReadModel model)
        {
            
            string contents = GenerateToml(model.Version, ValueScheme.UnQuoted);
            contents += GenerateToml(model.Module, ValueScheme.UnQuoted);
            contents += Environment.NewLine + GenerateToml(model.Block, ValueScheme.Quoted);

            return await Task.FromResult(contents);            
        }

        string GenerateToml(string jsonContent, ValueScheme scheme)
        {
            var dictionary = _parser.ToConverted(jsonContent);
            string contents = _builder.ToTOML(dictionary, scheme);
            
            return contents;
        }
       
        public async Task<bool> UpdateConfig(string properties)
        {
            return await _config.UpdateConfiguration(properties);
        }
    }
}

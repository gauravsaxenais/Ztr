namespace Business.Parsers.Core.Converter
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Models;
    
    public class ConverterService
    {
        private readonly ConvertConfig _config;
        private readonly IJsonConverter _parser;
        private readonly IHTMLConverter _htmlparser;
        private readonly IBuilder<IDictionary<string, object>> _builder;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConverterService"/> class.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="builder">The builder.</param>
        /// <param name="config">The configuration.</param>
        public ConverterService(IJsonConverter parser, IHTMLConverter htmlparser, IBuilder<IDictionary<string, object>> builder, ConvertConfig config)
        {
            _config = config;
            _parser = parser;
            _htmlparser = htmlparser;
            _builder = builder;
        }

        /// <summary>
        /// Creates the configuration toml asynchronous.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        public async Task<string> CreateConfigTomlAsync(ConfigReadModel model)
        {
            string contents = GenerateToml(model.Version, ValueScheme.UnQuoted);
            contents += GenerateToml(model.Module, ValueScheme.UnQuoted);
            contents += Environment.NewLine + GenerateToml(model.Block, ValueScheme.Quoted);

            return await Task.FromResult(contents);            
        }

        public async Task<string> CreateFromHtmlAsync(string device, string firmware, string html)
        {
            var dictionary = _htmlparser.ToConverted(html);
            var contents = _builder.ToTOML(dictionary, ValueScheme.Quoted);
          
            return await Task.FromResult(contents);
        }

        /// <summary>
        /// Updates the configuration.
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <returns></returns>
        public async Task<bool> UpdateConfig(string properties)
        {
            return await _config.UpdateConfiguration(properties);
        }

        private string GenerateToml(string jsonContent, ValueScheme scheme)
        {
            var dictionary = _parser.ToConverted(jsonContent);
            var contents = _builder.ToTOML(dictionary, scheme);
            
            return contents;
        }
    }
}

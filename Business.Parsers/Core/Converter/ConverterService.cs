namespace Business.Parsers.Core.Converter
{
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class ConverterService
    {
        private readonly ConvertConfig _config;
        private readonly IJsonConverter _parser;
        private readonly IHTMLConverter _htmlParser;
        private readonly IBuilder<ITree> _builder;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConverterService"/> class.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="htmlParser">The HTML parser.</param>
        /// <param name="builder">The builder.</param>
        /// <param name="config">The configuration.</param>
        public ConverterService(IJsonConverter parser, IHTMLConverter htmlParser, IBuilder<ITree> builder, ConvertConfig config)
        {
            _config = config;
            _parser = parser;
            _htmlParser = htmlParser;
            _builder = builder;
        }

        /// <summary>
        /// Creates the configuration toml asynchronous.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        public async Task<string> CreateConfigTomlAsync(ConfigReadModel model, bool enableHidden = false)
        {
            _config.EnableHidden = enableHidden;
            string contents = GenerateToml(model.Version, ValueScheme.UnQuoted);
            contents += GenerateToml(model.Module, ValueScheme.UnQuoted) + Environment.NewLine;
            contents += Environment.NewLine + GenerateToml(model.Block, ValueScheme.Quoted);

            return await Task.FromResult(contents);
        }

        /// <summary>
        /// Creates from HTML asynchronous.
        /// </summary>
        /// <param name="html">The HTML.</param>
        /// <returns></returns>
        public async Task<string> CreateFromHtmlAsync(string html, ConfigTOML Toml)
        {
            _config.Toml = Toml;
            var dictionary = _htmlParser.ToConverted(html);
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

            contents = contents.Trim(Environment.NewLine.ToCharArray());
            return contents;
        }
    }
}

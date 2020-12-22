using Business.Core;
using Business.Parsers.Core.Converter;
using Business.Parsers.Models;
using EnsureThat;
using Microsoft.Extensions.Logging;
using Nett;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Business.Parsers.Core
{
    public class ConverterService
    {
        private ILogger<ConverterService> _logger;
        private ConvertConfig _config;
        private IJsonConverter _parser;
        private IBuilder<IDictionary<string, object>> _builder;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigGeneratorManager"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public ConverterService(ILogger<ConverterService> logger, IJsonConverter parser, IBuilder<IDictionary<string, object>> builder, ConvertConfig config)
        {
            _logger = logger;
            _config = config;
            _parser = parser;
            _builder = builder;
        }


        public async Task<string> CreateConfigTomlAsync(ConfigReadModel model)
        {
            string contents = GenerateToml(model.Module);
            contents += Environment.NewLine + GenerateToml(model.Block);

            return await Task.FromResult(contents);

            
        }
        string GenerateToml(string jsonContent)
        {
            var dictionary = _parser.ToConverted(jsonContent);
            string contents = _builder.ToTOML(dictionary);
            // string contents = JsonConvert.SerializeObject(dictionary);
            return contents;
        }
       
        public async Task<bool> UpdateConfig(string properties)
        {
            return await _config.UpdateConfiguration(properties);
        }
    }
}

namespace Business.Parsers.Core.Converter
{
    using Business.Core;
    using Business.Parsers.Models;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    public class ConvertConfig
    {
        internal string Value => "value";
        internal string Name => "name";
        internal string Fields => "fields";
        internal string Arrays => "arrays";
        private readonly ILogger<ConverterService> _logger;
        private static readonly object _syncRoot = new object();
        internal string[] Properties { get; set; }
        const string _skipConfigFolder = "configsetting";
        const string _skipConfigFile = "convertconfig.txt";
        internal IEnumerable<ConfigConvertRuleReadModel> Rules { get; set; }

        private string Path => $"{Global.WebRoot}/{_skipConfigFolder}/{_skipConfigFile}";

        public ConvertConfig(ILogger<ConverterService> logger)
        {
            _logger = logger;
            InitiateRule();
        }

        void InitiateRule()
        {
            string setting = string.Empty;
            if (File.Exists(Path))
            {
                setting = File.ReadAllText(Path);
            }
            var tags = setting.Replace("\r", string.Empty).Split(Environment.NewLine);
            Properties = tags.Where(o => !o.StartsWith("Rule:")).ToArray();
            Rules = tags.Where(o => o.StartsWith("Rule:")).Select(o =>
            {
                var ruleConfig = o.Split(':');
                var rule = new ConfigConvertRuleReadModel
                {
                    Property = ruleConfig[1],
                    Schema = new List<ConfigConvertObjectReadModel> { new ConfigConvertObjectReadModel
                         {
                              Name = ruleConfig[2],
                              Value= ruleConfig[3]
                         } }
                };
                return rule;
            });
          
        }

        internal async Task<bool> UpdateConfiguration(string properties)
        {
            var path = $"{Global.WebRoot}/{_skipConfigFolder}";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var values = properties.Split(',').Select(o => o.ToLower().Trim()).ToList();

            //Thread-safe operation .....
            lock (_syncRoot)
            {
                using var file = File.AppendText(Path);
                values.ForEach(o => file.WriteLine(o));
                file.Flush();
                file.Close();

            }
            return await Task.FromResult(true);
        }
    }
}

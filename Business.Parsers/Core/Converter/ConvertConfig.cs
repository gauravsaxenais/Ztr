namespace Business.Parsers.Core.Converter
{
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Models;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using ZTR.Framework.Business;

    public class ConvertConfig
    {
        internal string Value => "value";
        internal string Name => "name";
        internal string Fields => "fields";
        internal string Arrays => "arrays";
     
        private readonly ILogger<ConverterService> _logger;
        private static readonly object SyncRoot = new object();
        internal string[] Properties { get; private set; }
        internal string[] RmArrays { get; private set; }
        internal IEnumerable<ConfigConvertRuleReadModel> JsonProperties { get; private set; }
        private const string _skipConfigFolder = "configsetting";
       
        internal IEnumerable<ConfigConvertRuleReadModel> Rules { get; set; }
        internal ConfigTag HTMLTags { get; set; }

        private string Path => $"{Global.WebRoot}/{_skipConfigFolder}/convert.config";
        private string HTMLPath => $"{Global.WebRoot}/{_skipConfigFolder}/config.html";
        private string TomlPath => $"{Global.WebRoot}/{_skipConfigFolder}/config.toml";
        private string MapperPath => $"{Global.WebRoot}/{_skipConfigFolder}/map.config";

        public ConvertConfig(ILogger<ConverterService> logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));

            _logger = logger;
            EnableHidden = false;
            InitiateRule();
        }
        internal string GetHtml()
        {
            string html = string.Empty;
            if (File.Exists(HTMLPath))
            {
                html = File.ReadAllText(HTMLPath);
            }
            return html;

        }
        internal IEnumerable<ConfigMap> GetMapping()
        {
            string mapping = string.Empty;
            if (File.Exists(MapperPath))
            {
                mapping = File.ReadAllText(MapperPath);
            }
            var tags = mapping.Split(Environment.NewLine);
            var map = tags.Where(o => o.StartsWith("map:")).Select(o => new ConfigMap(o)).Distinct().ToArray();
            return map;
        }       
        internal ConfigTOML Toml { get; set; }
        internal bool EnableHidden { get; set; }
        void InitiateRule()
        {
            string setting = string.Empty;
            if (File.Exists(Path))
            {
                setting = File.ReadAllText(Path);
            }           

            var tags = setting.Split(Environment.NewLine);
            Properties = tags.Where(o => o.StartsWith("rm:")).Select(o => o.Replace("rm:", string.Empty).ToLower().RemoveNewline()).ToArray();
            JsonProperties = tags.Where(o => o.StartsWith("json:")).Select(o => MapConfig(o));
            Rules = tags.Where(o => o.StartsWith("rule:")).Select(o => MapConfig(o));
            HTMLTags = tags.Where(o => o.StartsWith("html:")).Select(o => new ConfigTag(o)).First();
            RmArrays = tags.Where(o => o.StartsWith("nrmarry:")).Select(o => o.Replace("nrmarry:", string.Empty).RemoveNewline()).ToArray();

            static ConfigConvertRuleReadModel MapConfig(string o)
            {
                var ruleConfig = o.RemoveNewline().Split(':');
                var rule = new ConfigConvertRuleReadModel
                {
                    Property = ruleConfig[1],
                    Schema = ConfigConvertRuleReadModel.TryScheme(ruleConfig[2]),
                    Value = ruleConfig.Length > 3 ? ruleConfig[3] : string.Empty
                };
                return rule;
            }
        }

        internal async Task<bool> UpdateConfiguration(string properties)
        {
            var path = System.IO.Path.Combine(Global.WebRoot, _skipConfigFolder);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var values = properties.Split(',').Select(o => o.ToLower().Trim()).ToList();

            //Thread-safe operation .....
            lock (SyncRoot)
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

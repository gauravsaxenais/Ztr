using Business.Core;
using Business.Parsers.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Parsers.Core.Converter
{
    public class ConvertConfig
    {
        internal string value => "value";
        internal string name => "name";
        internal string Fields => "fields";
        internal string Arrays => "arrays";

        private static object _syncRoot = new object();
        internal string[] properties { get; set; }
        const string _skipConfigFolder = "configsetting";
        const string _skipConfigFile = "convertconfig.txt";
        internal IEnumerable<ConfigConvertRuleReadModel> rules { get; set; }

        private string _path => $"{Global.WebRoot}/{_skipConfigFolder}/{_skipConfigFile}";

        public ConvertConfig()
        {
            InitiateRule();
        }

        void InitiateRule()
        {
            string setting = string.Empty;
            if (File.Exists(_path))
            {
                setting = File.ReadAllText(_path);
            }
            var tags = setting.Split(Environment.NewLine);
            properties = tags.Where(o => !o.StartsWith("Rule:")).ToArray();
            rules = tags.Where(o => o.StartsWith("Rule:")).Select(o =>
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
                using var file = File.AppendText(_path);
                values.ForEach(o => file.WriteLine(o));
                file.Flush();
                file.Close();

            }
            return await Task.FromResult(true);
        }
    }
}

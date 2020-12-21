using Business.Core;
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
        private static object _syncRoot = new object();
        private string[] _properties;
        const string _skipConfigFolder = "configsetting";
        const string _skipConfigFile = "convertconfig.txt";
        private IEnumerable<ConfigConvertRuleReadModel> _rules;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigGeneratorManager"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public ConverterService(ILogger<ConverterService> logger)
        {
            _logger = logger;
            InitiateRule();
        }

        private void RemoveProperties<T>(T input) where T : IDictionary<string, object>
        {
            //_logger.LogInformation($"Properties : {_properties.Count()}");
            foreach (var item in input)
            {               
                if (_properties.Contains(item.Key.ToLower()))
                {
                    input.Remove(item);
                    continue;
                }

                
                
                if (item.Value is Array)
                {
                    ((object[])item.Value).ToList().ForEach(o => {

                        if (o is string)
                        {
                            return;
                        }

                        if (o is object[])
                        {
                            ((object[])o).ToList().ForEach(u => RemoveProperties((T)u));                            
                        }
                        else
                        {
                            try
                            {
                                RemoveProperties((T)o);
                            }
                            catch (Exception ex)
                            {
                                var i = o;
                            }

                        }                                               
                    });
                }

                if (item.Value is T)
                {
                    RemoveProperties((T)item.Value);
                }

            }
        }
        private object ToDictionary(object configObject)
        {

            if (configObject == null)
            {
                return null;
            }
            if (configObject is JValue)
            {
                return ((JValue)configObject).ToString();
            }

            if (configObject is JArray)
            {
                return ((JArray)configObject).Select(o => ToDictionary(o)).ToArray();
            }

            var dictionary = new Dictionary<string, object>();
            if (configObject is JObject)
            {
                foreach (var o in (JObject)configObject)
                {
                    dictionary.Add(o.Key, ToDictionary(o.Value));
                }
            }

            return dictionary;

        }

        private void InitiateRule()
        {
            string setting = string.Empty;
            if (File.Exists(_path))
            {
                setting = File.ReadAllText(_path);
            }
            var tags = setting.Split(Environment.NewLine);
            _properties = tags.Where(o => !o.StartsWith("Rule:")).ToArray();
            _rules = tags.Where(o => o.StartsWith("Rule:")).Select(o =>
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


        private List<KeyValuePair<string, IDictionary<string, string>>> changeKeys;
        private void ConvertCompatibleJson<T>(T input) where T : IDictionary<string, object>
        {
            KeyValuePair<string, Dictionary<string, string>> newKey = default;
            foreach (var item in input)
            {
               // var rule = _rules.FirstOrDefault(o => o.Property == item.Key.ToLower());
                if (item.Value is object[])
                {
                    var dict = new Dictionary<string, string>();
                    ((object[])item.Value).ToList().ForEach(x =>
                    {
                        if (x is object[])
                        {
                            ((object[])x).ToList().ForEach(z => ConvertCompatibleJson((T)z));
                        }
                        else
                        {
                            var o = (IDictionary<string, object>)x;
                            if(!o.ContainsKey("name"))
                            {
                                return;
                            }
                            //rule.Schema.ToList().ForEach(u =>
                            //{
                            dict.Add(o["name"].ToString(), o.ContainsKey("value") ? o["value"].ToString() : string.Empty);
                            //});
                        }
                    });

                    if (dict.Count > 0)
                    {
                        newKey = new KeyValuePair<string, Dictionary<string, string>>(item.Key, dict);
                        continue;
                    }
                   
                }
                if (item.Value is Array)
                {
                    ((object[])item.Value).ToList().ForEach(o => ConvertCompatibleJson((T)o));
                }

                if (item.Value is T)
                {
                    ConvertCompatibleJson((T)item.Value);
                }

            }

            if (newKey.Key != null)
            {
                input[newKey.Key] = newKey.Value;
            }

        }
        private string _path => $"{Global.WebRoot}/{_skipConfigFolder}/{_skipConfigFile}";
        /// <summary>
        /// Creates the configuration asynchronous.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>
        ///   <br />
        /// </returns>
        public async Task<string> CreateConfigAsync(ConfigReadModel model)
        {
            string contents = GenerateConfigToml(model.Module);
            //contents += Environment.NewLine + GenerateConfigToml(model.Module);

            return await Task.FromResult(contents);

            
        }
        string GenerateConfigToml(string jsonContent)
        {
            var configurationObject = JsonConvert.DeserializeObject(jsonContent);
            var dictionary = (Dictionary<string, object>)ToDictionary(configurationObject);
            RemoveProperties(dictionary);

            ConvertCompatibleJson(dictionary);

            string contents = Toml.WriteString(dictionary);
            return contents;
        }
        /// <summary>
        /// Updates the toml configuration.
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <returns></returns>
        public async Task<bool> UpdateTomlConfig(string properties)
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

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
        private const string _value = "value";
        private const string _name = "name";
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
                          RemoveProperties((T)o);                          
                        }                                               
                    });
                }

                if (item.Value is T)
                {
                    RemoveProperties((T)item.Value);
                }

            }
        }

        #region private helper functions
        object ToDictionary(object configObject)
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
        void InitiateRule()
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
        bool IsValueEmpty(object value)
        {
            if (value == null)
                return true;
            
            return string.IsNullOrEmpty(value.ToString());
        }
        bool CanConvert(object value)
        {
            bool result = false;
            if(value is object[] v && v.Length > 0 && v[0] is IDictionary<string, object> dictinary)
            {
                result = dictinary.Values.ToList().Any(o => o.GetType() == typeof(string));
            }
            return result;
        }
        object Extractvalue<T>(T dictionary) where T : Dictionary<string, object>
        {
            object value = null;

            if (dictionary.ContainsKey(_value))
            {
                value = dictionary[_value];
            }

            if (IsValueEmpty(value) && dictionary.ContainsKey("arrays"))
            {
                value = ((object[])dictionary["arrays"]).ToList().Select(o =>
                           {
                               if (o is object[] v)
                               {
                                   return v.ToList().Select(u => ConvertCompatibleJson((T)u,1)).ToArray();
                               }
                               return null;
                           }).ToArray();
            }

            return value;
        }
        void ConvertArray(object[] array)
        {
            array.ToList().ForEach(o =>
            {
                if (o is object[] v)
                {
                    ConvertArray(v);
                }
                else
                {
                    if (o is string)
                    {
                        return;
                    }
                    ConvertCompatibleJson((Dictionary<string, object>)o,1);
                }

            });
        }
        private T Convert<T>(T input, T dictionary, int loop) where T : Dictionary<string, object>
        {           
            if (loop > 0)
            {
                var o = (T)input;
                if (o.ContainsKey(_name))
                {
                    dictionary.Add(o[_name].ToString(), Extractvalue(o));
                }
                
            }
            return dictionary;
        }
        #endregion private helper functions

        private T ConvertCompatibleJson<T>(T input,int loop) where T : Dictionary<string, object>
        {
            KeyValuePair<string, Dictionary<string, object>> newKey = default;
            var dict = new Dictionary<string, object>();
            if (input is T)
            {
                dict = Convert(input, dict, ++loop);
            }

            foreach (var item in input)
            {                
                if (item.Value is Array )
                {
                   
                   // if (CanConvert(item.Value))
                    {
                        ((object[])item.Value).ToList().ForEach(x =>
                        {
                            dict = Convert((Dictionary<string, object>)x, dict,++loop);
                        });

                    }


                    ConvertArray((object[])item.Value);

                    if (dict.Count > 0)
                    {
                        newKey = new KeyValuePair<string, Dictionary<string, object>>(item.Key, dict);
                        continue;
                    }
                   
                }               

            }

            if (newKey.Key != null)
            {
                input[newKey.Key] = newKey.Value;
            }

            return input;
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

            ConvertCompatibleJson(dictionary,0);

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

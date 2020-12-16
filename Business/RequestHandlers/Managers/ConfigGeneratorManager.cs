namespace Business.RequestHandlers.Managers
{
    using Business.RequestHandlers.Interfaces;
    using Nett;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using EnsureThat;
    using Business.Core;
    using System.Linq;
    using Business.Models;
    using Newtonsoft.Json.Converters;

    /// <summary>
    ///   <br />
    /// </summary>
    public class ConfigGeneratorManager : IConfigGeneratorManager
    {


        private static object _syncRoot = new object();
        private string[] _properties;
        const string _skipConfigFolder = "configsetting";
        const string _skipConfigFile = "convertconfig.txt";
        private IEnumerable<ConfigConvertRule> _rules;

        public ConfigGeneratorManager()
        {
            InitiateRule();
        }
        
        private void RemoveProperties<T>(T input) where T : IDictionary<string, object>
        {
            foreach (var item in input)
            {
                if (_properties.Contains(item.Key.ToLower()))
                {
                    input.Remove(item);
                    continue;
                }

                if (item.Value is Array)
                {
                    ((object[])item.Value).ToList().ForEach(o => RemoveProperties((T)o));
                }
                if (item.Value is T )
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
            _properties = tags.Where(o => !o.StartsWith("rule:")).ToArray();
            _rules = tags.Where(o => o.StartsWith("Rule:")).Select(o =>
            {
                var ruleConfig = o.Split(':');
                var rule = new ConfigConvertRule
                {
                    Property = ruleConfig[1],
                    Schema = new List<ConfigConvertObject> { new ConfigConvertObject
                         {
                              Name = ruleConfig[2],
                              Value= ruleConfig[3]
                         } }
                };
                return rule;
            });
        }
        /// <summary>Creates the configuration asynchronous.</summary>
        /// <param name="jsonContent">Content of the json.</param>
        /// <returns>
        ///   <br />
        /// </returns>
        public async Task<string> CreateConfigAsync(ConfigModel model)
        {
            //EnsureArg.IsNotEmptyOrWhiteSpace(model.Module);
            EnsureArg.IsNotEmptyOrWhiteSpace(model.Block);


            var jsonContent = model.Block;
                // File.ReadAllText($"{Global.WebRoot}/test/block.json");

           
            var configurationObject = JsonConvert.DeserializeObject(jsonContent);           
            var dictionary = (Dictionary<string,object>)ToDictionary(configurationObject);
            RemoveProperties(dictionary);

            changeKeys = new List<KeyValuePair<string, IDictionary<string, string>>>();
            ConvertCompatibleJson(dictionary);
            //var json = JsonConvert.SerializeObject(dictionary);


            string contents = Toml.WriteString(dictionary);           

            return contents;
        }

        private List<KeyValuePair<string, IDictionary<string, string>>> changeKeys;
        private void ConvertCompatibleJson<T>(T input) where T : IDictionary<string, object>
        {
            KeyValuePair<string, Dictionary<string, string>> newKey = default;
            foreach (var item in input)
            {
                var rule = _rules.FirstOrDefault(o => o.Property == item.Key.ToLower());
                if (rule != null)
                {
                    var dict = new Dictionary<string, string>();
                    ((object[])item.Value).ToList().ForEach(x =>
                    {
                        var o = (IDictionary<string, object>)x;
                        rule.Schema.ToList().ForEach(u =>
                        {                           
                            dict.Add(o[u.Name].ToString(), o.ContainsKey(u.Value) ? o[u.Value].ToString() : string.Empty);
                        });
                       
                    });

                    if(dict != null)
                    {
                       newKey = new KeyValuePair<string, Dictionary<string, string>>(item.Key, dict);
                    }
                    //input.Remove(item.Key);
                   
                    continue;
                }

                if (item.Value is Array)
                {
                    ((object[])item.Value).ToList().ForEach(o => ConvertCompatibleJson((T)o));
                }
                if (item.Value is T)
                {
                    RemoveProperties((T)item.Value);
                }

            }

            if(newKey.Key != null)
            {
                input[newKey.Key] = newKey.Value;
            }
           
        }
        private string _path => $"{Global.WebRoot}/{_skipConfigFolder}/{_skipConfigFile}";
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

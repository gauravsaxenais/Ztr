using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.Parsers.Core.Converter
{
    public class JsonConverter : IJsonConverter
    {
        private ConvertConfig _config;
        public JsonConverter(ConvertConfig config)
        {
            _config = config;
        }

        #region private helper functions
        private void RemoveProperties<T>(T input) where T : IDictionary<string, object>
        {
            //_logger.LogInformation($"Properties : {_properties.Count()}");
            foreach (var item in input)
            {
                if (_config.properties.Contains(item.Key.ToLower()))
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
       
        bool IsValueEmpty(object value)
        {
            if (value == null)
                return true;

            return string.IsNullOrEmpty(value.ToString());
        }
       
        object Extractvalue<T>(T dictionary) where T : Dictionary<string, object>
        {
            object value = null;
            var dict = new Dictionary<string, object>();
            if (dictionary.ContainsKey(_config.value))
            {
                value = dictionary[_config.value];
            }
            if (IsValueEmpty(value) && dictionary.ContainsKey("fields"))
            {
                dict = Convert<T>((object[])dictionary["fields"]);
                if (dict.Count > 0)
                    value = dict;
            }
            if (IsValueEmpty(value) && dictionary.ContainsKey("arrays"))
            {
                value = ((object[])dictionary["arrays"]).ToList().Select(o =>
                {
                    if (o is object[] v)
                    {
                        var res = Convert<T>(v);
                        return res;
                    }
                    return null;
                }).ToArray();
            }

            return value == null ? string.Empty : value;
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
                    ConvertCompatibleJson((Dictionary<string, object>)o);
                }

            });
        }

        private T Convert<T>(object[] input) where T : Dictionary<string, object>
        {
            var dict = new Dictionary<string, object>();
            input.ToList().ForEach(u =>
            {
                var o = (T)u;
                if (o.ContainsKey(_config.name))
                {
                    dict.Add(o[_config.name].ToString(), Extractvalue(o));
                }

            });
            return (T)dict;
        }

        #endregion private helper functions

        private T ConvertCompatibleJson<T>(T input) where T : Dictionary<string, object>
        {
            KeyValuePair<string, object> newKey = default;
            T dictionary = null;          
            foreach (var item in input)
            {
                if (item.Value is Array)
                {
                    if (_config.rules.Any(o => o.Property == item.Key.ToLower()))
                    {
                        // ********* Do not delete this line *******************
                        //
                        //((object[])item.Value).ToList().ForEach(x =>
                        //{ 
                        //    Convert((T)x);
                        //});
                        dictionary = Convert<T>((object[])item.Value);

                    }

                    if (dictionary != null)
                    {
                        newKey = new KeyValuePair<string, object>(item.Key, dictionary);                        
                    }
                    else
                    {
                        ConvertArray((object[])item.Value);
                    }

                }

            }


            if (newKey.Key != null)
            {
                input[newKey.Key] = newKey.Value;
            }
            return input;
        }


        public IDictionary<string, object> ToConverted(string json)
        {
            var configurationObject = JsonConvert.DeserializeObject(json);
            var dictionary = (Dictionary<string, object>)ToDictionary(configurationObject);
            RemoveProperties(dictionary);
            return ConvertCompatibleJson(dictionary);
        }

        public string ToJson(string json)
        {
            return JsonConvert.SerializeObject(ToConverted(json));
        }
    }
}

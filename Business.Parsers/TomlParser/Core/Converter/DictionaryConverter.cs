namespace Business.Parsers.TomlParser.Core.Converter
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class DictionaryConverter : IJsonConverter
    {
        private readonly ConvertConfig _config;
        public DictionaryConverter(ConvertConfig config)
        {
            _config = config;
        }

        #region Private Helper methods
        private IExtractor<T> GetExtractor<T>()
        {
            return (IExtractor<T>)new Extractor(_config);
        }

        private void RemoveProperties<T>(T input) where T : IDictionary<string, object>
        {
            foreach (var item in input)
            {
                if (_config.Properties.Contains(item.Key.ToLower()))
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

                        if (o is object[] v)
                        {
                            v.ToList().ForEach(u => RemoveProperties((T)u));
                        }
                        else
                        {
                            RemoveProperties((T)o);
                        }
                    });
                }

                if (item.Value is T t)
                {
                    RemoveProperties(t);
                }

            }
        }

        private object ToDictionary(object configObject)
        {
            if (configObject == null)
            {
                return null;
            }
            if (configObject is JValue value)
            {
                return value.ToString();
            }

            if (configObject is JArray array)
            {
                return array.Select(o => ToDictionary(o)).ToArray();
            }

            var dictionary = new Dictionary<string, object>();
            if (configObject is JObject @object)
            {
                foreach (var o in @object)
                {
                    dictionary.Add(o.Key, ToDictionary(o.Value));
                }
            }

            return dictionary;

        }
       
        private void ConvertArray(object[] array)
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

        private T ConvertCompatibleJson<T>(T input) where T : Dictionary<string, object>
        {
            KeyValuePair<string, object> newKey = default;
            T dictionary = null;
            foreach (var item in input)
            {
                if (item.Value is Array)
                {
                    if (_config.Rules.Any(o => o.Property == item.Key.ToLower()))
                    {
                        // ********* Do not delete this line *******************
                        //
                        //((object[])item.Value).ToList().ForEach(x =>
                        //{ 
                        //    Convert((T)x);
                        //});
                        dictionary = GetExtractor<T>().Convert((object[])item.Value);
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
        #endregion private helper functions

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

namespace Business.Parsers.TomlParser.Core.Converter
{
    using Business.Parsers.Core.Models;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class DictionaryConverter : IJsonConverter
    {
        private readonly ConvertConfig _config;
        private readonly ICollection<ConfigConvertRuleReadModel>_omitKeys;
        public DictionaryConverter(ConvertConfig config)
        {
            _config = config;
            _omitKeys = _config.Rules.Where(o => o.Schema == ConversionScheme.Omit).ToList();                                     
        }

        #region Private Helper methods
        private IExtractor<T> GetExtractor<T>()
        {
            return (IExtractor<T>)new Extractor(_config);
        }

         
        private bool RemoveProperties<T>(T input) where T : IDictionary<string, object>
        {

            var isForOmit = _omitKeys.Any(o => input.Any(u => 
                                                            u.Key.ToLower() == o.Property.ToLower() &&
                                                            u.Value.ToString().ToLower() == o.Value.ToLower()
                                                         ));
            if (isForOmit)
            {
                return true;
            }

            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            foreach (var item in input)
            {                
                if (_config.Properties.Contains(item.Key.ToLower()) )
                {                   
                    input.Remove(item);
                    continue;
                }

                if (item.Value is Array)
                {
                    IList<object> objects = new List<object>();
                    ((object[])item.Value).ToList().ForEach(o =>
                    {
                        if (o is string)
                        {
                            return;
                        }

                        if (o is object[] v)
                        {
                            IList<object> objectinternal = new List<object>();
                            v.ToList().ForEach(u =>
                            {
                                var omit = RemoveProperties((T)u);
                                if (!omit) objectinternal.Add(u);
                            });

                            objects.Add(objectinternal.ToArray());
                        }
                        else
                        {
                            var omit = RemoveProperties((T)o);
                            if (!omit) objects.Add(o);
                        }
                    });
                   
                    dictionary.Add(item.Key, objects.ToArray());
                }

                if (item.Value is T t)
                {
                    var omit = RemoveProperties(t);
                    if (!omit) dictionary.Add(item.Key, t);
                }
            }

            foreach (var item in dictionary)
            {
                input[item.Key] = dictionary[item.Key];
            }

            return false;
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
                if (item.Value is T v)
                {
                    ConvertCompatibleJson(v);
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

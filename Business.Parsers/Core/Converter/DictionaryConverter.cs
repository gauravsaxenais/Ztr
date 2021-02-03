namespace Business.Parsers.Core.Converter
{
    using Models;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class DictionaryConverter : IJsonConverter
    {
        private readonly ConvertConfig _config;
        private readonly ICollection<ConfigConvertRuleReadModel> _omitKeys;

        /// <summary>
        /// Initializes a new instance of the <see cref="DictionaryConverter"/> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
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


        private bool RemoveProperties<T>(T input) where T : ITree, new()
        {

            if (!_config.EnableHidden)
            {
                var isForOmit = _omitKeys.Any(o => input.Any(u =>
                                                                u.Key.ToLower() == o.Property.ToLower() &&
                                                                u.Value.ToString().ToLower() == o.Value.ToLower()
                                                             ));
                if (isForOmit)
                {
                    return true;
                }
            }

            T dictionary = new T();
            foreach (var item in input)
            {

                if (string.IsNullOrEmpty(item.Value.ToString()) || _config.Properties.Contains(item.Key.ToLower()))
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
                            objects.Add(o);
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

            var dictionary = new Tree();
            if (configObject is JObject @object)
            {
                foreach (var o in @object)
                {
                    dictionary.Add(o.Key.ToLower(), ToDictionary(o.Value));
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

                    ConvertCompatibleJson((ITree)o);
                }
            });
        }

        private T ConvertCompatibleJson<T>(T input) where T : ITree
        {
            KeyValuePair<string, object> newKey = default;
            T dictionary = default;
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

        /// <summary>
        /// Converts to converted.
        /// </summary>
        /// <param name="json">The json.</param>
        /// <returns></returns>
        public ITree ToConverted(string json)
        {
            var configurationObject = JsonConvert.DeserializeObject(json);
            var dictionary = (Tree)ToDictionary(configurationObject);

            RemoveProperties(dictionary);
            return ConvertCompatibleJson((ITree)dictionary);
        }

        /// <summary>
        /// Converts to json.
        /// </summary>
        /// <param name="json">The json.</param>
        /// <returns></returns>
        public string ToJson(string json)
        {
            return JsonConvert.SerializeObject(ToConverted(json));
        }
    }
}

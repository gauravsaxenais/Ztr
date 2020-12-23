using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.Parsers.Core.Converter
{
    internal class Extractor : IExtractor<Dictionary<string, object>>
    {
        private ConvertConfig _config;
        public Extractor(ConvertConfig config)
        {         
            _config = config;
        }

        private object ExtractFields<T>(object value , T dictionary) where T : Dictionary<string, object>
        {
            if (IsValueEmpty(value) && dictionary.ContainsKey(_config.Fields))
            {
                Dictionary<string, object> dict;
                dict = Convert((object[])dictionary[_config.Fields]);
                if (dict.Count > 0)
                    value = dict;
            }
            return value;
        }
        private object ExtractArray<T>(object value, T dictionary) where T : Dictionary<string, object>
        {
            if (IsValueEmpty(value) && dictionary.ContainsKey(_config.Arrays))
            {
                Dictionary<string, object> dict;
                value = ((object[])dictionary[_config.Arrays]).ToList().Select(o =>
                {
                    if (o is object[] v)
                    {
                        var res = Convert(v);
                        return res;
                    }
                    return null;
                }).ToArray();
            }
            return value;
        }
        private object Extractvalue<T>(T dictionary) where T : Dictionary<string,object>
        {
            object value = null;           
            if (dictionary.ContainsKey(_config.value))
            {
                value = dictionary[_config.value];
            }
            value = ExtractFields(value, dictionary);
            value = ExtractArray(value, dictionary);
            return value ?? string.Empty;
        }

        private bool IsValueEmpty(object value)
        {
            if (value == null)
                return true;

            return string.IsNullOrEmpty(value.ToString());
        }

        public Dictionary<string, object> Convert(object[] input) 
        {
            var dict = new Dictionary<string, object>();
            input.ToList().ForEach(u =>
            {
                var o = (Dictionary<string, object>)u;
                if (o.ContainsKey(_config.name))
                {
                    dict.Add(o[_config.name].ToString(), Extractvalue(o));
                }

            });
            return dict;
        }
    }
}

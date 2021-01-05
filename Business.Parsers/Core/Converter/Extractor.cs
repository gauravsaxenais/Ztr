namespace Business.Parsers.TomlParser.Core.Converter
{
    using Business.Parsers.Core.Converter;
    using System.Collections.Generic;
    using System.Linq;

    internal class Extractor : IExtractor<ITree>
    {
        private readonly ConvertConfig _config;
        public Extractor(ConvertConfig config)
        {         
            _config = config;
        }

        private object ExtractFields<T>(object value , T dictionary) where T : IDictionary<string, object>
        {
            if (IsValueEmpty(value) && dictionary.ContainsKey(_config.Fields))
            {
                T dict;
                dict = (T)Convert((object[])dictionary[_config.Fields]);
                
                if (dict.Count > 0)
                    value = dict;
            }

            return value;
        }
        private object ExtractArray<T>(object value, T dictionary) where T : IDictionary<string, object>
        {
            if (IsValueEmpty(value) && dictionary.ContainsKey(_config.Arrays))
            {
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
            if (dictionary.ContainsKey(_config.Value))
            {
                value = dictionary[_config.Value];
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

        public ITree Convert(object[] input) 
        {
            var dict = new Tree();
            input.ToList().ForEach(u =>
            {
                var o = (Tree)u;
                if (o.ContainsKey(_config.Name))
                {
                    dict.Add(o[_config.Name].ToString(), Extractvalue(o));
                }

            });
            return dict;
        }
    }
}

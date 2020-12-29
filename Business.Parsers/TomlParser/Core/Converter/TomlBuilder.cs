namespace Business.Parsers.TomlParser.Core.Converter
{
    using Nett;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class TomlBuilder : IBuilder<IDictionary<string,object>>
    {
        private readonly ConvertConfig _config;
        public TomlBuilder(ConvertConfig config)
        {
            _config = config;
        }
        public string ToTOML(IDictionary<string, object> content)
        {
            Process(content);
            return Toml.WriteString(content);
        }
        private void Process<T>(T input) where T : IDictionary<string, object>
        {
            IDictionary<string, object> dictionary = new Dictionary<string, object>();
            foreach (var item in input)
            {
                if (_config.JsonProperties.Contains(item.Key.ToLower()))
                {
                   dictionary.Add(item.Key, SerializeWithoutQuote(item.Value));                  
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
                            v.ToList().ForEach(u => Process((T)u));
                        }
                        else
                        {
                            Process((T)o);
                        }
                    });
                }
                
                if (item.Value is T t)
                {
                    Process(t);
                }

            }
            foreach (var item in dictionary)
            {
                input[item.Key] = item.Value;
            }
        }

        private static string SerializeWithoutQuote(object value)
        {
            var serializer = JsonSerializer.Create(null);

            var stringWriter = new StringWriter();

            using (var jsonWriter = new JsonTextWriter(stringWriter))
            {
                jsonWriter.QuoteName = false;
                jsonWriter.Formatting = Formatting.Indented;
                serializer.Serialize(jsonWriter, value);

                return stringWriter.ToString();
            }
        }
    }
}

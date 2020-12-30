namespace Business.Parsers.TomlParser.Core.Converter
{
    using Nett;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using ZTR.Framework.Business;

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
            var toml = Neutralize(Toml.WriteString(content));
            return toml;
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
        private  string SerializeWithoutQuote(object value)
        {           
            var serializer = JsonSerializer.Create(null);
            var stringWriter = new StringWriter();

            using (var jsonWriter = new JsonTextWriter(stringWriter))
            {
                jsonWriter.QuoteName = false;
                jsonWriter.Formatting = Formatting.Indented;
                serializer.Serialize(jsonWriter, value);

                var s = stringWriter.ToString();
                s = UnEscape(s);
                return s;
            }
        }
        private string UnEscape(string input)
        {
            string pattern = @"^[^{}]*(((?'Open'\{)[^{}]*)+((?'Close-Open'\})[^{}]*)+)*(?(Open)(?!))$";

            string InArray(string s)
            {
                var match = Regex.Matches(s, pattern);
                if (match.Count >= 1 && match[0].Groups.Count >= 2)
                {
                    match[0].Groups[1].Captures.ToList().ForEach(o =>
                        {
                            Regex.Replace(o.Value, @"(\{.*\})", n =>
                            {
                                s = s.Replace(n.Groups[1].Value, n.Groups[1].Value.RemoveNewline());
                                return string.Empty;

                            }, RegexOptions.Singleline | RegexOptions.IgnoreCase);

                        });
                }
                return s;

            }

           input = Regex.Replace(input, @"(\[.*\])", m =>
              InArray(m.Groups[1].Value), 
              RegexOptions.Singleline | RegexOptions.IgnoreCase);

            ////[\s\S]*?
            //input = Regex.Replace(input, @"("+o+ @": \{.*\})", m =>
            //     InArray(m.Groups[1].Value),
            //     RegexOptions.Singleline | RegexOptions.IgnoreCase);

            return input;
        }
        private string Neutralize(string input)
        {
            return input
                .Replace(":", " =")
                .Replace("\\\"", @"""")
                .Replace("\"{", "{")
                .Replace("}\"", "}")
                .Replace("\"[", "[")
                .Replace("]\"", "]")
                .Replace("\\r\\n", Environment.NewLine)
                .Replace("\\n", Environment.NewLine);
        }
    }
}

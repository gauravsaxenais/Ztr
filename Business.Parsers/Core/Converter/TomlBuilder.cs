namespace Business.Parsers.TomlParser.Core.Converter
{
    using Business.Parsers.Core.Models;
    using Nett;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using ZTR.Framework.Business;

    public class TomlBuilder : IBuilder<IDictionary<string,object>>
    {
        private readonly ConvertConfig _config;
        private ValueScheme _scheme;
        public TomlBuilder(ConvertConfig config)
        {
            _config = config;           
        }
        public string ToTOML(IDictionary<string, object> content, ValueScheme scheme)
        {
            _scheme = scheme;
            Process(content, ConversionScheme.Object);
            var toml = Neutralize(Toml.WriteString(content));
            return toml;
        }     
        private void Process<T>(T input, ConversionScheme scheme) where T : IDictionary<string, object>
        {
            IDictionary<string, object> dictionary = new Dictionary<string, object>();
            foreach (var item in input)
            {
                if(scheme == ConversionScheme.Inline)
                {
                    ProcessItem((T)dictionary, item);
                }
                if (_config.JsonProperties.Any(o => o.Property == item.Key.ToLower() && o.Schema == ConversionScheme.Object))
                {
                    ProcessItem((T)dictionary, item);
                }
                if (_config.JsonProperties.Any(o => o.Property == item.Key.ToLower() && o.Schema == ConversionScheme.Inline))
                {
                    scheme = ConversionScheme.Inline;
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
                            v.ToList().ForEach(u => Process((T)u, scheme));
                        }
                        else
                        {
                            Process((T)o, scheme);
                        }
                    });
                }
                
                if (item.Value is T t)
                {
                    Process(t,scheme);
                }

            }
            foreach (var item in dictionary)
            {
                input[item.Key] = item.Value;
            }

            void ProcessItem(T dictionary, KeyValuePair<string, object> item)
            {
                if(!(item.Value is string))
                {
                    dictionary.Add(item.Key, SerializeWithoutQuote(item.Value));
                }               
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
        private string UnQuote(string s)
        {
            if (_scheme == ValueScheme.UnQuoted)
            {
                s = Regex.Replace(s, @"(""[^""]*"")", m =>
                {
                    string value = m.Groups[1].Value;
                    value = (value.RemoveQuotes().IsNumber() ? value.RemoveQuotes() : value);
                    return value;
                },
                RegexOptions.Singleline | RegexOptions.IgnoreCase);
            }
            return s;
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
                                s = s.Replace(n.Groups[1].Value, UnQuote(n.Groups[1].Value.RemoveNewline()));
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
            input = input                
                .Replace($"{Environment.NewLine}[", "[")
                .Replace("[[", $"{Environment.NewLine}{Environment.NewLine}[[")
                .Replace(":", " =")
                .Replace("\\\"", @"""")
                .Replace("\"{", "{")
                .Replace("}\"", "}")
                .Replace("\"[", "[")
                .Replace("]\"", "]")
                .Replace("\\r\\n", Environment.NewLine)
                .Replace("\\n", Environment.NewLine);
            return UnQuote(input);
        }
    }
}

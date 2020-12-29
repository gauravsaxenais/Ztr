namespace Business.Parsers.Core.Converter
{
    using Microsoft.Extensions.Logging;
    using Nett;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using ZTR.Framework.Business.Models;

    public class TomlBuilder : IBuilder<IDictionary<string,object>>
    {
        private readonly ConvertConfig _config;
        private HashSet<string> _formatters = new HashSet<string>();
        private JsonSerializerSettings _settings = new JsonSerializerSettings
        {         
            Formatting = Formatting.Indented
        };
        public TomlBuilder(ConvertConfig config)
        {
            _config = config;           
        }
        public string ToTOML(IDictionary<string, object> content)
        {            
            TraverseProperties(_formatters, content);        
            Process(content);
            var toml = Neutralize(Toml.WriteString(content));
            return toml;
        }
        private void TraverseProperties<T>(HashSet<string> list, T content) where T : IDictionary<string, object>
        {
            foreach (var item in content)
            {
                if (_config.JsonProperties.Contains(item.Key.ToLower()))
                {
                    if (item.Value is object[] v)
                    {
                        ((object[])item.Value).ToList().ForEach(o => {
                            list.UnionWith(((T)o).Keys);
                        });
                    }
                    else
                    {
                        list.UnionWith(((T)item.Value).Keys);
                    }
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
                            v.ToList().ForEach(u => TraverseProperties(list,(T)u));
                        }
                        else
                        {
                          TraverseProperties(list, (T)o);
                        }
                    });
                }

                if (item.Value is T t)
                {
                   TraverseProperties(list,t);
                }

            }

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
            string InArray(string s)
            {
                s = Regex.Replace(s, @"(\{[\s\S]*?\})", m =>
                    m.Groups[1].Value.RemoveNewline(),
                    RegexOptions.Multiline | RegexOptions.IgnoreCase);

                return s;
            }
            var builder = new StringBuilder(input);
            _formatters.ToList().ForEach( o => {
                // DO not remove....
                //var regex = new Regex(@$"({o}: \[[\s\S]*?\])", RegexOptions.IgnoreCase);
                //input = regex.Replace(input, m =>
                //m.Groups[1].Value.RemoveNewline());

            input = Regex.Replace(input, @$"({o}: \[[\s\S]*?\])", m =>
              InArray(m.Groups[1].Value), 
              RegexOptions.Multiline | RegexOptions.IgnoreCase);

            input = Regex.Replace(input, @"("+o+@": \{[\s\S]*?\})", m =>
                 InArray(m.Groups[1].Value),
                 RegexOptions.Multiline | RegexOptions.IgnoreCase);

            });
            return input;
        }
        private string Neutralize(string input)
        {
            return input
                .Replace("\\\"", @"""")
                .Replace("\"{", "{")
                .Replace("}\"", "}")
                .Replace("\"[", "[")
                .Replace("]\"", "]");
        }
    }
}

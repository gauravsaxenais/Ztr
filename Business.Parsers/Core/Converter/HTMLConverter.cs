using Business.Parsers.Core.Models;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using ZTR.Framework.Business;

namespace Business.Parsers.Core.Converter
{
    public class HTMLConverter : IHTMLConverter
    {
        private readonly ConvertConfig _config;       
        private HtmlDocument _document;
        private ITree _tree;
        private IEnumerable<ConfigMap> _map;
        private IDictionary<string, object> _tomlTree;
        private IBuilder<IDictionary<string, object>> _builder;
        public HTMLConverter(ConvertConfig config, IBuilder<IDictionary<string, object>> builder)
        {
            _config = config;
            _tree = new Tree();
            _builder = builder;
        }
        public IDictionary<string,object> ToConverted(string html)
        {
            _map = _config.GetMapping();
            CleanToCompatible(ref html);
            ToDictionary(html);

            var toml = _config.GetBaseToml();
            _tomlTree = _builder.ToDictionary(toml);
            RemoveModule(_tomlTree, false);
            RemoveArrays(_tomlTree);
            MergeValues();

            return _tomlTree;
        }
        
        public string ToJson(string json)
        {
            return JsonConvert.SerializeObject(ToConverted(json));
        }

        #region Private members
        private void MergeValues()
        {           
            TraverseSource(_tree, _tomlTree);
        }
        private void TraverseSource<T>(T input, T tree) where T : IDictionary<string, object>
        {
            foreach (var item in input)
            {       
                TraverseTarget(tree, item.Key, item);  
            }
        }
        private T CreateDictionary<T>(T from, T source) where T : IDictionary<string, object>
        {
            IDictionary<string, object> dictionary = new Dictionary<string, object>();
            foreach (var item in from)
            {
                if (source.Keys.Any(u => u.Compares(item.Key)))
                {
                    dictionary.Add(item.Key, source[item.Key]);
                }
               
            }
            return (T)dictionary;

        }
        private object TryMerge<T>(T input, KeyValuePair<string, object> source, object target) where T : IDictionary<string, object>
        {
           
            if (source.Value is T[] s && target is T[] tar)
            {
                var to = tar.First();
                var converted = s.Select(o => CreateDictionary(to, o))
                                 .Where(o=> o.Count > 0)
                                 .ToArray();

                return converted;
            }

            if (source.Value is T t)
            {
                TraverseSource(t, (T)target);
            }

            return target;
        }

        private void TraverseTarget<T>(T input, string key, KeyValuePair<string, object> source) where T : IDictionary<string, object>
        {
            Tree dictionary = new Tree();
            foreach (var item in input)
            {
                if(item.Key.Compares(key))
                {
                    var d = TryMerge(input, source, item.Value);
                    dictionary.Add(item.Key, d);                   
                    return;
                }
                if(item.Key.Compares("name") && item.Value is string && key.Compares(item.Value.ToString()) )
                {
                    var d = TryMerge(input,source, input);
                    dictionary.Add(item.Key, d);                  
                    return;
                }

                if (item.Value is object[] s)
                {
                    s.OfType<T>().ToList().ForEach(o =>
                    {
                        TraverseTarget(o, key, source);
                    });
                }

                if (item.Value is T t)
                {
                    TraverseTarget(t, key, source);
                }
            }

            foreach(var item in dictionary)
            {
                input[item.Key] = item.Value;
            }
        }
        private bool RemoveModule<T>(T input, bool removing) where T : IDictionary<string, object>
        {
            var dictionary = new Dictionary<string, object>();
            foreach (var item in input)
            {
                if(removing)
                {                   
                    if (item.Key.Compares("name") && !_tree.Keys.Any(o=> o.Compares(item.Value.ToString()) ))
                    {
                        return true;                       
                    }
                }
                if(item.Key.Compares("module"))
                {
                    removing = true;
                }
                if (item.Value is object[] s)
                {
                  var list =  s.Where(o =>
                    {
                        if (o is T)
                        {
                          return !RemoveModule((T)o, removing);
                        }
                        return true;
                    }).ToArray();

                    dictionary.Add(item.Key, list);
                }

                if (item.Value is T t)
                {
                    RemoveModule(t, removing);
                   
                }
            }

            foreach (var item in dictionary)
            {
                input[item.Key] = dictionary[item.Key];
            }

            return false;
        }
        private void RemoveArrays<T>(T input) where T : IDictionary<string,object>
        {
            var dictionary = new Dictionary<string, object>();
            foreach (var item in input)
            {
                if (item.Value is object[] s)
                {                  

                    if (_config.RmArrays.Contains(item.Key))
                    {
                        s.ToList().ForEach(o =>
                        {
                            RemoveArrays((T)o);
                        });
                    }
                    else
                    {
                        var l = s.OfType<T>();
                        if (l.Any())
                        {
                            var countofproperties = l.Max(o => o.Count);
                            var first = l.Where(o => o.Count == countofproperties).Take(1).ToArray();
                            dictionary.Add(item.Key, first);
                            RemoveArrays(first.First());
                        }
                    }
                }

                if (item.Value is T t)
                {
                    RemoveArrays(t);                   
                }
            }

            foreach (var item in dictionary)
            {
                input[item.Key] = dictionary[item.Key];
            }

          
        }

        private void ToDictionary(string html)
        {
            _document = new HtmlDocument();
            _document.LoadHtml(html);
            foreach (var h1 in _document.DocumentNode.Descendants("h1"))
            {
                ToRoot(h1);
            }
        }
        private void ToRoot(HtmlNode h1)
        {
            var root = h1.InnerText;
            var obj = ToObject(h1.NextSibling);
            if (obj != null)
            {
                _tree.Add(root, obj);
            }
        }
        private ConverterNode GetValue(HtmlNode node, string tag)
        {
            var result = new ConverterNode();
            var sibling = node;
            result.Key = string.Empty;
            while (sibling != null && !sibling.Name.Compares("h1"))
            {
                var name = sibling.Descendants(tag).FirstOrDefault();
                if (sibling.Name.Compares(tag))
                {
                    result.Key = sibling.InnerText;
                    result.SerialNode = sibling;
                    result.TagNode = sibling;
                    break;
                }
                if (name != null && name.Name.Compares(tag))
                {
                    result.Key = name.InnerText;
                    result.SerialNode = sibling;
                    result.TagNode = name;
                    break;
                }

                sibling = sibling.NextSibling;
            }
            return result;
        }
        private Tree ToObject(HtmlNode node)
        {
            ConverterNode pickerNode;
            Tree t = new Tree();
            while (node != null && !node.Name.Compares("h1"))
            {
                pickerNode = GetValue(node, "h2");
                string key = pickerNode.Key;
                if (string.IsNullOrEmpty(key))
                {
                    return null;
                }
                pickerNode = GetValue(pickerNode.SerialNode, "table");
                var obj = ToData(pickerNode.TagNode);
                if (obj != null)
                {
                    AddKeyValue(t, key, obj);
                }

                node = pickerNode.SerialNode.NextSibling;
            }
            return t;
        }
        private Tree[] ToData(HtmlNode table)
        {
            Tree t;
            List<Tree> trees = new List<Tree>();
            Dictionary<int, string> keys = new Dictionary<int, string>();
            int start = 0;
            foreach (var tr in table.Descendants("tr"))
            {
                if (start++ == 0)
                {
                    int key = 1;
                    foreach (var td in tr.Descendants("td"))
                    {
                        if (td.Attributes.Any(o => o.Name.Compares("colspan")  && o.Value.ToInt() > 1))
                        {
                            start = 0;
                            break;
                        }
                        keys.Add(key++, td.InnerText);
                    }
                }
                else
                {
                    int key = 1;
                    t = new Tree();
                    foreach (var td in tr.Descendants("td"))
                    {
                        AddKeyValue(t, keys[key], td.InnerText.CleanHTML());
                        key++;
                    }
                    if (t.Count > 0)
                    {
                        trees.Add(t);
                    }
                }
            }
            return trees.ToArray();
        }
        private void AddKeyValue(Tree dictionary, string key, object value)
        {
            key = key.CleanHTML();
            if (string.IsNullOrEmpty(key))
            {
                return;
            }
            if (dictionary.Keys.Contains(key))
            {
                return;
            }
            var m = _map.FirstOrDefault(o => o.From.Compares(key));
            if(m!=null)
            {
                key = m.To;
                if(value is string && m.From.ToLower().Contains("hex"))
                {
                    value = value.ToString().FromHex();
                }
            }
            dictionary.Add(key, value);

        }
        private void CleanToCompatible(ref string html)
        {
            foreach (var o in _config.HTMLTags.Tags)
            {
                html = Regex.Replace(html, @$"(<{o}[^>]*>.*?</{o}>)", m =>
                 string.Empty,
                 RegexOptions.Singleline | RegexOptions.IgnoreCase);
            }

            foreach (var o in _config.HTMLTags.Inline)
            {
                html = Regex.Replace(html, @$"(<{o}[^>]*>[^>]*>)", m =>
                string.Empty,
                RegexOptions.Singleline | RegexOptions.IgnoreCase);

                html = html.Replace($"</{o}>", string.Empty);
            }
        }
        #endregion private members
    }
}

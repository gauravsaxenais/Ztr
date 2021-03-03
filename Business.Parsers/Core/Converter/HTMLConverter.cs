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
        private ITree _tomlTree;
        private ITree _ReaderTomlTree;
        private IBuilder<ITree> _builder;
        public HTMLConverter(ConvertConfig config, IBuilder<ITree> builder)
        {
            _config = config;
            _tree = new Tree();
            _builder = builder;
        }
        public ITree ToConverted(string html)
        {
            _map = _config.GetMapping();
            CleanToCompatible(ref html);
            ToDictionary(html);
          
            _ReaderTomlTree = _builder.ToDictionary(_config.Toml.BaseToml);
            _tomlTree = _builder.ToDictionary(_config.Toml.ViewToml);
            RemoveModule(_tomlTree, false);
            //RemoveArrays(_tomlTree);
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
        private void TraverseSource<T>(T input, T tree) where T : ITree
        {
            foreach (var item in input)
            {       
                TraverseTarget(tree, item.Key, item);  
            }
        }
        private void TryGetKeyValue(ITree converted, string key,out string value)
        {
            value = string.Empty;
            if(converted.Keys.Any(u => u.Compares(key)))
            {
                value = converted[converted.Keys.First(u => u.Compares(key))].ToString();                
            }
        }
        private T CreateDictionary<T>(T from, T source) where T : ITree
        {
            ITree converted = new Tree();
            source.ToDictionary(t =>
            {
                var m = GetMapped(t.Key, t.Value);
                m.ForEach(o => converted.Add(o));                
                return t.Key;
            });
           
            ITree dictionary = new Tree();
            foreach (var item in from)
            {               
                if (item.Value is T t)
                {
                    var d = CreateDictionary(t, source);
                    if (d.Count > 0)
                    {
                        dictionary.Add(item.Key,d);
                    }
                } 
                else
                {
                    string value;
                    TryGetKeyValue(converted, item.Key, out value);
                    dictionary.Add(item.Key, value);
                }
            }

            return (T)dictionary;

        }
        private bool ValidateValues<T>(T input) where T : ITree
        {
            bool valid = false;
            foreach(var item in input)
            {
                if(item.Value is T t)
                {
                    valid = ValidateValues(t);
                    if(valid)
                    {
                        break;
                    }
                   
                }
                else if(!string.IsNullOrEmpty(item.Value.ToString()))
                {
                    valid = true;
                    break;
                }
            }
            return valid;
        }
        private object TryMerge<T>(T input, KeyValuePair<string, object> source, object target) where T : ITree
        {

            if (source.Value is T[] s && target is object[] tar)
            {
                if (tar.First() is T to)
                { 
                 var converted = s.Select(o =>
                                 {
                                    var d = CreateDictionary(to, o);
                                    if( ValidateValues(d))
                                    {
                                        return d;
                                    }
                                    return(ITree) new Tree();
                                 })
                                 .Where(o => o.Count > 0)
                                 .ToArray();

                 return converted;
              }
            }

            if (source.Value is T t && target is T tarDic)
            {
                TraverseSource(t, tarDic);
            }

            return target;
        }

        private void TraverseTarget<T>(T input, string key, KeyValuePair<string, object> source) where T : ITree
        {
            Tree dictionary = new Tree();
            foreach (var item in input)
            {
                if(item.Key.Compares(key))
                {
                    var d = TryMerge(input, source, item.Value);
                    dictionary.Add(item.Key, d);                   
                    break;
                }
                if(item.Key.Compares("name") && item.Value is string && key.Compares(item.Value.ToString()) )
                {
                    var d = TryMerge(input,source, input);
                   // dictionary.Add(item.Key, d);                  
                    break;
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
        private bool RemoveModule<T>(T input, bool removing) where T : ITree
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
                AddKeyValue(_tree, root, obj, true);              
            }
        }
        private ConverterNode GetValue(HtmlNode node, string tag, string stoptag)
        {
            var result = new ConverterNode();
            var sibling = node;
            result.Key = string.Empty;
            var tags = new List<HtmlNode>();
            while (sibling != null && !sibling.Name.Compares("h1") )
            {
                if (sibling.Name.Compares(tag))
                {
                    result.Key = sibling.InnerText;
                    result.SerialNode = sibling;
                    result.TagNode = new []{ sibling };
                    break;
                }
                var name = sibling.Descendants(tag);
                if (name.Any(o => o.Name.Compares(tag)))
                {
                    tags.AddRange(name);
                    result.SerialNode = sibling;
                }
                result.Key = name.FirstOrDefault()?.InnerText;
                result.TagNode = tags;
                
                sibling = sibling.NextSibling;
                if(sibling == null || sibling.Name.Compares(stoptag))
                {
                    break;
                }
            }
            return result;
        }
        private Tree ToObject(HtmlNode node)
        {
            ConverterNode pickerNode;
            Tree t = new Tree();
            while (node != null && !node.Name.Compares("h1"))
            {
                pickerNode = GetValue(node, "h2", "h1");
                string key = pickerNode.Key;
                if (string.IsNullOrEmpty(key))
                {
                    return null;
                }
                pickerNode = GetValue(pickerNode.SerialNode, "table", "h2");
                if (pickerNode.TagNode.Any())
                {
                    List<Tree> obj = new List<Tree>();
                    pickerNode.TagNode.ToList().ForEach(o =>
                    {
                        obj.AddRange(ToData(o));                        
                    });
                    if (obj.Any())
                    {
                        AddKeyValue(t, key, obj.ToArray(), true);
                    }
                }

                node = pickerNode.SerialNode?.NextSibling;
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
                        AddKeyValue(t, keys[key], td.InnerText.CleanHTML(), false);
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
        private void AddKeyValue(IDictionary<string,object> dictionary, string key, object value, bool enablemap)
        {
                  
            if (string.IsNullOrEmpty(key))
            {
                return;
            }
            key = key.CleanHTML().Replace(" ",string.Empty);
            if (dictionary.Keys.Any(o => o.Compares(key)))
            {
                return;
            }
            if (enablemap)
            {
                var k = GetMapped(key, value).First();
                key = k.Key;
                value = k.Value;
            }
            dictionary.Add(key, value);

        }
        private List<KeyValuePair<string,object>> GetMapped(string key, object value)
        {           
            var res = _map.Where(o => o.From.Compares(key)).Select(m =>
            {
                key = m.To;
                if (value is string && m.From.ToLower().Contains("hex"))
                {
                    value = value.ToString().FromHex();
                }
                return KeyValuePair.Create(key, value);
            }).ToList();

            if(!res.Any())
            {
                res.Add(KeyValuePair.Create(key, value));
            }
            return res;
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

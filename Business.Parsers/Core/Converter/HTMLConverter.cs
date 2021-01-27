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
        private IDictionary<string, object> _tomlTree;
        private IBuilder<IDictionary<string, object>> _builder;
        public HTMLConverter(ConvertConfig config, IBuilder<IDictionary<string, object>> builder)
        {
            _config = config;
            _tree = new Tree();
            _builder = builder;
        }
        public ITree ToConverted(string html)
        {
           
            CleanToCompatible(ref html);
            ToDictionary(html);

            var toml = _config.GetBaseToml();
            _tomlTree = _builder.ToDictionary(toml);

            var map = _config.GetMapping();
            MergeValues(map);

            return _tree;
        }
        
        public string ToJson(string json)
        {
            return JsonConvert.SerializeObject(ToConverted(json));
        }

        #region Private members
        private void MergeValues(IEnumerable<ConfigMap> map)
        {

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
            while (sibling != null && sibling.Name.ToLower() != "h1")
            {
                var name = sibling.Descendants(tag).FirstOrDefault();
                if (sibling.Name.ToLower() == tag)
                {
                    result.Key = sibling.InnerText;
                    result.SerialNode = sibling;
                    result.TagNode = sibling;
                    break;
                }
                if (name != null && name.Name.ToLower() == tag)
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
            while (node != null && node.Name.ToLower() != "h1")
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
                        if (td.Attributes.Any(o => o.Name.ToLower() == "colspan" && o.Value.ToInt() > 1))
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

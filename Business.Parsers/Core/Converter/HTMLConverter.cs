using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Business.Parsers.Core.Converter
{
    public class HTMLConverter : IHTMLConverter
    {
        private readonly ConvertConfig _config;
        private XDocument _document;
        private Tree _tree;
        public HTMLConverter(ConvertConfig config)
        {
            _config = config;
            _tree = new Tree();
        }
        public ITree ToConverted(string html)
        {
            ToDictionary(html);
            return _tree;
        }
        private void ToDictionary(string html)
        {
            _document = XDocument.Parse(html);
            foreach(var table in _document.Descendants("table"))
            {
                ToRoot(table);
            }
        }
        private void ToRoot(XElement table)
        {            
            _tree.Add(GetValue(table, "h1"), ToObject(table));
        }
        private string GetValue(XElement table,string tag)
        {
            var parent = table.Parent;
            var key = "root";
            while (parent != null)
            {
                var name = parent.Descendants(tag).FirstOrDefault();
                if (name != null)
                {
                    if (name.Name.LocalName.ToLower() == tag)
                    {
                        key = name.Value;
                        break;
                    }
                }
                parent = parent.Parent;
            }
            return key;
        }
        private Tree ToObject(XElement table)
        {
            Tree t = new Tree();
            t.Add(GetValue(table, "h2"), ToData(table));
            return t;
        }
        private Tree ToData(XElement table)
        {
            Tree t = new Tree();
            Dictionary<int, string> keys = new Dictionary<int, string>();
            int start = 0;
            foreach (var tr in table.Descendants("tr"))
            {
                if (start++ == 0)
                {
                    int key = 1;
                    foreach (var td in tr.Descendants("td"))
                    {
                        keys.Add(key++, td.Value);
                    }
                }
                else
                {
                    int key = 1;
                    foreach (var td in tr.Descendants("td"))
                    {
                        t.Add(keys[key++], td.Value);
                    }

                }
            }
            return t.Count > 0 ? t: null;
        }

        public string ToJson(string json)
        {
            return JsonConvert.SerializeObject(ToConverted(json));
        }
    }
}

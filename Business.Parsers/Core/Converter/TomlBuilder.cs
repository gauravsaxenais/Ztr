using Nett;
using System;
using System.Collections.Generic;
using System.Text;

namespace Business.Parsers.Core.Converter
{
    public class TomlBuilder : IBuilder<IDictionary<string,object>>
    {
        public string ToTOML(IDictionary<string, object> content)
        {
            return Toml.WriteString(content);
        }
    }
}

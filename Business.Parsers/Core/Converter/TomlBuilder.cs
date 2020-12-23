namespace Business.Parsers.Core.Converter
{
    using Nett;
    using System.Collections.Generic;

    public class TomlBuilder : IBuilder<IDictionary<string,object>>
    {
        public string ToTOML(IDictionary<string, object> content)
        {
            return Toml.WriteString(content);
        }
    }
}

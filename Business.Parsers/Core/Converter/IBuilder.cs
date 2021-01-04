using Business.Parsers.Core.Models;

namespace Business.Parsers.TomlParser.Core.Converter
{
    public interface IBuilder<T>
    {
        string ToTOML(T content, ValueScheme scheme);
    }
}

namespace Business.Parsers.TomlParser.Core.Converter
{
    public interface IConverter<T>
    {
        T ToConverted(string json);

        string ToJson(string json);
    }
}

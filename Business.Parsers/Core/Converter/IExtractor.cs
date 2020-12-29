namespace Business.Parsers.TomlParser.Core.Converter
{
    public interface IExtractor<T>
    {
        public T Convert(object[] input);
    }
}

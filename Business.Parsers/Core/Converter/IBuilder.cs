namespace Business.Parsers.Core.Converter
{
    using Models;
    public interface IBuilder<T>
    {
        string ToTOML(T content, ValueScheme scheme);
    }
}

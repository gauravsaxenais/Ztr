namespace Business.Parsers.Core.Converter
{
    public interface IBuilder<T>
    {
        string ToTOML(T content);
    }
}

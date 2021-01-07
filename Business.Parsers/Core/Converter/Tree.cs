namespace Business.Parsers.Core.Converter
{
    using System.Collections.Generic;
    public interface ITree : IDictionary<string, object>
    {
    }
    public class Tree : Dictionary<string, object>, ITree
    {
      
    }   
}

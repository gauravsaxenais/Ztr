using System;
using System.Collections.Generic;
using System.Text;

namespace Business.Parsers.Core.Converter
{
    public interface ITree : IDictionary<string, object>
    {
    }
    public class Tree : Dictionary<string, object>, ITree
    {
      
    }   
}

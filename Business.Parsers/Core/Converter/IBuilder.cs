using System;
using System.Collections.Generic;
using System.Text;

namespace Business.Parsers.Core.Converter
{
    public interface IBuilder<T>
    {
        string ToTOML(T content);
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Business.Parsers.Core.Converter
{
    public interface IConverter<T>
    {
        T ToConverted(string json);

        string ToJson(string json);
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Business.Parsers.Core.Converter
{
    public interface IExtractor<T>
    {
        public T Convert(object[] input);
    }
}

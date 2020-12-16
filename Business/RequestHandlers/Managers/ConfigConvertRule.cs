using System.Collections.Generic;

namespace Business.RequestHandlers.Managers
{
    public class ConfigConvertRule
    {
        public string Property { get; set; }
        public IList<ConfigConvertObject> Schema { get; set; }
    }
   
    public class ConfigConvertObject
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}

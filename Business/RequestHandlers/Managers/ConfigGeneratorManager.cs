using Business.RequestHandlers.Interfaces;
using Nett;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Business.RequestHandlers.Managers
{
    public class ConfigGeneratorManager : IConfigGeneratorManager
    {
        public async Task<string> CreateConfigAsync(string jsonContent)
        {
            TextReader readFile = new StreamReader(@"C:\Users\admin.DESKTOP-G7578TS\source\ZTR\DeviceConfigAPI\bin\Debug\netcoreapp3.1\BlockConfig\config\config.json");
            string content = await readFile.ReadToEndAsync();
            //var fileContent = Toml.ReadString(content);
            //dynamic configurationObject = JsonConvert.DeserializeObject(content);
            JObject configurationObject = (JObject)JsonConvert.DeserializeObject(content);
            var serMe = configurationObject.ToObject<Dictionary<string, object>>();
            StringBuilder sb = new StringBuilder();
            foreach (var item in serMe)
            {
                Console.WriteLine(item.Key);
                sb.Append("[" + item.Key + "]");
                
                if (item.Value.GetType().Name == "JObject")
                {
                    Console.WriteLine(item.Value.GetType().Name);
                    foreach (var itemV in item.Value.ToString().Trim().Split(','))
                    {
                        sb.Append(Environment.NewLine);
                        sb.Append(itemV.ToString()
                            .Replace("{", "")
                            .Replace("\"", "")
                            .Replace("\r\n", "")
                            .Replace(" ", "")
                            .Replace(":", "=")
                            .Replace("}", ""));
                    }
                }
                else if (item.Value.GetType().Name == "JArray")
                {
                    dynamic dynamicArray = item.Value as JArray;
                    foreach (var itemV in dynamicArray)
                    {
                        foreach (var i in item.Value.ToString().Trim().Split(','))
                        {
                            sb.Append(Environment.NewLine);
                            sb.Append(i.ToString().Replace("{", "").Replace("\"", "").Replace("\r\n", "").Replace(" ", "").Replace(":", "=").Replace("}", ""));
                        }
                    }
                }


                sb.Append(Environment.NewLine);
                sb.Append(Environment.NewLine);
            }
            string contents = string.Empty;
            contents = Toml.WriteString(serMe);
            Console.WriteLine(contents);
            
            return contents;
        }
    }
}

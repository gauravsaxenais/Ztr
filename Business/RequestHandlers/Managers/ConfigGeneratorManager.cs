namespace Business.RequestHandlers.Managers
{
    using Business.RequestHandlers.Interfaces;
    using Nett;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using EnsureThat;

    /// <summary>
    ///   <br />
    /// </summary>
    public class ConfigGeneratorManager : IConfigGeneratorManager
    {
        /// <summary>Creates the configuration asynchronous.</summary>
        /// <param name="jsonContent">Content of the json.</param>
        /// <returns>
        ///   <br />
        /// </returns>
        public async Task<string> CreateConfigAsync(string jsonContent)
        {
            EnsureArg.IsNotEmptyOrWhiteSpace(jsonContent);

            TextReader readFile = new StreamReader(@"C:\Users\admin.DESKTOP-G7578TS\source\ZTR\DeviceConfigAPI\bin\Debug\netcoreapp3.1\BlockConfig\config\config.json");
            string content = await readFile.ReadToEndAsync();

            JObject configurationObject = (JObject)JsonConvert.DeserializeObject(content);
            var serMe = configurationObject.ToObject<Dictionary<string, object>>();

            var sb = new StringBuilder();

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

            string contents = Toml.WriteString(serMe);
            Console.WriteLine(contents);
            
            return contents;
        }
    }
}

namespace Business.Parsers
{
    using Business.Parser.Models;
    using EnsureThat;
    using Nett;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class ModuleParser
    {
        public string ReadFileAsJson(string fileContent, TomlSettings settings, Message message)
        {
            EnsureArg.IsNotEmptyOrWhiteSpace(fileContent, (nameof(fileContent)));

            var json = string.Empty;

            var fileData = Toml.ReadString(fileContent, settings);

            var dictionary = fileData.ToDictionary();

            var module = (Dictionary<string, object>[])dictionary["module"];

            // here message.name means Power, j1939 etc.
            var moduleDetail = module.Where(dic => dic.Values.Contains(message.Name.ToLower())).FirstOrDefault();

            if (moduleDetail != null)
            {
                var configValues = (Dictionary<string, object>)moduleDetail["config"];

                json += "{";
                WriteData(configValues, message, ref json);
                json = json.TrimEnd(',');

                json += "}";
            }

            return json;
        }

        public static void WriteData(Dictionary<string, object> configValues, Message message, ref string json)
        {
            foreach (KeyValuePair<string, object> entry in configValues)
            {
                var key = entry.Key;

                var fields = message.Fields;
                var messages = message.Messages;

                var foundField = fields.Where(x => string.Equals(x.Name, key, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                var foundMessage = messages.Where(x => string.Equals(x.Name, key, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

                // its a field
                if (foundField != null)
                {
                    json += $"\"{foundField.Name}\": {{ \"min\": {foundField.Min}, \"max\": {foundField.Max}, \"value\": {entry.Value}, \"datatype\": \"{foundField.DataType}\" }}";
                }

                if (foundMessage != null)
                {
                    if (!foundMessage.Messages.Any())
                    {
                        json += $"\"name\": \"{foundMessage.Name}\"" + ", ";
                        json += $"\"datatype\": " + (foundMessage.IsRepeated ? "\"array\"" : "\"notarray\"") + ", ";
                        json += $"\"args\":" + (foundMessage.IsRepeated ? "[" : string.Empty);
                        json += WriteMessageField(foundMessage.Fields, (Dictionary<string, object>[])entry.Value);
                        json += foundMessage.IsRepeated ? "]" : string.Empty;
                    }

                    else
                    {
                        foreach (var msg in foundMessage.Messages)
                        {
                            WriteData((Dictionary<string, object>)entry.Value, msg, ref json);
                        }
                    }
                }

                json += ",";
            }
        }

        public static string WriteMessageField(List<Field> fields, Dictionary<string, object>[] values)
        {
            var json = new StringBuilder();
            if (fields == null || !fields.Any() || values == null || !values.Any())
            {
                return json.ToString();
            }

            foreach (var dictionary in values)
            {
                json.Append("[");
                for(int temp = 0; temp  < fields.Count; temp++)
                {
                    json.Append("{");
                    object value = dictionary.ContainsKey(fields[temp].Name) ? dictionary[fields[temp].Name] : fields[temp].Value;

                    json.Append($"\"id\": {temp}, \"name\": \"{fields[temp].Name} \", \"min\": {fields[temp].Min}, \"max\": {fields[temp].Max}, \"value\": {value}, \"datatype\": \"{fields[temp].DataType}\"");

                    json.Append("}");
                    json.Append(",");
                }

                if (json.Length > 0)
                {
                    json.Length --;
                }

                json.Append("]");

                json.Append(",");
            }

            if (json.Length > 0)
            {
                json.Length--;
            }

            return json.ToString();
        }
    }
}

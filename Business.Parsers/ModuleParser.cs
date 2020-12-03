namespace Business.Parsers
{
    using Business.Parser.Models;
    using Business.Parsers.Models;
    using EnsureThat;
    using Nett;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Message = Parser.Models.Message;

    public class ModuleParser
    {
        public void ReadFileAsJson(string fileContent, TomlSettings settings, Message message)
        {
            EnsureArg.IsNotEmptyOrWhiteSpace(fileContent, (nameof(fileContent)));
            
            var fileData = Toml.ReadString(fileContent, settings);

            var dictionary = fileData.ToDictionary();

            var module = (Dictionary<string, object>[])dictionary["module"];

            // here message.name means Power, j1939 etc.
            var moduleDetail = module.Where(dic => dic.Values.Contains(message.Name.ToLower())).FirstOrDefault();

            if (moduleDetail != null)
            {
                var configValues = new Dictionary<string, object>();
                
                if (moduleDetail.ContainsKey("config"))
                {
                    configValues = (Dictionary<string, object>)moduleDetail["config"];
                }

                WriteData(configValues, message);
            }
        }

        public static void WriteData(Dictionary<string, object> configValues, Message message)
        {
            foreach (KeyValuePair<string, object> entry in configValues)
            {
                var key = entry.Key;

                var fields = message.Fields.FirstOrDefault();
                var messages = message.Messages;

                Field foundField = null;
                if (fields != null)
                {
                    foundField = fields.Where(x => string.Equals(x.Name, key, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                }

                var foundMessage = messages.Where(x => string.Equals(x.Name, key, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

                // its a field
                if (foundField != null)
                {
                    foundField.Value = GetFieldValue(entry.Value);
                }

                if (foundMessage != null)
                {
                    if (!foundMessage.Messages.Any())
                    {
                        var arrayFields = foundMessage.Fields.FirstOrDefault();
                        var fieldsWithData = GetMessageFields(arrayFields, (Dictionary<string, object>[])entry.Value);

                        foundMessage.Fields.Clear();
                        foundMessage.Fields.AddRange(fieldsWithData);
                    }

                    else
                    {
                        foreach (var msg in foundMessage.Messages)
                        {
                            WriteData((Dictionary<string, object>)entry.Value, msg);
                        }
                    }
                }
            }
        }

        private static bool IsValueType(object obj)
        {
            var objType = obj.GetType();
            return obj != null && objType.GetTypeInfo().IsValueType;
        }

        private static object GetFieldValue(object field)
        {
            string result = string.Empty;

            Type stringType = typeof(string);
            Type fieldType = field.GetType();

            if (fieldType.IsArray)
            {
                result += "[";
                var element = ((IEnumerable)field).Cast<object>().FirstOrDefault();

                if (IsValueType(element))
                {
                    IEnumerable fields = field as IEnumerable;

                    foreach (var tempItem in fields)
                    {
                        result += tempItem + ",";
                    }

                    result = result.TrimEnd(',');
                }

                else if (stringType.IsAssignableFrom(element.GetType()))
                {
                    string[] stringFields = ((IEnumerable)field).Cast<object>()
                                                                .Select(x => x.ToString())
                                                                .ToArray();

                    stringFields = stringFields.ToList().Select(c => { c = $"\"{c}\""; return c; }).ToArray();

                    result += string.Join(",", stringFields);
                }

                result += "]";
            }

            else
            {
                if (IsValueType(field))
                {
                    result = field.ToString();
                }

                else
                {
                    result = $"\"{field}\":";
                }
            }

            return result;
        }

        public static List<List<Field>> GetMessageFields(List<Field> fields, Dictionary<string, object>[] values)
        {
            var arrayOfDataAsFields = new List<List<Field>>();

            if (fields == null || !fields.Any() || values == null || !values.Any())
            {
                return arrayOfDataAsFields;
            }
            
            foreach (var dictionary in values)
            {
                // make a copy of first list;
                var copyFirstList = fields.Select(x => new Field() { Id = x.Id, DataType = x.DataType, Max = x.Max, Min = x.Min, Name = x.Name, Value = x.Value }).ToList();

                for (int tempIndex = 0; tempIndex < copyFirstList.Count; tempIndex++)
                {
                    object value = dictionary.ContainsKey(copyFirstList[tempIndex].Name) ? dictionary[copyFirstList[tempIndex].Name] : copyFirstList[tempIndex].Value;

                    // fix the indexes.
                    copyFirstList[tempIndex].Id = tempIndex;
                    copyFirstList[tempIndex].Value = value;
                }

                arrayOfDataAsFields.Add(copyFirstList);
            }

            return arrayOfDataAsFields;
        }
    }
}

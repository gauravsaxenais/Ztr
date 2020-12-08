namespace Business.Parsers
{
    using Business.Parsers.Models;
    using EnsureThat;
    using Nett;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    
    public class ModuleParser
    {
        public JsonModel GetJsonFromDefaultValueAndProtoFile(string fileContent, TomlSettings settings, ProtoParsedMessage protoParserMessage)
        {
            EnsureArg.IsNotEmptyOrWhiteSpace(fileContent, (nameof(fileContent)));


            var jsonModel = new JsonModel
            {
                Name = protoParserMessage.Name,
                Arrays = new List<object>()
            };

            var fileData = Toml.ReadString(fileContent, settings);

            var dictionary = fileData.ToDictionary();
            var module = (Dictionary<string, object>[])dictionary["module"];

            // here message.name means Power, j1939 etc.
            var moduleDetail = module.Where(dic => dic.Values.Contains(protoParserMessage.Name.ToLower())).FirstOrDefault();

            if (moduleDetail != null)
            {
                var configValues = new Dictionary<string, object>();

                if (moduleDetail.ContainsKey("config"))
                {
                    configValues = (Dictionary<string, object>)moduleDetail["config"];
                }

                MergeTomlWithProtoMessage(configValues, protoParserMessage, jsonModel);
            }

            return jsonModel;
        }

        public void MergeTomlWithProtoMessage(Dictionary<string, object> configValues, ProtoParsedMessage protoParsedMessage, JsonModel model)
        {
            for (int tempIndex = 0; tempIndex < configValues.Count; tempIndex++)
            {
                var key = configValues.ElementAt(tempIndex).Key;

                var messages = protoParsedMessage.Messages;
                var fields = protoParsedMessage.Fields;

                var field = fields.Where(x => string.Equals(x.Name, key, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                var repeatedMessage = messages.Where(x => string.Equals(x.Name, key, StringComparison.OrdinalIgnoreCase) && x.IsRepeated).FirstOrDefault();

                // its a field
                if (field != null)
                {
                    field.Id = tempIndex;
                    field.Value = GetFieldValue(configValues.ElementAt(tempIndex).Value);

                    var newField = field.DeepCopy();
                    model.Fields.Add(newField);
                }

                else if (repeatedMessage != null)
                {
                    var jsonArray = new JsonArray
                    {
                        Name = repeatedMessage.Name,
                        IsRepeated = repeatedMessage.IsRepeated
                    };

                    Dictionary<string, object>[] values = null;
                    var arrayMessages = repeatedMessage.Messages.Where(x => x.IsRepeated);

                    if (configValues.ElementAt(tempIndex).Value is Dictionary<string, object>[])
                    {
                        values = (Dictionary<string, object>[])configValues.ElementAt(tempIndex).Value;
                    }

                    // declare empty.
                    else values = new Dictionary<string, object>[0];

                    if (!arrayMessages.Any())
                    {
                        var fieldsWithData = GetFieldsData(repeatedMessage, values);
                        jsonArray.Data = fieldsWithData;

                        model.Arrays = jsonArray;
                    }

                    else
                    {
                        var jsonModels = new List<JsonModel>();
                        
                        for (int temp = 0; temp < values.Length; temp++)
                        {
                            var tempJsonModel = new JsonModel();
                            tempJsonModel.Id = temp;

                            MergeTomlWithProtoMessage(values[temp], repeatedMessage, tempJsonModel);
                            jsonModels.Add(tempJsonModel);
                        }

                        model.Arrays = jsonModels;
                    }
                }
            }
        }

        private bool IsValueType(object obj)
        {
            var objType = obj.GetType();
            return obj != null && objType.GetTypeInfo().IsValueType;
        }

        private object GetFieldValue(object field)
        {
            object result = new object();

            var stringType = typeof(string);
            var fieldType = field.GetType();

            if (fieldType.IsArray)
            {
                var arrayResult = "[";
                var element = ((IEnumerable)field).Cast<object>().FirstOrDefault();

                if (IsValueType(element))
                {
                    IEnumerable fields = field as IEnumerable;

                    foreach (var tempItem in fields)
                    {
                        arrayResult += tempItem + ",";
                    }

                    arrayResult += arrayResult.TrimEnd(',');
                }

                else if (stringType.IsAssignableFrom(element.GetType()))
                {
                    string[] stringFields = ((IEnumerable)field).Cast<object>()
                                                                .Select(x => x.ToString())
                                                                .ToArray();

                    arrayResult += string.Join(",", stringFields);
                }

                arrayResult += "]";

                result = arrayResult;
            }

            else
            {
                result = field;
            }

            return result;
        }

        private List<List<Field>> GetFieldsData(ProtoParsedMessage message, Dictionary<string, object>[] values)
        {
            var fields = message.Fields;

            var arrayOfDataAsFields = new List<List<Field>>();

            if (fields == null || !fields.Any() || values == null || !values.Any())
            {
                return arrayOfDataAsFields;
            }

            foreach (var dictionary in values)
            {
                var copyFirstList = fields.Select(x => x.DeepCopy()).ToList();

                for (int tempIndex = 0; tempIndex < copyFirstList.Count; tempIndex++)
                {
                    object value = dictionary.ContainsKey(copyFirstList[tempIndex].Name) ? dictionary[copyFirstList[tempIndex].Name] : copyFirstList[tempIndex].Value;

                    if (value != null)
                    {
                        // fix the indexes.
                        copyFirstList[tempIndex].Id = tempIndex;
                        copyFirstList[tempIndex].Value = value;
                    }
                }

                arrayOfDataAsFields.Add(copyFirstList);
            }

            return arrayOfDataAsFields;
        }
    }
}

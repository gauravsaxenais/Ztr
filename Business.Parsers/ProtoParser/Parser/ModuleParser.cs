namespace Business.Parsers.ProtoParser.Parser
{
    using Business.Parsers.ProtoParser.Models;
    using EnsureThat;
    using Nett;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using ZTR.Framework.Business.File.FileReaders;

    public class ModuleParser
    {
        public List<Dictionary<string, object>> GetListOfModulesFromTomlFile(string fileContent, TomlSettings settings)
        {
            EnsureArg.IsNotEmptyOrWhiteSpace(fileContent, (nameof(fileContent)));

            var fileData = Toml.ReadString(fileContent, settings);

            var dictionary = fileData.ToDictionary();
            var modules = (Dictionary<string, object>[])dictionary["module"];

            return modules.ToList();
        }

        public List<JsonField> GetJsonFromTomlAndProtoFile(string fileContent, ProtoParsedMessage protoParserMessage)
        {
            var tomlSettings = TomlFileReader.LoadLowerCaseTomlSettingsWithMappingForDefaultValues();
            var jsonModels = new List<JsonField>();
            var listOfModules = GetListOfModulesFromTomlFile(fileContent, tomlSettings);

            // here message.name means Power, j1939 etc.
            var module = listOfModules.Where(dic => dic.Values.Contains(protoParserMessage.Name.ToLower())).FirstOrDefault();

            if (module != null)
            {
                var configValues = new Dictionary<string, object>();

                if (module.ContainsKey("config"))
                {
                    configValues = (Dictionary<string, object>)module["config"];
                }

                jsonModels = MergeTomlWithProtoMessage(configValues, protoParserMessage);
            }

            return jsonModels;
        }

        public List<JsonField> MergeTomlWithProtoMessage(Dictionary<string, object> configValues, ProtoParsedMessage protoParsedMessage)
        {
            var listOfData = new List<JsonField>();
            listOfData.AddRange(GetFieldsFromProtoMessage(protoParsedMessage));

            foreach (var dicItem in configValues.Select((Kvp, Index) => new { Kvp, Index }))
            {
                var field = listOfData.Where(x => string.Equals(x.Name, dicItem.Kvp.Key, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

                if (field != null)
                {
                    field.Value = GetFieldValue(dicItem.Kvp.Value);
                    field.IsVisible = true;
                }

                // we have a repeated / non repeated message.
                else
                {
                    var repeatedMessage = protoParsedMessage.Messages.Where(x => string.Equals(x.Name, dicItem.Kvp.Key, StringComparison.OrdinalIgnoreCase) && x.IsRepeated).FirstOrDefault();
                    var nonRepeatedMessage = protoParsedMessage.Messages.Where(x => string.Equals(x.Name, dicItem.Kvp.Key, StringComparison.OrdinalIgnoreCase) && !x.IsRepeated).FirstOrDefault();

                    var tempJsonModel = new JsonField();

                    if (repeatedMessage != null)
                    {
                        tempJsonModel.Name = repeatedMessage.Name;
                        tempJsonModel.IsVisible = true;

                        if (dicItem.Kvp.Value is Dictionary<string, object>[] repeatedValues)
                        {
                            repeatedValues.ToList().ForEach(item => tempJsonModel.Arrays.Add(MergeTomlWithProtoMessage(item, repeatedMessage)));
                        }
                    }

                    else if (nonRepeatedMessage != null)
                    {
                        tempJsonModel.Name = nonRepeatedMessage.Name;
                        tempJsonModel.IsVisible = true;

                        if (dicItem.Kvp.Value is Dictionary<string, object> nonRepeatedValues)
                        {
                            var subArrayData = MergeTomlWithProtoMessage(nonRepeatedValues, nonRepeatedMessage);

                            var fields = subArrayData.FirstOrDefault();
                            tempJsonModel.Fields.Add(fields);
                        }
                    }

                    listOfData.Add(tempJsonModel);
                }
            }

            listOfData.Select((item, index) => { item.Id = index; return item; }).ToList();

            return listOfData;
        }

        private List<JsonField> GetFieldsFromProtoMessage(ProtoParsedMessage protoParsedMessage)
        {
            EnsureArg.IsNotNull(protoParsedMessage);

            var listOfFields = new List<JsonField>();
            for (int tempIndex = 0; tempIndex < protoParsedMessage.Fields.Count; tempIndex++)
            {
                var newField = (Field)protoParsedMessage.Fields[tempIndex].Clone();

                var jsonModel = new JsonField
                {
                    Name = newField.Name,
                    Value = newField.Value,
                    Min = newField.Min,
                    Max = newField.Max,
                    DataType = newField.DataType,
                    DefaultValue = newField.DefaultValue,
                    IsVisible = false
                };

                listOfFields.Add(jsonModel);
            }

            return listOfFields;
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
    }
}

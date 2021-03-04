namespace Business.Parsers.ProtoParser.Parser
{
    using EnsureThat;
    using Models;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using ZTR.Framework.Business;

    public class ModuleParser : IModuleParser
    {
        /// <summary>
        /// Merges the toml with proto message.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configValues">The configuration values.</param>
        /// <param name="protoParsedMessage">The proto parsed message.</param>
        /// <returns></returns>
        public List<JsonField> MergeTomlWithProtoMessage<T>(T configValues, ProtoParsedMessage protoParsedMessage) where T : Dictionary<string, object>
        {
            var listOfData = new List<JsonField>();

            // add fields, repeated and non-repeated messages to the list
            // in case there is no data, just return fields, repeated and non-repeated messages
            // as basic data.
            listOfData.AddRange(AddEmptyFieldsAndArrays(protoParsedMessage));
            if (configValues.Any())
            {
                // remove repeated and non-repeated messages from the list
                // and keep fields only.
                RemoveEmptyArrays(listOfData);
            }
            foreach (var dicItem in configValues.Select((kvp, index) => new { Kvp = kvp, Index = index }))
            {
                var field = listOfData.FirstOrDefault(x =>
                     string.Equals(x.Name, dicItem.Kvp.Key, StringComparison.OrdinalIgnoreCase));

                if (field != null)
                {
                    field.Value = field.IsRepeated ? GetRepeatedFieldValue(dicItem.Kvp.Value) : GetFieldValue(dicItem.Kvp.Value);
                    field.IsVisible = true;
                    field.IsFieldRepeated = field.IsRepeated;
                }
                // we have a repeated / non repeated protoParsedMessage.
                else
                {
                    foreach (var message in protoParsedMessage.Messages)
                    {
                        if (string.Equals(message.Name, dicItem.Kvp.Key, StringComparison.OrdinalIgnoreCase))
                        {
                            var tempJsonModel = new JsonField
                            {
                                Name = message.Name,
                                IsVisible = true,
                                DataType = "array"
                            };
                            if (dicItem.Kvp.Value is T[] repeatedValues)
                            {
                                repeatedValues.ToList().ForEach(item =>
                                    tempJsonModel.Arrays.Add(MergeTomlWithProtoMessage(item, message)));
                            }
                            else if (dicItem.Kvp.Value is T nonRepeatedValues)
                            {
                                var subArrayData = MergeTomlWithProtoMessage(nonRepeatedValues, message);
                                tempJsonModel.Fields.AddRange(subArrayData);
                            }
                            else
                            {
                                tempJsonModel.IsVisible = false;
                                if (message.IsRepeated)
                                {
                                    tempJsonModel.Arrays.Add(MergeTomlWithProtoMessage(new Dictionary<string, object>(), message));
                                }

                                else
                                {
                                    tempJsonModel.Fields.AddRange(MergeTomlWithProtoMessage(new Dictionary<string, object>(), message));
                                }
                            }

                            listOfData.Add(tempJsonModel);
                            break;
                        }
                    }
                }
            }

            // fix the indexes
            FixIndex(listOfData);
            return listOfData;
        }

        #region Private methods
        private List<JsonField> AddEmptyFieldsAndArrays(ProtoParsedMessage protoParsedMessage)
        {
            var listOfData = new List<JsonField>();
            listOfData.AddRange(GetFieldsFromProtoMessage(protoParsedMessage));

            foreach (var message in protoParsedMessage.Messages)
            {
                var tempJsonModel = new JsonField
                {
                    Name = message.Name,
                    IsVisible = true,
                    DataType = "array"
                };

                if (message.IsRepeated)
                {
                    tempJsonModel.Arrays.Add(AddEmptyFieldsAndArrays(message));
                }

                else tempJsonModel.Fields.AddRange(AddEmptyFieldsAndArrays(message));

                listOfData.Add(tempJsonModel);
            }

            // fix the indexes
            FixIndex(listOfData);

            return listOfData;
        }

        private static IEnumerable<JsonField> GetFieldsFromProtoMessage(ProtoParsedMessage protoParsedMessage)
        {
            EnsureArg.IsNotNull(protoParsedMessage);
            var list = new List<JsonField>();
            foreach (var field in protoParsedMessage.Fields)
            {
                var clonedField = field.Copy();
                var jsonField = new JsonField()
                {
                    Name = clonedField.Name,
                    Value = clonedField.IsRepeated ? new List<object>() { clonedField.DefaultValue } : clonedField.Value,
                    Min = clonedField.Min,
                    Max = clonedField.Max,
                    DataType = clonedField.DataType,
                    DefaultValue = clonedField.DefaultValue,
                    IsVisible = false,
                    IsRepeated = clonedField.IsRepeated,
                    IsFieldRepeated = clonedField.IsRepeated
                };

                list.Add(jsonField);
            }

            return list;
        }
        #endregion

        #region Private helper methods
        private static void FixIndex(IReadOnlyList<JsonField> listOfData)
        {
            for (var index = 0; index < listOfData.Count; index++)
            {
                listOfData[index].Id = index;
            }
        }

        private void RemoveEmptyArrays(List<JsonField> listOfData)
        {
            if (listOfData != null && listOfData.Any())
            {
                var arrayTypes = listOfData.Where(x => x.DataType == "array").ToList();

                for (int index = 0; index < arrayTypes.Count; index++)
                {
                    var fields = arrayTypes[index].Fields;
                    var arrays = arrayTypes[index].Arrays;

                    RemoveEmptyArrays(fields);
                    arrays.ForEach(RemoveEmptyArrays);
                    fields.Clear();
                    arrays.Clear();
                    listOfData.RemoveAll(t => t.Name == arrayTypes[index].Name);
                }
            }

            // fix the indexes
            FixIndex(listOfData);
        }
        private static object GetFieldValue(object fieldValue)
        {
            var fieldType = fieldValue.GetType();
            object result;
            if (fieldType.IsArray)
            {
                var stringFields = ((IEnumerable)fieldValue).Cast<object>().ToList();
                result = "[" + string.Join(",", stringFields) + "]";
            }
            else result = fieldValue;
            return result;
        }
        private static object GetRepeatedFieldValue(object fieldValue)
        {
            return fieldValue.GetType().IsArray ? ((IEnumerable)fieldValue).Cast<object>().ToList() : new List<object>();
        }
        #endregion
    }
}
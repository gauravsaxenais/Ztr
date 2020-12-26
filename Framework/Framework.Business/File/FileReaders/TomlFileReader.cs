namespace ZTR.Framework.Business.File.FileReaders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using EnsureThat;
    using Nett;
    using ZTR.Framework.Business.Models;
    using ZTR.Framework.Configuration.Content;

    public static class TomlFileReader
    {
        public static T ReadDataFromString<T>(string data, TomlSettings settings) where T : class, new()
        {
            EnsureArg.IsNotEmptyOrWhiteSpace(data, nameof(data));
            EnsureArg.IsNotNull(settings, nameof(settings));

            T fileData = null;
            try
            {
                fileData = Toml.ReadString<T>(data, settings);
            }
            catch (Exception)
            {
                throw new CustomArgumentException(Resource.TomlParsingError);
            }

            return fileData;
        }

        public static T ReadDataFromFile<T>(string filePath, TomlSettings settings) where T : class, new()
        {
            EnsureArg.IsNotEmptyOrWhiteSpace(filePath, (nameof(filePath)));
            EnsureArg.IsNotNull(settings, nameof(settings));

            T fileData = null;

            try
            {
                fileData = Toml.ReadFile<T>(filePath, settings);
            }
            catch (Exception)
            {
                throw new CustomArgumentException(Resource.TomlParsingError);
            }

            return fileData;
        }

        public static List<T> ReadDataAsListFromFile<T>(string filePath, TomlSettings settings, string fieldToRead) where T : class, new()
        {
            EnsureArg.IsNotEmptyOrWhiteSpace(filePath, (nameof(filePath)));
            EnsureArg.IsNotNull(settings, nameof(settings));
            EnsureArg.IsNotEmptyOrWhiteSpace(fieldToRead, nameof(fieldToRead));

            var items = new List<T>();

            try
            {
                TomlTable fileData = null;

                fileData = Toml.ReadFile(filePath, settings);

                var readModels = (TomlTableArray)fileData[fieldToRead];


                foreach (var tempItem in readModels.Items)
                {
                    var dictionary = tempItem.Rows.ToDictionary(t => t.Key, t => (object)t.Value.ToString());

                    items.Add(DictionaryExtensions.ToObject<T>(dictionary));
                }
            }
            catch (Exception)
            {
                throw new ArgumentException("An error occurred while parsing Toml file.");
            }

            return items;
        }

        public static List<T> ReadDataAsListFromString<T>(string data, string fieldToRead, TomlSettings settings) where T : class, new()
        {
            EnsureArg.IsNotEmptyOrWhiteSpace(data, nameof(data));
            EnsureArg.IsNotNull(settings, nameof(settings));
            EnsureArg.IsNotEmptyOrWhiteSpace(fieldToRead, nameof(fieldToRead));

            var items = new List<T>();

            try
            {
                TomlTable fileData = null;

                fileData = Toml.ReadString(data, settings);

                var readModels = (TomlTableArray)fileData[fieldToRead];

                foreach (var tempItem in readModels.Items)
                {
                    var dictionary = tempItem.Rows.ToDictionary(t => t.Key, t => (object)t.Value.ToString());

                    items.Add(DictionaryExtensions.ToObject<T>(dictionary));
                }
            }
            catch (Exception)
            {
                throw new ArgumentException("An error occurred while parsing Toml file.");
            }
            return items;
        }

        public static TomlSettings LoadLowerCaseTomlSettingsWithMappingForDefaultValues()
        {
            var fieldNamesToLowerCaseSettings = TomlSettings.Create(config => config
            .ConfigurePropertyMapping(mapping => mapping
            .UseKeyGenerator(standardGenerators => standardGenerators.LowerCase)));

            return fieldNamesToLowerCaseSettings;
        }
    }
}

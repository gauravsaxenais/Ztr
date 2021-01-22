namespace ZTR.Framework.Business.File.FileReaders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Content;
    using EnsureThat;
    using Models;
    using Nett;

    /// <summary>
    /// TomlFileReader.
    /// </summary>
    public static class TomlFileReader
    {
        /// <summary>
        /// Reads the data from string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">The data.</param>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        /// <exception cref="CustomArgumentException"></exception>
        public static T ReadDataFromString<T>(string data, TomlSettings settings = null) where T : class, new()
        {
            EnsureArg.IsNotEmptyOrWhiteSpace(data, nameof(data));
            
            T fileData;
            try
            {
                settings ??= LoadLowerCaseTomlSettings();

                fileData = Toml.ReadString<T>(data, settings);
            }
            catch (Exception exception)
            {
                throw new CustomArgumentException(Resource.TomlParsingError, exception);
            }

            return fileData;
        }

        /// <summary>
        /// Reads the data from file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="fieldToRead">The field to read.</param>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        /// <exception cref="CustomArgumentException"></exception>
        public static object ReadDataFromFile(string filePath, string fieldToRead, TomlSettings settings)
        {
            EnsureArg.IsNotEmptyOrWhiteSpace(filePath, (nameof(filePath)));
            EnsureArg.IsNotNull(settings, nameof(settings));

            try
            {
                var data = Toml.ReadFile(filePath, settings);
                return data.Get<object>(fieldToRead);
            }
            catch (Exception exception)
            {
                throw new CustomArgumentException(Resource.TomlParsingError, exception);
            }
        }

        /// <summary>
        /// Reads the data from file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="fieldToRead">The field to read.</param>
        /// <returns></returns>
        /// <exception cref="CustomArgumentException"></exception>
        public static List<Dictionary<string, object>> ReadDataFromFile(string filePath, TomlSettings settings, string fieldToRead)
        {
            EnsureArg.IsNotEmptyOrWhiteSpace(filePath, (nameof(filePath)));
            EnsureArg.IsNotNull(settings, nameof(settings));
            EnsureArg.IsNotEmptyOrWhiteSpace(fieldToRead, nameof(fieldToRead));

            var items = new List<Dictionary<string, object>>();

            try
            {
                TomlTable fileData = null;

                fileData = Toml.ReadFile(filePath, settings);

                var readModels = (TomlTableArray)fileData[fieldToRead];

                foreach (var tempItem in readModels.Items)
                {
                    var dictionary = tempItem.Rows.ToDictionary(t => t.Key, t => (object)t.Value.ToString());

                    items.Add(dictionary);
                }
            }
            catch (Exception exception)
            {
                throw new CustomArgumentException(Resource.TomlParsingError, exception);
            }

            return items;
        }

        /// <summary>
        /// Loads the lower case toml settings.
        /// </summary>
        /// <returns></returns>
        public static TomlSettings LoadLowerCaseTomlSettings()
        {
            var fieldNamesToLowerCaseSettings = TomlSettings.Create(config => config
            .ConfigurePropertyMapping(mapping => mapping
            .UseKeyGenerator(standardGenerators => standardGenerators.LowerCase)));

            return fieldNamesToLowerCaseSettings;
        }
    }
}

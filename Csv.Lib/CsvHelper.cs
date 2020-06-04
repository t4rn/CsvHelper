using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Csv.Lib
{
    public class CsvHelper
    {
        private readonly Type[] _dateTimeTypes = new Type[] { typeof(DateTime), typeof(DateTime?) };
        private readonly List<string> _errors;

        private static string ValidateHeaderMessage(string headerStart) => $"Incorrect file header starting with: {headerStart}.";
        private static string FormatInvalidMessage(int lineNumber, string fieldName) => $"Line {lineNumber} - the format of field {fieldName} is invalid.";
        private static string CsvExceptionMessage(int lineNumber, string message) => $"Line {lineNumber} - {message}";
        private static string MissingElementsMessage(int actual, int required) => $"insufficient number of elements ('{actual}') - required '{required}'.";
        private static string InvalidIndexMappingMessage(CsvPropertyMap property) => $"missing value of '{property.ColumnName}' with index '{property.Index}'.";
        private static string MissingConfigurationMessage(string config, string className) => $"Missing '{config}' configuration in '{className}' class.";
        private static string DuplicatedIndexesMessage(string indexes, string className) => $"Duplicated index/es '{indexes}' in '{className}' class.";

        public CsvHelperConfig Config { get; set; }

        public CsvHelper()
        {
            _errors = new List<string>();
            Config = new CsvHelperConfig();
        }

        public string WriteRecords<T>(IEnumerable<T> records)
        {
            if (records is null) throw new ArgumentNullException(nameof(records));

            var propertyMap = GetPropertyMappingConfig<T>();
            var orderedByIndex = propertyMap.OrderBy(x => x.Value.Index).ToArray();

            StringBuilder sb = new StringBuilder();

            if (Config.HasHeaderRecord)
            {
                for (int i = 0; i < orderedByIndex.Length; i++)
                {
                    string column = orderedByIndex[i].Value.ColumnName;
                    if (i > 0)
                    {
                        sb.Append(Config.Delimiter);
                    }
                    sb.Append(column);
                }
                sb.Append(Environment.NewLine);
            }

            foreach (T item in records)
            {
                for (int i = 0; i < orderedByIndex.Length; i++)
                {
                    string value;
                    object propertyValue = item.GetType().GetProperty(orderedByIndex[i].Key).GetValue(item, null);

                    if (!string.IsNullOrWhiteSpace(Config.DateTimeFormat)
                        && _dateTimeTypes.Contains(orderedByIndex[i].Value.PropertyType)
                        && propertyValue is DateTime dt)
                    {
                        value = dt.ToString(Config.DateTimeFormat);
                    }
                    else
                    {
                        value = propertyValue?.ToString();
                    }

                    if (i > 0)
                    {
                        //  add delimiter at the begining of new field, but ommit first field
                        sb.Append(Config.Delimiter);
                    }

                    sb.Append(value);
                }
                sb.Append(Environment.NewLine);
            }

            return sb.ToString();
        }

        public CsvReadResult<T> GetRecords<T>(string fileContent) where T : ICsvRow, new()
        {
            if (string.IsNullOrWhiteSpace(fileContent)) throw new ArgumentNullException(nameof(fileContent));

            var result = new CsvReadResult<T>();

            using (var reader = new StringReader(fileContent))
            {
                result = GetRecords<T>(reader);
            }

            return result;
        }

        public CsvReadResult<T> GetRecords<T>(Stream inputStream) where T : ICsvRow, new()
        {
            if (inputStream == null) throw new ArgumentNullException(nameof(inputStream));

            var result = new CsvReadResult<T>();

            using (var reader = new StreamReader(inputStream))
            {
                result = GetRecords<T>(reader);
            }

            return result;
        }


        private CsvReadResult<T> GetRecords<T>(TextReader reader) where T : ICsvRow, new()
        {
            var propertyMap = GetPropertyMappingConfig<T>();

            var result = new CsvReadResult<T>()
            {
                Errors = _errors,
            };

            int lineNumber = 0;

            using (reader)
            {
                string currentLine;

                while ((currentLine = reader.ReadLine()) != null)
                {
                    lineNumber++;

                    try
                    {
                        string[] currentRow = currentLine.Split(new[] { Config.Delimiter }, StringSplitOptions.None);

                        if (lineNumber == 1 && Config.HasHeaderRecord)
                        {
                            if (Config.ValidateHeader && !ValidateHeaderRow(propertyMap, currentRow))
                            {
                                _errors.Add(ValidateHeaderMessage(currentRow[0]));
                            }
                        }
                        else
                        {
                            T record = GetRecord<T>(propertyMap, lineNumber, currentRow);
                            result.Records.Add(record);
                        }
                    }
                    catch (Exception ex)
                    {
                        _errors.Add(CsvExceptionMessage(lineNumber, ex.Message));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// If HeaderValidationFunc is not provided - checks if row contains all columns which are mapped in Class
        /// </summary>
        private bool ValidateHeaderRow(Dictionary<string, CsvPropertyMap> propertyMap, string[] row)
        {
            if (Config.ValidateHeaderFunc != null)
            {
                return Config.ValidateHeaderFunc(row);
            }
            return !propertyMap.Select(x => x.Value.ColumnName).Except(row).Any();
        }

        /// <summary>
        /// Returns an object filled with values from the given CSV row. Properties of T have to have CsvFieldAttributes for mapping to work
        /// </summary>
        private T GetRecord<T>(Dictionary<string, CsvPropertyMap> propertyMap, int rowNumber, string[] csvRow)
            where T : ICsvRow, new()
        {
            T obj = new T() { RowNumber = rowNumber };

            if (propertyMap.Count > csvRow.Length)
            {
                throw new Exception(MissingElementsMessage(csvRow.Length, propertyMap.Count));
            }

            CsvPropertyMap propertyWithMaxIndex = propertyMap.Values.OrderByDescending(x => x.Index).First();
            if (propertyWithMaxIndex.Index >= csvRow.Length)
            {
                throw new Exception(InvalidIndexMappingMessage(propertyWithMaxIndex));
            }

            foreach (KeyValuePair<string, CsvPropertyMap> item in propertyMap)
            {
                var inputValue = csvRow[item.Value.Index];
                if (!string.IsNullOrWhiteSpace(inputValue))
                {
                    // convert string to proper value type eg. int
                    object value = GetValueFromString(item.Key, item.Value.PropertyType, csvRow[item.Value.Index], rowNumber);
                    obj.GetType().GetProperty(item.Key).SetValue(obj, value);
                }
            }

            return obj;
        }

        /// <summary>
        /// Returns a dictionary with values from property attributes - key is the Property Name
        /// </summary>
        private static Dictionary<string, CsvPropertyMap> GetPropertyMappingConfig<T>()
        {
            var mapping = new Dictionary<string, CsvPropertyMap>();

            PropertyInfo[] props = typeof(T).GetProperties();
            foreach (PropertyInfo prop in props)
            {
                CsvFieldAttribute csvAttr = prop.GetCustomAttribute<CsvFieldAttribute>(false);
                if (csvAttr != null)
                {
                    var cfg = new CsvPropertyMap(csvAttr.Index, csvAttr.ColumnName ?? prop.Name, prop.Name, prop.PropertyType);
                    mapping.Add(prop.Name, cfg);
                }
            }

            ValidatePropertyMappingConfig<T>(mapping);

            return mapping;
        }

        private static void ValidatePropertyMappingConfig<T>(Dictionary<string, CsvPropertyMap> mapping)
        {
            if (!mapping.Any())
            {
                throw new Exception(MissingConfigurationMessage(nameof(CsvFieldAttribute), typeof(T).Name));
            }

            var duplicatedIndexes = mapping.GroupBy(m => m.Value.Index).Where(x => x.Count() > 1);
            if (duplicatedIndexes.Any())
            {
                throw new Exception(DuplicatedIndexesMessage(string.Join(";", duplicatedIndexes.Select(x => x.Key)), typeof(T).Name));
            }
        }

        /// <summary>
        /// Converts string value to given type
        /// </summary>
        private object GetValueFromString(string propertyName, Type type, string inputValue, int lineNumber)
        {
            if (string.IsNullOrWhiteSpace(inputValue)) return null;

            object outputValue = null;

            try
            {
                if (!string.IsNullOrWhiteSpace(Config?.DateTimeFormat) && _dateTimeTypes.Contains(type))
                {
                    // DateTime types with custom format
                    outputValue = DateTime.ParseExact(inputValue, Config?.DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None);
                }
                else if (Nullable.GetUnderlyingType(type) != null)
                {
                    // nullable types
                    TypeConverter conv = TypeDescriptor.GetConverter(type);
                    outputValue = conv.ConvertFrom(inputValue);
                }
                else
                {
                    // all other types
                    outputValue = Convert.ChangeType(inputValue, type);
                }
            }
            catch (FormatException)
            {
                _errors.Add(FormatInvalidMessage(lineNumber, propertyName));
            }
            catch (Exception)
            {
                throw;
            }

            return outputValue;
        }
    }
}

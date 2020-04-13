using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CsvHelperLib.CsvHelpers
{
    public class CsvHelper<T> where T : ICsvRow, new()
    {
        private readonly Type[] _dateTimeTypes = new Type[] { typeof(DateTime), typeof(DateTime?) };
        private readonly Dictionary<string, CsvPropertyMap> _propertyMap;
        private readonly List<string> _errors;

        private static string ValidateHeaderMessage(string headerStart) => $"Incorrect file header starting with: {headerStart}.";
        private static string FormatInvalidMessage(int lineNumber, string fieldName) => $"Line {lineNumber} - the format of field {fieldName} is invalid.";
        private static string ParsingExceptionMessage(int lineNumber, string message) => $"Line {lineNumber} - error occured: {message}.";

        public CsvConfig Config { get; set; }
        public CsvHelper()
        {
            _propertyMap = GetPropertyMappingConfig();
            _errors = new List<string>();
            Config = new CsvConfig();
        }

        public string WriteRecords(List<T> records)
        {
            var orderedByIndex = _propertyMap.OrderBy(x => x.Value.Index);

            StringBuilder sb = new StringBuilder();

            if (Config.HasHeaderRecord)
            {
                foreach (var property in orderedByIndex)
                {
                    string column = property.Value.ColumnName;
                    sb.Append(column);
                    sb.Append(Config.Delimiter);
                }
                sb.Append(Environment.NewLine);
            }

            foreach (T item in records)
            {
                foreach (var propertyMap in orderedByIndex)
                {
                    string value;
                    object propertyValue = item.GetType().GetProperty(propertyMap.Key).GetValue(item, null);

                    if (!string.IsNullOrWhiteSpace(Config.DateTimeFormat)
                        && _dateTimeTypes.Contains(propertyMap.Value.PropertyType)
                        && propertyValue is DateTime dt)
                    {
                        value = dt.ToString(Config.DateTimeFormat);
                    }
                    else
                    {
                        value = propertyValue?.ToString();
                    }

                    sb.Append(value);
                    sb.Append(Config.Delimiter);
                }
                sb.Append(Environment.NewLine);
            }

            string csv = sb.ToString();
            return csv;
        }

        public CsvReaderResult<T> GetRecords(Stream inputStream)
        {
            if (inputStream is null) throw new ArgumentNullException(nameof(inputStream));

            var result = new CsvReaderResult<T>()
            {
                Errors = _errors,
            };

            int lineNumber = 0;

            using (inputStream)
            {
                using (var rowReader = new StreamReader(inputStream))
                {
                    string currentLine;

                    while ((currentLine = rowReader.ReadLine()) != null)
                    {
                        lineNumber++;

                        try
                        {
                            string[] currentRow = currentLine.Split(new[] { Config.Delimiter }, StringSplitOptions.None);


                            if (lineNumber == 1 && Config.HasHeaderRecord)
                            {
                                if (Config.ValidateHeader && !ValidateHeaderRow(currentRow))
                                {
                                    _errors.Add(ValidateHeaderMessage(currentRow[0]));
                                }
                            }
                            else
                            {
                                T record = GetRecord(lineNumber, currentRow);
                                result.Records.Add(record);
                            }
                        }
                        catch (Exception ex)
                        {
                            _errors.Add(ParsingExceptionMessage(lineNumber, ex.Message));
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Checks if row contains all columns which are mapped in Class
        /// </summary>
        private bool ValidateHeaderRow(string[] row)
        {
            return !row.Except(_propertyMap.Keys).Any();
        }

        /// <summary>
        /// Returns an object filled with values from the given CSV row. Properties of T have to have CsvFieldAttributes for mapping to work
        /// </summary>
        private T GetRecord(int rowNumber, string[] csvRow)
        {
            T obj = new T() { RowNumber = rowNumber };

            foreach (KeyValuePair<string, CsvPropertyMap> item in _propertyMap)
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
        private static Dictionary<string, CsvPropertyMap> GetPropertyMappingConfig()
        {
            var configList = new Dictionary<string, CsvPropertyMap>();

            PropertyInfo[] props = typeof(T).GetProperties();
            foreach (PropertyInfo prop in props)
            {
                CsvFieldAttribute csvAttr = prop.GetCustomAttribute<CsvFieldAttribute>(false);
                if (csvAttr != null)
                {
                    var cfg = new CsvPropertyMap(csvAttr.Index, csvAttr.ColumnName ?? prop.Name, prop.Name, prop.PropertyType);
                    configList.Add(prop.Name, cfg);
                }
            }

            return configList;
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

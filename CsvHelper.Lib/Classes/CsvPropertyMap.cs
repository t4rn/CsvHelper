using System;

namespace CsvHelper.Lib.Classes
{
    /// <summary>
    /// Class for storing mapping of a property in a CSV file
    /// </summary>
    public class CsvPropertyMap
    {
        public CsvPropertyMap(int index, string columnName, string propertyName, Type propertyType)
        {
            Index = index;
            ColumnName = columnName;
            PropertyName = propertyName;
            PropertyType = propertyType;
        }

        /// <summary>
        /// Index of property in CSV file
        /// </summary>
        public int Index { get; private set; }

        /// <summary>
        /// Name of property column in CSV file
        /// </summary>
        public string ColumnName { get; private set; }

        public string PropertyName { get; private set; }

        public Type PropertyType { get; private set; }
    }
}

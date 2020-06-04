using System;

namespace Csv.Lib
{
    /// <summary>
    /// Describes a position and name of Property in CSV file.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class CsvFieldAttribute : Attribute
    {
        /// <summary>
        /// Gets the index.
        /// </summary>
        public int Index { get; private set; }

        /// <summary>
        /// Gets the name of a column.
        /// </summary>
        public string ColumnName { get; private set; }


        /// <summary>
        /// Describes a position and name of Property in CSV file.
        /// </summary>
        /// <param name="index">Position of property in CSV file.</param>
        /// <param name="columnName">Name for column which will be generated for this Property (if null then name of Property will be used).</param>
        public CsvFieldAttribute(int index, string columnName = null)
        {
            Index = index;
            ColumnName = columnName;
        }
    }
}

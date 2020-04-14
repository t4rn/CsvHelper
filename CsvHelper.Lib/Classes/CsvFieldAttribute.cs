﻿using System;

namespace CsvHelper.Lib.Classes
{
    /// <summary>
    /// Describes a position and name of Property in CSV file
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class CsvFieldAttribute : Attribute
    {
        /// <summary>
        /// Gets the index.
        /// </summary>
        public int Index { get; private set; }

        /// <summary>
        /// Gets the name of a column.
        /// </summary>
        public string ColumnName { get; private set; }

        public CsvFieldAttribute(int index, string columnName = null)
        {
            Index = index;
            ColumnName = columnName;
        }
    }
}

using CsvHelper.Lib.Classes;
using System;

namespace CsvHelper.DomainForTests.Classes.Errors
{
    public class NoAttributes : ICsvRow
    {
        public int RowNumber { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string Name { get; set; }
        public int Units { get; set; }
    }
}

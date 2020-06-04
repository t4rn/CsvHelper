using Csv.Lib;
using System;

namespace Csv.TestsDomain.Classes.Errors
{
    public class NoAttributes : ICsvRow
    {
        public int RowNumber { get ; set; }

        public string Name { get; set; }

        public int Units { get; set; }
    }
}

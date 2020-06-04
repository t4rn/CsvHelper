using Csv.Lib;
using System;

namespace Csv.TestsDomain.Classes
{
    public class User : ICsvRow
    {
        public int RowNumber { get; set; }


        [CsvField(0, "identifier")]
        public decimal? Id { get; set; }

        [CsvField(2, "forename")]
        public string FirstName { get; set; }

        [CsvField(1, "surname")]
        public string LastName { get; set; }

        [CsvField(3)]
        public string Title { get; set; }

        [CsvField(4)]
        public DateTime? BirthDate { get; set; }

        [CsvField(5, "status")]
        public char? WorkStatus { get; set; }

        public int NotMapped { get; set; }
    }
}

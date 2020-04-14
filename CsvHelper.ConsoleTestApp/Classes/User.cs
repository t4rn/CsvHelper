using CsvHelper.Lib.Classes;
using System;

namespace CsvHelper.ConsoleTestApp.Classes
{
    public class User : ICsvRow
    {
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


        public int RowNumber { get; set; }
    }
}

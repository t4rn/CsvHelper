using CsvHelper.Lib.Classes;

namespace CsvHelper.DomainForTests.Classes.Errors
{
    public class DuplicatedIndex : ICsvRow
    {
        public int RowNumber { get; set; }

        [CsvField(0)]
        public int Id { get; set; }
        [CsvField(0)]
        public string Name { get; set; }
        [CsvField(1)]
        public int Units { get; set; }
        [CsvField(1)]
        public decimal? Cost { get; set; }
    }
}

using System.Collections.Generic;

namespace CsvHelperLib.CsvHelpers
{
    public class CsvReaderResult<T>
    {
        public List<T> Records { get; set; } = new List<T>();
        public List<string> Errors { get; set; }
    }
}

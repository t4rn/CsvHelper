using System.Collections.Generic;

namespace Csv.Lib
{
    public class CsvReadResult<T>
    {
        public List<T> Records { get; set; } = new List<T>();
        public List<string> Errors { get; set; }
    }
}

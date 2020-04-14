using System.Collections.Generic;

namespace CsvHelper.Lib.Classes
{
    public class CsvReadResult<T>
    {
        public List<T> Records { get; set; } = new List<T>();
        public List<string> Errors { get; set; }
    }
}

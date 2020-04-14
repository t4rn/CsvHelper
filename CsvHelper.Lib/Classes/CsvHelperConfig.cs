namespace CsvHelper.Lib.Classes
{
    public class CsvHelperConfig
    {
        /// <summary>
        /// Default is ','
        /// </summary>
        public char Delimiter { get; set; } = ',';

        /// <summary>
        /// Default is TRUE
        /// </summary>
        public bool HasHeaderRecord { get; set; } = true;

        /// <summary>
        /// Should the CSV reader validate the header
        /// </summary>
        public bool ValidateHeader { get; set; }

        /// <summary>
        /// Custom DateTime format in CSV file
        /// </summary>
        public string DateTimeFormat { get; set; }
    }
}

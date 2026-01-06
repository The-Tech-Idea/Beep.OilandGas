namespace Beep.OilandGas.Models.DTOs
{
    /// <summary>
    /// Maps a well identifier to its data source
    /// </summary>
    public class WellSourceMapping
    {
        /// <summary>
        /// Well identifier (UWI, Well ID, etc.)
        /// </summary>
        public string WellIdentifier { get; set; }

        /// <summary>
        /// Data source name/connection name
        /// </summary>
        public string DataSource { get; set; }

        /// <summary>
        /// Optional well name for display
        /// </summary>
        public string WellName { get; set; }
    }
}





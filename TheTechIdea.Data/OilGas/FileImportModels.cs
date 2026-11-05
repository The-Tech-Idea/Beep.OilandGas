using System.Collections.Generic;

namespace Beep.OilandGas.PPDM39.Core
{
    /// <summary>
    /// Result of file import operation
    /// </summary>
    public class FileImportResult
    {
        /// <summary>
        /// Path to the imported file
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// Total number of rows processed
        /// </summary>
        public int TotalRows { get; set; }

        /// <summary>
        /// Number of successfully imported rows
        /// </summary>
        public int SuccessCount { get; set; }

        /// <summary>
        /// Number of rows with errors
        /// </summary>
        public int ErrorCount { get; set; }

        /// <summary>
        /// List of import errors
        /// </summary>
        public List<FileImportError> Errors { get; set; } = new List<FileImportError>();

        /// <summary>
        /// Whether the import was successful (no errors)
        /// </summary>
        public bool IsSuccess => ErrorCount == 0;
    }

    /// <summary>
    /// Represents an error during file import
    /// </summary>
    public class FileImportError
    {
        /// <summary>
        /// Row number where error occurred (1-based)
        /// </summary>
        public int RowNumber { get; set; }

        /// <summary>
        /// Error message
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Column name where error occurred (if applicable)
        /// </summary>
        public string ColumnName { get; set; } = string.Empty;
    }

}

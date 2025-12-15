namespace Beep.OilandGas.Web.Components
{
    /// <summary>
    /// Represents a foreign key validation error
    /// </summary>
    public class ForeignKeyValidationError
    {
        public int RowNumber { get; set; }
        public string ForeignKeyColumn { get; set; } = string.Empty;
        public string ReferencedTable { get; set; } = string.Empty;
        public string ReferencedPrimaryKeyColumn { get; set; } = string.Empty;
        public string ForeignKeyValue { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents the result of a file import operation
    /// </summary>
    public class FileImportResult
    {
        public string FilePath { get; set; } = string.Empty;
        public int TotalRows { get; set; }
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public List<FileImportError> Errors { get; set; } = new List<FileImportError>();
        public bool IsSuccess => ErrorCount == 0;
    }

    /// <summary>
    /// Represents an error that occurred during file import
    /// </summary>
    public class FileImportError
    {
        public int RowNumber { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}


namespace Beep.OilandGas.Models.Data.DataManagement;

/// <summary>One file-import row/column error; not a persisted module entity.</summary>
public class FileImportError
{
    public int RowNumber { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ColumnName { get; set; }
}

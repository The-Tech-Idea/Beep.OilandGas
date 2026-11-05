namespace Beep.OilandGas.Models.Data.DataManagement;

/// <summary>Transport result for a completed file import; not a persisted module entity.</summary>
public class FileImportResult
{
    public string FilePath { get; set; } = string.Empty;
    public int TotalRows { get; set; }
    public int SuccessCount { get; set; }
    public int ErrorCount { get; set; }
    public List<FileImportError> Errors { get; set; } = new();
    public bool IsSuccess => ErrorCount == 0;
}

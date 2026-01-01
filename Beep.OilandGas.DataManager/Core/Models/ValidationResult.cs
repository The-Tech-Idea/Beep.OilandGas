namespace Beep.OilandGas.DataManager.Core.Models
{
    /// <summary>
    /// Result of script validation
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<ValidationError> Errors { get; set; } = new List<ValidationError>();
        public List<ValidationWarning> Warnings { get; set; } = new List<ValidationWarning>();
        public int TotalScriptsChecked { get; set; }
        public int ValidScripts { get; set; }
        public int InvalidScripts { get; set; }
    }

    /// <summary>
    /// Validation error details
    /// </summary>
    public class ValidationError
    {
        public string ScriptFileName { get; set; } = string.Empty;
        public string ErrorType { get; set; } = string.Empty; // Syntax, Missing, Dependency, etc.
        public string Message { get; set; } = string.Empty;
        public int? LineNumber { get; set; }
        public string? Details { get; set; }
    }

    /// <summary>
    /// Validation warning details
    /// </summary>
    public class ValidationWarning
    {
        public string ScriptFileName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Details { get; set; }
    }
}

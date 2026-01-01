namespace Beep.OilandGas.DataManager.Core.Models
{
    /// <summary>
    /// Result of error checking after script execution
    /// </summary>
    public class ErrorCheckResult
    {
        public string ExecutionId { get; set; } = string.Empty;
        public bool HasErrors { get; set; }
        public List<ScriptError> ScriptErrors { get; set; } = new List<ScriptError>();
        public List<ObjectVerificationError> ObjectErrors { get; set; } = new List<ObjectVerificationError>();
        public int TotalObjectsChecked { get; set; }
        public int ValidObjects { get; set; }
        public int MissingObjects { get; set; }
    }

    /// <summary>
    /// Script execution error
    /// </summary>
    public class ScriptError
    {
        public string ScriptFileName { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public Exception? Exception { get; set; }
        public DateTime ErrorTime { get; set; }
    }

    /// <summary>
    /// Object verification error
    /// </summary>
    public class ObjectVerificationError
    {
        public string ObjectName { get; set; } = string.Empty;
        public string ObjectType { get; set; } = string.Empty; // Table, Index, Constraint, etc.
        public string ExpectedScript { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }
}

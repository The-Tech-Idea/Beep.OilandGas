namespace Beep.OilandGas.DataManager.Core.Exceptions
{
    /// <summary>
    /// Exception thrown when script execution fails
    /// </summary>
    public class ScriptExecutionException : DataManagerException
    {
        public string ScriptFileName { get; }
        public int? LineNumber { get; }

        public ScriptExecutionException(string scriptFileName, string message) 
            : base($"Script execution failed for {scriptFileName}: {message}")
        {
            ScriptFileName = scriptFileName;
        }

        public ScriptExecutionException(string scriptFileName, string message, Exception innerException) 
            : base($"Script execution failed for {scriptFileName}: {message}", innerException)
        {
            ScriptFileName = scriptFileName;
        }

        public ScriptExecutionException(string scriptFileName, int lineNumber, string message) 
            : base($"Script execution failed for {scriptFileName} at line {lineNumber}: {message}")
        {
            ScriptFileName = scriptFileName;
            LineNumber = lineNumber;
        }

        public ScriptExecutionException(string scriptFileName, int lineNumber, string message, Exception innerException) 
            : base($"Script execution failed for {scriptFileName} at line {lineNumber}: {message}", innerException)
        {
            ScriptFileName = scriptFileName;
            LineNumber = lineNumber;
        }
    }
}

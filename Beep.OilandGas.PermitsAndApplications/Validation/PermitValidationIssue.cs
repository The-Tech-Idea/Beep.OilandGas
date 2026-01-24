namespace Beep.OilandGas.PermitsAndApplications.Validation
{
    public class PermitValidationIssue
    {
        public PermitValidationIssue(string code, string message, PermitValidationSeverity severity, string? field = null)
        {
            Code = code;
            Message = message;
            Severity = severity;
            Field = field;
        }

        public string Code { get; }
        public string Message { get; }
        public PermitValidationSeverity Severity { get; }
        public string? Field { get; }
    }
}

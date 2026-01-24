using System.Collections.Generic;

namespace Beep.OilandGas.PermitsAndApplications.Validation
{
    public class PermitValidationRuleResult
    {
        public List<PermitValidationIssue> Issues { get; } = new();
        public int RequiredFieldCount { get; set; }
        public int MissingRequiredFieldCount { get; set; }
        public List<string> MissingForms { get; } = new();
    }
}

namespace Beep.OilandGas.PermitsAndApplications.Validation.Rules
{
    public class AttachmentWarningRule : IPermitValidationRule
    {
        public string Name => "Attachment Presence";

        public PermitValidationRuleResult Evaluate(PermitValidationRequest request)
        {
            var result = new PermitValidationRuleResult();

            if (request.Attachments == null || request.Attachments.Count == 0)
            {
                result.Issues.Add(new PermitValidationIssue(
                    "ATTACHMENT_MISSING",
                    "No attachments found; supporting documents may be required.",
                    PermitValidationSeverity.Warning,
                    "ATTACHMENTS"));
            }

            return result;
        }
    }
}

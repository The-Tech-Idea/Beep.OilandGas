using System;

namespace Beep.OilandGas.PermitsAndApplications.Validation.Rules
{
    public class StatusDateRule : IPermitValidationRule
    {
        public string Name => "Status Dates";

        public PermitValidationRuleResult Evaluate(PermitValidationRequest request)
        {
            var result = new PermitValidationRuleResult();
            var status = request.Application.STATUS;

            if (status ==  Models.Data.PermitsAndApplications.PermitApplicationStatus.Submitted && request.Application.SUBMITTED_DATE == null)
            {
                result.Issues.Add(new PermitValidationIssue(
                    "STATUS_DATE",
                    "Submitted applications require a submitted date.",
                    PermitValidationSeverity.Error,
                    "SUBMITTED_DATE"));
            }

            if (status ==  Models.Data.PermitsAndApplications.PermitApplicationStatus.UnderReview && request.Application.RECEIVED_DATE == null)
            {
                result.Issues.Add(new PermitValidationIssue(
                    "STATUS_DATE",
                    "Under review applications require a received date.",
                    PermitValidationSeverity.Warning,
                    "RECEIVED_DATE"));
            }

            if ((status ==  Models.Data.PermitsAndApplications.PermitApplicationStatus.Approved || status ==  Models.Data.PermitsAndApplications.PermitApplicationStatus.Rejected) && request.Application.DECISION_DATE == null)
            {
                result.Issues.Add(new PermitValidationIssue(
                    "STATUS_DATE",
                    "Decisions require a decision date.",
                    PermitValidationSeverity.Error,
                    "DECISION_DATE"));
            }

            if (status ==  Models.Data.PermitsAndApplications.PermitApplicationStatus.Approved && request.Application.EFFECTIVE_DATE == null)
            {
                result.Issues.Add(new PermitValidationIssue(
                    "STATUS_DATE",
                    "Approved applications require an effective date.",
                    PermitValidationSeverity.Error,
                    "EFFECTIVE_DATE"));
            }

            return result;
        }

        private static string Normalize(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? "DRAFT" : value.Trim().ToUpperInvariant();
        }
    }
}

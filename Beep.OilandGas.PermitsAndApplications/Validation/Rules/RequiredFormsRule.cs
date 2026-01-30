using System;
using System.Collections.Generic;
using System.Linq;

namespace Beep.OilandGas.PermitsAndApplications.Validation.Rules
{
    public class RequiredFormsRule : IPermitValidationRule
    {
        public string Name => "Required Forms";

        public PermitValidationRuleResult Evaluate(PermitValidationRequest request)
        {
            var result = new PermitValidationRuleResult();

            if (request.RequiredForms.Count == 0)
                return result;

            var status = Normalize(request.Application.STATUS.ToString());
            var submissionComplete = string.Equals(request.Application.SUBMISSION_COMPLETE_IND, "Y", StringComparison.OrdinalIgnoreCase);
            if (status == "DRAFT" && !submissionComplete)
                return result;

            var attachments = request.Attachments ?? Array.Empty<Beep.OilandGas.Models.Data.PermitsAndApplications.APPLICATION_ATTACHMENT>();
            var missingForms = new List<string>();

            foreach (var form in request.RequiredForms)
            {
                if (!IsFormSatisfied(form.FORM_CODE, form.FORM_NAME, attachments))
                {
                    var label = string.IsNullOrWhiteSpace(form.FORM_NAME) ? form.FORM_CODE : form.FORM_NAME;
                    missingForms.Add(label);
                    result.Issues.Add(new PermitValidationIssue(
                        "MISSING_FORM",
                        $"Required form missing: {label}.",
                        PermitValidationSeverity.Error,
                        "FORM_CODE"));
                }
            }

            result.MissingForms.AddRange(missingForms);
            return result;
        }

        private static bool IsFormSatisfied(
            string? formCode,
            string? formName,
            IReadOnlyList<Beep.OilandGas.Models.Data.PermitsAndApplications.APPLICATION_ATTACHMENT> attachments)
        {
            if (attachments.Count == 0)
                return false;

            foreach (var attachment in attachments)
            {
                if (Matches(formCode, attachment.DOCUMENT_TYPE) || Matches(formName, attachment.DOCUMENT_TYPE))
                    return true;

                if (Matches(formCode, attachment.DESCRIPTION) || Matches(formName, attachment.DESCRIPTION))
                    return true;
            }

            return false;
        }

        private static bool Matches(string? expected, string? actual)
        {
            if (string.IsNullOrWhiteSpace(expected) || string.IsNullOrWhiteSpace(actual))
                return false;

            return string.Equals(expected.Trim(), actual.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        private static string Normalize(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? "DRAFT" : value.Trim().ToUpperInvariant();
        }
    }
}

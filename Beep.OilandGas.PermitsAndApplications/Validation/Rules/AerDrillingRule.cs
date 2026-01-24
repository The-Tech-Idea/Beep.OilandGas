using System;

namespace Beep.OilandGas.PermitsAndApplications.Validation.Rules
{
    public class AerDrillingRule : IPermitValidationRule
    {
        public string Name => "AER Drilling Requirements";

        public PermitValidationRuleResult Evaluate(PermitValidationRequest request)
        {
            var result = new PermitValidationRuleResult();
            var drilling = request.DrillingApplication;

            if (drilling == null)
            {
                result.Issues.Add(new PermitValidationIssue(
                    "AER_DRILLING_DETAIL_MISSING",
                    "AER drilling applications require drilling permit details.",
                    PermitValidationSeverity.Error));
                return result;
            }

            AddIfMissing(result, drilling.WELL_UWI, "WELL_UWI", "AER drilling requires a well UWI.");
            AddIfMissing(result, drilling.LEGAL_DESCRIPTION, "LEGAL_DESCRIPTION", "AER drilling requires a legal land description.");
            AddIfMissing(result, drilling.TARGET_FORMATION, "TARGET_FORMATION", "AER drilling requires a target formation.");
            AddIfMissing(result, drilling.DRILLING_METHOD, "DRILLING_METHOD", "AER drilling requires a drilling method.");
            AddIfMissing(result, drilling.SURFACE_OWNER_NOTIFIED_IND, "SURFACE_OWNER_NOTIFIED_IND", "AER drilling requires surface owner consultation status.");

            return result;
        }

        private static void AddIfMissing(PermitValidationRuleResult result, string? value, string field, string message)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                result.Issues.Add(new PermitValidationIssue(
                    "AER_REQUIRED_FIELD",
                    message,
                    PermitValidationSeverity.Error,
                    field));
                result.RequiredFieldCount++;
                result.MissingRequiredFieldCount++;
            }
            else
            {
                result.RequiredFieldCount++;
            }
        }
    }
}

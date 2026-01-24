using System;

namespace Beep.OilandGas.PermitsAndApplications.Validation.Rules
{
    public class BcerDrillingRule : IPermitValidationRule
    {
        public string Name => "BCER Drilling Requirements";

        public PermitValidationRuleResult Evaluate(PermitValidationRequest request)
        {
            var result = new PermitValidationRuleResult();
            var drilling = request.DrillingApplication;

            if (drilling == null)
            {
                result.Issues.Add(new PermitValidationIssue(
                    "BCER_DRILLING_DETAIL_MISSING",
                    "BCER drilling applications require drilling permit details.",
                    PermitValidationSeverity.Error));
                return result;
            }

            AddIfMissing(result, drilling.WELL_UWI, "WELL_UWI", "BCER drilling requires a well UWI.");
            AddIfMissing(result, drilling.LEGAL_DESCRIPTION, "LEGAL_DESCRIPTION", "BCER drilling requires a legal description.");
            AddIfMissing(result, drilling.TARGET_FORMATION, "TARGET_FORMATION", "BCER drilling requires a target formation.");
            AddIfMissing(result, drilling.DRILLING_METHOD, "DRILLING_METHOD", "BCER drilling requires a drilling method.");
            AddIfMissing(result, request.Application.OPERATOR_ID, "OPERATOR_ID", "BCER drilling requires an operator ID.");

            return result;
        }

        private static void AddIfMissing(PermitValidationRuleResult result, string? value, string field, string message)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                result.Issues.Add(new PermitValidationIssue(
                    "BCER_REQUIRED_FIELD",
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

using System;

namespace Beep.OilandGas.PermitsAndApplications.Validation.Rules
{
    public class BoemDrillingRule : IPermitValidationRule
    {
        public string Name => "BOEM Drilling Requirements";

        public PermitValidationRuleResult Evaluate(PermitValidationRequest request)
        {
            var result = new PermitValidationRuleResult();
            var drilling = request.DrillingApplication;

            if (drilling == null)
            {
                result.Issues.Add(new PermitValidationIssue(
                    "BOEM_DRILLING_DETAIL_MISSING",
                    "BOEM drilling applications require drilling permit details.",
                    PermitValidationSeverity.Error));
                return result;
            }

            AddIfMissing(result, drilling.WELL_UWI, "WELL_UWI", "BOEM drilling requires a well UWI.");
            AddIfMissing(result, drilling.TARGET_FORMATION, "TARGET_FORMATION", "BOEM drilling requires a target formation.");
            AddIfMissing(result, drilling.PROPOSED_DEPTH, "PROPOSED_DEPTH", "BOEM drilling requires a proposed depth.");
            AddIfMissing(result, drilling.DRILLING_METHOD, "DRILLING_METHOD", "BOEM drilling requires a drilling method.");
            AddIfMissing(result, drilling.SPACING_UNIT, "SPACING_UNIT", "BOEM drilling requires a spacing unit.");

            return result;
        }

        private static void AddIfMissing(PermitValidationRuleResult result, string? value, string field, string message)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                result.Issues.Add(new PermitValidationIssue(
                    "BOEM_REQUIRED_FIELD",
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

        private static void AddIfMissing(PermitValidationRuleResult result, decimal? value, string field, string message)
        {
            if (value == null || value <= 0)
            {
                result.Issues.Add(new PermitValidationIssue(
                    "BOEM_REQUIRED_FIELD",
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

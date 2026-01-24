using System;

namespace Beep.OilandGas.PermitsAndApplications.Validation.Rules
{
    public class RrcDrillingRule : IPermitValidationRule
    {
        public string Name => "RRC Drilling Requirements";

        public PermitValidationRuleResult Evaluate(PermitValidationRequest request)
        {
            var result = new PermitValidationRuleResult();
            var drilling = request.DrillingApplication;

            if (drilling == null)
            {
                result.Issues.Add(new PermitValidationIssue(
                    "RRC_DRILLING_DETAIL_MISSING",
                    "RRC drilling applications require drilling permit details.",
                    PermitValidationSeverity.Error));
                return result;
            }

            AddIfMissing(result, drilling.WELL_UWI, "WELL_UWI", "RRC drilling requires a well UWI.");
            AddIfMissing(result, drilling.LEGAL_DESCRIPTION, "LEGAL_DESCRIPTION", "RRC drilling requires a legal description.");
            AddIfMissing(result, drilling.TARGET_FORMATION, "TARGET_FORMATION", "RRC drilling requires a target formation.");
            AddIfMissing(result, drilling.PROPOSED_DEPTH, "PROPOSED_DEPTH", "RRC drilling requires a proposed depth.");
            AddIfMissing(result, drilling.DRILLING_METHOD, "DRILLING_METHOD", "RRC drilling requires a drilling method.");
            AddIfMissing(result, drilling.SURFACE_OWNER_NOTIFIED_IND, "SURFACE_OWNER_NOTIFIED_IND", "RRC drilling requires surface owner notification status.");

            if (string.Equals(drilling.ENVIRONMENTAL_ASSESSMENT_REQUIRED_IND, "Y", StringComparison.OrdinalIgnoreCase))
            {
                AddIfMissing(result, drilling.ENVIRONMENTAL_ASSESSMENT_REFERENCE, "ENVIRONMENTAL_ASSESSMENT_REFERENCE",
                    "RRC drilling requires an environmental assessment reference when required.");
            }

            return result;
        }

        private static void AddIfMissing(PermitValidationRuleResult result, string? value, string field, string message)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                result.Issues.Add(new PermitValidationIssue(
                    "RRC_REQUIRED_FIELD",
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
                    "RRC_REQUIRED_FIELD",
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

using System;

namespace Beep.OilandGas.PermitsAndApplications.Validation.Rules
{
    public class RrcEnvironmentalRule : IPermitValidationRule
    {
        public string Name => "RRC Environmental Requirements";

        public PermitValidationRuleResult Evaluate(PermitValidationRequest request)
        {
            var result = new PermitValidationRuleResult();
            var environmental = request.EnvironmentalApplication;

            if (environmental == null)
            {
                result.Issues.Add(new PermitValidationIssue(
                    "RRC_ENV_DETAIL_MISSING",
                    "RRC environmental applications require environmental permit details.",
                    PermitValidationSeverity.Error));
                return result;
            }

            AddIfMissing(result, environmental.ENVIRONMENTAL_PERMIT_TYPE, "ENVIRONMENTAL_PERMIT_TYPE", "RRC environmental permits require a permit type.");
            AddIfMissing(result, environmental.WASTE_TYPE, "WASTE_TYPE", "RRC environmental permits require a waste type.");
            AddIfMissing(result, environmental.WASTE_VOLUME, "WASTE_VOLUME", "RRC environmental permits require a waste volume.");
            AddIfMissing(result, environmental.DISPOSAL_METHOD, "DISPOSAL_METHOD", "RRC environmental permits require a disposal method.");
            AddIfMissing(result, environmental.MONITORING_PLAN, "MONITORING_PLAN", "RRC environmental permits require a monitoring plan.");

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

using System;

namespace Beep.OilandGas.PermitsAndApplications.Validation.Rules
{
    public class BcerEnvironmentalRule : IPermitValidationRule
    {
        public string Name => "BCER Environmental Requirements";

        public PermitValidationRuleResult Evaluate(PermitValidationRequest request)
        {
            var result = new PermitValidationRuleResult();
            var environmental = request.EnvironmentalApplication;

            if (environmental == null)
            {
                result.Issues.Add(new PermitValidationIssue(
                    "BCER_ENV_DETAIL_MISSING",
                    "BCER environmental applications require environmental permit details.",
                    PermitValidationSeverity.Error));
                return result;
            }

            AddIfMissing(result, environmental.ENVIRONMENTAL_PERMIT_TYPE, "ENVIRONMENTAL_PERMIT_TYPE", "BCER requires an environmental permit type.");
            AddIfMissing(result, environmental.WASTE_TYPE, "WASTE_TYPE", "BCER requires a waste type.");
            AddIfMissing(result, environmental.WASTE_VOLUME, "WASTE_VOLUME", "BCER requires a waste volume.");
            AddIfMissing(result, environmental.DISPOSAL_METHOD, "DISPOSAL_METHOD", "BCER requires a disposal method.");
            AddIfMissing(result, environmental.FACILITY_LOCATION, "FACILITY_LOCATION", "BCER requires a facility location.");
            AddIfMissing(result, environmental.MONITORING_PLAN, "MONITORING_PLAN", "BCER requires a monitoring plan.");

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

        private static void AddIfMissing(PermitValidationRuleResult result, decimal? value, string field, string message)
        {
            if (value == null || value <= 0)
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

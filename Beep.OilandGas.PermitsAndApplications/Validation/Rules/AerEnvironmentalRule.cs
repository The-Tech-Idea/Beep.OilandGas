using System;

namespace Beep.OilandGas.PermitsAndApplications.Validation.Rules
{
    public class AerEnvironmentalRule : IPermitValidationRule
    {
        public string Name => "AER Environmental Requirements";

        public PermitValidationRuleResult Evaluate(PermitValidationRequest request)
        {
            var result = new PermitValidationRuleResult();
            var environmental = request.EnvironmentalApplication;

            if (environmental == null)
            {
                result.Issues.Add(new PermitValidationIssue(
                    "AER_ENV_DETAIL_MISSING",
                    "AER environmental applications require environmental permit details.",
                    PermitValidationSeverity.Error));
                return result;
            }

            AddIfMissing(result, environmental.ENVIRONMENTAL_PERMIT_TYPE, "ENVIRONMENTAL_PERMIT_TYPE", "AER requires an environmental permit type.");
            AddIfMissing(result, environmental.MONITORING_PLAN, "MONITORING_PLAN", "AER requires a monitoring plan.");
            AddIfMissing(result, environmental.FACILITY_LOCATION, "FACILITY_LOCATION", "AER requires a facility location.");

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

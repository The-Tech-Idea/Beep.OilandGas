using System;

namespace Beep.OilandGas.PermitsAndApplications.Validation.Rules
{
    public class BseeEnvironmentalRule : IPermitValidationRule
    {
        public string Name => "BSEE Environmental Requirements";

        public PermitValidationRuleResult Evaluate(PermitValidationRequest request)
        {
            var result = new PermitValidationRuleResult();
            var environmental = request.EnvironmentalApplication;

            if (environmental == null)
            {
                result.Issues.Add(new PermitValidationIssue(
                    "BSEE_ENV_DETAIL_MISSING",
                    "BSEE environmental applications require environmental permit details.",
                    PermitValidationSeverity.Error));
                return result;
            }

            AddIfMissing(result, environmental.ENVIRONMENTAL_PERMIT_TYPE, "ENVIRONMENTAL_PERMIT_TYPE", "BSEE requires an environmental permit type.");
            AddIfMissing(result, environmental.ENVIRONMENTAL_IMPACT, "ENVIRONMENTAL_IMPACT", "BSEE requires an environmental impact description.");
            AddIfMissing(result, environmental.MONITORING_PLAN, "MONITORING_PLAN", "BSEE requires a monitoring plan.");
            AddIfMissing(result, environmental.FACILITY_LOCATION, "FACILITY_LOCATION", "BSEE requires a facility location.");
            AddIfMissing(result, environmental.NORM_INVOLVED_IND, "NORM_INVOLVED_IND", "BSEE requires NORM involvement status.");

            return result;
        }

        private static void AddIfMissing(PermitValidationRuleResult result, string? value, string field, string message)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                result.Issues.Add(new PermitValidationIssue(
                    "BSEE_REQUIRED_FIELD",
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

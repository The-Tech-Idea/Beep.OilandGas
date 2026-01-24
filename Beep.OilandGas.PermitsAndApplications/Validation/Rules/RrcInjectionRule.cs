using System;

namespace Beep.OilandGas.PermitsAndApplications.Validation.Rules
{
    public class RrcInjectionRule : IPermitValidationRule
    {
        public string Name => "RRC Injection Requirements";

        public PermitValidationRuleResult Evaluate(PermitValidationRequest request)
        {
            var result = new PermitValidationRuleResult();
            var injection = request.InjectionApplication;

            if (injection == null)
            {
                result.Issues.Add(new PermitValidationIssue(
                    "RRC_INJECTION_DETAIL_MISSING",
                    "RRC injection applications require injection permit details.",
                    PermitValidationSeverity.Error));
                return result;
            }

            AddIfMissing(result, injection.INJECTION_TYPE, "INJECTION_TYPE", "RRC injection permits require an injection type.");
            AddIfMissing(result, injection.INJECTION_ZONE, "INJECTION_ZONE", "RRC injection permits require an injection zone.");
            AddIfMissing(result, injection.INJECTION_FLUID, "INJECTION_FLUID", "RRC injection permits require an injection fluid.");
            AddIfMissing(result, injection.INJECTION_WELL_UWI, "INJECTION_WELL_UWI", "RRC injection permits require an injection well UWI.");
            AddIfMissing(result, injection.MAXIMUM_INJECTION_PRESSURE, "MAXIMUM_INJECTION_PRESSURE", "RRC injection permits require a maximum injection pressure.");
            AddIfMissing(result, injection.MAXIMUM_INJECTION_RATE, "MAXIMUM_INJECTION_RATE", "RRC injection permits require a maximum injection rate.");

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

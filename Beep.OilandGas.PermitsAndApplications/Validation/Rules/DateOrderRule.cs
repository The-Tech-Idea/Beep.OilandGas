using System;

namespace Beep.OilandGas.PermitsAndApplications.Validation.Rules
{
    public class DateOrderRule : IPermitValidationRule
    {
        public string Name => "Date Order";

        public PermitValidationRuleResult Evaluate(PermitValidationRequest request)
        {
            var result = new PermitValidationRuleResult();
            var application = request.Application;

            AddIfInvalid(result, application.CREATED_DATE, application.SUBMITTED_DATE, "CREATED_DATE", "SUBMITTED_DATE");
            AddIfInvalid(result, application.SUBMITTED_DATE, application.DECISION_DATE, "SUBMITTED_DATE", "DECISION_DATE");
            AddIfInvalid(result, application.DECISION_DATE, application.EFFECTIVE_DATE, "DECISION_DATE", "EFFECTIVE_DATE");
            AddIfInvalid(result, application.EFFECTIVE_DATE, application.EXPIRY_DATE, "EFFECTIVE_DATE", "EXPIRY_DATE");

            return result;
        }

        private static void AddIfInvalid(
            PermitValidationRuleResult result,
            DateTime? earlier,
            DateTime? later,
            string earlierField,
            string laterField)
        {
            if (earlier == null || later == null)
                return;

            if (earlier.Value > later.Value)
            {
                result.Issues.Add(new PermitValidationIssue(
                    "DATE_ORDER",
                    $"{earlierField} must be on or before {laterField}.",
                    PermitValidationSeverity.Error,
                    laterField));
            }
        }
    }
}

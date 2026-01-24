using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data.PermitsAndApplications;

namespace Beep.OilandGas.PermitsAndApplications.Validation
{
    public class PermitValidationEngine
    {
        private readonly PermitValidationRulesFactory _rulesFactory;

        public PermitValidationEngine(PermitValidationRulesFactory rulesFactory)
        {
            _rulesFactory = rulesFactory ?? throw new ArgumentNullException(nameof(rulesFactory));
        }

        public PermitValidationResult Validate(
            PERMIT_APPLICATION application,
            IReadOnlyList<APPLICATION_ATTACHMENT> attachments,
            IReadOnlyList<REQUIRED_FORM> requiredForms,
            JURISDICTION_REQUIREMENTS? requirements,
            DRILLING_PERMIT_APPLICATION? drillingApplication,
            ENVIRONMENTAL_PERMIT_APPLICATION? environmentalApplication,
            INJECTION_PERMIT_APPLICATION? injectionApplication,
            string? configDirectory = null)
        {
            if (application == null)
                throw new ArgumentNullException(nameof(application));

            var normalizedAuthority = Normalize(application.REGULATORY_AUTHORITY);
            var normalizedApplicationType = Normalize(application.APPLICATION_TYPE);

            var request = new PermitValidationRequest(
                application,
                attachments ?? Array.Empty<APPLICATION_ATTACHMENT>(),
                requiredForms ?? Array.Empty<REQUIRED_FORM>(),
                requirements,
                normalizedAuthority,
                normalizedApplicationType,
                drillingApplication,
                environmentalApplication,
                injectionApplication);

            var result = new PermitValidationResult();
            var rulesFactory = string.IsNullOrWhiteSpace(configDirectory)
                ? _rulesFactory
                : new PermitValidationRulesFactory(configDirectory);
            var rules = rulesFactory.CreateRules(normalizedAuthority, normalizedApplicationType);

            var allIssues = new List<PermitValidationIssue>();
            var requiredFieldCount = 0;
            var missingRequiredFieldCount = 0;
            var missingForms = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var rule in rules)
            {
                var ruleResult = rule.Evaluate(request);
                if (ruleResult == null)
                    continue;

                allIssues.AddRange(ruleResult.Issues);
                requiredFieldCount += ruleResult.RequiredFieldCount;
                missingRequiredFieldCount += ruleResult.MissingRequiredFieldCount;

                foreach (var missingForm in ruleResult.MissingForms)
                {
                    missingForms.Add(missingForm);
                }
            }

            foreach (var issue in allIssues)
            {
                var message = FormatIssue(issue);
                if (issue.Severity == PermitValidationSeverity.Error)
                    result.Errors.Add(message);
                else
                    result.Warnings.Add(message);
            }

            result.MissingForms.AddRange(missingForms);
            result.IsValid = result.Errors.Count == 0;

            if (requiredFieldCount > 0)
            {
                var completed = Math.Max(0, requiredFieldCount - missingRequiredFieldCount);
                result.CompletionPercentage = (decimal)completed / requiredFieldCount * 100m;
            }
            else
            {
                result.CompletionPercentage = 100m;
            }

            return result;
        }

        private static string FormatIssue(PermitValidationIssue issue)
        {
            if (!string.IsNullOrWhiteSpace(issue.Field))
            {
                return $"{issue.Field}: {issue.Message}";
            }

            return issue.Message;
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "OTHER" : value.Trim().ToUpperInvariant();
        }
    }
}

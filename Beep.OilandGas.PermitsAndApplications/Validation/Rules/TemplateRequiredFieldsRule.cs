using System;
using System.Collections.Generic;
using Beep.OilandGas.PermitsAndApplications.Forms;

namespace Beep.OilandGas.PermitsAndApplications.Validation.Rules
{
    public class TemplateRequiredFieldsRule : IPermitValidationRule
    {
        private readonly PermitFormTemplateRegistry _registry;

        public TemplateRequiredFieldsRule(PermitFormTemplateRegistry registry)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        }

        public string Name => "Template Required Fields";

        public PermitValidationRuleResult Evaluate(PermitValidationRequest request)
        {
            var result = new PermitValidationRuleResult();
            var templates = _registry.GetTemplates(request.RegulatoryAuthority, request.ApplicationType);

            var requiredFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var template in templates)
            {
                foreach (var field in template.FieldKeys)
                {
                    requiredFields.Add(field);
                }
            }

            result.RequiredFieldCount = requiredFields.Count;

            foreach (var field in requiredFields)
            {
                if (!PermitFieldValueResolver.TryResolve(request, field, out var value) || string.IsNullOrWhiteSpace(value))
                {
                    result.MissingRequiredFieldCount++;
                    result.Issues.Add(new PermitValidationIssue(
                        "CONFIG_REQUIRED_FIELD",
                        $"Required field missing for {request.RegulatoryAuthority}: {field}.",
                        PermitValidationSeverity.Error,
                        field));
                }
            }

            return result;
        }
    }
}

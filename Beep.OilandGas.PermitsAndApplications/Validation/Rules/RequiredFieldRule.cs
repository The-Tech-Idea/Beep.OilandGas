using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.PermitsAndApplications;

namespace Beep.OilandGas.PermitsAndApplications.Validation.Rules
{
    public class RequiredFieldRule : IPermitValidationRule
    {
        private readonly IReadOnlyList<RequiredFieldDefinition> _fields;
        private readonly PermitValidationSeverity _severity;
        private readonly string _code;

        public RequiredFieldRule(
            string name,
            IReadOnlyList<RequiredFieldDefinition> fields,
            PermitValidationSeverity severity = PermitValidationSeverity.Error,
            string code = "REQUIRED_FIELD")
        {
            Name = name;
            _fields = fields ?? Array.Empty<RequiredFieldDefinition>();
            _severity = severity;
            _code = code;
        }

        public string Name { get; }

        public PermitValidationRuleResult Evaluate(PermitValidationRequest request)
        {
            var result = new PermitValidationRuleResult
            {
                RequiredFieldCount = _fields.Count
            };

            foreach (var field in _fields)
            {
                if (field.IsMissing(request.Application))
                {
                    result.MissingRequiredFieldCount++;
                    result.Issues.Add(new PermitValidationIssue(
                        _code,
                        field.Message,
                        _severity,
                        field.FieldName));
                }
            }

            return result;
        }

        public class RequiredFieldDefinition
        {
            public RequiredFieldDefinition(string fieldName, Func<PERMIT_APPLICATION, bool> isMissing, string message)
            {
                FieldName = fieldName;
                IsMissing = isMissing;
                Message = message;
            }

            public string FieldName { get; }
            public Func<PERMIT_APPLICATION, bool> IsMissing { get; }
            public string Message { get; }
        }
    }
}

using System;
using System.Collections.Generic;
using Beep.OilandGas.PermitsAndApplications.Forms;
using Beep.OilandGas.PermitsAndApplications.Validation.Rules;

namespace Beep.OilandGas.PermitsAndApplications.Validation
{
    public class PermitValidationRulesFactory
    {
        private readonly string? _configDirectory;

        public PermitValidationRulesFactory(string? configDirectory = null)
        {
            _configDirectory = configDirectory;
        }

        public IReadOnlyList<IPermitValidationRule> CreateRules(string authority, string applicationType)
        {
            var rules = new List<IPermitValidationRule>();
            var normalizedAuthority = Normalize(authority);
            var normalizedType = Normalize(applicationType);

            var templateRegistry = new PermitFormTemplateRegistry();
            if (!string.IsNullOrWhiteSpace(_configDirectory))
            {
                templateRegistry.LoadFromDirectory(_configDirectory);
            }
            rules.Add(new TemplateRequiredFieldsRule(templateRegistry));

            rules.Add(new RequiredFieldRule(
                "Base Required Fields",
                new[]
                {
                    new RequiredFieldRule.RequiredFieldDefinition(
                        "APPLICATION_TYPE",
                        app => string.IsNullOrWhiteSpace(app.APPLICATION_TYPE),
                        "Application type is required."),
                    new RequiredFieldRule.RequiredFieldDefinition(
                        "COUNTRY",
                        app => string.IsNullOrWhiteSpace(app.COUNTRY),
                        "Country is required."),
                    new RequiredFieldRule.RequiredFieldDefinition(
                        "STATE_PROVINCE",
                        app => string.IsNullOrWhiteSpace(app.STATE_PROVINCE),
                        "State or province is required."),
                    new RequiredFieldRule.RequiredFieldDefinition(
                        "REGULATORY_AUTHORITY",
                        app => string.IsNullOrWhiteSpace(app.REGULATORY_AUTHORITY),
                        "Regulatory authority is required."),
                    new RequiredFieldRule.RequiredFieldDefinition(
                        "APPLICANT_ID",
                        app => string.IsNullOrWhiteSpace(app.APPLICANT_ID),
                        "Applicant ID is required.")
                }));

            rules.Add(new StatusDateRule());
            rules.Add(new DateOrderRule());
            rules.Add(new AttachmentWarningRule());
            rules.Add(new RequiredFormsRule());

            if (normalizedType == "DRILLING")
            {
                rules.Add(new RequiredFieldRule(
                    "Drilling Required Fields",
                    new[]
                    {
                        new RequiredFieldRule.RequiredFieldDefinition(
                            "RELATED_WELL_UWI",
                            app => string.IsNullOrWhiteSpace(app.RELATED_WELL_UWI),
                            "Related well UWI is required for drilling applications.")
                    }));
            }

            if (normalizedType == "ENVIRONMENTAL")
            {
                rules.Add(new RequiredFieldRule(
                    "Environmental Required Fields",
                    new[]
                    {
                        new RequiredFieldRule.RequiredFieldDefinition(
                            "RELATED_FACILITY_ID",
                            app => string.IsNullOrWhiteSpace(app.RELATED_FACILITY_ID),
                            "Related facility ID is required for environmental applications.")
                    },
                    PermitValidationSeverity.Warning));
            }

            if (normalizedType == "INJECTION")
            {
                rules.Add(new RequiredFieldRule(
                    "Injection Required Fields",
                    new[]
                    {
                        new RequiredFieldRule.RequiredFieldDefinition(
                            "RELATED_WELL_UWI",
                            app => string.IsNullOrWhiteSpace(app.RELATED_WELL_UWI),
                            "Related well UWI should be provided for injection applications.")
                    },
                    PermitValidationSeverity.Warning));
            }

            if (normalizedAuthority == "AER")
            {
                rules.Add(new RequiredFieldRule(
                    "AER Required Fields",
                    new[]
                    {
                        new RequiredFieldRule.RequiredFieldDefinition(
                            "OPERATOR_ID",
                            app => string.IsNullOrWhiteSpace(app.OPERATOR_ID),
                            "Operator ID is required for AER submissions.")
                    }));
            }

            if (normalizedAuthority == "RRC" && normalizedType == "DRILLING")
            {
                rules.Add(new RrcDrillingRule());
            }

            if (normalizedAuthority == "RRC" && normalizedType == "ENVIRONMENTAL")
            {
                rules.Add(new RrcEnvironmentalRule());
            }

            if (normalizedAuthority == "RRC" && normalizedType == "INJECTION")
            {
                rules.Add(new RrcInjectionRule());
            }

            if (normalizedAuthority == "TCEQ" && normalizedType == "ENVIRONMENTAL")
            {
                rules.Add(new TceqEnvironmentalRule());
            }

            if (normalizedAuthority == "AER" && normalizedType == "DRILLING")
            {
                rules.Add(new AerDrillingRule());
            }

            if (normalizedAuthority == "AER" && normalizedType == "ENVIRONMENTAL")
            {
                rules.Add(new AerEnvironmentalRule());
            }

            if (normalizedAuthority == "BOEM" && normalizedType == "DRILLING")
            {
                rules.Add(new BoemDrillingRule());
            }

            if (normalizedAuthority == "BSEE" && normalizedType == "ENVIRONMENTAL")
            {
                rules.Add(new BseeEnvironmentalRule());
            }

            if (normalizedAuthority == "BCER" && normalizedType == "DRILLING")
            {
                rules.Add(new BcerDrillingRule());
            }

            if (normalizedAuthority == "BCER" && normalizedType == "ENVIRONMENTAL")
            {
                rules.Add(new BcerEnvironmentalRule());
            }

            return rules;
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "OTHER" : value.Trim().ToUpperInvariant();
        }
    }
}

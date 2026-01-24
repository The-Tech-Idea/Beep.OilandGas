using System;
using System.Collections.Generic;
using System.Linq;

namespace Beep.OilandGas.PermitsAndApplications.Forms
{
    public class PermitFormTemplateRegistry
    {
        private readonly List<PermitFormTemplate> _templates = new()
        {
            new PermitFormTemplate(
                "PERMIT_APPLICATION_FORM",
                "General Permit Application Form",
                "ALL",
                "ALL",
                new[] { "APPLICATION_ID", "APPLICATION_TYPE", "COUNTRY", "STATE_PROVINCE", "REGULATORY_AUTHORITY", "APPLICANT_ID" }),

            new PermitFormTemplate(
                "RRC_DRILLING_PLAN",
                "RRC Drilling Plan",
                "RRC",
                "DRILLING",
                new[]
                {
                    "WELL_UWI",
                    "LEGAL_DESCRIPTION",
                    "TARGET_FORMATION",
                    "PROPOSED_DEPTH",
                    "DRILLING_METHOD",
                    "SURFACE_OWNER_NOTIFIED_IND"
                }),

            new PermitFormTemplate(
                "RRC_EIA_REPORT",
                "RRC Environmental Impact Assessment",
                "RRC",
                "ENVIRONMENTAL",
                new[]
                {
                    "ENVIRONMENTAL_PERMIT_TYPE",
                    "WASTE_TYPE",
                    "WASTE_VOLUME",
                    "DISPOSAL_METHOD",
                    "ENVIRONMENTAL_IMPACT",
                    "MONITORING_PLAN"
                }),

            new PermitFormTemplate(
                "RRC_INJECTION_PLAN",
                "RRC Injection Plan",
                "RRC",
                "INJECTION",
                new[]
                {
                    "INJECTION_TYPE",
                    "INJECTION_ZONE",
                    "INJECTION_FLUID",
                    "INJECTION_WELL_UWI",
                    "MAXIMUM_INJECTION_PRESSURE",
                    "MAXIMUM_INJECTION_RATE"
                }),

            new PermitFormTemplate(
                "AER_WELL_LICENCE",
                "AER Well Licence Application",
                "AER",
                "DRILLING",
                new[]
                {
                    "WELL_UWI",
                    "LEGAL_DESCRIPTION",
                    "TARGET_FORMATION",
                    "DRILLING_METHOD",
                    "OPERATOR_ID"
                }),

            new PermitFormTemplate(
                "AER_WATER_ACT",
                "AER Water Act Application",
                "AER",
                "ENVIRONMENTAL",
                new[]
                {
                    "ENVIRONMENTAL_PERMIT_TYPE",
                    "WASTE_TYPE",
                    "WASTE_VOLUME",
                    "FACILITY_LOCATION",
                    "MONITORING_PLAN"
                }),

            new PermitFormTemplate(
                "TCEQ_ENV_PERMIT",
                "TCEQ Environmental Permit",
                "TCEQ",
                "ENVIRONMENTAL",
                new[]
                {
                    "ENVIRONMENTAL_PERMIT_TYPE",
                    "WASTE_TYPE",
                    "WASTE_VOLUME",
                    "DISPOSAL_METHOD",
                    "FACILITY_LOCATION",
                    "NORM_INVOLVED_IND"
                }),

            new PermitFormTemplate(
                "BCER_DRILLING_NOTICE",
                "BCER Drilling Notice",
                "BCER",
                "DRILLING",
                new[]
                {
                    "WELL_UWI",
                    "LEGAL_DESCRIPTION",
                    "TARGET_FORMATION",
                    "DRILLING_METHOD",
                    "OPERATOR_ID"
                }),

            new PermitFormTemplate(
                "BCER_ENV_AUTHORIZATION",
                "BCER Environmental Authorization",
                "BCER",
                "ENVIRONMENTAL",
                new[]
                {
                    "ENVIRONMENTAL_PERMIT_TYPE",
                    "WASTE_TYPE",
                    "WASTE_VOLUME",
                    "DISPOSAL_METHOD",
                    "FACILITY_LOCATION",
                    "MONITORING_PLAN"
                }),

            new PermitFormTemplate(
                "BOEM_DRILLING_PERMIT",
                "BOEM Drilling Permit",
                "BOEM",
                "DRILLING",
                new[]
                {
                    "WELL_UWI",
                    "TARGET_FORMATION",
                    "PROPOSED_DEPTH",
                    "DRILLING_METHOD",
                    "SPACING_UNIT"
                }),

            new PermitFormTemplate(
                "BSEE_SAFETY_ENV",
                "BSEE Safety and Environmental",
                "BSEE",
                "ENVIRONMENTAL",
                new[]
                {
                    "ENVIRONMENTAL_PERMIT_TYPE",
                    "ENVIRONMENTAL_IMPACT",
                    "MONITORING_PLAN",
                    "FACILITY_LOCATION",
                    "NORM_INVOLVED_IND"
                })
        };

        public IReadOnlyList<PermitFormTemplate> GetTemplates(string authority, string applicationType)
        {
            var normalizedAuthority = Normalize(authority);
            var normalizedType = Normalize(applicationType);

            return _templates
                .Where(template =>
                    (template.Authority == "ALL" || string.Equals(template.Authority, normalizedAuthority, StringComparison.OrdinalIgnoreCase)) &&
                    (template.ApplicationType == "ALL" || string.Equals(template.ApplicationType, normalizedType, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        public void AddTemplates(IEnumerable<PermitFormTemplate> templates)
        {
            if (templates == null)
                return;

            foreach (var template in templates)
            {
                AddTemplate(template);
            }
        }

        public void LoadFromDirectory(string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
                throw new ArgumentException("Directory path is required.", nameof(directoryPath));

            var configs = JurisdictionConfigLoader.LoadFromDirectory(directoryPath);
            foreach (var config in configs)
            {
                foreach (var template in config.ToTemplates())
                {
                    AddTemplate(template);
                }
            }
        }

        private void AddTemplate(PermitFormTemplate template)
        {
            if (template == null)
                return;

            var exists = _templates.Any(existing =>
                string.Equals(existing.FormCode, template.FormCode, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(existing.Authority, template.Authority, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(existing.ApplicationType, template.ApplicationType, StringComparison.OrdinalIgnoreCase));

            if (!exists)
            {
                _templates.Add(template);
            }
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "OTHER" : value.Trim().ToUpperInvariant();
        }
    }
}

using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.PermitsAndApplications;

namespace Beep.OilandGas.PermitsAndApplications.Forms
{
    public class PermitFormRenderer
    {
        public IDictionary<string, string> Render(PERMIT_APPLICATION application, PermitFormTemplate template)
        {
            var context = new PermitFormRenderContext(application, null, null, null);
            return Render(context, template);
        }

        public IDictionary<string, string> Render(
            PermitFormRenderContext context,
            PermitFormTemplate template)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (template == null)
                throw new ArgumentNullException(nameof(template));

            var data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var key in template.FieldKeys)
            {
                if (TryResolveField(context, key, out var value))
                {
                    data[key] = value ?? string.Empty;
                }
            }

            return data;
        }

        private static bool TryResolveField(PermitFormRenderContext context, string key, out string? value)
        {
            var application = context.Application;
            value = key switch
            {
                "APPLICATION_ID" => application.PERMIT_APPLICATION_ID,
                "APPLICATION_TYPE" => application.APPLICATION_TYPE,
                "COUNTRY" => application.COUNTRY,
                "STATE_PROVINCE" => application.STATE_PROVINCE,
                "REGULATORY_AUTHORITY" => application.REGULATORY_AUTHORITY,
                "APPLICANT_ID" => application.APPLICANT_ID,
                "OPERATOR_ID" => application.OPERATOR_ID,
                "RELATED_WELL_UWI" => application.RELATED_WELL_UWI,
                "RELATED_FACILITY_ID" => application.RELATED_FACILITY_ID,
                _ => null
            };

            if (value != null)
                return true;

            if (context.DrillingApplication != null)
            {
                value = key switch
                {
                    "WELL_UWI" => context.DrillingApplication.WELL_UWI,
                    "LEGAL_DESCRIPTION" => context.DrillingApplication.LEGAL_DESCRIPTION,
                    "TARGET_FORMATION" => context.DrillingApplication.TARGET_FORMATION,
                    "PROPOSED_DEPTH" => context.DrillingApplication.PROPOSED_DEPTH?.ToString(),
                    "DRILLING_METHOD" => context.DrillingApplication.DRILLING_METHOD,
                    "SURFACE_OWNER_NOTIFIED_IND" => context.DrillingApplication.SURFACE_OWNER_NOTIFIED_IND,
                    "ENVIRONMENTAL_ASSESSMENT_REQUIRED_IND" => context.DrillingApplication.ENVIRONMENTAL_ASSESSMENT_REQUIRED_IND,
                    "ENVIRONMENTAL_ASSESSMENT_REFERENCE" => context.DrillingApplication.ENVIRONMENTAL_ASSESSMENT_REFERENCE,
                    "SPACING_UNIT" => context.DrillingApplication.SPACING_UNIT,
                    "PERMIT_TYPE" => context.DrillingApplication.PERMIT_TYPE,
                    _ => null
                };
            }

            if (value != null)
                return true;

            if (context.EnvironmentalApplication != null)
            {
                value = key switch
                {
                    "ENVIRONMENTAL_PERMIT_TYPE" => context.EnvironmentalApplication.ENVIRONMENTAL_PERMIT_TYPE,
                    "WASTE_TYPE" => context.EnvironmentalApplication.WASTE_TYPE,
                    "WASTE_VOLUME" => context.EnvironmentalApplication.WASTE_VOLUME?.ToString(),
                    "WASTE_VOLUME_UNIT" => context.EnvironmentalApplication.WASTE_VOLUME_UNIT,
                    "DISPOSAL_METHOD" => context.EnvironmentalApplication.DISPOSAL_METHOD,
                    "ENVIRONMENTAL_IMPACT" => context.EnvironmentalApplication.ENVIRONMENTAL_IMPACT,
                    "MONITORING_PLAN" => context.EnvironmentalApplication.MONITORING_PLAN,
                    "NORM_INVOLVED_IND" => context.EnvironmentalApplication.NORM_INVOLVED_IND,
                    "FACILITY_LOCATION" => context.EnvironmentalApplication.FACILITY_LOCATION,
                    _ => null
                };
            }

            if (value != null)
                return true;

            if (context.InjectionApplication != null)
            {
                value = key switch
                {
                    "INJECTION_TYPE" => context.InjectionApplication.INJECTION_TYPE,
                    "INJECTION_ZONE" => context.InjectionApplication.INJECTION_ZONE,
                    "INJECTION_FLUID" => context.InjectionApplication.INJECTION_FLUID,
                    "MAXIMUM_INJECTION_PRESSURE" => context.InjectionApplication.MAXIMUM_INJECTION_PRESSURE?.ToString(),
                    "MAXIMUM_INJECTION_RATE" => context.InjectionApplication.MAXIMUM_INJECTION_RATE?.ToString(),
                    "INJECTION_RATE_UNIT" => context.InjectionApplication.INJECTION_RATE_UNIT,
                    "MONITORING_REQUIREMENTS" => context.InjectionApplication.MONITORING_REQUIREMENTS,
                    "INJECTION_WELL_UWI" => context.InjectionApplication.INJECTION_WELL_UWI,
                    "IS_CO2_STORAGE_IND" => context.InjectionApplication.IS_CO2_STORAGE_IND,
                    "IS_GAS_STORAGE_IND" => context.InjectionApplication.IS_GAS_STORAGE_IND,
                    "IS_BRINE_MINING_IND" => context.InjectionApplication.IS_BRINE_MINING_IND,
                    _ => null
                };
            }

            return value != null;
        }
    }
}

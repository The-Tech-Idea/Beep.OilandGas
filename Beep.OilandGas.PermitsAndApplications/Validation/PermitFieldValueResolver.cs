using Beep.OilandGas.Models.Data.PermitsAndApplications;

namespace Beep.OilandGas.PermitsAndApplications.Validation
{
    public static class PermitFieldValueResolver
    {
        public static bool TryResolve(PermitValidationRequest request, string key, out string? value)
        {
            value = key switch
            {
                "APPLICATION_ID" => request.Application.PERMIT_APPLICATION_ID,
                "APPLICATION_TYPE" => request.Application.APPLICATION_TYPE.ToString(),
                "COUNTRY" => request.Application.COUNTRY.ToString(),
                "STATE_PROVINCE" => request.Application.STATE_PROVINCE.ToString(),
                "REGULATORY_AUTHORITY" => request.Application.REGULATORY_AUTHORITY.ToString(),
                "APPLICANT_ID" => request.Application.APPLICANT_ID,
                "OPERATOR_ID" => request.Application.OPERATOR_ID,
                "RELATED_WELL_UWI" => request.Application.RELATED_WELL_UWI,
                "RELATED_FACILITY_ID" => request.Application.RELATED_FACILITY_ID,
                _ => null
            };

            if (!string.IsNullOrWhiteSpace(value))
                return true;

            if (request.DrillingApplication != null)
            {
                value = key switch
                {
                    "WELL_UWI" => request.DrillingApplication.WELL_UWI,
                    "LEGAL_DESCRIPTION" => request.DrillingApplication.LEGAL_DESCRIPTION,
                    "TARGET_FORMATION" => request.DrillingApplication.TARGET_FORMATION,
                    "PROPOSED_DEPTH" => request.DrillingApplication.PROPOSED_DEPTH?.ToString(),
                    "DRILLING_METHOD" => request.DrillingApplication.DRILLING_METHOD,
                    "SURFACE_OWNER_NOTIFIED_IND" => request.DrillingApplication.SURFACE_OWNER_NOTIFIED_IND,
                    "ENVIRONMENTAL_ASSESSMENT_REQUIRED_IND" => request.DrillingApplication.ENVIRONMENTAL_ASSESSMENT_REQUIRED_IND,
                    "ENVIRONMENTAL_ASSESSMENT_REFERENCE" => request.DrillingApplication.ENVIRONMENTAL_ASSESSMENT_REFERENCE,
                    "SPACING_UNIT" => request.DrillingApplication.SPACING_UNIT,
                    "PERMIT_TYPE" => request.DrillingApplication.PERMIT_TYPE,
                    _ => null
                };
            }

            if (!string.IsNullOrWhiteSpace(value))
                return true;

            if (request.EnvironmentalApplication != null)
            {
                value = key switch
                {
                    "ENVIRONMENTAL_PERMIT_TYPE" => request.EnvironmentalApplication.ENVIRONMENTAL_PERMIT_TYPE,
                    "WASTE_TYPE" => request.EnvironmentalApplication.WASTE_TYPE,
                    "WASTE_VOLUME" => request.EnvironmentalApplication.WASTE_VOLUME?.ToString(),
                    "WASTE_VOLUME_UNIT" => request.EnvironmentalApplication.WASTE_VOLUME_UNIT,
                    "DISPOSAL_METHOD" => request.EnvironmentalApplication.DISPOSAL_METHOD,
                    "ENVIRONMENTAL_IMPACT" => request.EnvironmentalApplication.ENVIRONMENTAL_IMPACT,
                    "MONITORING_PLAN" => request.EnvironmentalApplication.MONITORING_PLAN,
                    "NORM_INVOLVED_IND" => request.EnvironmentalApplication.NORM_INVOLVED_IND,
                    "FACILITY_LOCATION" => request.EnvironmentalApplication.FACILITY_LOCATION,
                    _ => null
                };
            }

            if (!string.IsNullOrWhiteSpace(value))
                return true;

            if (request.InjectionApplication != null)
            {
                value = key switch
                {
                    "INJECTION_TYPE" => request.InjectionApplication.INJECTION_TYPE,
                    "INJECTION_ZONE" => request.InjectionApplication.INJECTION_ZONE,
                    "INJECTION_FLUID" => request.InjectionApplication.INJECTION_FLUID,
                    "MAXIMUM_INJECTION_PRESSURE" => request.InjectionApplication.MAXIMUM_INJECTION_PRESSURE.ToString(),
                    "MAXIMUM_INJECTION_RATE" => request.InjectionApplication.MAXIMUM_INJECTION_RATE.ToString(),
                    "INJECTION_RATE_UNIT" => request.InjectionApplication.INJECTION_RATE_UNIT,
                    "MONITORING_REQUIREMENTS" => request.InjectionApplication.MONITORING_REQUIREMENTS,
                    "INJECTION_WELL_UWI" => request.InjectionApplication.INJECTION_WELL_UWI,
                    "IS_CO2_STORAGE_IND" => request.InjectionApplication.IS_CO2_STORAGE_IND,
                    "IS_GAS_STORAGE_IND" => request.InjectionApplication.IS_GAS_STORAGE_IND,
                    "IS_BRINE_MINING_IND" => request.InjectionApplication.IS_BRINE_MINING_IND,
                    _ => null
                };
            }

            return !string.IsNullOrWhiteSpace(value);
        }
    }
}

using System.Globalization;
using Beep.OilandGas.PermitsAndApplications.Data.PermitTables;

namespace TheTechIdea.Data.OilGas;

/// <summary>Supporting records supply details for one exact parent; they do not own its filing state.</summary>
public static class PermitApplicationSupportingRecords
{
    public static void RequireParent(PERMIT_APPLICATION parent, DRILLING_PERMIT_APPLICATION? drilling,
        ENVIRONMENTAL_PERMIT_APPLICATION? environmental, INJECTION_PERMIT_APPLICATION? injection)
    {
        ArgumentNullException.ThrowIfNull(parent);
        if (parent.PERMIT_APPLICATION_ID is null ||
            drilling is not null && drilling.PERMIT_APPLICATION_ID != parent.PERMIT_APPLICATION_ID ||
            environmental is not null && environmental.PERMIT_APPLICATION_ID != parent.PERMIT_APPLICATION_ID ||
            injection is not null && injection.PERMIT_APPLICATION_ID != parent.PERMIT_APPLICATION_ID)
            throw new InvalidOperationException("Supporting permit records must belong to the exact parent application.");
    }
    /// <summary>Recorded values for form payloads and automated checks, without inferred parent state.</summary>
    public static bool TryResolve(PERMIT_APPLICATION application, DRILLING_PERMIT_APPLICATION? drilling,
        ENVIRONMENTAL_PERMIT_APPLICATION? environmental, INJECTION_PERMIT_APPLICATION? injection, string key, out string? value)
{
            RequireParent(application, drilling, environmental, injection);
            value = key switch
            {
                "PROPOSED_DEPTH_UNIT" => drilling?.PROPOSED_DEPTH_UNIT,
                "APPLICATION_ID" => application.PERMIT_APPLICATION_ID,
                "APPLICATION_TYPE" => application.APPLICATION_TYPE.ToString(),
                "STATUS" => application.STATUS.ToString(),
                "COUNTRY" => application.COUNTRY.ToString(),
                "STATE_PROVINCE" => application.STATE_PROVINCE.ToString(),
                "REGULATORY_AUTHORITY" => application.REGULATORY_AUTHORITY.ToString(),
                "APPLICANT_ID" => application.APPLICANT_ID,
                "REMARKS" => application.REMARKS ?? string.Empty,
                "FIELD_ID" => application.FIELD_ID,
                "WELL_ID" or "WELL_UWI" or "INJECTION_WELL_UWI" => application.WELL_ID ?? string.Empty,
                "FACILITY_ID" => application.FACILITY_ID ?? string.Empty,
                "FACILITY_TYPE" => application.FACILITY_TYPE ?? string.Empty,
                "SUBMITTED_DATE" => application.SUBMITTED_DATE?.ToString("O", CultureInfo.InvariantCulture) ?? string.Empty,
                "RECEIVED_DATE" => application.RECEIVED_DATE?.ToString("O", CultureInfo.InvariantCulture) ?? string.Empty,
                "DECISION_DATE" => application.DECISION_DATE?.ToString("O", CultureInfo.InvariantCulture) ?? string.Empty,
                "EFFECTIVE_DATE" => application.EFFECTIVE_DATE?.ToString("O", CultureInfo.InvariantCulture) ?? string.Empty,
                "EXPIRY_DATE" => application.EXPIRY_DATE?.ToString("O", CultureInfo.InvariantCulture) ?? string.Empty,
                "DECISION" => application.DECISION ?? string.Empty,
                "REFERENCE_NUMBER" => application.REFERENCE_NUMBER ?? string.Empty,
                "FEES_DESCRIPTION" => application.FEES_DESCRIPTION ?? string.Empty,
                "FEES_PAID_IND" => application.FEES_PAID ? "Y" : "N",
                "SUBMISSION_COMPLETE_IND" => application.SUBMISSION_COMPLETE ? "Y" : "N",
                "SUBMISSION_DESCRIPTION" => application.SUBMISSION_DESCRIPTION ?? string.Empty,
                "OPERATOR_ID" => application.OPERATOR_ID,
                "RELATED_WELL_UWI" => application.RELATED_WELL_UWI,
                "RELATED_FACILITY_ID" => application.RELATED_FACILITY_ID,
                _ => null
            };

            if (value != null)
                return true;

            if (drilling != null)
            {
                value = key switch
                {
                    "DRILLING_REMARKS" => drilling.REMARKS ?? string.Empty,
                    "LEGAL_DESCRIPTION" => drilling.LEGAL_DESCRIPTION,
                    "TARGET_FORMATION" => drilling.TARGET_FORMATION,
                    "PROPOSED_DEPTH" => drilling.PROPOSED_DEPTH.ToString(CultureInfo.InvariantCulture),
                    "DRILLING_METHOD" => drilling.DRILLING_METHOD,
                    "SURFACE_OWNER_NOTIFIED_IND" => drilling.SURFACE_OWNER_NOTIFIED_IND,
                    "ENVIRONMENTAL_ASSESSMENT_REQUIRED_IND" => drilling.ENVIRONMENTAL_ASSESSMENT_REQUIRED_IND,
                    "ENVIRONMENTAL_ASSESSMENT_REFERENCE" => drilling.ENVIRONMENTAL_ASSESSMENT_REFERENCE,
                    "SPACING_UNIT" => drilling.SPACING_UNIT,
                    "PERMIT_TYPE" => drilling.PERMIT_TYPE,
                    _ => null
                };
            }

            if (value != null)
                return true;

            if (environmental != null)
            {
                value = key switch
                {
                    "ENVIRONMENTAL_REMARKS" => environmental.REMARKS ?? string.Empty,
                    "ENVIRONMENTAL_PERMIT_TYPE" => environmental.ENVIRONMENTAL_PERMIT_TYPE,
                    "WASTE_TYPE" => environmental.WASTE_TYPE,
                    "WASTE_VOLUME" => environmental.WASTE_VOLUME?.ToString(CultureInfo.InvariantCulture),
                    "WASTE_VOLUME_UNIT" => environmental.WASTE_VOLUME_UNIT,
                    "DISPOSAL_METHOD" => environmental.DISPOSAL_METHOD,
                    "ENVIRONMENTAL_IMPACT" => environmental.ENVIRONMENTAL_IMPACT,
                    "MONITORING_PLAN" => environmental.MONITORING_PLAN,
                    "NORM_INVOLVED_IND" => environmental.NORM_INVOLVED_IND,
                    "FACILITY_LOCATION" => environmental.FACILITY_LOCATION,
                    _ => null
                };
            }

            if (value != null)
                return true;

            if (injection != null)
            {
                value = key switch
                {
                    "INJECTION_REMARKS" => injection.REMARKS ?? string.Empty,
                    "INJECTION_TYPE" => injection.INJECTION_TYPE,
                    "INJECTION_ZONE" => injection.INJECTION_ZONE,
                    "INJECTION_FLUID" => injection.INJECTION_FLUID,
                    "MAXIMUM_INJECTION_PRESSURE" => injection.MAXIMUM_INJECTION_PRESSURE?.ToString(CultureInfo.InvariantCulture),
                    "MAXIMUM_INJECTION_RATE" => injection.MAXIMUM_INJECTION_RATE?.ToString(CultureInfo.InvariantCulture),
                    "INJECTION_RATE_UNIT" => injection.INJECTION_RATE_UNIT,
                    "MONITORING_REQUIREMENTS" => injection.MONITORING_REQUIREMENTS,
                    "IS_CO2_STORAGE_IND" => injection.IS_CO2_STORAGE_IND,
                    "IS_GAS_STORAGE_IND" => injection.IS_GAS_STORAGE_IND,
                    "IS_BRINE_MINING_IND" => injection.IS_BRINE_MINING_IND,
                    _ => null
                };
            }

            return value != null;
        }
}

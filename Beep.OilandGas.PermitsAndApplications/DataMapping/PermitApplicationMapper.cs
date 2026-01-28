using System;
using Beep.OilandGas.Models.Data.PermitsAndApplications;

namespace Beep.OilandGas.PermitsAndApplications.DataMapping
{
    /// <summary>
    /// Maps between PERMIT_APPLICATION domain model and PERMIT_APPLICATION data entity.
    /// </summary>
    public class PermitApplicationMapper
    {
        public PERMIT_APPLICATION MapToDomain(PERMIT_APPLICATION data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            return new PERMIT_APPLICATION
            {
                APPLICANT_ID = data.PERMIT_APPLICATION_ID ?? string.Empty,
                APPLICATION_TYPE = data.APPLICATION_TYPE,
                STATUS = data.STATUS,
                COUNTRY = data.COUNTRY,
                STATE_PROVINCE = data.STATE_PROVINCE,
                REGULATORY_AUTHORITY = data.REGULATORY_AUTHORITY,

                OPERATOR_ID = data.OPERATOR_ID,
                RELATED_WELL_UWI = data.RELATED_WELL_UWI,
                RELATED_FACILITY_ID = data.RELATED_FACILITY_ID,
                CREATED_DATE = data.CREATED_DATE,
                SUBMITTED_DATE = data.SUBMITTED_DATE,
                RECEIVED_DATE = data.RECEIVED_DATE,
                DECISION_DATE = data.DECISION_DATE,
                EFFECTIVE_DATE = data.EFFECTIVE_DATE,
                EXPIRY_DATE = data.EXPIRY_DATE,
                DECISION = data.DECISION,
                REFERENCE_NUMBER = data.REFERENCE_NUMBER,
                FEES_DESCRIPTION = data.FEES_DESCRIPTION,
                FEES_PAID = string.Equals(data.FEES_PAID_IND, "Y", StringComparison.OrdinalIgnoreCase),
                REMARKS = data.REMARKS,
                SUBMISSION_COMPLETE = string.Equals(data.SUBMISSION_COMPLETE_IND, "Y", StringComparison.OrdinalIgnoreCase),
                SUBMISSION_DESCRIPTION = data.SUBMISSION_DESCRIPTION
            };
        }

        public PERMIT_APPLICATION MapToData(PERMIT_APPLICATION application, PERMIT_APPLICATION? existing = null)
        {
            if (application == null)
                throw new ArgumentNullException(nameof(application));

            var data = existing ?? new PERMIT_APPLICATION();

            data.PERMIT_APPLICATION_ID = application.APPLICATION_ID;
            data.APPLICATION_TYPE = application.APPLICATION_TYPE;
            data.STATUS = application.STATUS;
            data.COUNTRY = application.COUNTRY;
            data.STATE_PROVINCE = application.STATE_PROVINCE;
            data.REGULATORY_AUTHORITY = application.REGULATORY_AUTHORITY;
            data.APPLICANT_ID = application.APPLICANT_ID;
            data.OPERATOR_ID = application.OPERATOR_ID;
            data.RELATED_WELL_UWI = application.RELATED_WELL_UWI;
            data.RELATED_FACILITY_ID = application.RELATED_FACILITY_ID;
            data.CREATED_DATE = application.CREATED_DATE;
            data.SUBMITTED_DATE = application.SUBMITTED_DATE;
            data.RECEIVED_DATE = application.RECEIVED_DATE;
            data.DECISION_DATE = application.DECISION_DATE;
            data.EFFECTIVE_DATE = application.EFFECTIVE_DATE;
            data.EXPIRY_DATE = application.EXPIRY_DATE;
            data.DECISION = application.DECISION;
            data.REFERENCE_NUMBER = application.REFERENCE_NUMBER;
            data.FEES_DESCRIPTION = application.FEES_DESCRIPTION;
            data.FEES_PAID_IND = application.FEES_PAID ? "Y" : "N";
            data.REMARKS = application.REMARKS;
            data.SUBMISSION_COMPLETE_IND = application.SUBMISSION_COMPLETE ? "Y" : "N";
            data.SUBMISSION_DESCRIPTION = application.SUBMISSION_DESCRIPTION;

            return data;
        }

        private PermitApplicationType MapApplicationType(string? applicationType)
        {
            if (string.IsNullOrWhiteSpace(applicationType))
                return PermitApplicationType.Other;

            return applicationType.ToUpperInvariant() switch
            {
                "DRILLING" => PermitApplicationType.Drilling,
                "ENVIRONMENTAL" => PermitApplicationType.Environmental,
                "INJECTION" => PermitApplicationType.Injection,
                "STORAGE" => PermitApplicationType.Storage,
                "FACILITY" => PermitApplicationType.Facility,
                "SEISMIC" => PermitApplicationType.Seismic,
                "GROUNDWATER" => PermitApplicationType.Groundwater,
                _ => PermitApplicationType.Other
            };
        }

        private string MapApplicationTypeToString(PermitApplicationType applicationType)
        {
            return applicationType switch
            {
                PermitApplicationType.Drilling => "DRILLING",
                PermitApplicationType.Environmental => "ENVIRONMENTAL",
                PermitApplicationType.Injection => "INJECTION",
                PermitApplicationType.Storage => "STORAGE",
                PermitApplicationType.Facility => "FACILITY",
                PermitApplicationType.Seismic => "SEISMIC",
                PermitApplicationType.Groundwater => "GROUNDWATER",
                _ => "OTHER"
            };
        }

        private PermitApplicationStatus MapApplicationStatus(string? status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return PermitApplicationStatus.Draft;

            return status.ToUpperInvariant() switch
            {
                "DRAFT" => PermitApplicationStatus.Draft,
                "SUBMITTED" => PermitApplicationStatus.Submitted,
                "UNDER_REVIEW" => PermitApplicationStatus.UnderReview,
                "ADDITIONAL_INFO_REQUIRED" => PermitApplicationStatus.AdditionalInformationRequired,
                "APPROVED" => PermitApplicationStatus.Approved,
                "REJECTED" => PermitApplicationStatus.Rejected,
                "WITHDRAWN" => PermitApplicationStatus.Withdrawn,
                "EXPIRED" => PermitApplicationStatus.Expired,
                "RENEWED" => PermitApplicationStatus.Renewed,
                _ => PermitApplicationStatus.Draft
            };
        }

        private string MapApplicationStatusToString(PermitApplicationStatus status)
        {
            return status switch
            {
                PermitApplicationStatus.Draft => "DRAFT",
                PermitApplicationStatus.Submitted => "SUBMITTED",
                PermitApplicationStatus.UnderReview => "UNDER_REVIEW",
                PermitApplicationStatus.AdditionalInformationRequired => "ADDITIONAL_INFO_REQUIRED",
                PermitApplicationStatus.Approved => "APPROVED",
                PermitApplicationStatus.Rejected => "REJECTED",
                PermitApplicationStatus.Withdrawn => "WITHDRAWN",
                PermitApplicationStatus.Expired => "EXPIRED",
                PermitApplicationStatus.Renewed => "RENEWED",
                _ => "DRAFT"
            };
        }

        private Country MapCountry(string? country)
        {
            if (string.IsNullOrWhiteSpace(country))
                return Country.Other;

            return Enum.TryParse<Country>(country, true, out var parsed) ? parsed : Country.Other;
        }

        private StateProvince MapStateProvince(string? stateProvince)
        {
            if (string.IsNullOrWhiteSpace(stateProvince))
                return StateProvince.Other;

            return Enum.TryParse<StateProvince>(stateProvince, true, out var parsed) ? parsed : StateProvince.Other;
        }

        private RegulatoryAuthority MapRegulatoryAuthority(string? authority)
        {
            if (string.IsNullOrWhiteSpace(authority))
                return RegulatoryAuthority.Other;

            return Enum.TryParse<RegulatoryAuthority>(authority, true, out var parsed)
                ? parsed
                : RegulatoryAuthority.Other;
        }

        private string MapRegulatoryAuthorityToString(RegulatoryAuthority authority)
        {
            return authority switch
            {
                RegulatoryAuthority.RRC => "RRC",
                RegulatoryAuthority.TCEQ => "TCEQ",
                RegulatoryAuthority.AOGCC => "AOGCC",
                RegulatoryAuthority.NDIC => "NDIC",
                RegulatoryAuthority.WOGCC => "WOGCC",
                RegulatoryAuthority.COGCC => "COGCC",
                RegulatoryAuthority.OCC => "OCC",
                RegulatoryAuthority.LADNR => "LADNR",
                RegulatoryAuthority.NMOCD => "NMOCD",
                RegulatoryAuthority.CEC => "CEC",
                RegulatoryAuthority.BLM => "BLM",
                RegulatoryAuthority.USACE => "USACE",
                RegulatoryAuthority.EPA => "EPA",
                RegulatoryAuthority.BOEM => "BOEM",
                RegulatoryAuthority.BSEE => "BSEE",
                RegulatoryAuthority.AER => "AER",
                RegulatoryAuthority.BCER => "BCER",
                RegulatoryAuthority.SER => "SER",
                RegulatoryAuthority.NLDET => "NLDET",
                RegulatoryAuthority.CNH => "CNH",
                RegulatoryAuthority.ASEA => "ASEA",
                RegulatoryAuthority.NPD => "NPD",
                RegulatoryAuthority.NSTA => "NSTA",
                RegulatoryAuthority.NOPSEMA => "NOPSEMA",
                RegulatoryAuthority.QLD_DNRME => "QLD_DNRME",
                RegulatoryAuthority.WA_DMIRS => "WA_DMIRS",
                RegulatoryAuthority.ANP => "ANP",
                RegulatoryAuthority.ARG_NEUQUEN => "ARG_NEUQUEN",
                RegulatoryAuthority.ARG_MENDOZA => "ARG_MENDOZA",
                RegulatoryAuthority.DPR => "DPR",
                RegulatoryAuthority.SKKMigas => "SKKMigas",
                RegulatoryAuthority.KZ_MOE => "KZ_MOE",
                _ => "OTHER"
            };
        }
    }
}

using System;
using Beep.OilandGas.Models.Data.PermitsAndApplications;

namespace Beep.OilandGas.PermitsAndApplications.DataMapping
{
    /// <summary>
    /// Maps between PermitApplication domain model and PERMIT_APPLICATION data entity.
    /// </summary>
    public class PermitApplicationMapper
    {
        public PermitApplication MapToDomain(PERMIT_APPLICATION data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            return new PermitApplication
            {
                ApplicationId = data.PERMIT_APPLICATION_ID ?? string.Empty,
                ApplicationType = MapApplicationType(data.APPLICATION_TYPE),
                Status = MapApplicationStatus(data.STATUS),
                Country = MapCountry(data.COUNTRY),
                StateProvince = MapStateProvince(data.STATE_PROVINCE),
                RegulatoryAuthority = MapRegulatoryAuthority(data.REGULATORY_AUTHORITY),
                ApplicantId = data.APPLICANT_ID,
                OperatorId = data.OPERATOR_ID,
                RelatedWellUwi = data.RELATED_WELL_UWI,
                RelatedFacilityId = data.RELATED_FACILITY_ID,
                CreatedDate = data.CREATED_DATE,
                SubmittedDate = data.SUBMITTED_DATE,
                ReceivedDate = data.RECEIVED_DATE,
                DecisionDate = data.DECISION_DATE,
                EffectiveDate = data.EFFECTIVE_DATE,
                ExpiryDate = data.EXPIRY_DATE,
                Decision = data.DECISION,
                ReferenceNumber = data.REFERENCE_NUMBER,
                FeesDescription = data.FEES_DESCRIPTION,
                FeesPaid = string.Equals(data.FEES_PAID_IND, "Y", StringComparison.OrdinalIgnoreCase),
                Remarks = data.REMARKS,
                SubmissionComplete = string.Equals(data.SUBMISSION_COMPLETE_IND, "Y", StringComparison.OrdinalIgnoreCase),
                SubmissionDescription = data.SUBMISSION_DESCRIPTION
            };
        }

        public PERMIT_APPLICATION MapToData(PermitApplication application, PERMIT_APPLICATION? existing = null)
        {
            if (application == null)
                throw new ArgumentNullException(nameof(application));

            var data = existing ?? new PERMIT_APPLICATION();

            data.PERMIT_APPLICATION_ID = application.ApplicationId;
            data.APPLICATION_TYPE = MapApplicationTypeToString(application.ApplicationType);
            data.STATUS = MapApplicationStatusToString(application.Status);
            data.COUNTRY = application.Country.ToString();
            data.STATE_PROVINCE = application.StateProvince.ToString();
            data.REGULATORY_AUTHORITY = MapRegulatoryAuthorityToString(application.RegulatoryAuthority);
            data.APPLICANT_ID = application.ApplicantId;
            data.OPERATOR_ID = application.OperatorId;
            data.RELATED_WELL_UWI = application.RelatedWellUwi;
            data.RELATED_FACILITY_ID = application.RelatedFacilityId;
            data.CREATED_DATE = application.CreatedDate;
            data.SUBMITTED_DATE = application.SubmittedDate;
            data.RECEIVED_DATE = application.ReceivedDate;
            data.DECISION_DATE = application.DecisionDate;
            data.EFFECTIVE_DATE = application.EffectiveDate;
            data.EXPIRY_DATE = application.ExpiryDate;
            data.DECISION = application.Decision;
            data.REFERENCE_NUMBER = application.ReferenceNumber;
            data.FEES_DESCRIPTION = application.FeesDescription;
            data.FEES_PAID_IND = application.FeesPaid ? "Y" : "N";
            data.REMARKS = application.Remarks;
            data.SUBMISSION_COMPLETE_IND = application.SubmissionComplete ? "Y" : "N";
            data.SUBMISSION_DESCRIPTION = application.SubmissionDescription;

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

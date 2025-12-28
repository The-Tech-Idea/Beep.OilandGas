using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.PermitsAndApplications.Models;

namespace Beep.OilandGas.PermitsAndApplications.DataMapping
{
    /// <summary>
    /// Maps between PPDM39 APPLICATION entity and domain models.
    /// </summary>
    public class ApplicationMapper
    {
        /// <summary>
        /// Maps PPDM39 APPLICATION to PermitApplication domain model.
        /// </summary>
        /// <param name="ppdmApplication">The PPDM39 APPLICATION entity.</param>
        /// <param name="attachments">Optional list of APPLIC_ATTACH entities.</param>
        /// <param name="areas">Optional list of APPLIC_AREA entities.</param>
        /// <param name="components">Optional list of APPLICATION_COMPONENT entities.</param>
        /// <returns>The mapped PermitApplication.</returns>
        public PermitApplication MapToDomain(
            APPLICATION ppdmApplication,
            IEnumerable<APPLIC_ATTACH>? attachments = null,
            IEnumerable<APPLIC_AREA>? areas = null,
            IEnumerable<APPLICATION_COMPONENT>? components = null)
        {
            if (ppdmApplication == null)
                throw new ArgumentNullException(nameof(ppdmApplication));

            // Extract country and state/province from regulatory authority or application data
            var regulatoryAuthority = MapRegulatoryAuthority(ppdmApplication.SUBMITTED_TO);
            var (country, stateProvince) = InferJurisdiction(regulatoryAuthority, ppdmApplication);

            var application = new PermitApplication
            {
                ApplicationId = ppdmApplication.APPLICATION_ID ?? string.Empty,
                ApplicationType = MapApplicationType(ppdmApplication.APPLICATION_TYPE),
                Status = MapApplicationStatus(ppdmApplication.CURRENT_STATUS),
                Country = country,
                StateProvince = stateProvince,
                RegulatoryAuthority = regulatoryAuthority,
                CreatedDate = ppdmApplication.ROW_CREATED_DATE,
                SubmittedDate = ppdmApplication.SUBMITTED_DATE,
                ReceivedDate = ppdmApplication.RECEIVED_DATE,
                DecisionDate = ppdmApplication.DECISION_DATE,
                EffectiveDate = ppdmApplication.EFFECTIVE_DATE,
                ExpiryDate = ppdmApplication.EXPIRY_DATE,
                Decision = ppdmApplication.DECISION,
                ReferenceNumber = ppdmApplication.REFERENCE_NUM,
                FeesDescription = ppdmApplication.FEES_DESC,
                FeesPaid = ppdmApplication.FEES_PAID_IND == "Y",
                Remarks = ppdmApplication.REMARK,
                SubmissionComplete = ppdmApplication.SUBMISSION_COMPLETE_IND == "Y",
                SubmissionDescription = ppdmApplication.SUBMISSION_DESC
            };

            // Map attachments
            if (attachments != null)
            {
                application.Attachments = attachments.Select(a => new ApplicationAttachment
                {
                    AttachmentId = a.ATTACHMENT_ID ?? string.Empty,
                    FileName = a.PHYSICAL_ITEM_ID ?? string.Empty, // Physical item ID may contain file reference
                    FileType = a.ATTACHMENT_TYPE,
                    FileSize = null, // Not available in APPLIC_ATTACH
                    UploadDate = a.ROW_CREATED_DATE,
                    Description = a.ATTACHMENT_DESCRIPTION,
                    DocumentType = a.ATTACHMENT_TYPE
                }).ToList();
            }

            // Map areas
            if (areas != null)
            {
                application.Areas = areas.Select(a => new ApplicationArea
                {
                    AreaId = a.AREA_ID ?? string.Empty,
                    AreaName = a.DESCRIPTION, // Use DESCRIPTION as area name
                    AreaType = a.AREA_TYPE,
                    LegalDescription = a.REMARK // Use REMARK for legal description
                }).ToList();
            }

            // Map components
            if (components != null)
            {
                application.Components = components
                    .Where(c => !string.IsNullOrEmpty(c.APPLICATION_COMPONENT_TYPE))
                    .Select((c, index) => new ApplicationComponent
                    {
                        ComponentId = $"{c.APPLICATION_ID}_{c.COMPONENT_OBS_NO}",
                        ComponentType = c.APPLICATION_COMPONENT_TYPE ?? string.Empty,
                        Description = c.REMARK,
                        Value = c.UWI ?? c.AREA_ID ?? c.FACILITY_ID ?? c.EQUIPMENT_ID, // Use relevant ID as value
                        SequenceNumber = (int)c.COMPONENT_OBS_NO
                    }).ToList();
            }

            return application;
        }

        /// <summary>
        /// Maps PermitApplication domain model to PPDM39 APPLICATION entity.
        /// </summary>
        /// <param name="application">The domain model.</param>
        /// <param name="existingApplication">Optional existing PPDM39 entity to update.</param>
        /// <returns>The mapped APPLICATION entity.</returns>
        public APPLICATION MapToPPDM39(
            PermitApplication application,
            APPLICATION? existingApplication = null)
        {
            if (application == null)
                throw new ArgumentNullException(nameof(application));

            var ppdmApplication = existingApplication ?? new APPLICATION();

            ppdmApplication.APPLICATION_ID = application.ApplicationId;
            ppdmApplication.APPLICATION_TYPE = MapApplicationTypeToString(application.ApplicationType);
            ppdmApplication.CURRENT_STATUS = MapApplicationStatusToString(application.Status);
            ppdmApplication.SUBMITTED_TO = MapRegulatoryAuthorityToString(application.RegulatoryAuthority);
            
            // Store jurisdiction information in REMARK or use a component
            var jurisdictionInfo = $"{application.Country}|{application.StateProvince}";
            ppdmApplication.REMARK = string.IsNullOrEmpty(application.Remarks) 
                ? $"Jurisdiction: {jurisdictionInfo}" 
                : $"{application.Remarks}\nJurisdiction: {jurisdictionInfo}";
            
            ppdmApplication.SUBMITTED_DATE = (DateTime)application.SubmittedDate;
            ppdmApplication.RECEIVED_DATE = (DateTime)application.ReceivedDate;
            ppdmApplication.DECISION_DATE = (DateTime)application.DecisionDate;
            ppdmApplication.EFFECTIVE_DATE = (DateTime)application.EffectiveDate;
            ppdmApplication.EXPIRY_DATE = (DateTime)application.ExpiryDate;
            ppdmApplication.DECISION = application.Decision;
            ppdmApplication.REFERENCE_NUM = application.ReferenceNumber;
            ppdmApplication.FEES_DESC = application.FeesDescription;
            ppdmApplication.FEES_PAID_IND = application.FeesPaid ? "Y" : "N";
            ppdmApplication.SUBMISSION_COMPLETE_IND = application.SubmissionComplete ? "Y" : "N";
            ppdmApplication.SUBMISSION_DESC = application.SubmissionDescription;
            ppdmApplication.ACTIVE_IND = application.Status != PermitApplicationStatus.Expired ? "Y" : "N";

            return ppdmApplication;
        }

        /// <summary>
        /// Maps DrillingPermitApplication to PPDM39 APPLICATION.
        /// </summary>
        public APPLICATION MapDrillingPermitToPPDM39(
            DrillingPermitApplication application,
            APPLICATION? existingApplication = null)
        {
            var ppdmApplication = MapToPPDM39(application, existingApplication);
            
            // Add drilling-specific information to components or remarks
            if (!string.IsNullOrEmpty(application.TargetFormation))
            {
                ppdmApplication.REMARK = $"{ppdmApplication.REMARK}\nTarget Formation: {application.TargetFormation}";
            }
            if (application.ProposedDepth.HasValue)
            {
                ppdmApplication.REMARK = $"{ppdmApplication.REMARK}\nProposed Depth: {application.ProposedDepth} ft";
            }

            return ppdmApplication;
        }

        /// <summary>
        /// Maps EnvironmentalPermitApplication to PPDM39 APPLICATION.
        /// </summary>
        public APPLICATION MapEnvironmentalPermitToPPDM39(
            EnvironmentalPermitApplication application,
            APPLICATION? existingApplication = null)
        {
            var ppdmApplication = MapToPPDM39(application, existingApplication);
            
            // Add environmental-specific information
            if (!string.IsNullOrEmpty(application.WasteType))
            {
                ppdmApplication.REMARK = $"{ppdmApplication.REMARK}\nWaste Type: {application.WasteType}";
            }

            return ppdmApplication;
        }

        /// <summary>
        /// Maps InjectionPermitApplication to PPDM39 APPLICATION.
        /// </summary>
        public APPLICATION MapInjectionPermitToPPDM39(
            InjectionPermitApplication application,
            APPLICATION? existingApplication = null)
        {
            var ppdmApplication = MapToPPDM39(application, existingApplication);
            
            // Add injection-specific information
            if (!string.IsNullOrEmpty(application.InjectionType))
            {
                ppdmApplication.REMARK = $"{ppdmApplication.REMARK}\nInjection Type: {application.InjectionType}";
            }
            if (!string.IsNullOrEmpty(application.InjectionZone))
            {
                ppdmApplication.REMARK = $"{ppdmApplication.REMARK}\nInjection Zone: {application.InjectionZone}";
            }

            return ppdmApplication;
        }

        private PermitApplicationType MapApplicationType(string? applicationType)
        {
            if (string.IsNullOrWhiteSpace(applicationType))
                return PermitApplicationType.Other;

            return applicationType.ToUpper() switch
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

        public string MapApplicationTypeToString(PermitApplicationType applicationType)
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

            return status.ToUpper() switch
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

        public string MapApplicationStatusToString(PermitApplicationStatus status)
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

        private RegulatoryAuthority MapRegulatoryAuthority(string? authority)
        {
            if (string.IsNullOrWhiteSpace(authority))
                return RegulatoryAuthority.Other;

            return authority.ToUpper() switch
            {
                // United States
                "RRC" => RegulatoryAuthority.RRC,
                "TCEQ" => RegulatoryAuthority.TCEQ,
                "AOGCC" => RegulatoryAuthority.AOGCC,
                "NDIC" => RegulatoryAuthority.NDIC,
                "WOGCC" => RegulatoryAuthority.WOGCC,
                "COGCC" => RegulatoryAuthority.COGCC,
                "OCC" => RegulatoryAuthority.OCC,
                "LADNR" => RegulatoryAuthority.LADNR,
                "NMOCD" => RegulatoryAuthority.NMOCD,
                "CEC" => RegulatoryAuthority.CEC,
                "BLM" => RegulatoryAuthority.BLM,
                "USACE" => RegulatoryAuthority.USACE,
                "EPA" => RegulatoryAuthority.EPA,
                "BOEM" => RegulatoryAuthority.BOEM,
                "BSEE" => RegulatoryAuthority.BSEE,
                
                // Canada
                "AER" => RegulatoryAuthority.AER,
                "BCER" => RegulatoryAuthority.BCER,
                "SER" => RegulatoryAuthority.SER,
                "NLDET" => RegulatoryAuthority.NLDET,
                
                // Mexico
                "CNH" => RegulatoryAuthority.CNH,
                "ASEA" => RegulatoryAuthority.ASEA,
                
                // Norway
                "NPD" => RegulatoryAuthority.NPD,
                
                // United Kingdom
                "NSTA" => RegulatoryAuthority.NSTA,
                "OGA" => RegulatoryAuthority.NSTA, // Legacy name
                
                // Australia
                "NOPSEMA" => RegulatoryAuthority.NOPSEMA,
                "QLD_DNRME" => RegulatoryAuthority.QLD_DNRME,
                "WA_DMIRS" => RegulatoryAuthority.WA_DMIRS,
                
                // Brazil
                "ANP" => RegulatoryAuthority.ANP,
                
                // Argentina
                "ARG_NEUQUEN" => RegulatoryAuthority.ARG_NEUQUEN,
                "ARG_MENDOZA" => RegulatoryAuthority.ARG_MENDOZA,
                
                // Nigeria
                "DPR" => RegulatoryAuthority.DPR,
                
                // Indonesia
                "SKKMIGAS" => RegulatoryAuthority.SKKMigas,
                
                // Kazakhstan
                "KZ_MOE" => RegulatoryAuthority.KZ_MOE,
                
                _ => RegulatoryAuthority.Other
            };
        }

        private string MapRegulatoryAuthorityToString(RegulatoryAuthority authority)
        {
            return authority switch
            {
                // United States
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
                
                // Canada
                RegulatoryAuthority.AER => "AER",
                RegulatoryAuthority.BCER => "BCER",
                RegulatoryAuthority.SER => "SER",
                RegulatoryAuthority.NLDET => "NLDET",
                
                // Mexico
                RegulatoryAuthority.CNH => "CNH",
                RegulatoryAuthority.ASEA => "ASEA",
                
                // Norway
                RegulatoryAuthority.NPD => "NPD",
                
                // United Kingdom
                RegulatoryAuthority.NSTA => "NSTA",
                
                // Australia
                RegulatoryAuthority.NOPSEMA => "NOPSEMA",
                RegulatoryAuthority.QLD_DNRME => "QLD_DNRME",
                RegulatoryAuthority.WA_DMIRS => "WA_DMIRS",
                
                // Brazil
                RegulatoryAuthority.ANP => "ANP",
                
                // Argentina
                RegulatoryAuthority.ARG_NEUQUEN => "ARG_NEUQUEN",
                RegulatoryAuthority.ARG_MENDOZA => "ARG_MENDOZA",
                
                // Nigeria
                RegulatoryAuthority.DPR => "DPR",
                
                // Indonesia
                RegulatoryAuthority.SKKMigas => "SKKMigas",
                
                // Kazakhstan
                RegulatoryAuthority.KZ_MOE => "KZ_MOE",
                
                _ => "OTHER"
            };
        }

        /// <summary>
        /// Infers country and state/province from regulatory authority and application data.
        /// </summary>
        private (Country country, StateProvince stateProvince) InferJurisdiction(
            RegulatoryAuthority authority,
            APPLICATION application)
        {
            // Try to extract from REMARK field first (if stored there)
            if (!string.IsNullOrEmpty(application.REMARK))
            {
                var jurisdictionMatch = System.Text.RegularExpressions.Regex.Match(
                    application.REMARK, 
                    @"Jurisdiction:\s*([^|]+)\|([^\n]+)");
                if (jurisdictionMatch.Success)
                {
                    if (Enum.TryParse<Country>(jurisdictionMatch.Groups[1].Value.Trim(), true, out var parsedCountry) &&
                        Enum.TryParse<StateProvince>(jurisdictionMatch.Groups[2].Value.Trim(), true, out var parsedStateProvince))
                    {
                        return (parsedCountry, parsedStateProvince);
                    }
                }
            }

            // Fall back to inferring from regulatory authority using JurisdictionHelper
            var country = JurisdictionHelper.GetCountry(authority);
            var stateProvince = JurisdictionHelper.GetStateProvince(authority);
            return (country, stateProvince);
        }
    }
}


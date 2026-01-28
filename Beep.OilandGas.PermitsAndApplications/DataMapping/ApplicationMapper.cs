using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data.PermitsAndApplications;
using Beep.OilandGas.PPDM39.Models;
using DomainApplicationComponent = Beep.OilandGas.Models.Data.PermitsAndApplications.APPLICATION_COMPONENT;
using PpdmApplicationComponent = Beep.OilandGas.PPDM39.Models.APPLICATION_COMPONENT;

namespace Beep.OilandGas.PermitsAndApplications.DataMapping
{
    /// <summary>
    /// Maps between PPDM39 APPLICATION entity and domain models.
    /// </summary>
    public class ApplicationMapper
    {
        /// <summary>
        /// Maps PPDM39 APPLICATION to PERMIT_APPLICATION domain model.
        /// </summary>
        /// <param name="ppdmApplication">The PPDM39 APPLICATION entity.</param>
        /// <param name="attachments">Optional list of APPLIC_ATTACH entities.</param>
        /// <param name="areas">Optional list of APPLIC_AREA entities.</param>
        /// <param name="components">Optional list of APPLICATION_COMPONENT entities.</param>
        /// <returns>The mapped PERMIT_APPLICATION.</returns>
        public PERMIT_APPLICATION MapToDomain(
            APPLICATION ppdmApplication,
            IEnumerable<APPLIC_ATTACH>? attachments = null,
            IEnumerable<APPLIC_AREA>? areas = null,
            IEnumerable<PpdmApplicationComponent>? components = null)
        {
            if (ppdmApplication == null)
                throw new ArgumentNullException(nameof(ppdmApplication));

            // Extract country and state/province from regulatory authority or application data
            var regulatoryAuthority = MapRegulatoryAuthority(ppdmApplication.SUBMITTED_TO);
            var (country, stateProvince) = InferJurisdiction(regulatoryAuthority, ppdmApplication);

            var application = new PERMIT_APPLICATION
            {
                APPLICANT_ID = ppdmApplication.APPLICATION_ID ?? string.Empty,
                APPLICATION_TYPE = MapApplicationType(ppdmApplication.APPLICATION_TYPE),
                STATUS = MapApplicationStatus(ppdmApplication.CURRENT_STATUS),
                COUNTRY = country,
                STATE_PROVINCE = stateProvince,
                REGULATORY_AUTHORITY = regulatoryAuthority,
                CREATED_DATE = ppdmApplication.ROW_CREATED_DATE ?? ppdmApplication.ROW_CREATED_DATE,
                SUBMITTED_DATE = ppdmApplication.SUBMITTED_DATE,
                RECEIVED_DATE = ppdmApplication.RECEIVED_DATE,
                DECISION_DATE = ppdmApplication.DECISION_DATE,
                EFFECTIVE_DATE = ppdmApplication.EFFECTIVE_DATE,
                EXPIRY_DATE = ppdmApplication.EXPIRY_DATE,
                DECISION = ppdmApplication.DECISION,
                REFERENCE_NUMBER = ppdmApplication.REFERENCE_NUM,
                FEES_DESCRIPTION = ppdmApplication.FEES_DESC,
                FEES_PAID = string.Equals(ppdmApplication.FEES_PAID_IND, "Y", StringComparison.OrdinalIgnoreCase),
                REMARKS = ppdmApplication.REMARK,
                SUBMISSION_COMPLETE = string.Equals(ppdmApplication.SUBMISSION_COMPLETE_IND, "Y", StringComparison.OrdinalIgnoreCase),
                SUBMISSION_DESCRIPTION = ppdmApplication.SUBMISSION_DESC
            };

            // Map attachments
            if (attachments != null)
            {
                application.ATTACHMENTS = attachments.Select(a => new APPLICATION_ATTACHMENT
                {
                    APPLICATION_ATTACHMENT_ID = a.ATTACHMENT_ID ?? Guid.NewGuid().ToString(),
                    PERMIT_APPLICATION_ID = ppdmApplication.APPLICATION_ID ?? string.Empty,
                    FILE_NAME = a.PHYSICAL_ITEM_ID,
                    FILE_TYPE = a.ATTACHMENT_TYPE,
                    FILE_SIZE = null,
                    UPLOAD_DATE = a.ROW_CREATED_DATE,
                    DESCRIPTION = a.ATTACHMENT_DESCRIPTION,
                    DOCUMENT_TYPE = a.ATTACHMENT_TYPE,
                    ACTIVE_IND = a.ACTIVE_IND
                }).ToList();
            }

            // Map areas
            if (areas != null)
            {
                application.Areas = areas.Select(a => new APPLICATION_AREA
                {
                    AREA_ID = a.AREA_ID ?? string.Empty,
                    AREA_NAME = a.AREA_ID ?? a.DESCRIPTION,
                    AREA_TYPE = a.AREA_TYPE,
                    LEGAL_DESCRIPTION = a.DESCRIPTION ?? a.REMARK
                }).ToList();
            }

            // Map components
            if (components != null)
            {           
                application.Components = components
                    .Where(c => !string.IsNullOrEmpty(c.APPLICATION_COMPONENT_TYPE))
                    .Select((c, index) => new DomainApplicationComponent
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
        /// Maps PERMIT_APPLICATION domain model to PPDM39 APPLICATION entity.
        /// </summary>
        /// <param name="application">The domain model.</param>
        /// <param name="existingApplication">Optional existing PPDM39 entity to update.</param>
        /// <returns>The mapped APPLICATION entity.</returns>
        public APPLICATION MapToPPDM39(
            PERMIT_APPLICATION application,
            APPLICATION? existingApplication = null)
        {
            if (application == null)
                throw new ArgumentNullException(nameof(application));

            var ppdmApplication = existingApplication ?? new APPLICATION();

            ppdmApplication.APPLICATION_ID = application.APPLICATION_ID;
            ppdmApplication.APPLICATION_TYPE = MapApplicationTypeToString(application.APPLICATION_TYPE);
            ppdmApplication.CURRENT_STATUS = MapApplicationStatusToString(application.STATUS);
            ppdmApplication.SUBMITTED_TO = MapRegulatoryAuthorityToString(application.REGULATORY_AUTHORITY);
            
            // Store jurisdiction information in REMARK or use a component
            var jurisdictionInfo = $"{application.COUNTRY}|{application.STATE_PROVINCE}";
            ppdmApplication.REMARK = string.IsNullOrEmpty(application.REMARKS) 
                ? $"Jurisdiction: {jurisdictionInfo}" 
                : $"{application.REMARKS}\nJurisdiction: {jurisdictionInfo}";
            
            ppdmApplication.SUBMITTED_DATE = (DateTime)application.SUBMITTED_DATE;
            ppdmApplication.RECEIVED_DATE = (DateTime)application.RECEIVED_DATE;
            ppdmApplication.DECISION_DATE = (DateTime)application.DECISION_DATE;
            ppdmApplication.EFFECTIVE_DATE = (DateTime)application.EFFECTIVE_DATE;
            ppdmApplication.EXPIRY_DATE = (DateTime)application.EXPIRY_DATE;
            ppdmApplication.DECISION = application.DECISION;
            ppdmApplication.REFERENCE_NUM = application.REFERENCE_NUMBER;
            ppdmApplication.FEES_DESC = application.FEES_DESCRIPTION;
            ppdmApplication.FEES_PAID_IND = application.FEES_PAID ? "Y" : "N";
            ppdmApplication.SUBMISSION_COMPLETE_IND = application.SUBMISSION_COMPLETE ? "Y" : "N";
            ppdmApplication.SUBMISSION_DESC = application.SUBMISSION_DESCRIPTION;
            ppdmApplication.ACTIVE_IND = application.STATUS != PermitApplicationStatus.Expired ? "Y" : "N";

            return ppdmApplication;
        }

        /// <summary>
        /// Maps DRILLING_PERMIT_APPLICATION to PPDM39 APPLICATION.
        /// </summary>
        public APPLICATION MapDrillingPermitToPPDM39(
            DRILLING_PERMIT_APPLICATION application,
            APPLICATION? existingApplication = null)
        {
            var ppdmApplication = MapToPPDM39(application, existingApplication);
            
            // Add drilling-specific information to components or remarks
            if (!string.IsNullOrEmpty(application.TARGET_FORMATION))
            {
                ppdmApplication.REMARK = $"{ppdmApplication.REMARK}\nTarget Formation: {application.TARGET_FORMATION}";
            }
            if (application.PROPOSED_DEPTH > 0m)
            {
                ppdmApplication.REMARK = $"{ppdmApplication.REMARK}\nProposed Depth: {application.PROPOSED_DEPTH} ft";
            }

            return ppdmApplication;
        }

        /// <summary>
        /// Maps ENVIRONMENTAL_PERMIT_APPLICATION to PPDM39 APPLICATION.
        /// </summary>
        public APPLICATION MapEnvironmentalPermitToPPDM39(
            ENVIRONMENTAL_PERMIT_APPLICATION application,
            APPLICATION? existingApplication = null)
        {
            var ppdmApplication = MapToPPDM39(application, existingApplication);
            
            // Add environmental-specific information
            if (!string.IsNullOrEmpty(application.WASTE_TYPE))
            {
                ppdmApplication.REMARK = $"{ppdmApplication.REMARK}\nWaste Type: {application.WASTE_TYPE}";
            }

            return ppdmApplication;
        }

        /// <summary>
        /// Maps INJECTION_PERMIT_APPLICATION to PPDM39 APPLICATION.
        /// </summary>
        public APPLICATION MapInjectionPermitToPPDM39(
            INJECTION_PERMIT_APPLICATION application,
            APPLICATION? existingApplication = null)
        {
            var ppdmApplication = MapToPPDM39(application, existingApplication);
            
            // Add injection-specific information
            if (!string.IsNullOrEmpty(application.INJECTION_TYPE))
            {
                ppdmApplication.REMARK = $"{ppdmApplication.REMARK}\nInjection Type: {application.INJECTION_TYPE}";
            }
            if (!string.IsNullOrEmpty(application.INJECTION_ZONE))
            {
                ppdmApplication.REMARK = $"{ppdmApplication.REMARK}\nInjection Zone: {application.INJECTION_ZONE}";
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


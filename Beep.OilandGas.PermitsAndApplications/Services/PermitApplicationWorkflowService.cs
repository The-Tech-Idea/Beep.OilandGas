using Beep.OilandGas.PermitsAndApplications.Models;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PermitsAndApplications.Exceptions;
using Beep.OilandGas.PPDM39.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.PermitsAndApplications;
using Beep.OilandGas.PermitsAndApplications.Data.PermitTables;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.DataBase;
using Microsoft.Extensions.Logging;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Repositories;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.PermitsAndApplications.Constants;
using Beep.OilandGas.PermitsAndApplications.Forms;
using Beep.OilandGas.PermitsAndApplications.Validation;

namespace Beep.OilandGas.PermitsAndApplications.Services
{
    /// <summary>
    /// Service implementation for permit and application management.
    /// Provides comprehensive permit application lifecycle management with PPDM compliance.
    /// </summary>
    public class PermitApplicationWorkflowService : PermitsServiceBase, IPermitApplicationWorkflowService
    {
        private readonly ILogger<PermitApplicationWorkflowService>? _logger;

        public PermitApplicationWorkflowService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ICompanyPermitApplicationDataAccess dataAccess,
            ILogger<PermitApplicationWorkflowService>? logger = null)
            : base(editor, commonColumnHandler, defaults, metadata, dataAccess, logger)
        {
            _logger = logger;
        }

        // Explicit overload to satisfy interface which uses string parameters
        public async Task<JURISDICTION_REQUIREMENTS?> GetJurisdictionRequirementsAsync(
            string country,
            string stateProvince,
            string authority)
        {
            if (string.IsNullOrWhiteSpace(country)) throw new InvalidApplicationException("Choose the country.");
            if (string.IsNullOrWhiteSpace(stateProvince)) throw new InvalidApplicationException("Choose the state or province.");
            if (string.IsNullOrWhiteSpace(authority)) throw new InvalidApplicationException("Choose the regulatory authority.");

            // A name that is not a known country, state or authority is refused. The failed parses had been ignored, so
            // an unknown name was looked up as the enum's first member (the United States, Texas, the RRC).
            if (!Enum.TryParse<Country>(country, true, out var parsedCountry) || !Enum.IsDefined(parsedCountry))
                throw new InvalidApplicationException($"\"{country}\" is not a country permits are filed in.");
            if (!Enum.TryParse<StateProvince>(stateProvince, true, out var parsedState) || !Enum.IsDefined(parsedState))
                throw new InvalidApplicationException($"\"{stateProvince}\" is not a known state or province.");
            if (!Enum.TryParse<RegulatoryAuthority>(authority, true, out var parsedAuthority) || !Enum.IsDefined(parsedAuthority))
                throw new InvalidApplicationException($"\"{authority}\" is not a known regulatory authority.");

            return await GetJurisdictionRequirementsAsync(parsedCountry, parsedState, parsedAuthority);
        }

        /// <summary>
        /// Retrieves a permit application by ID.
        /// </summary>
        public async Task<PERMIT_APPLICATION?> GetPermitApplicationAsync(string applicationId)
        {
            try
            {
                _logger?.LogInformation("Retrieving permit application: {ApplicationId}", applicationId);

                var repo = await CreateRepositoryAsync<PERMIT_APPLICATION>("PERMIT_APPLICATION");

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "PERMIT_APPLICATION_ID", Operator = "=", FilterValue = applicationId },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var result = await repo.GetByIdAsync(applicationId);

                _logger?.LogInformation("Permit application retrieved: {ApplicationId}", applicationId);
                return result is PERMIT_APPLICATION entity ? entity : null;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving permit application: {ApplicationId}", applicationId);
                throw;
            }
        }

        /// <summary>
        /// Retrieves permit applications by status.
        /// </summary>
        public async Task<List<PERMIT_APPLICATION>> GetPermitApplicationsByStatusAsync(string status)
        {
            try
            {
                _logger?.LogInformation("Retrieving permit applications with status: {Status}", status);

                var repo = await CreateRepositoryAsync<PERMIT_APPLICATION>("PERMIT_APPLICATION");

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "STATUS", Operator = "=", FilterValue = status },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var results = await repo.GetAsync(filters);
                var applications = results.OfType<PERMIT_APPLICATION>().ToList();

                _logger?.LogInformation("Retrieved {Count} permit applications with status: {Status}", applications.Count, status);
                return applications;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving permit applications by status: {Status}", status);
                throw;
            }
        }

        /// <summary>
        /// Retrieves permit applications by regulatory authority.
        /// </summary>
        public async Task<List<PERMIT_APPLICATION>> GetPermitApplicationsByAuthorityAsync(string authority)
        {
            try
            {
                _logger?.LogInformation("Retrieving permit applications for authority: {Authority}", authority);

                var repo = await CreateRepositoryAsync<PERMIT_APPLICATION>("PERMIT_APPLICATION");

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "REGULATORY_AUTHORITY", Operator = "=", FilterValue = authority },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var results = await repo.GetAsync(filters);
                var applications = results.OfType<PERMIT_APPLICATION>().ToList();

                _logger?.LogInformation("Retrieved {Count} permit applications for authority: {Authority}", applications.Count, authority);
                return applications;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving permit applications by authority: {Authority}", authority);
                throw;
            }
        }

        /// <summary>
        /// Retrieves attachments for a permit application.
        /// </summary>
        public async Task<List<APPLICATION_ATTACHMENT>> GetApplicationAttachmentsAsync(string applicationId)
        {
            try
            {
                _logger?.LogInformation("Retrieving attachments for application: {ApplicationId}", applicationId);

                var repo = await CreateRepositoryAsync<APPLICATION_ATTACHMENT>("APPLICATION_ATTACHMENT", applicationId);

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "PERMIT_APPLICATION_ID", Operator = "=", FilterValue = applicationId },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var results = await repo.GetAsync(filters);
                var attachments = results.OfType<APPLICATION_ATTACHMENT>().ToList();

                _logger?.LogInformation("Retrieved {Count} attachments for application: {ApplicationId}", attachments.Count, applicationId);
                return attachments;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving attachments for application: {ApplicationId}", applicationId);
                throw;
            }
        }

        /// <summary>
        /// Validates a permit application for completeness.
        /// </summary>
        public async Task<PermitValidationResult> ValidatePermitApplicationAsync(string applicationId, string? configDirectory = null)
        {
            try
            {
                _logger?.LogInformation("Validating permit application: {ApplicationId}", applicationId);

                var application = await GetPermitApplicationAsync(applicationId);

                if (application == null)
                {
                    var validationResult = new PermitValidationResult();
                    validationResult.Errors.Add("Permit application not found");
                    validationResult.IsValid = false;
                    return validationResult;
                }

                var attachments = await GetApplicationAttachmentsAsync(applicationId);
                var requiredForms = GetRequiredFormsForValidation(application);
                var applicationType = application.APPLICATION_TYPE;

                var drillingApplication = applicationType == PermitApplicationType.Drilling
                    ? await GetDrillingApplicationAsync(applicationId)
                    : null;
                var environmentalApplication = applicationType ==  PermitApplicationType.Environmental
                    ? await GetEnvironmentalApplicationAsync(applicationId)
                    : null;
                var injectionApplication = applicationType ==  PermitApplicationType.Injection
                    ? await GetInjectionApplicationAsync(applicationId)
                    : null;

                var validator = new PermitValidationEngine(new PermitValidationRulesFactory());
                var result = validator.Validate(
                    application,
                    attachments,
                    requiredForms,
                    null,
                    drillingApplication,
                    environmentalApplication,
                    injectionApplication,
                    configDirectory);

                _logger?.LogInformation("Permit application validation completed: {ApplicationId} - Valid: {IsValid}",
                    applicationId, result.IsValid);
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error validating permit application: {ApplicationId}", applicationId);
                throw;
            }
        }

        /// <summary>
        /// Retrieves jurisdiction requirements for a specific authority.
        /// </summary>
        public async Task<JURISDICTION_REQUIREMENTS?> GetJurisdictionRequirementsAsync(
            Country country,
            StateProvince stateProvince,
            RegulatoryAuthority authority)
        {
            try
            {
                _logger?.LogInformation("Retrieving jurisdiction requirements for {Country}, {State}, {Authority}",
                    country, stateProvince, authority);

                var repo = await CreateRepositoryAsync<JURISDICTION_REQUIREMENTS>("JURISDICTION_REQUIREMENTS");

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "COUNTRY", Operator = "=", FilterValue = country.ToString() },
                    new AppFilter { FieldName = "STATE_PROVINCE", Operator = "=", FilterValue = stateProvince.ToString() },
                    new AppFilter { FieldName = "REGULATORY_AUTHORITY", Operator = "=", FilterValue = authority.ToString() },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var results = await repo.GetAsync(filters);
                var requirements = results.OfType<JURISDICTION_REQUIREMENTS>().FirstOrDefault();

                _logger?.LogInformation("Jurisdiction requirements retrieved for {Country}, {State}, {Authority}",
                    country, stateProvince, authority);
                return requirements;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving jurisdiction requirements");
                throw;
            }
        }

        /// <summary>
        /// Calculates application fees based on jurisdiction and application type.
        /// </summary>
        public async Task<decimal> CalculateApplicationFeesAsync(string applicationId)
        {
            try
            {
                _logger?.LogInformation("Calculating fees for application: {ApplicationId}", applicationId);

                var application = await GetPermitApplicationAsync(applicationId);
                if (application == null)
                    throw new PermitNotFoundException($"Permit application {applicationId} was not found.", applicationId);

                // Base fee calculation (simplified - would be more complex in real implementation)
                decimal baseFee = 1000.00m; // Base fee

                // Jurisdiction multipliers
                var jurisdictionMultiplier = GetJurisdictionFeeMultiplier(application.REGULATORY_AUTHORITY);
                var typeMultiplier = GetApplicationTypeFeeMultiplier(application.APPLICATION_TYPE);

                decimal totalFee = baseFee * jurisdictionMultiplier * typeMultiplier;

                _logger?.LogInformation("Calculated fees for application: {ApplicationId} - Amount: {Amount:C}", applicationId, totalFee);
                return totalFee;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error calculating fees for application: {ApplicationId}", applicationId);
                throw;
            }
        }

        /// <summary>
        /// Generates JSON payloads for required forms by application ID.
        /// </summary>
        public async Task<string> GenerateFormPayloadJsonAsync(string applicationId, string? configDirectory = null)
        {
            try
            {
                _logger?.LogInformation("Generating form payload JSON for application: {ApplicationId}", applicationId);

                var application = await GetPermitApplicationAsync(applicationId);
                if (application == null)
                    throw new PermitNotFoundException($"Permit application {applicationId} was not found.", applicationId);

                var applicationType = NormalizeApplicationType(application.APPLICATION_TYPE);
                var drillingApplication = applicationType == "DRILLING"
                    ? await GetDrillingApplicationAsync(applicationId)
                    : null;
                var environmentalApplication = applicationType == "ENVIRONMENTAL"
                    ? await GetEnvironmentalApplicationAsync(applicationId)
                    : null;
                var injectionApplication = applicationType == "INJECTION"
                    ? await GetInjectionApplicationAsync(applicationId)
                    : null;

                var context = new PermitFormRenderContext(
                    application,
                    drillingApplication,
                    environmentalApplication,
                    injectionApplication);

                var builder = new PermitFormPayloadBuilder();
                var payload = builder.BuildJsonPayload(
                    context,
                    application.REGULATORY_AUTHORITY,
                    application.APPLICATION_TYPE,
                    configDirectory);

                _logger?.LogInformation("Generated form payload JSON for application: {ApplicationId}", applicationId);
                return payload;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating form payload JSON for application: {ApplicationId}", applicationId);
                throw;
            }
        }

        // Helper methods

        private decimal GetJurisdictionFeeMultiplier(RegulatoryAuthority authority)
        {
            // Simplified fee multipliers by jurisdiction
            return authority switch
            {
                RegulatoryAuthority.RRC => 1.0m,    // Texas RRC - base rate
                RegulatoryAuthority.TCEQ => 1.2m,   // Texas TCEQ - higher for environmental
                RegulatoryAuthority.EPA => 1.5m,    // Federal EPA - highest fees
                RegulatoryAuthority.AER => 1.1m,    // Alberta Energy Regulator
                RegulatoryAuthority.BCER => 1.1m,   // BC Energy Regulator
                RegulatoryAuthority.CNH => 1.3m,    // Mexico CNH
                // Every other authority: the base rate.
                RegulatoryAuthority.AOGCC or RegulatoryAuthority.NDIC or RegulatoryAuthority.WOGCC
                    or RegulatoryAuthority.COGCC or RegulatoryAuthority.OCC or RegulatoryAuthority.LADNR
                    or RegulatoryAuthority.NMOCD or RegulatoryAuthority.CEC or RegulatoryAuthority.BLM
                    or RegulatoryAuthority.USACE or RegulatoryAuthority.BOEM or RegulatoryAuthority.BSEE
                    or RegulatoryAuthority.SER or RegulatoryAuthority.NLDET or RegulatoryAuthority.ASEA
                    or RegulatoryAuthority.NPD or RegulatoryAuthority.NSTA or RegulatoryAuthority.NOPSEMA
                    or RegulatoryAuthority.QLD_DNRME or RegulatoryAuthority.WA_DMIRS or RegulatoryAuthority.NT_DITT
                    or RegulatoryAuthority.SA_DMRE or RegulatoryAuthority.ANP or RegulatoryAuthority.ARG_NEUQUEN
                    or RegulatoryAuthority.ARG_MENDOZA or RegulatoryAuthority.DPR or RegulatoryAuthority.SKKMigas
                    or RegulatoryAuthority.KZ_MOE or RegulatoryAuthority.Other => 1.0m
            };
        }

        private decimal GetApplicationTypeFeeMultiplier(PermitApplicationType applicationType)
        {
            return applicationType switch
            {
                PermitApplicationType.Drilling => 1.5m, // Drilling permits are most complex
                PermitApplicationType.Environmental => 1.3m, // Environmental assessments are detailed
                PermitApplicationType.Injection => 1.2m,     // Injection permits require monitoring
                PermitApplicationType.Storage or PermitApplicationType.Facility or PermitApplicationType.Seismic
                    or PermitApplicationType.Groundwater or PermitApplicationType.Other => 1.0m
            };
        }

        private List<REQUIRED_FORM> GetRequiredFormsForValidation(PERMIT_APPLICATION application)
        {
            var registry = new PermitFormTemplateRegistry();
            var normalizedAuthority = NormalizeAuthority(application.REGULATORY_AUTHORITY.ToString());
            var normalizedType = NormalizeApplicationType(application.APPLICATION_TYPE);
            var templates = registry.GetTemplates(application.REGULATORY_AUTHORITY, application.APPLICATION_TYPE);

            return templates.Select(template => new REQUIRED_FORM
            {
                FORM_CODE = template.FormCode,
                FORM_NAME = template.FormName,
                REQUIRED_IND = "Y",
                ACTIVE_IND = "Y"
            }).ToList();
        }

        private static string NormalizeApplicationType(PermitApplicationType applicationType)
        {
           

            return applicationType switch
            {
                PermitApplicationType.Drilling =>"DRILLING",

                PermitApplicationType.Environmental => "ENVIRONMENTAL",

                PermitApplicationType.Injection => "INJECTION",
                PermitApplicationType.Storage => "STORAGE",
                 PermitApplicationType.Facility => "FACILITY",
                 PermitApplicationType.Seismic => "SEISMIC",
                 PermitApplicationType.Groundwater => "GROUNDWATER",
                PermitApplicationType.Other => "OTHER"
            };
        }

        private static string NormalizeAuthority(string authority)
        {
            return string.IsNullOrWhiteSpace(authority) ? "OTHER" : authority.Trim().ToUpperInvariant();
        }

        private async Task<DRILLING_PERMIT_APPLICATION?> GetDrillingApplicationAsync(string permitApplicationId)
        {
            var repo = await CreateRepositoryAsync<DRILLING_PERMIT_APPLICATION>("DRILLING_PERMIT_APPLICATION", permitApplicationId);
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PERMIT_APPLICATION_ID", Operator = "=", FilterValue = permitApplicationId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            return results.FirstOrDefault() as DRILLING_PERMIT_APPLICATION;
        }

        private async Task<ENVIRONMENTAL_PERMIT_APPLICATION?> GetEnvironmentalApplicationAsync(string permitApplicationId)
        {
            var repo = await CreateRepositoryAsync<ENVIRONMENTAL_PERMIT_APPLICATION>("ENVIRONMENTAL_PERMIT_APPLICATION", permitApplicationId);
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PERMIT_APPLICATION_ID", Operator = "=", FilterValue = permitApplicationId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            return results.FirstOrDefault() as ENVIRONMENTAL_PERMIT_APPLICATION;
        }

        private async Task<INJECTION_PERMIT_APPLICATION?> GetInjectionApplicationAsync(string permitApplicationId)
        {
            var repo = await CreateRepositoryAsync<INJECTION_PERMIT_APPLICATION>("INJECTION_PERMIT_APPLICATION", permitApplicationId);
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PERMIT_APPLICATION_ID", Operator = "=", FilterValue = permitApplicationId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            return results.FirstOrDefault() as INJECTION_PERMIT_APPLICATION;
        }

    }
}

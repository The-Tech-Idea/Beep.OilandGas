using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.PermitsAndApplications;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.PermitsAndApplications.Services
{
    /// <summary>
    /// Performs compliance checks against jurisdiction requirements and application data.
    /// </summary>
    public class PermitComplianceCheckService : PermitsServiceBase, IPermitComplianceCheckService
    {
        private readonly ILogger<PermitComplianceCheckService> _logger;

        public PermitComplianceCheckService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<PermitComplianceCheckService> logger = null,
            string connectionName = "PPDM39")
            : base(editor, commonColumnHandler, defaults, metadata, logger, connectionName)
        {
            _logger = logger;
        }

        public async Task<PermitComplianceResult> CheckComplianceAsync(string applicationId, string? configDirectory = null)
        {
            if (string.IsNullOrWhiteSpace(applicationId))
                throw new ArgumentNullException(nameof(applicationId));

            var result = new PermitComplianceResult();

            var application = await GetPermitApplicationAsync(applicationId);
            if (application == null)
            {
                result.Violations.Add("Permit application not found.");
                result.IsCompliant = false;
                result.ComplianceScore = 0m;
                return result;
            }

            var drilling = await GetDrillingApplicationAsync(application.PERMIT_APPLICATION_ID);
            var environmental = await GetEnvironmentalApplicationAsync(application.PERMIT_APPLICATION_ID);
            var injection = await GetInjectionApplicationAsync(application.PERMIT_APPLICATION_ID);
            var mitResults = await GetMitResultsAsync(injection?.INJECTION_PERMIT_APPLICATION_ID);

            ValidateCoreFields(application, result);
            ValidateTypeSpecific(application, drilling, environmental, injection, mitResults, result);
            ApplyJurisdictionRules(application, drilling, environmental, injection, mitResults, result);
            await ValidateAttachmentsAsync(applicationId, result);
            await ValidateJurisdictionRequirementsAsync(application, result);
            ApplyConfigBasedValidation(application, drilling, environmental, injection, configDirectory, result);

            result.IsCompliant = result.Violations.Count == 0;
            result.ComplianceScore = CalculateScore(result);

            _logger?.LogInformation(
                "Compliance check completed for {ApplicationId}: compliant={IsCompliant}",
                applicationId,
                result.IsCompliant);

            return result;
        }

        private void ApplyConfigBasedValidation(
            PERMIT_APPLICATION application,
            DRILLING_PERMIT_APPLICATION? drilling,
            ENVIRONMENTAL_PERMIT_APPLICATION? environmental,
            INJECTION_PERMIT_APPLICATION? injection,
            string? configDirectory,
            PermitComplianceResult result)
        {
            if (string.IsNullOrWhiteSpace(configDirectory))
                return;

            var request = new Validation.PermitValidationRequest(
                application,
                Array.Empty<APPLICATION_ATTACHMENT>(),
                Array.Empty<REQUIRED_FORM>(),
                null,
                PermitStatusTransitionRules.Normalize(application.REGULATORY_AUTHORITY),
                PermitStatusTransitionRules.Normalize(application.APPLICATION_TYPE),
                drilling,
                environmental,
                injection);
            var templateRegistry = new Forms.PermitFormTemplateRegistry();
            templateRegistry.LoadFromDirectory(configDirectory);
            var rule = new Validation.Rules.TemplateRequiredFieldsRule(templateRegistry);
            var ruleResult = rule.Evaluate(request);

            foreach (var issue in ruleResult.Issues)
            {
                result.Violations.Add(issue.Message);
            }
        }

        private void ValidateCoreFields(PERMIT_APPLICATION application, PermitComplianceResult result)
        {
            if (string.IsNullOrWhiteSpace(application.APPLICATION_TYPE))
                result.Violations.Add("Application type is required.");
            if (string.IsNullOrWhiteSpace(application.COUNTRY))
                result.Violations.Add("Country is required.");
            if (string.IsNullOrWhiteSpace(application.STATE_PROVINCE))
                result.Violations.Add("State/Province is required.");
            if (string.IsNullOrWhiteSpace(application.REGULATORY_AUTHORITY))
                result.Violations.Add("Regulatory authority is required.");
            if (string.IsNullOrWhiteSpace(application.APPLICANT_ID))
                result.Violations.Add("Applicant ID is required.");
        }

        private void ValidateTypeSpecific(
            PERMIT_APPLICATION application,
            DRILLING_PERMIT_APPLICATION? drilling,
            ENVIRONMENTAL_PERMIT_APPLICATION? environmental,
            INJECTION_PERMIT_APPLICATION? injection,
            IReadOnlyList<MIT_RESULT> mitResults,
            PermitComplianceResult result)
        {
            var type = application.APPLICATION_TYPE?.ToUpperInvariant();

            if (type == "DRILLING")
            {
                if (drilling == null)
                {
                    result.Violations.Add("Drilling permit details are required.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(drilling.WELL_UWI) && string.IsNullOrWhiteSpace(drilling.LEGAL_DESCRIPTION))
                    result.Violations.Add("Well UWI or legal description is required.");
                if (string.IsNullOrWhiteSpace(drilling.TARGET_FORMATION))
                    result.Violations.Add("Target formation is required.");
                if (!drilling.PROPOSED_DEPTH.HasValue || drilling.PROPOSED_DEPTH <= 0)
                    result.Violations.Add("Proposed depth is required.");
                if (string.IsNullOrWhiteSpace(drilling.DRILLING_METHOD))
                    result.Violations.Add("Drilling method is required.");
                if (string.IsNullOrWhiteSpace(drilling.SURFACE_OWNER_NOTIFIED_IND))
                    result.Warnings.Add("Surface owner notification is not recorded.");
            }
            else if (type == "ENVIRONMENTAL")
            {
                if (environmental == null)
                {
                    result.Violations.Add("Environmental permit details are required.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(environmental.ENVIRONMENTAL_PERMIT_TYPE))
                    result.Violations.Add("Environmental permit type is required.");
                if (string.IsNullOrWhiteSpace(environmental.WASTE_TYPE))
                    result.Violations.Add("Waste type is required.");
                if (string.IsNullOrWhiteSpace(environmental.DISPOSAL_METHOD))
                    result.Violations.Add("Disposal method is required.");
                if (string.IsNullOrWhiteSpace(environmental.MONITORING_PLAN))
                    result.Warnings.Add("Monitoring plan is not recorded.");
            }
            else if (type == "INJECTION")
            {
                if (injection == null)
                {
                    result.Violations.Add("Injection permit details are required.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(injection.INJECTION_TYPE))
                    result.Violations.Add("Injection type is required.");
                if (string.IsNullOrWhiteSpace(injection.INJECTION_ZONE))
                    result.Violations.Add("Injection zone is required.");
                if (string.IsNullOrWhiteSpace(injection.INJECTION_FLUID))
                    result.Violations.Add("Injection fluid is required.");

                if (injection.IS_CO2_STORAGE_IND == "Y" && string.IsNullOrWhiteSpace(injection.MONITORING_REQUIREMENTS))
                    result.Violations.Add("Monitoring requirements are required for CO2 storage.");

                if (mitResults.Count == 0)
                    result.Warnings.Add("No MIT results recorded for injection permit.");
            }
        }

        private void ApplyJurisdictionRules(
            PERMIT_APPLICATION application,
            DRILLING_PERMIT_APPLICATION? drilling,
            ENVIRONMENTAL_PERMIT_APPLICATION? environmental,
            INJECTION_PERMIT_APPLICATION? injection,
            IReadOnlyList<MIT_RESULT> mitResults,
            PermitComplianceResult result)
        {
            var authority = application.REGULATORY_AUTHORITY?.ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(authority))
                return;

            switch (authority)
            {
                case "RRC":
                    ApplyRrcRules(application, drilling, environmental, injection, mitResults, result);
                    break;
                case "TCEQ":
                    ApplyTceqRules(application, environmental, injection, result);
                    break;
                case "AER":
                    ApplyAerRules(application, drilling, environmental, result);
                    break;
            }
        }

        private void ApplyRrcRules(
            PERMIT_APPLICATION application,
            DRILLING_PERMIT_APPLICATION? drilling,
            ENVIRONMENTAL_PERMIT_APPLICATION? environmental,
            INJECTION_PERMIT_APPLICATION? injection,
            IReadOnlyList<MIT_RESULT> mitResults,
            PermitComplianceResult result)
        {
            var type = application.APPLICATION_TYPE?.ToUpperInvariant();
            if (type == "DRILLING" && drilling != null)
            {
                if (string.IsNullOrWhiteSpace(drilling.SURFACE_OWNER_NOTIFIED_IND))
                    result.Violations.Add("RRC drilling requires surface owner notification.");
                if (string.IsNullOrWhiteSpace(drilling.LEGAL_DESCRIPTION) && string.IsNullOrWhiteSpace(drilling.WELL_UWI))
                    result.Violations.Add("RRC drilling requires legal description or well UWI.");
            }

            if (type == "ENVIRONMENTAL" && environmental != null)
            {
                if (string.IsNullOrWhiteSpace(environmental.DISPOSAL_METHOD))
                    result.Violations.Add("RRC environmental permits require disposal method.");
                if (string.IsNullOrWhiteSpace(environmental.MONITORING_PLAN))
                    result.Warnings.Add("RRC environmental permits recommend monitoring plan.");
            }

            if (type == "INJECTION" && injection != null)
            {
                if (string.IsNullOrWhiteSpace(injection.INJECTION_ZONE))
                    result.Violations.Add("RRC injection requires injection zone.");
                if (mitResults.Count == 0)
                    result.Violations.Add("RRC injection requires MIT results.");
            }
        }

        private void ApplyTceqRules(
            PERMIT_APPLICATION application,
            ENVIRONMENTAL_PERMIT_APPLICATION? environmental,
            INJECTION_PERMIT_APPLICATION? injection,
            PermitComplianceResult result)
        {
            var type = application.APPLICATION_TYPE?.ToUpperInvariant();
            if (type == "ENVIRONMENTAL" && environmental != null)
            {
                if (string.IsNullOrWhiteSpace(environmental.WASTE_TYPE))
                    result.Violations.Add("TCEQ environmental permits require waste type.");
                if (string.IsNullOrWhiteSpace(environmental.FACILITY_LOCATION))
                    result.Warnings.Add("TCEQ environmental permits recommend facility location.");
                if (string.IsNullOrWhiteSpace(environmental.MONITORING_PLAN))
                    result.Violations.Add("TCEQ environmental permits require monitoring plan.");
            }

            if (type == "INJECTION" && injection != null)
            {
                if (string.IsNullOrWhiteSpace(injection.INJECTION_FLUID))
                    result.Violations.Add("TCEQ injection permits require injection fluid.");
                if (string.IsNullOrWhiteSpace(injection.MONITORING_REQUIREMENTS))
                    result.Violations.Add("TCEQ injection permits require monitoring requirements.");
            }
        }

        private void ApplyAerRules(
            PERMIT_APPLICATION application,
            DRILLING_PERMIT_APPLICATION? drilling,
            ENVIRONMENTAL_PERMIT_APPLICATION? environmental,
            PermitComplianceResult result)
        {
            var type = application.APPLICATION_TYPE?.ToUpperInvariant();
            if (type == "DRILLING" && drilling != null)
            {
                if (string.IsNullOrWhiteSpace(drilling.SURFACE_OWNER_NOTIFIED_IND))
                    result.Violations.Add("AER drilling requires surface owner consultation.");
                if (string.IsNullOrWhiteSpace(drilling.ENVIRONMENTAL_ASSESSMENT_REQUIRED_IND) &&
                    string.IsNullOrWhiteSpace(drilling.ENVIRONMENTAL_ASSESSMENT_REFERENCE))
                    result.Warnings.Add("AER drilling recommends environmental assessment reference.");
            }

            if (type == "ENVIRONMENTAL" && environmental != null)
            {
                if (string.IsNullOrWhiteSpace(environmental.ENVIRONMENTAL_IMPACT))
                    result.Warnings.Add("AER environmental permits recommend impact summary.");
            }
        }

        private async Task ValidateAttachmentsAsync(string applicationId, PermitComplianceResult result)
        {
            var repo = await CreateRepositoryAsync<APPLICATION_ATTACHMENT>("APPLICATION_ATTACHMENT");
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PERMIT_APPLICATION_ID", Operator = "=", FilterValue = applicationId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var attachments = await repo.GetAsync(filters);
            if (attachments == null || !attachments.Any())
                result.Warnings.Add("No attachments found for this application.");
        }

        private async Task ValidateJurisdictionRequirementsAsync(PERMIT_APPLICATION application, PermitComplianceResult result)
        {
            var repo = await CreateRepositoryAsync<JURISDICTION_REQUIREMENTS>("JURISDICTION_REQUIREMENTS");
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "COUNTRY", Operator = "=", FilterValue = application.COUNTRY },
                new AppFilter { FieldName = "STATE_PROVINCE", Operator = "=", FilterValue = application.STATE_PROVINCE },
                new AppFilter { FieldName = "REGULATORY_AUTHORITY", Operator = "=", FilterValue = application.REGULATORY_AUTHORITY },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var requirements = (await repo.GetAsync(filters))
                .Select(r => r as JURISDICTION_REQUIREMENTS)
                .FirstOrDefault();

            if (requirements == null)
            {
                result.Warnings.Add("No jurisdiction requirements configured.");
                return;
            }

            if (string.IsNullOrWhiteSpace(requirements.REQUIREMENTS_DESCRIPTION))
                result.Warnings.Add("Jurisdiction requirements description is empty.");
        }

        private decimal CalculateScore(PermitComplianceResult result)
        {
            var score = 100m - (result.Violations.Count * 20m) - (result.Warnings.Count * 5m);
            if (score < 0m)
                score = 0m;
            return score;
        }

        private async Task<PERMIT_APPLICATION?> GetPermitApplicationAsync(string applicationId)
        {
            var repo = await CreateRepositoryAsync<PERMIT_APPLICATION>("PERMIT_APPLICATION");
            return await repo.GetByIdAsync(applicationId) as PERMIT_APPLICATION;
        }

        private async Task<DRILLING_PERMIT_APPLICATION?> GetDrillingApplicationAsync(string applicationId)
        {
            var repo = await CreateRepositoryAsync<DRILLING_PERMIT_APPLICATION>("DRILLING_PERMIT_APPLICATION");
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PERMIT_APPLICATION_ID", Operator = "=", FilterValue = applicationId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            return results.Select(r => r as DRILLING_PERMIT_APPLICATION).FirstOrDefault();
        }

        private async Task<ENVIRONMENTAL_PERMIT_APPLICATION?> GetEnvironmentalApplicationAsync(string applicationId)
        {
            var repo = await CreateRepositoryAsync<ENVIRONMENTAL_PERMIT_APPLICATION>("ENVIRONMENTAL_PERMIT_APPLICATION");
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PERMIT_APPLICATION_ID", Operator = "=", FilterValue = applicationId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            return results.Select(r => r as ENVIRONMENTAL_PERMIT_APPLICATION).FirstOrDefault();
        }

        private async Task<INJECTION_PERMIT_APPLICATION?> GetInjectionApplicationAsync(string applicationId)
        {
            var repo = await CreateRepositoryAsync<INJECTION_PERMIT_APPLICATION>("INJECTION_PERMIT_APPLICATION");
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PERMIT_APPLICATION_ID", Operator = "=", FilterValue = applicationId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            return results.Select(r => r as INJECTION_PERMIT_APPLICATION).FirstOrDefault();
        }

        private async Task<IReadOnlyList<MIT_RESULT>> GetMitResultsAsync(string injectionApplicationId)
        {
            if (string.IsNullOrWhiteSpace(injectionApplicationId))
                return Array.Empty<MIT_RESULT>();

            var repo = await CreateRepositoryAsync<MIT_RESULT>("MIT_RESULT");
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "INJECTION_PERMIT_APPLICATION_ID", Operator = "=", FilterValue = injectionApplicationId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            return results
                .Select(r => r as MIT_RESULT)
                .Where(r => r != null)
                .ToList();
        }

    }
}

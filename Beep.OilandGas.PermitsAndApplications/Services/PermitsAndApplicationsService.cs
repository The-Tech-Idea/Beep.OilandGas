using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.PermitsAndApplications;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.DataManagement;
using TheTechIdea.Beep.DataBase;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.PermitsAndApplications.Services
{
    /// <summary>
    /// Service implementation for permit and application management.
    /// Provides comprehensive permit application lifecycle management with PPDM compliance.
    /// </summary>
    public class PermitsAndApplicationsService : IPermitsAndApplicationsService
    {
        private readonly IDMEEditor _editor;
        private readonly ILogger<PermitsAndApplicationsService> _logger;
        private readonly string _connectionName;

        public PermitsAndApplicationsService(
            IDMEEditor editor,
            ILogger<PermitsAndApplicationsService> logger = null,
            string connectionName = "PPDM39")
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _logger = logger;
            _connectionName = connectionName;
        }

        /// <summary>
        /// Creates a new permit application.
        /// </summary>
        public async Task<PERMIT_APPLICATION> CreatePermitApplicationAsync(
            PERMIT_APPLICATION application,
            string userId)
        {
            try
            {
                _logger?.LogInformation("Creating permit application for user: {UserId}", userId);

                // Set audit fields
                SetAuditFields(application, userId);
                application.ACTIVE_IND = "Y";
                application.CREATED_DATE = DateTime.UtcNow;
                application.STATUS = "Draft";

                // Generate ID if not provided
                if (string.IsNullOrEmpty(application.PERMIT_APPLICATION_ID))
                {
                    application.PERMIT_APPLICATION_ID = await GeneratePermitApplicationIdAsync();
                }

                var metadata = await _editor.GetTableMetadataAsync("PERMIT_APPLICATION");
                var entityType = Type.GetType($"Beep.OilandGas.Models.Data.PermitsAndApplications.PERMIT_APPLICATION");

                var repo = new PPDMGenericRepository(
                    _editor, _editor.GetCommonColumnHandler(), _editor.GetDefaultsRepository(),
                    _editor.GetMetadataRepository(), entityType, _connectionName, "PERMIT_APPLICATION");

                await repo.InsertAsync(application, userId);

                _logger?.LogInformation("Permit application created with ID: {ApplicationId}", application.PERMIT_APPLICATION_ID);
                return application;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating permit application");
                throw;
            }
        }

        /// <summary>
        /// Updates an existing permit application.
        /// </summary>
        public async Task<PERMIT_APPLICATION> UpdatePermitApplicationAsync(
            string applicationId,
            PERMIT_APPLICATION application,
            string userId)
        {
            try
            {
                _logger?.LogInformation("Updating permit application: {ApplicationId}", applicationId);

                // Ensure the ID is set
                application.PERMIT_APPLICATION_ID = applicationId;

                // Update audit fields
                SetAuditFields(application, userId);

                var metadata = await _editor.GetTableMetadataAsync("PERMIT_APPLICATION");
                var entityType = Type.GetType($"Beep.OilandGas.Models.Data.PermitsAndApplications.PERMIT_APPLICATION");

                var repo = new PPDMGenericRepository(
                    _editor, _editor.GetCommonColumnHandler(), _editor.GetDefaultsRepository(),
                    _editor.GetMetadataRepository(), entityType, _connectionName, "PERMIT_APPLICATION");

                await repo.UpdateAsync(application, userId);

                _logger?.LogInformation("Permit application updated: {ApplicationId}", applicationId);
                return application;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error updating permit application: {ApplicationId}", applicationId);
                throw;
            }
        }

        /// <summary>
        /// Retrieves a permit application by ID.
        /// </summary>
        public async Task<PERMIT_APPLICATION> GetPermitApplicationAsync(string applicationId)
        {
            try
            {
                _logger?.LogInformation("Retrieving permit application: {ApplicationId}", applicationId);

                var metadata = await _editor.GetTableMetadataAsync("PERMIT_APPLICATION");
                var entityType = Type.GetType($"Beep.OilandGas.Models.Data.PermitsAndApplications.PERMIT_APPLICATION");

                var repo = new PPDMGenericRepository(
                    _editor, _editor.GetCommonColumnHandler(), _editor.GetDefaultsRepository(),
                    _editor.GetMetadataRepository(), entityType, _connectionName, "PERMIT_APPLICATION");

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "PERMIT_APPLICATION_ID", Operator = "=", FilterValue = applicationId },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var result = await repo.GetByIdAsync(applicationId);

                _logger?.LogInformation("Permit application retrieved: {ApplicationId}", applicationId);
                return result as PERMIT_APPLICATION;
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

                var metadata = await _editor.GetTableMetadataAsync("PERMIT_APPLICATION");
                var entityType = Type.GetType($"Beep.OilandGas.Models.Data.PermitsAndApplications.PERMIT_APPLICATION");

                var repo = new PPDMGenericRepository(
                    _editor, _editor.GetCommonColumnHandler(), _editor.GetDefaultsRepository(),
                    _editor.GetMetadataRepository(), entityType, _connectionName, "PERMIT_APPLICATION");

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "STATUS", Operator = "=", FilterValue = status },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var results = await repo.GetAsync(filters);
                var applications = results.Select(r => r as PERMIT_APPLICATION).ToList();

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

                var metadata = await _editor.GetTableMetadataAsync("PERMIT_APPLICATION");
                var entityType = Type.GetType($"Beep.OilandGas.Models.Data.PermitsAndApplications.PERMIT_APPLICATION");

                var repo = new PPDMGenericRepository(
                    _editor, _editor.GetCommonColumnHandler(), _editor.GetDefaultsRepository(),
                    _editor.GetMetadataRepository(), entityType, _connectionName, "PERMIT_APPLICATION");

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "REGULATORY_AUTHORITY", Operator = "=", FilterValue = authority },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var results = await repo.GetAsync(filters);
                var applications = results.Select(r => r as PERMIT_APPLICATION).ToList();

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
        /// Submits a permit application for review.
        /// </summary>
        public async Task<PERMIT_APPLICATION> SubmitPermitApplicationAsync(
            string applicationId,
            string userId)
        {
            try
            {
                _logger?.LogInformation("Submitting permit application: {ApplicationId}", applicationId);

                var application = await GetPermitApplicationAsync(applicationId);
                if (application == null)
                    throw new InvalidOperationException($"Permit application not found: {applicationId}");

                // Validate before submission
                var validation = await ValidatePermitApplicationAsync(applicationId);
                if (!validation.IsValid)
                    throw new InvalidOperationException($"Cannot submit invalid application. Errors: {string.Join(", ", validation.Errors)}");

                application.STATUS = "Submitted";
                application.SUBMITTED_DATE = DateTime.UtcNow;
                application.SUBMISSION_COMPLETE_IND = "Y";

                var updated = await UpdatePermitApplicationAsync(applicationId, application, userId);

                _logger?.LogInformation("Permit application submitted: {ApplicationId}", applicationId);
                return updated;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error submitting permit application: {ApplicationId}", applicationId);
                throw;
            }
        }

        /// <summary>
        /// Approves or rejects a permit application.
        /// </summary>
        public async Task<PERMIT_APPLICATION> ProcessPermitDecisionAsync(
            string applicationId,
            string decision,
            string decisionRemarks,
            string userId)
        {
            try
            {
                _logger?.LogInformation("Processing permit decision for application: {ApplicationId}", applicationId);

                var application = await GetPermitApplicationAsync(applicationId);
                if (application == null)
                    throw new InvalidOperationException($"Permit application not found: {applicationId}");

                application.DECISION = decision;
                application.DECISION_DATE = DateTime.UtcNow;
                application.REMARKS = decisionRemarks;

                if (decision == "Approved")
                {
                    application.STATUS = "Approved";
                    application.EFFECTIVE_DATE = DateTime.UtcNow;
                }
                else if (decision == "Rejected")
                {
                    application.STATUS = "Rejected";
                }

                var updated = await UpdatePermitApplicationAsync(applicationId, application, userId);

                _logger?.LogInformation("Permit decision processed: {ApplicationId} - {Decision}", applicationId, decision);
                return updated;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error processing permit decision for application: {ApplicationId}", applicationId);
                throw;
            }
        }

        /// <summary>
        /// Adds an attachment to a permit application.
        /// </summary>
        public async Task<APPLICATION_ATTACHMENT> AddApplicationAttachmentAsync(
            string applicationId,
            APPLICATION_ATTACHMENT attachment,
            string userId)
        {
            try
            {
                _logger?.LogInformation("Adding attachment to application: {ApplicationId}", applicationId);

                // Set audit fields
                SetAuditFields(attachment, userId);
                attachment.PERMIT_APPLICATION_ID = applicationId;
                attachment.ACTIVE_IND = "Y";
                attachment.UPLOAD_DATE = DateTime.UtcNow;

                // Generate ID if not provided
                if (string.IsNullOrEmpty(attachment.APPLICATION_ATTACHMENT_ID))
                {
                    attachment.APPLICATION_ATTACHMENT_ID = await GenerateAttachmentIdAsync();
                }

                var metadata = await _editor.GetTableMetadataAsync("APPLICATION_ATTACHMENT");
                var entityType = Type.GetType($"Beep.OilandGas.Models.Data.PermitsAndApplications.APPLICATION_ATTACHMENT");

                var repo = new PPDMGenericRepository(
                    _editor, _editor.GetCommonColumnHandler(), _editor.GetDefaultsRepository(),
                    _editor.GetMetadataRepository(), entityType, _connectionName, "APPLICATION_ATTACHMENT");

                await repo.InsertAsync(attachment, userId);

                _logger?.LogInformation("Attachment added to application: {ApplicationId}, AttachmentId: {AttachmentId}",
                    applicationId, attachment.APPLICATION_ATTACHMENT_ID);
                return attachment;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error adding attachment to application: {ApplicationId}", applicationId);
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

                var metadata = await _editor.GetTableMetadataAsync("APPLICATION_ATTACHMENT");
                var entityType = Type.GetType($"Beep.OilandGas.Models.Data.PermitsAndApplications.APPLICATION_ATTACHMENT");

                var repo = new PPDMGenericRepository(
                    _editor, _editor.GetCommonColumnHandler(), _editor.GetDefaultsRepository(),
                    _editor.GetMetadataRepository(), entityType, _connectionName, "APPLICATION_ATTACHMENT");

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "PERMIT_APPLICATION_ID", Operator = "=", FilterValue = applicationId },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var results = await repo.GetAsync(filters);
                var attachments = results.Select(r => r as APPLICATION_ATTACHMENT).ToList();

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
        /// Creates a drilling permit application.
        /// </summary>
        public async Task<DRILLING_PERMIT_APPLICATION> CreateDrillingPermitApplicationAsync(
            DRILLING_PERMIT_APPLICATION drillingApplication,
            string userId)
        {
            try
            {
                _logger?.LogInformation("Creating drilling permit application for user: {UserId}", userId);

                // Set audit fields
                SetAuditFields(drillingApplication, userId);
                drillingApplication.ACTIVE_IND = "Y";

                // Generate ID if not provided
                if (string.IsNullOrEmpty(drillingApplication.DRILLING_PERMIT_APPLICATION_ID))
                {
                    drillingApplication.DRILLING_PERMIT_APPLICATION_ID = await GenerateDrillingApplicationIdAsync();
                }

                var metadata = await _editor.GetTableMetadataAsync("DRILLING_PERMIT_APPLICATION");
                var entityType = Type.GetType($"Beep.OilandGas.Models.Data.PermitsAndApplications.DRILLING_PERMIT_APPLICATION");

                var repo = new PPDMGenericRepository(
                    _editor, _editor.GetCommonColumnHandler(), _editor.GetDefaultsRepository(),
                    _editor.GetMetadataRepository(), entityType, _connectionName, "DRILLING_PERMIT_APPLICATION");

                await repo.InsertAsync(drillingApplication, userId);

                _logger?.LogInformation("Drilling permit application created with ID: {ApplicationId}", drillingApplication.DRILLING_PERMIT_APPLICATION_ID);
                return drillingApplication;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating drilling permit application");
                throw;
            }
        }

        /// <summary>
        /// Creates an environmental permit application.
        /// </summary>
        public async Task<ENVIRONMENTAL_PERMIT_APPLICATION> CreateEnvironmentalPermitApplicationAsync(
            ENVIRONMENTAL_PERMIT_APPLICATION environmentalApplication,
            string userId)
        {
            try
            {
                _logger?.LogInformation("Creating environmental permit application for user: {UserId}", userId);

                // Set audit fields
                SetAuditFields(environmentalApplication, userId);
                environmentalApplication.ACTIVE_IND = "Y";

                // Generate ID if not provided
                if (string.IsNullOrEmpty(environmentalApplication.ENVIRONMENTAL_PERMIT_APPLICATION_ID))
                {
                    environmentalApplication.ENVIRONMENTAL_PERMIT_APPLICATION_ID = await GenerateEnvironmentalApplicationIdAsync();
                }

                var metadata = await _editor.GetTableMetadataAsync("ENVIRONMENTAL_PERMIT_APPLICATION");
                var entityType = Type.GetType($"Beep.OilandGas.Models.Data.PermitsAndApplications.ENVIRONMENTAL_PERMIT_APPLICATION");

                var repo = new PPDMGenericRepository(
                    _editor, _editor.GetCommonColumnHandler(), _editor.GetDefaultsRepository(),
                    _editor.GetMetadataRepository(), entityType, _connectionName, "ENVIRONMENTAL_PERMIT_APPLICATION");

                await repo.InsertAsync(environmentalApplication, userId);

                _logger?.LogInformation("Environmental permit application created with ID: {ApplicationId}", environmentalApplication.ENVIRONMENTAL_PERMIT_APPLICATION_ID);
                return environmentalApplication;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating environmental permit application");
                throw;
            }
        }

        /// <summary>
        /// Creates an injection permit application.
        /// </summary>
        public async Task<INJECTION_PERMIT_APPLICATION> CreateInjectionPermitApplicationAsync(
            INJECTION_PERMIT_APPLICATION injectionApplication,
            string userId)
        {
            try
            {
                _logger?.LogInformation("Creating injection permit application for user: {UserId}", userId);

                // Set audit fields
                SetAuditFields(injectionApplication, userId);
                injectionApplication.ACTIVE_IND = "Y";

                // Generate ID if not provided
                if (string.IsNullOrEmpty(injectionApplication.INJECTION_PERMIT_APPLICATION_ID))
                {
                    injectionApplication.INJECTION_PERMIT_APPLICATION_ID = await GenerateInjectionApplicationIdAsync();
                }

                var metadata = await _editor.GetTableMetadataAsync("INJECTION_PERMIT_APPLICATION");
                var entityType = Type.GetType($"Beep.OilandGas.Models.Data.PermitsAndApplications.INJECTION_PERMIT_APPLICATION");

                var repo = new PPDMGenericRepository(
                    _editor, _editor.GetCommonColumnHandler(), _editor.GetDefaultsRepository(),
                    _editor.GetMetadataRepository(), entityType, _connectionName, "INJECTION_PERMIT_APPLICATION");

                await repo.InsertAsync(injectionApplication, userId);

                _logger?.LogInformation("Injection permit application created with ID: {ApplicationId}", injectionApplication.INJECTION_PERMIT_APPLICATION_ID);
                return injectionApplication;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating injection permit application");
                throw;
            }
        }

        /// <summary>
        /// Validates a permit application for completeness.
        /// </summary>
        public async Task<PermitValidationResult> ValidatePermitApplicationAsync(string applicationId)
        {
            try
            {
                _logger?.LogInformation("Validating permit application: {ApplicationId}", applicationId);

                var result = new PermitValidationResult();
                var application = await GetPermitApplicationAsync(applicationId);

                if (application == null)
                {
                    result.Errors.Add("Permit application not found");
                    result.IsValid = false;
                    return result;
                }

                // Basic field validation
                if (string.IsNullOrEmpty(application.APPLICATION_TYPE))
                    result.Errors.Add("Application type is required");

                if (string.IsNullOrEmpty(application.COUNTRY))
                    result.Errors.Add("Country is required");

                if (string.IsNullOrEmpty(application.STATE_PROVINCE))
                    result.Errors.Add("State/Province is required");

                if (string.IsNullOrEmpty(application.REGULATORY_AUTHORITY))
                    result.Errors.Add("Regulatory authority is required");

                if (string.IsNullOrEmpty(application.APPLICANT_ID))
                    result.Errors.Add("Applicant ID is required");

                // Check for required attachments
                var attachments = await GetApplicationAttachmentsAsync(applicationId);
                if (attachments.Count == 0)
                {
                    result.Warnings.Add("No attachments found - may require supporting documents");
                }

                // Calculate completion percentage
                var requiredFields = 6; // Adjust based on actual requirements
                var completedFields = requiredFields - result.Errors.Count;
                result.CompletionPercentage = (decimal)completedFields / requiredFields * 100;

                result.IsValid = result.Errors.Count == 0;

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
        public async Task<JURISDICTION_REQUIREMENTS> GetJurisdictionRequirementsAsync(
            string country,
            string stateProvince,
            string authority)
        {
            try
            {
                _logger?.LogInformation("Retrieving jurisdiction requirements for {Country}, {State}, {Authority}",
                    country, stateProvince, authority);

                var metadata = await _editor.GetTableMetadataAsync("JURISDICTION_REQUIREMENTS");
                var entityType = Type.GetType($"Beep.OilandGas.Models.Data.PermitsAndApplications.JURISDICTION_REQUIREMENTS");

                var repo = new PPDMGenericRepository(
                    _editor, _editor.GetCommonColumnHandler(), _editor.GetDefaultsRepository(),
                    _editor.GetMetadataRepository(), entityType, _connectionName, "JURISDICTION_REQUIREMENTS");

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "COUNTRY", Operator = "=", FilterValue = country },
                    new AppFilter { FieldName = "STATE_PROVINCE", Operator = "=", FilterValue = stateProvince },
                    new AppFilter { FieldName = "REGULATORY_AUTHORITY", Operator = "=", FilterValue = authority },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var results = await repo.GetAsync(filters);
                var requirements = results.FirstOrDefault() as JURISDICTION_REQUIREMENTS;

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
        /// Generates required forms for a permit application.
        /// </summary>
        public async Task<List<REQUIRED_FORM>> GenerateRequiredFormsAsync(
            string applicationId,
            string userId)
        {
            try
            {
                _logger?.LogInformation("Generating required forms for application: {ApplicationId}", applicationId);

                var application = await GetPermitApplicationAsync(applicationId);
                if (application == null)
                    throw new InvalidOperationException($"Permit application not found: {applicationId}");

                // Get jurisdiction requirements
                var requirements = await GetJurisdictionRequirementsAsync(
                    application.COUNTRY,
                    application.STATE_PROVINCE,
                    application.REGULATORY_AUTHORITY);

                // Generate forms based on requirements and application type
                var forms = await GenerateFormsForApplicationTypeAsync(application, requirements, userId);

                _logger?.LogInformation("Generated {Count} required forms for application: {ApplicationId}", forms.Count, applicationId);
                return forms;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating required forms for application: {ApplicationId}", applicationId);
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
                    throw new InvalidOperationException($"Permit application not found: {applicationId}");

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

        // Helper methods

        private async Task<string> GeneratePermitApplicationIdAsync()
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            return $"PA-{timestamp}";
        }

        private async Task<string> GenerateAttachmentIdAsync()
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
            return $"ATT-{timestamp}";
        }

        private async Task<string> GenerateDrillingApplicationIdAsync()
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            return $"DPA-{timestamp}";
        }

        private async Task<string> GenerateEnvironmentalApplicationIdAsync()
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            return $"EPA-{timestamp}";
        }

        private async Task<string> GenerateInjectionApplicationIdAsync()
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            return $"IPA-{timestamp}";
        }

        private async Task<List<REQUIRED_FORM>> GenerateFormsForApplicationTypeAsync(
            PERMIT_APPLICATION application,
            JURISDICTION_REQUIREMENTS requirements,
            string userId)
        {
            var forms = new List<REQUIRED_FORM>();

            // Base forms for all applications
            forms.Add(await CreateRequiredFormAsync("PERMIT_APPLICATION_FORM", "General Permit Application Form", userId));

            // Type-specific forms
            switch (application.APPLICATION_TYPE)
            {
                case "Drilling":
                    forms.Add(await CreateRequiredFormAsync("DRILLING_PLAN", "Well Drilling Plan", userId));
                    forms.Add(await CreateRequiredFormAsync("GEOLOGICAL_REPORT", "Geological Survey Report", userId));
                    break;
                case "Environmental":
                    forms.Add(await CreateRequiredFormAsync("EIA_REPORT", "Environmental Impact Assessment", userId));
                    forms.Add(await CreateRequiredFormAsync("MITIGATION_PLAN", "Environmental Mitigation Plan", userId));
                    break;
                case "Injection":
                    forms.Add(await CreateRequiredFormAsync("INJECTION_PLAN", "Fluid Injection Plan", userId));
                    forms.Add(await CreateRequiredFormAsync("DISPOSAL_REPORT", "Waste Disposal Report", userId));
                    break;
            }

            return forms;
        }

        private async Task<REQUIRED_FORM> CreateRequiredFormAsync(string formCode, string formName, string userId)
        {
            var form = new REQUIRED_FORM
            {
                FORM_CODE = formCode,
                FORM_NAME = formName,
                REQUIRED_IND = "Y",
                ACTIVE_IND = "Y"
            };

            SetAuditFields(form, userId);
            return form;
        }

        private decimal GetJurisdictionFeeMultiplier(string authority)
        {
            // Simplified fee multipliers by jurisdiction
            return authority switch
            {
                "RRC" => 1.0m,    // Texas RRC - base rate
                "TCEQ" => 1.2m,   // Texas TCEQ - higher for environmental
                "EPA" => 1.5m,    // Federal EPA - highest fees
                "AER" => 1.1m,    // Alberta Energy Regulator
                "BCER" => 1.1m,   // BC Energy Regulator
                "CNH" => 1.3m,    // Mexico CNH
                _ => 1.0m         // Default
            };
        }

        private decimal GetApplicationTypeFeeMultiplier(string applicationType)
        {
            return applicationType switch
            {
                "Drilling" => 1.5m,      // Drilling permits are most complex
                "Environmental" => 1.3m, // Environmental assessments are detailed
                "Injection" => 1.2m,     // Injection permits require monitoring
                _ => 1.0m                // Default
            };
        }

        private void SetAuditFields(dynamic entity, string userId)
        {
            if (string.IsNullOrEmpty(entity.ROW_CREATED_BY))
            {
                entity.ROW_CREATED_BY = userId;
                entity.ROW_CREATED_DATE = DateTime.UtcNow;
            }

            entity.ROW_CHANGED_BY = userId;
            entity.ROW_CHANGED_DATE = DateTime.UtcNow;
        }
    }
}
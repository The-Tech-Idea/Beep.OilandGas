using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.PermitsAndApplications;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.DataBase;
using Microsoft.Extensions.Logging;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Repositories;
using TheTechIdea.Beep.Report;
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
        private readonly ILogger<PermitApplicationWorkflowService> _logger;

        public PermitApplicationWorkflowService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<PermitApplicationWorkflowService> logger = null,
            string connectionName = "PPDM39")
            : base(editor, commonColumnHandler, defaults, metadata, logger, connectionName)
        {
            _logger = logger;
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

                var repo = await CreateRepositoryAsync<PERMIT_APPLICATION>("PERMIT_APPLICATION");

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

                var repo = await CreateRepositoryAsync<PERMIT_APPLICATION>("PERMIT_APPLICATION");

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

                var repo = await CreateRepositoryAsync<PERMIT_APPLICATION>("PERMIT_APPLICATION");

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

                var repo = await CreateRepositoryAsync<PERMIT_APPLICATION>("PERMIT_APPLICATION");

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

                var repo = await CreateRepositoryAsync<PERMIT_APPLICATION>("PERMIT_APPLICATION");

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

                ValidateStatusTransition(application.STATUS, "SUBMITTED");
                application.STATUS = "SUBMITTED";
                application.SUBMITTED_DATE = DateTime.UtcNow;
                application.SUBMISSION_COMPLETE_IND = "Y";

                var updated = await UpdatePermitApplicationAsync(applicationId, application, userId);
                await AddStatusHistoryAsync(applicationId, application.STATUS, "Application submitted", userId);

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
                    ValidateStatusTransition(application.STATUS, "APPROVED");
                    application.STATUS = "Approved";
                    application.EFFECTIVE_DATE = DateTime.UtcNow;
                }
                else if (decision == "Rejected")
                {
                    ValidateStatusTransition(application.STATUS, "REJECTED");
                    application.STATUS = "Rejected";
                }

                var updated = await UpdatePermitApplicationAsync(applicationId, application, userId);
                await AddStatusHistoryAsync(applicationId, application.STATUS, decisionRemarks, userId);

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

                var repo = await CreateRepositoryAsync<APPLICATION_ATTACHMENT>("APPLICATION_ATTACHMENT");

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

                var repo = await CreateRepositoryAsync<APPLICATION_ATTACHMENT>("APPLICATION_ATTACHMENT");

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

                var repo = await CreateRepositoryAsync<DRILLING_PERMIT_APPLICATION>("DRILLING_PERMIT_APPLICATION");

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

                var repo = await CreateRepositoryAsync<ENVIRONMENTAL_PERMIT_APPLICATION>("ENVIRONMENTAL_PERMIT_APPLICATION");

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

                var repo = await CreateRepositoryAsync<INJECTION_PERMIT_APPLICATION>("INJECTION_PERMIT_APPLICATION");

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
        public async Task<JURISDICTION_REQUIREMENTS> GetJurisdictionRequirementsAsync(
            string country,
            string stateProvince,
            string authority)
        {
            try
            {
                _logger?.LogInformation("Retrieving jurisdiction requirements for {Country}, {State}, {Authority}",
                    country, stateProvince, authority);

                var repo = await CreateRepositoryAsync<JURISDICTION_REQUIREMENTS>("JURISDICTION_REQUIREMENTS");

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
                    throw new InvalidOperationException($"Permit application not found: {applicationId}");

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
                    NormalizeAuthority(application.REGULATORY_AUTHORITY),
                    applicationType,
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

        /// <summary>
        /// Generates and stores form payload files as attachments for an application.
        /// </summary>
        public async Task<IReadOnlyList<APPLICATION_ATTACHMENT>> GenerateFormPayloadAttachmentsAsync(
            string applicationId,
            string userId,
            string outputDirectory,
            string? configDirectory = null)
        {
            try
            {
                _logger?.LogInformation("Generating form payload attachments for application: {ApplicationId}", applicationId);

                if (string.IsNullOrWhiteSpace(outputDirectory))
                    throw new ArgumentException("Output directory is required.", nameof(outputDirectory));

                var application = await GetPermitApplicationAsync(applicationId);
                if (application == null)
                    throw new InvalidOperationException($"Permit application not found: {applicationId}");

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

                var authority = NormalizeAuthority(application.REGULATORY_AUTHORITY);
                var builder = new PermitFormPayloadBuilder();
                var payloads = builder.BuildPayloads(context, authority, applicationType, configDirectory);

                var storagePath = Path.Combine(outputDirectory, applicationId);
                Directory.CreateDirectory(storagePath);

                var attachments = new List<APPLICATION_ATTACHMENT>();

                foreach (var payload in payloads)
                {
                    var fileName = $"{applicationId}_{authority}_{applicationType}_{payload.FormCode}_{DateTime.UtcNow:yyyyMMddHHmmssfff}.json";
                    var filePath = Path.Combine(storagePath, fileName);
                    var json = System.Text.Json.JsonSerializer.Serialize(payload, new System.Text.Json.JsonSerializerOptions
                    {
                        WriteIndented = true
                    });

                    await File.WriteAllTextAsync(filePath, json);

                    var fileInfo = new FileInfo(filePath);
                    var attachment = new APPLICATION_ATTACHMENT
                    {
                        FILE_NAME = fileName,
                        FILE_TYPE = "application/json",
                        FILE_SIZE = fileInfo.Length,
                        DOCUMENT_TYPE = payload.FormCode,
                        DESCRIPTION = $"Generated form payload for {payload.FormName}"
                    };

                    attachments.Add(await AddApplicationAttachmentAsync(applicationId, attachment, userId));
                }

                _logger?.LogInformation("Generated {Count} form payload attachments for application: {ApplicationId}", attachments.Count, applicationId);
                return attachments;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating form payload attachments for application: {ApplicationId}", applicationId);
                throw;
            }
        }

        /// <summary>
        /// Generates JSON and HTML packets for forms and stores them as attachments.
        /// </summary>
        public async Task<IReadOnlyList<APPLICATION_ATTACHMENT>> GenerateFormPacketAttachmentsAsync(
            string applicationId,
            string userId,
            string outputDirectory,
            string? configDirectory = null)
        {
            try
            {
                _logger?.LogInformation("Generating form packet attachments for application: {ApplicationId}", applicationId);

                if (string.IsNullOrWhiteSpace(outputDirectory))
                    throw new ArgumentException("Output directory is required.", nameof(outputDirectory));

                var application = await GetPermitApplicationAsync(applicationId);
                if (application == null)
                    throw new InvalidOperationException($"Permit application not found: {applicationId}");

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

                var authority = NormalizeAuthority(application.REGULATORY_AUTHORITY);
                var builder = new PermitFormPayloadBuilder();
                var payloads = builder.BuildPayloads(context, authority, applicationType, configDirectory);

                var writer = new PermitFormPacketWriter();
                var files = writer.WritePackets(payloads, outputDirectory, applicationId, authority, applicationType);

                var attachments = new List<APPLICATION_ATTACHMENT>();
                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file.Path);
                    var attachment = new APPLICATION_ATTACHMENT
                    {
                        FILE_NAME = Path.GetFileName(file.Path),
                        FILE_TYPE = file.ContentType,
                        FILE_SIZE = fileInfo.Length,
                        DOCUMENT_TYPE = file.FormCode,
                        DESCRIPTION = file.Description
                    };

                    attachments.Add(await AddApplicationAttachmentAsync(applicationId, attachment, userId));
                }

                _logger?.LogInformation("Generated {Count} form packet attachments for application: {ApplicationId}", attachments.Count, applicationId);
                return attachments;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating form packet attachments for application: {ApplicationId}", applicationId);
                throw;
            }
        }

        /// <summary>
        /// Adds a business associate record for a permit application.
        /// </summary>
        public async Task<APPLIC_BA> AddApplicBusinessAssociateAsync(
            string applicationId,
            APPLIC_BA associate,
            string userId)
        {
            try
            {
                _logger?.LogInformation("Adding APPLIC_BA for application: {ApplicationId}", applicationId);

                associate.PERMIT_APPLICATION_ID = applicationId;
                associate.ACTIVE_IND = "Y";
                SetAuditFields(associate, userId);

                if (string.IsNullOrEmpty(associate.APPLIC_BA_ID))
                {
                    associate.APPLIC_BA_ID = await GenerateApplicBusinessAssociateIdAsync();
                }

                var repo = await CreateRepositoryAsync<APPLIC_BA>("APPLIC_BA");
                await repo.InsertAsync(associate, userId);

                return associate;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error adding APPLIC_BA for application: {ApplicationId}", applicationId);
                throw;
            }
        }

        /// <summary>
        /// Retrieves business associate records for a permit application.
        /// </summary>
        public async Task<List<APPLIC_BA>> GetApplicBusinessAssociatesAsync(string applicationId)
        {
            try
            {
                var repo = await CreateRepositoryAsync<APPLIC_BA>("APPLIC_BA");
                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "PERMIT_APPLICATION_ID", Operator = "=", FilterValue = applicationId },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var results = await repo.GetAsync(filters);
                return results.Select(result => result as APPLIC_BA).ToList();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving APPLIC_BA for application: {ApplicationId}", applicationId);
                throw;
            }
        }

        /// <summary>
        /// Adds a description record for a permit application.
        /// </summary>
        public async Task<APPLIC_DESC> AddApplicDescriptionAsync(
            string applicationId,
            APPLIC_DESC description,
            string userId)
        {
            try
            {
                _logger?.LogInformation("Adding APPLIC_DESC for application: {ApplicationId}", applicationId);

                description.PERMIT_APPLICATION_ID = applicationId;
                description.ACTIVE_IND = "Y";
                SetAuditFields(description, userId);

                if (string.IsNullOrEmpty(description.APPLIC_DESC_ID))
                {
                    description.APPLIC_DESC_ID = await GenerateApplicDescriptionIdAsync();
                }

                var repo = await CreateRepositoryAsync<APPLIC_DESC>("APPLIC_DESC");
                await repo.InsertAsync(description, userId);

                return description;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error adding APPLIC_DESC for application: {ApplicationId}", applicationId);
                throw;
            }
        }

        /// <summary>
        /// Retrieves description records for a permit application.
        /// </summary>
        public async Task<List<APPLIC_DESC>> GetApplicDescriptionsAsync(string applicationId)
        {
            try
            {
                var repo = await CreateRepositoryAsync<APPLIC_DESC>("APPLIC_DESC");
                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "PERMIT_APPLICATION_ID", Operator = "=", FilterValue = applicationId },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var results = await repo.GetAsync(filters);
                return results.Select(result => result as APPLIC_DESC).ToList();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving APPLIC_DESC for application: {ApplicationId}", applicationId);
                throw;
            }
        }

        /// <summary>
        /// Adds a remark record for a permit application.
        /// </summary>
        public async Task<APPLIC_REMARK> AddApplicRemarkAsync(
            string applicationId,
            APPLIC_REMARK remark,
            string userId)
        {
            try
            {
                _logger?.LogInformation("Adding APPLIC_REMARK for application: {ApplicationId}", applicationId);

                remark.PERMIT_APPLICATION_ID = applicationId;
                remark.ACTIVE_IND = "Y";
                SetAuditFields(remark, userId);

                if (string.IsNullOrEmpty(remark.APPLIC_REMARK_ID))
                {
                    remark.APPLIC_REMARK_ID = await GenerateApplicRemarkIdAsync();
                }

                var repo = await CreateRepositoryAsync<APPLIC_REMARK>("APPLIC_REMARK");
                await repo.InsertAsync(remark, userId);

                return remark;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error adding APPLIC_REMARK for application: {ApplicationId}", applicationId);
                throw;
            }
        }

        /// <summary>
        /// Retrieves remark records for a permit application.
        /// </summary>
        public async Task<List<APPLIC_REMARK>> GetApplicRemarksAsync(string applicationId)
        {
            try
            {
                var repo = await CreateRepositoryAsync<APPLIC_REMARK>("APPLIC_REMARK");
                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "PERMIT_APPLICATION_ID", Operator = "=", FilterValue = applicationId },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var results = await repo.GetAsync(filters);
                return results.Select(result => result as APPLIC_REMARK).ToList();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving APPLIC_REMARK for application: {ApplicationId}", applicationId);
                throw;
            }
        }

        /// <summary>
        /// Adds a business associate permit record.
        /// </summary>
        public async Task<BA_PERMIT> AddBusinessAssociatePermitAsync(BA_PERMIT permit, string userId)
        {
            try
            {
                _logger?.LogInformation("Adding BA_PERMIT for business associate: {BusinessAssociateId}", permit?.BUSINESS_ASSOCIATE_ID);

                permit.ACTIVE_IND = "Y";
                SetAuditFields(permit, userId);

                var repo = await CreateRepositoryAsync<BA_PERMIT>("BA_PERMIT");
                await repo.InsertAsync(permit, userId);

                return permit;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error adding BA_PERMIT");
                throw;
            }
        }

        /// <summary>
        /// Retrieves business associate permits for a business associate.
        /// </summary>
        public async Task<List<BA_PERMIT>> GetBusinessAssociatePermitsAsync(string businessAssociateId)
        {
            try
            {
                var repo = await CreateRepositoryAsync<BA_PERMIT>("BA_PERMIT");
                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "BUSINESS_ASSOCIATE_ID", Operator = "=", FilterValue = businessAssociateId },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var results = await repo.GetAsync(filters);
                return results.Select(result => result as BA_PERMIT).ToList();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving BA_PERMIT for business associate: {BusinessAssociateId}", businessAssociateId);
                throw;
            }
        }

        /// <summary>
        /// Adds a facility license record.
        /// </summary>
        public async Task<FACILITY_LICENSE> AddFacilityLicenseAsync(
            string facilityId,
            FACILITY_LICENSE license,
            string userId)
        {
            try
            {
                _logger?.LogInformation("Adding FACILITY_LICENSE for facility: {FacilityId}", facilityId);

                license.FACILITY_ID = facilityId;
                license.ACTIVE_IND = "Y";
                SetAuditFields(license, userId);

                if (string.IsNullOrEmpty(license.LICENSE_ID))
                {
                    license.LICENSE_ID = await GenerateFacilityLicenseIdAsync();
                }

                var repo = await CreateRepositoryAsync<FACILITY_LICENSE>("FACILITY_LICENSE");
                await repo.InsertAsync(license, userId);

                return license;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error adding FACILITY_LICENSE for facility: {FacilityId}", facilityId);
                throw;
            }
        }

        /// <summary>
        /// Retrieves facility licenses for a facility.
        /// </summary>
        public async Task<List<FACILITY_LICENSE>> GetFacilityLicensesAsync(string facilityId)
        {
            try
            {
                var repo = await CreateRepositoryAsync<FACILITY_LICENSE>("FACILITY_LICENSE");
                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "FACILITY_ID", Operator = "=", FilterValue = facilityId },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var results = await repo.GetAsync(filters);
                return results.Select(result => result as FACILITY_LICENSE).ToList();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving FACILITY_LICENSE for facility: {FacilityId}", facilityId);
                throw;
            }
        }

        /// <summary>
        /// Adds a well permit type record.
        /// </summary>
        public async Task<WELL_PERMIT_TYPE> AddWellPermitTypeAsync(WELL_PERMIT_TYPE permitType, string userId)
        {
            try
            {
                _logger?.LogInformation("Adding WELL_PERMIT_TYPE: {PermitType}", permitType?.PERMIT_TYPE);

                permitType.ACTIVE_IND = "Y";
                SetAuditFields(permitType, userId);

                var repo = await CreateRepositoryAsync<WELL_PERMIT_TYPE>("WELL_PERMIT_TYPE");
                await repo.InsertAsync(permitType, userId);

                return permitType;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error adding WELL_PERMIT_TYPE");
                throw;
            }
        }

        /// <summary>
        /// Retrieves well permit types, optionally filtered by granting authority.
        /// </summary>
        public async Task<List<WELL_PERMIT_TYPE>> GetWellPermitTypesAsync(string? grantedByBaId = null)
        {
            try
            {
                var repo = await CreateRepositoryAsync<WELL_PERMIT_TYPE>("WELL_PERMIT_TYPE");
                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                if (!string.IsNullOrWhiteSpace(grantedByBaId))
                {
                    filters.Add(new AppFilter { FieldName = "GRANTED_BY_BA_ID", Operator = "=", FilterValue = grantedByBaId });
                }

                var results = await repo.GetAsync(filters);
                return results.Select(result => result as WELL_PERMIT_TYPE).ToList();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving WELL_PERMIT_TYPE");
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

        private async Task<string> GenerateApplicBusinessAssociateIdAsync()
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
            return $"APBA-{timestamp}";
        }

        private async Task<string> GenerateApplicDescriptionIdAsync()
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
            return $"APDESC-{timestamp}";
        }

        private async Task<string> GenerateApplicRemarkIdAsync()
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
            return $"APREM-{timestamp}";
        }

        private async Task<string> GenerateFacilityLicenseIdAsync()
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
            return $"FL-{timestamp}";
        }

        private void ValidateStatusTransition(string? currentStatus, string? nextStatus)
        {
            var normalizedCurrent = PermitStatusTransitionRules.Normalize(currentStatus);
            var normalizedNext = PermitStatusTransitionRules.Normalize(nextStatus);

            if (PermitStatusTransitionRules.IsTransitionAllowed(normalizedCurrent, normalizedNext))
                return;

            throw new InvalidOperationException($"Invalid status transition: {normalizedCurrent} -> {normalizedNext}");
        }

        private async Task AddStatusHistoryAsync(string applicationId, string? status, string? remarks, string userId)
        {
            var repo = await CreateRepositoryAsync<PERMIT_STATUS_HISTORY>("PERMIT_STATUS_HISTORY");
            var history = new PERMIT_STATUS_HISTORY
            {
                PERMIT_STATUS_HISTORY_ID = GenerateStatusHistoryId(),
                PERMIT_APPLICATION_ID = applicationId,
                STATUS = PermitStatusTransitionRules.Normalize(status),
                STATUS_DATE = DateTime.UtcNow,
                STATUS_REMARKS = remarks,
                UPDATED_BY = userId,
                ACTIVE_IND = "Y"
            };

            SetAuditFields(history, userId);
            await repo.InsertAsync(history, userId);
        }

        private string GenerateStatusHistoryId()
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
            return $"PSH-{timestamp}";
        }

        private async Task<List<REQUIRED_FORM>> GenerateFormsForApplicationTypeAsync(
            PERMIT_APPLICATION application,
            JURISDICTION_REQUIREMENTS requirements,
            string userId)
        {
            var forms = new List<REQUIRED_FORM>();

            var registry = new PermitFormTemplateRegistry();
            var normalizedAuthority = NormalizeAuthority(application.REGULATORY_AUTHORITY);
            var normalizedType = NormalizeApplicationType(application.APPLICATION_TYPE);
            var templates = registry.GetTemplates(normalizedAuthority, normalizedType);

            foreach (var template in templates)
            {
                forms.Add(await CreateRequiredFormAsync(template.FormCode, template.FormName, userId));
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
            return NormalizeAuthority(authority) switch
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
            return NormalizeApplicationType(applicationType) switch
            {
                "DRILLING" => 1.5m,      // Drilling permits are most complex
                "ENVIRONMENTAL" => 1.3m, // Environmental assessments are detailed
                "INJECTION" => 1.2m,     // Injection permits require monitoring
                _ => 1.0m                // Default
            };
        }

        private List<REQUIRED_FORM> GetRequiredFormsForValidation(PERMIT_APPLICATION application)
        {
            var registry = new PermitFormTemplateRegistry();
            var normalizedAuthority = NormalizeAuthority(application.REGULATORY_AUTHORITY);
            var normalizedType = NormalizeApplicationType(application.APPLICATION_TYPE);
            var templates = registry.GetTemplates(normalizedAuthority, normalizedType);

            return templates.Select(template => new REQUIRED_FORM
            {
                FORM_CODE = template.FormCode,
                FORM_NAME = template.FormName,
                REQUIRED_IND = "Y",
                ACTIVE_IND = "Y"
            }).ToList();
        }

        private static string NormalizeApplicationType(string applicationType)
        {
            if (string.IsNullOrWhiteSpace(applicationType))
                return "OTHER";

            var normalized = applicationType.Trim().ToUpperInvariant().Replace(" ", "_");

            return normalized switch
            {
                "DRILLING" => "DRILLING",
                "DRILLING_PERMIT" => "DRILLING",
                "ENVIRONMENTAL" => "ENVIRONMENTAL",
                "ENVIRONMENTAL_PERMIT" => "ENVIRONMENTAL",
                "INJECTION" => "INJECTION",
                "INJECTION_PERMIT" => "INJECTION",
                "STORAGE" => "STORAGE",
                "FACILITY" => "FACILITY",
                "SEISMIC" => "SEISMIC",
                "GROUNDWATER" => "GROUNDWATER",
                _ => normalized
            };
        }

        private static string NormalizeAuthority(string authority)
        {
            return string.IsNullOrWhiteSpace(authority) ? "OTHER" : authority.Trim().ToUpperInvariant();
        }

        private async Task<DRILLING_PERMIT_APPLICATION?> GetDrillingApplicationAsync(string permitApplicationId)
        {
            var repo = await CreateRepositoryAsync<DRILLING_PERMIT_APPLICATION>("DRILLING_PERMIT_APPLICATION");
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
            var repo = await CreateRepositoryAsync<ENVIRONMENTAL_PERMIT_APPLICATION>("ENVIRONMENTAL_PERMIT_APPLICATION");
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
            var repo = await CreateRepositoryAsync<INJECTION_PERMIT_APPLICATION>("INJECTION_PERMIT_APPLICATION");
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.PPDM39.Repositories;

using Beep.OilandGas.PermitsAndApplications.DataMapping;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Microsoft.Extensions.Logging;
using Beep.OilandGas.Models.Data.PermitsAndApplications;

namespace Beep.OilandGas.LifeCycle.Services.Permits
{
    /// <summary>
    /// Service for managing permits and applications throughout the oil and gas lifecycle
    /// </summary>
    public class PermitManagementService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly PPDMMappingService _mappingService;
        private readonly ApplicationMapper _applicationMapper;
        private readonly string _connectionName;
        private readonly ILogger<PermitManagementService>? _logger;

        public PermitManagementService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            PPDMMappingService mappingService,
            string connectionName = "PPDM39",
            ILogger<PermitManagementService>? logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _mappingService = mappingService ?? throw new ArgumentNullException(nameof(mappingService));
            _applicationMapper = new ApplicationMapper();
            _connectionName = connectionName ?? throw new ArgumentNullException(nameof(connectionName));
            _logger = logger;
        }

        #region Permit Application Management

        /// <summary>
        /// Creates a drilling permit application
        /// </summary>
        public async Task<DRILLING_PERMIT_APPLICATION> CreateDrillingPermitApplicationAsync(
            string fieldId,
            string wellId,
            DRILLING_PERMIT_APPLICATION application,
            string userId)
        {
            try
            {
                _logger?.LogInformation("Creating drilling permit application for well {WellId} in field {FieldId}",
                    wellId, fieldId);

                // Map to PPDM APPLICATION
                var ppdmApplication = _applicationMapper.MapDrillingPermitToPPDM39(application);
                // Set well reference if available
                if (!string.IsNullOrEmpty(wellId))
                {
                    // Store well reference in APPLICATION_COMPONENT or use UWI field if available
                    ppdmApplication.REMARK = $"{ppdmApplication.REMARK}\nWell ID: {wellId}";
                }

                // Store in PPDM
                var applicationRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(APPLICATION), _connectionName, "APPLICATION", null);

                var result = await applicationRepo.InsertAsync(ppdmApplication, userId);
                
                // Create APPLICATION_AREA record to link application to field
                var insertedApplication = result as APPLICATION;
                if (insertedApplication != null && !string.IsNullOrEmpty(insertedApplication.APPLICATION_ID))
                {
                    await CreateApplicationAreaAsync(insertedApplication.APPLICATION_ID, fieldId, "FIELD", userId);
                }

                _logger?.LogInformation("Created drilling permit application {ApplicationId} for well {WellId}",
                    application.PERMIT_APPLICATION_ID, wellId);

                // Map back to domain model
                var mappedApplication = _applicationMapper.MapToDomain(result as APPLICATION);
                // Convert to DRILLING_PERMIT_APPLICATION
                return new DRILLING_PERMIT_APPLICATION
                {
                    DRILLING_PERMIT_APPLICATION_ID = mappedApplication.APPLICATION_ID,
                    APPLICATION_TYPE = mappedApplication.APPLICATION_TYPE,
                    STATUS = mappedApplication.STATUS,
                    COUNTRY = mappedApplication.COUNTRY,
                    STATE_PROVINCE = mappedApplication.STATE_PROVINCE,
                    REGULATORY_AUTHORITY = mappedApplication.REGULATORY_AUTHORITY,
                    CREATED_DATE = mappedApplication.CREATED_DATE,
                    SUBMITTED_DATE = mappedApplication.SUBMITTED_DATE,
                    RECEIVED_DATE = mappedApplication.RECEIVED_DATE,
                    DECISION_DATE = mappedApplication.DECISION_DATE,
                    EFFECTIVE_DATE = mappedApplication.EFFECTIVE_DATE,
                    EXPIRY_DATE = mappedApplication.EXPIRY_DATE,
                    DECISION = mappedApplication.DECISION,
                    REFERENCE_NUMBER = mappedApplication.REFERENCE_NUMBER,
                    FEES_DESCRIPTION = mappedApplication.FEES_DESCRIPTION,
                    FEES_PAID = mappedApplication.FEES_PAID,
                    REMARKS = mappedApplication.REMARKS,
                    SUBMISSION_COMPLETE = mappedApplication.SUBMISSION_COMPLETE,
                    SUBMISSION_DESCRIPTION = mappedApplication.SUBMISSION_DESCRIPTION,
                    ATTACHMENTS = mappedApplication.ATTACHMENTS,
                    AREAS = mappedApplication.Areas,
                    COMPONENTS = mappedApplication.Components,
                    WELL_UWI = application.WELL_UWI, // Preserve from original
                    TARGET_FORMATION = application.TARGET_FORMATION,
                    PROPOSED_DEPTH = application.PROPOSED_DEPTH,
                    DRILLING_METHOD = application.DRILLING_METHOD,
                    SURFACE_OWNER_NOTIFIED = application.SURFACE_OWNER_NOTIFIED
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating drilling permit application for well {WellId}", wellId);
                throw;
            }
        }

        /// <summary>
        /// Creates an environmental permit application
        /// </summary>
        public async Task<ENVIRONMENTAL_PERMIT_APPLICATION> CreateEnvironmentalPermitApplicationAsync(
            string fieldId,
            string? facilityId,
            ENVIRONMENTAL_PERMIT_APPLICATION application,
            string userId)
        {
            try
            {
                _logger?.LogInformation("Creating environmental permit application for field {FieldId}",
                    fieldId);

                // Map to PPDM APPLICATION
                var ppdmApplication = _applicationMapper.MapEnvironmentalPermitToPPDM39(application);
                // Set facility reference if available
                if (!string.IsNullOrEmpty(facilityId))
                {
                    ppdmApplication.REMARK = $"{ppdmApplication.REMARK}\nFacility ID: {facilityId}";
                }

                // Store in PPDM
                var applicationRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(APPLICATION), _connectionName, "APPLICATION", null);

                var result = await applicationRepo.InsertAsync(ppdmApplication, userId);
                
                // Create APPLICATION_AREA record to link application to field
                var insertedApplication = result as APPLICATION;
                if (insertedApplication != null && !string.IsNullOrEmpty(insertedApplication.APPLICATION_ID))
                {
                    await CreateApplicationAreaAsync(insertedApplication.APPLICATION_ID, fieldId, "FIELD", userId);
                }

                _logger?.LogInformation("Created environmental permit application {ApplicationId}",
                    application.PERMIT_APPLICATION_ID);

                // Map back to domain model
                var mappedApplication = _applicationMapper.MapToDomain(result as APPLICATION);
                // Convert to ENVIRONMENTAL_PERMIT_APPLICATION
                return new ENVIRONMENTAL_PERMIT_APPLICATION
                {
                    ENVIRONMENTAL_PERMIT_APPLICATION_ID = mappedApplication.APPLICATION_ID,
                    APPLICATION_TYPE = mappedApplication.APPLICATION_TYPE,
                    STATUS = mappedApplication.STATUS,
                    COUNTRY = mappedApplication.COUNTRY,
                    STATE_PROVINCE = mappedApplication.STATE_PROVINCE,
                    REGULATORY_AUTHORITY = mappedApplication.REGULATORY_AUTHORITY,
                    CREATED_DATE = mappedApplication.CREATED_DATE,
                    SUBMITTED_DATE = mappedApplication.SUBMITTED_DATE,
                    RECEIVED_DATE = mappedApplication.RECEIVED_DATE,
                    DECISION_DATE = mappedApplication.DECISION_DATE,
                    EFFECTIVE_DATE = mappedApplication.EFFECTIVE_DATE,
                    EXPIRY_DATE = mappedApplication.EXPIRY_DATE,
                    DECISION = mappedApplication.DECISION,
                    REFERENCE_NUMBER = mappedApplication.REFERENCE_NUMBER,
                    FEES_DESCRIPTION = mappedApplication.FEES_DESCRIPTION,
                    FEES_PAID = mappedApplication.FEES_PAID,
                    REMARKS = mappedApplication.REMARKS,
                    SUBMISSION_COMPLETE = mappedApplication.SUBMISSION_COMPLETE,
                    SUBMISSION_DESCRIPTION = mappedApplication.SUBMISSION_DESCRIPTION,
                    ATTACHMENTS = mappedApplication.ATTACHMENTS,
                    AREAS = mappedApplication.Areas,
                    COMPONENTS = mappedApplication.Components,
                    WELL_UWI = application.WELL_UWI, // Preserve from original
                    TARGET_FORMATION = application.TARGET_FORMATION,
                    PROPOSED_DEPTH = application.PROPOSED_DEPTH,
                    DRILLING_METHOD = application.DRILLING_METHOD,
                    SURFACE_OWNER_NOTIFIED = application.SURFACE_OWNER_NOTIFIED,
                    ENVIRONMENTAL_PERMIT_TYPE = application.ENVIRONMENTAL_PERMIT_TYPE, // Preserve from original
                    WASTE_TYPE = application.WASTE_TYPE,
                    WASTE_VOLUME = application.WASTE_VOLUME,
                    NORMINVOLVED = application.NORMINVOLVED
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating environmental permit application");
                throw;
            }
        }

        /// <summary>
        /// Creates an injection permit application
        /// </summary>
        public async Task<INJECTION_PERMIT_APPLICATION> CreateInjectionPermitApplicationAsync(
            string fieldId,
            string? wellId,
            string? facilityId,
            INJECTION_PERMIT_APPLICATION application,
            string userId)
        {
            try
            {
                _logger?.LogInformation("Creating injection permit application for field {FieldId}",
                    fieldId);

                // Map to PPDM APPLICATION
                var ppdmApplication = _applicationMapper.MapInjectionPermitToPPDM39(application);
                // Set well/facility references if available
                if (!string.IsNullOrEmpty(wellId))
                {
                    ppdmApplication.REMARK = $"{ppdmApplication.REMARK}\nWell ID: {wellId}";
                }
                if (!string.IsNullOrEmpty(facilityId))
                {
                    ppdmApplication.REMARK = $"{ppdmApplication.REMARK}\nFacility ID: {facilityId}";
                }

                // Store in PPDM
                var applicationRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(APPLICATION), _connectionName, "APPLICATION", null);

                var result = await applicationRepo.InsertAsync(ppdmApplication, userId);
                
                // Create APPLICATION_AREA record to link application to field
                var insertedApplication = result as APPLICATION;
                if (insertedApplication != null && !string.IsNullOrEmpty(insertedApplication.APPLICATION_ID))
                {
                    await CreateApplicationAreaAsync(insertedApplication.APPLICATION_ID, fieldId, "FIELD", userId);
                }

                _logger?.LogInformation("Created injection permit application {ApplicationId}",
                    application.INJECTION_PERMIT_APPLICATION_ID);

                // Map back to domain model
                var mappedApplication = _applicationMapper.MapToDomain(result as APPLICATION);
                // Convert to INJECTION_PERMIT_APPLICATION
                return new INJECTION_PERMIT_APPLICATION
                {
                    INJECTION_PERMIT_APPLICATION_ID = mappedApplication.APPLICATION_ID,
                    APPLICATION_TYPE = mappedApplication.APPLICATION_TYPE,
                    STATUS = mappedApplication.STATUS,
                    COUNTRY = mappedApplication.COUNTRY,
                    STATE_PROVINCE = mappedApplication.STATE_PROVINCE,
                    REGULATORY_AUTHORITY = mappedApplication.REGULATORY_AUTHORITY,
                    CREATED_DATE = mappedApplication.CREATED_DATE,
                    SUBMITTED_DATE = mappedApplication.SUBMITTED_DATE,
                    RECEIVED_DATE = mappedApplication.RECEIVED_DATE,
                    DECISION_DATE = mappedApplication.DECISION_DATE,
                    EFFECTIVE_DATE = mappedApplication.EFFECTIVE_DATE,
                    EXPIRY_DATE = mappedApplication.EXPIRY_DATE,
                    DECISION = mappedApplication.DECISION,
                    REFERENCE_NUMBER = mappedApplication.REFERENCE_NUMBER,
                    FEES_DESCRIPTION = mappedApplication.FEES_DESCRIPTION,
                    FEES_PAID = mappedApplication.FEES_PAID,
                    REMARKS = mappedApplication.REMARKS,
                    SUBMISSION_COMPLETE = mappedApplication.SUBMISSION_COMPLETE,
                    SUBMISSION_DESCRIPTION = mappedApplication.SUBMISSION_DESCRIPTION,
                    ATTACHMENTS = mappedApplication.ATTACHMENTS,
                    AREAS = mappedApplication.Areas,
                    COMPONENTS = mappedApplication.Components,
                    WELL_UWI = application.WELL_UWI, // Preserve from original
                    INJECTION_TYPE = application.INJECTION_TYPE, // Preserve from original
                    INJECTION_ZONE = application.INJECTION_ZONE,
                    INJECTION_FLUID = application.INJECTION_FLUID,
                    MAXIMUM_INJECTION_PRESSURE = application.MAXIMUM_INJECTION_PRESSURE,
                    MAXIMUM_INJECTION_RATE = application.MAXIMUM_INJECTION_RATE
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating injection permit application");
                throw;
            }
        }

        /// <summary>
        /// Gets all permit applications for a field
        /// </summary>
        public async Task<List<PERMIT_APPLICATION>> GetPermitApplicationsForFieldAsync(
            string fieldId,
            PermitApplicationType? applicationType = null,
            PermitApplicationStatus? status = null)
        {
            try
            {
                _logger?.LogInformation("Getting permit applications for field {FieldId}", fieldId);

                // First, get application IDs from APPLICATION_AREA table
                var applicAreaRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(APPLIC_AREA), _connectionName, "APPLIC_AREA", null);

                var areaFilters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "AREA_ID", Operator = "=", FilterValue = fieldId },
                    new AppFilter { FieldName = "AREA_TYPE", Operator = "=", FilterValue = "FIELD" },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var applicAreaResults = await applicAreaRepo.GetAsync(areaFilters);
                var applicationIds = applicAreaResults.Cast<APPLIC_AREA>()
                    .Select(aa => aa.APPLICATION_ID)
                    .Where(id => !string.IsNullOrEmpty(id))
                    .ToList();

                if (applicationIds.Count == 0)
                {
                    _logger?.LogInformation("No permit applications found for field {FieldId}", fieldId);
                    return new List<PERMIT_APPLICATION>();
                }

                // Now get the applications - query each application ID individually and filter in memory
                var applicationRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(APPLICATION), _connectionName, "APPLICATION", null);

                var allApplications = new List<APPLICATION>();
                foreach (var appId in applicationIds)
                {
                    var appFilters = new List<AppFilter>
                    {
                        new AppFilter { FieldName = "APPLICATION_ID", Operator = "=", FilterValue = appId }
                    };

                    var appResults = await applicationRepo.GetAsync(appFilters);
                    var apps = appResults.Cast<APPLICATION>();
                    allApplications.AddRange(apps);
                }

                // Apply additional filters in memory
                var filteredApplications = allApplications.AsQueryable();

                if (applicationType.HasValue)
                {
                    var applicationTypeStr = _applicationMapper.MapApplicationTypeToString(applicationType.Value);
                    filteredApplications = filteredApplications.Where(app => app.APPLICATION_TYPE == applicationTypeStr);
                }

                if (status.HasValue)
                {
                    var statusStr = _applicationMapper.MapApplicationStatusToString(status.Value);
                    filteredApplications = filteredApplications.Where(app => app.CURRENT_STATUS == statusStr);
                }

                var results = filteredApplications.ToList();

                var applications = results
                    .Select(app => _applicationMapper.MapToDomain(app))
                    .ToList();

                _logger?.LogInformation("Retrieved {Count} permit applications for field {FieldId}",
                    applications.Count, fieldId);

                return applications;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting permit applications for field {FieldId}", fieldId);
                throw;
            }
        }

        /// <summary>
        /// Gets a permit application by ID
        /// </summary>
        public async Task<PERMIT_APPLICATION?> GetPermitApplicationAsync(string applicationId)
        {
            try
            {
                _logger?.LogInformation("Getting permit application {ApplicationId}", applicationId);

                var applicationRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(APPLICATION), _connectionName, "APPLICATION", null);

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "APPLICATION_ID", Operator = "=", FilterValue = applicationId }
                };

                var results = await applicationRepo.GetAsync(filters);
                var application = results?.Cast<APPLICATION>().FirstOrDefault();

                if (application == null)
                {
                    _logger?.LogWarning("Permit application {ApplicationId} not found", applicationId);
                    return null;
                }

                // Map to domain model based on application type
                var domainApplication = _applicationMapper.MapToDomain(application);

                _logger?.LogInformation("Retrieved permit application {ApplicationId}", applicationId);

                return domainApplication;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting permit application {ApplicationId}", applicationId);
                throw;
            }
        }

        /// <summary>
        /// Updates permit application status
        /// </summary>
        public async Task<PERMIT_APPLICATION> UpdatePermitApplicationStatusAsync(
            string applicationId,
            PermitApplicationStatus newStatus,
            DateTime? decisionDate = null,
            string? decision = null,
            string userId = "SYSTEM")
        {
            try
            {
                _logger?.LogInformation("Updating permit application {ApplicationId} status to {Status}",
                    applicationId, newStatus);

                var applicationRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(APPLICATION), _connectionName, "APPLICATION", null);

                var updateData = new Dictionary<string, object>
                {
                    ["APPLICATION_ID"] = applicationId,
                    ["CURRENT_STATUS"] = _applicationMapper.MapApplicationStatusToString(newStatus)
                };

                if (decisionDate.HasValue)
                {
                    updateData["DECISION_DATE"] = decisionDate.Value;
                }

                if (!string.IsNullOrEmpty(decision))
                {
                    updateData["DECISION"] = decision;
                }

                // Set effective date if approved
                if (newStatus == PermitApplicationStatus.Approved)
                {
                    updateData["EFFECTIVE_DATE"] = decisionDate ?? DateTime.UtcNow;
                }

                await applicationRepo.UpdateAsync(updateData, userId);

                // Retrieve updated application
                var updatedApplication = await GetPermitApplicationAsync(applicationId);

                _logger?.LogInformation("Updated permit application {ApplicationId} status to {Status}",
                    applicationId, newStatus);

                return updatedApplication!;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error updating permit application {ApplicationId} status", applicationId);
                throw;
            }
        }

        /// <summary>
        /// Submits a permit application
        /// </summary>
        public async Task<PERMIT_APPLICATION> SubmitPermitApplicationAsync(
            string applicationId,
            string userId)
        {
            try
            {
                _logger?.LogInformation("Submitting permit application {ApplicationId}", applicationId);

                var application = await GetPermitApplicationAsync(applicationId);
                if (application == null)
                {
                    throw new InvalidOperationException($"Permit application {applicationId} not found");
                }

                // Update status to Submitted
                var updatedApplication = await UpdatePermitApplicationStatusAsync(
                    applicationId,
                    PermitApplicationStatus.Submitted,
                    null,
                    null,
                    userId);

                // Update submitted date
                var applicationRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(APPLICATION), _connectionName, "APPLICATION", null);

                var updateData = new Dictionary<string, object>
                {
                    ["APPLICATION_ID"] = applicationId,
                    ["SUBMITTED_DATE"] = DateTime.UtcNow
                };

                await applicationRepo.UpdateAsync(updateData, userId);

                _logger?.LogInformation("Submitted permit application {ApplicationId}", applicationId);

                return await GetPermitApplicationAsync(applicationId) ?? updatedApplication;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error submitting permit application {ApplicationId}", applicationId);
                throw;
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Creates an APPLICATION_AREA record to link an application to an area (field, etc.)
        /// </summary>
        private async Task CreateApplicationAreaAsync(
            string applicationId,
            string areaId,
            string areaType,
            string userId)
        {
            try
            {
                var applicAreaRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(APPLIC_AREA), _connectionName, "APPLIC_AREA", null);

                var applicArea = new APPLIC_AREA
                {
                    APPLICATION_ID = applicationId,
                    AREA_ID = areaId,
                    AREA_TYPE = areaType,
                    ACTIVE_IND = "Y",
                    EFFECTIVE_DATE = DateTime.UtcNow.Date,
                    ROW_CREATED_BY = userId,
                    ROW_CREATED_DATE = DateTime.UtcNow.Date
                };

                await applicAreaRepo.InsertAsync(applicArea, userId);

                _logger?.LogInformation("Created APPLICATION_AREA record linking application {ApplicationId} to {AreaType} {AreaId}",
                    applicationId, areaType, areaId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating APPLICATION_AREA record for application {ApplicationId}", applicationId);
                throw;
            }
        }

        #endregion
    }
}


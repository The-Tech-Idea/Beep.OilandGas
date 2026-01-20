using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.LifeCycle.Services.Exploration
{
    /// <summary>
    /// Service for Exploration phase data management, field-scoped
    /// </summary>
    public class PPDMExplorationService : IFieldExplorationService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly PPDMMappingService _mappingService;
        private readonly string _connectionName;
        private readonly ILogger<PPDMExplorationService>? _logger;

        public PPDMExplorationService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            PPDMMappingService mappingService,
            string connectionName = "PPDM39",
            ILogger<PPDMExplorationService>? logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _mappingService = mappingService ?? throw new ArgumentNullException(nameof(mappingService));
            _connectionName = connectionName ?? throw new ArgumentNullException(nameof(connectionName));
            _logger = logger;
        }

        public async Task<List<PROSPECT>> GetProspectsForFieldAsync(string fieldId, List<AppFilter>? additionalFilters = null)
        {
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("PROSPECT");
                if (metadata == null)
                {
                    _logger?.LogWarning("PROSPECT table metadata not found");
                    return new List<PROSPECT>();
                }

                var entityType = typeof(PROSPECT);

                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, _connectionName, "PROSPECT", null);

                // Always filter by field ID
                var filters = new List<AppFilter>
                {
                    new AppFilter
                    {
                        FieldName = "FIELD_ID",
                        FilterValue = _defaults.FormatIdForTable("PROSPECT", fieldId),
                        Operator = "="
                    }
                };

                if (additionalFilters != null)
                    filters.AddRange(additionalFilters);

                var results = await repo.GetAsync(filters);
                
                // Return PPDM models directly
                return results.Cast<PROSPECT>().ToList();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error getting prospects for field: {fieldId}");
                throw;
            }
        }

        public async Task<PROSPECT> CreateProspectForFieldAsync(string fieldId, ProspectRequest prospectData, string userId)
        {
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("PROSPECT");
                if (metadata == null)
                {
                    throw new InvalidOperationException("PROSPECT table metadata not found");
                }

                var entityType = typeof(PROSPECT);

                // Convert DTO to PPDM model
                var prospectEntity = _mappingService.ConvertDTOToPPDMModel<PROSPECT, ProspectRequest>(prospectData);
                
                // Set FIELD_ID automatically using reflection
                var fieldIdProp = entityType.GetProperty("FIELD_ID", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
                if (fieldIdProp != null && fieldIdProp.CanWrite)
                {
                    fieldIdProp.SetValue(prospectEntity, _defaults.FormatIdForTable("PROSPECT", fieldId));
                }

                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, _connectionName, "PROSPECT", null);

                var result = await repo.InsertAsync(prospectEntity, userId);
                
                // Return PPDM model directly
                return (PROSPECT)result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error creating prospect for field: {fieldId}");
                throw;
            }
        }

        public async Task<PROSPECT> UpdateProspectForFieldAsync(string fieldId, string prospectId, ProspectRequest prospectData, string userId)
        {
            try
            {
                // Validate prospect belongs to field
                var existingProspect = await GetProspectForFieldAsync(fieldId, prospectId);
                if (existingProspect == null)
                {
                    throw new InvalidOperationException($"Prospect {prospectId} not found or does not belong to field {fieldId}");
                }

                var metadata = await _metadata.GetTableMetadataAsync("PROSPECT");
                if (metadata == null)
                {
                    throw new InvalidOperationException("PROSPECT table metadata not found");
                }

                var entityType = typeof(PROSPECT);

                // Convert DTO to PPDM model
                var prospectEntity = _mappingService.ConvertDTOToPPDMModel<PROSPECT, ProspectRequest>(prospectData);

                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, _connectionName, "PROSPECT", null);

                var formattedId = _defaults.FormatIdForTable("PROSPECT", prospectId);
                // Set the ID property on the entity before updating
                var idProp = entityType.GetProperty("PROSPECT_ID");
                if (idProp != null)
                {
                    idProp.SetValue(prospectEntity, formattedId);
                }
                var result = await repo.UpdateAsync(prospectEntity, userId);
                
                // Return PPDM model directly
                return (PROSPECT)result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error updating prospect {prospectId} for field: {fieldId}");
                throw;
            }
        }

        public async Task<PROSPECT?> GetProspectForFieldAsync(string fieldId, string prospectId)
        {
            try
            {
                var prospects = await GetProspectsForFieldAsync(fieldId, new List<AppFilter>
                {
                    new AppFilter
                    {
                        FieldName = "PROSPECT_ID",
                        FilterValue = _defaults.FormatIdForTable("PROSPECT", prospectId),
                        Operator = "="
                    }
                });

                return prospects.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error getting prospect {prospectId} for field: {fieldId}");
                throw;
            }
        }

        public async Task<bool> DeleteProspectForFieldAsync(string fieldId, string prospectId, string userId)
        {
            try
            {
                // Validate prospect belongs to field
                var existingProspect = await GetProspectForFieldAsync(fieldId, prospectId);
                if (existingProspect == null)
                {
                    return false;
                }

                var metadata = await _metadata.GetTableMetadataAsync("PROSPECT");
                if (metadata == null)
                {
                    throw new InvalidOperationException("PROSPECT table metadata not found");
                }

                var entityType = typeof(PROSPECT);

                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, _connectionName, "PROSPECT", null);

                var formattedId = _defaults.FormatIdForTable("PROSPECT", prospectId);
                return await repo.SoftDeleteAsync(formattedId, userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error deleting prospect {prospectId} for field: {fieldId}");
                throw;
            }
        }

        public async Task<List<SEIS_ACQTN_SURVEY>> GetSeismicSurveysForFieldAsync(string fieldId, List<AppFilter>? additionalFilters = null)
        {
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("SEIS_ACQTN_SURVEY");
                if (metadata == null)
                {
                    _logger?.LogWarning("SEIS_ACQTN_SURVEY table metadata not found");
                    return new List<SEIS_ACQTN_SURVEY>();
                }

                var entityType = typeof(SEIS_ACQTN_SURVEY);

                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, _connectionName, "SEIS_ACQTN_SURVEY", null);

                // Filter by field ID - check if SEIS_ACQTN_SURVEY has FIELD_ID or if we need to join through another table
                // For now, assuming SEIS_ACQTN_SURVEY has FIELD_ID directly
                var filters = new List<AppFilter>
                {
                    new AppFilter
                    {
                        FieldName = "FIELD_ID",
                        FilterValue = _defaults.FormatIdForTable("SEIS_ACQTN_SURVEY", fieldId),
                        Operator = "="
                    }
                };

                if (additionalFilters != null)
                    filters.AddRange(additionalFilters);

                var results = await repo.GetAsync(filters);
                
                // Return PPDM models directly
                return results.Cast<SEIS_ACQTN_SURVEY>().ToList();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error getting seismic surveys for field: {fieldId}");
                throw;
            }
        }

        public async Task<SEIS_ACQTN_SURVEY> CreateSeismicSurveyForFieldAsync(string fieldId, SeismicSurveyRequest surveyData, string userId)
        {
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("SEIS_ACQTN_SURVEY");
                if (metadata == null)
                {
                    throw new InvalidOperationException("SEIS_ACQTN_SURVEY table metadata not found");
                }

                var entityType = typeof(SEIS_ACQTN_SURVEY);

                // Convert DTO to PPDM model
                var surveyEntity = _mappingService.ConvertDTOToPPDMModel<SEIS_ACQTN_SURVEY, SeismicSurveyRequest>(surveyData);
                
                // Set FIELD_ID automatically using reflection
                var fieldIdProp = entityType.GetProperty("FIELD_ID", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
                if (fieldIdProp != null && fieldIdProp.CanWrite)
                {
                    fieldIdProp.SetValue(surveyEntity, _defaults.FormatIdForTable("SEIS_ACQTN_SURVEY", fieldId));
                }

                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, _connectionName, "SEIS_ACQTN_SURVEY", null);

                var result = await repo.InsertAsync(surveyEntity, userId);
                
                // Return PPDM model directly
                return (SEIS_ACQTN_SURVEY)result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error creating seismic survey for field: {fieldId}");
                throw;
            }
        }

        public async Task<List<WELL>> GetExploratoryWellsForFieldAsync(string fieldId, List<AppFilter>? additionalFilters = null)
        {
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("WELL");
                if (metadata == null)
                {
                    _logger?.LogWarning("WELL table metadata not found");
                    return new List<WELL>();
                }

                var entityType = typeof(WELL);

                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, _connectionName, "WELL", null);

                // Filter by field ID and well type = EXPLORATION
                var filters = new List<AppFilter>
                {
                    new AppFilter
                    {
                        FieldName = "FIELD_ID",
                        FilterValue = _defaults.FormatIdForTable("WELL", fieldId),
                        Operator = "="
                    },
                    new AppFilter
                    {
                        FieldName = "WELL_TYPE",
                        FilterValue = "EXPLORATION",
                        Operator = "="
                    }
                };

                if (additionalFilters != null)
                    filters.AddRange(additionalFilters);

                var results = await repo.GetAsync(filters);
                
                // Return PPDM models directly
                return results.Cast<WELL>().ToList();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error getting exploratory wells for field: {fieldId}");
                throw;
            }
        }

        public async Task<List<SEIS_LINE>> GetSeismicLinesForSurveyAsync(string surveyId, List<AppFilter>? additionalFilters = null)
        {
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("SEIS_LINE");
                if (metadata == null)
                {
                    _logger?.LogWarning("SEIS_LINE table metadata not found");
                    return new List<SEIS_LINE>();
                }

                var entityType = typeof(SEIS_LINE);

                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, _connectionName, "SEIS_LINE", null);

                // Filter by survey ID
                var filters = new List<AppFilter>
                {
                    new AppFilter
                    {
                        FieldName = "SEIS_ACQTN_SURVEY_ID",
                        FilterValue = _defaults.FormatIdForTable("SEIS_LINE", surveyId),
                        Operator = "="
                    }
                };

                if (additionalFilters != null)
                    filters.AddRange(additionalFilters);

                var results = await repo.GetAsync(filters);
                
                // Return PPDM models directly
                return results.Cast<SEIS_LINE>().ToList();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error getting seismic lines for survey: {surveyId}");
                throw;
            }
        }

        #region Prospect Identification Integration

        /// <summary>
        /// Identifies and evaluates a prospect using ProspectIdentification service
        /// </summary>
        public async Task<ProspectEvaluation> IdentifyProspectAsync(string fieldId, ProspectRequest prospectData, string userId)
        {
            try
            {
                _logger?.LogInformation("Identifying prospect for field: {FieldId}", fieldId);

                // Create prospect first
                var prospect = await CreateProspectForFieldAsync(fieldId, prospectData, userId);

                // If ProspectEvaluationService is available, evaluate it
                // For now, return basic evaluation
                return new ProspectEvaluation
                {
                    ProspectId = prospect.PROSPECT_ID ?? string.Empty,
                    FieldId = fieldId,
                    EvaluationDate = DateTime.UtcNow,
                    RiskLevel = "MEDIUM",
                    Potential = "MODERATE",
                    Recommendation = "FURTHER_EVALUATION"
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error identifying prospect for field: {FieldId}", fieldId);
                throw;
            }
        }

        /// <summary>
        /// Evaluates a prospect using ProspectIdentification service
        /// </summary>
        public async Task<ProspectEvaluation> EvaluateProspectAsync(string fieldId, string prospectId)
        {
            try
            {
                _logger?.LogInformation("Evaluating prospect: {ProspectId} for field: {FieldId}", prospectId, fieldId);

                var prospect = await GetProspectForFieldAsync(fieldId, prospectId);
                if (prospect == null)
                {
                    throw new InvalidOperationException($"Prospect {prospectId} not found for field {fieldId}");
                }

                // Basic evaluation - in full implementation, would use ProspectEvaluationService
                return new ProspectEvaluation
                {
                    ProspectId = prospectId,
                    FieldId = fieldId,
                    EvaluationDate = DateTime.UtcNow,
                    RiskLevel = "MEDIUM",
                    Potential = "MODERATE",
                    Recommendation = "FURTHER_EVALUATION"
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error evaluating prospect: {ProspectId}", prospectId);
                throw;
            }
        }

        #endregion

        #region Lease Acquisition Integration

        /// <summary>
        /// Acquires a lease using LeaseAcquisition service
        /// </summary>
        public async Task<Lease> AcquireLeaseAsync(string fieldId, CreateLease leaseData, string userId)
        {
            try
            {
                _logger?.LogInformation("Acquiring lease for field: {FieldId}", fieldId);

                // Create lease in PPDM LEASE table
                var metadata = await _metadata.GetTableMetadataAsync("LEASE");
                if (metadata == null)
                {
                    throw new InvalidOperationException("LEASE table metadata not found");
                }

                var entityType = typeof(LEASE);
                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, _connectionName, "LEASE", null);

                var lease = new LEASE();
                lease.LEASE_NAME = leaseData.LeaseNumber ?? string.Empty;
                lease.FIELD_ID = _defaults.FormatIdForTable("LEASE", fieldId);
                if (leaseData.StartDate.HasValue)
                    lease.LEASE_EFF_DATE = leaseData.StartDate.Value;
                if (leaseData.EndDate.HasValue)
                    lease.LEASE_EXPIRY_DATE = leaseData.EndDate.Value;
                lease.ACTIVE_IND = "Y";

                if (lease is IPPDMEntity entity)
                    _commonColumnHandler.PrepareForInsert(entity, userId);
                var result = await repo.InsertAsync(lease, userId);
                var createdLease = result as LEASE ?? throw new InvalidOperationException("Failed to create lease");

                _logger?.LogInformation("Lease acquired: {LeaseId}, Name: {LeaseName}", createdLease.LEASE_ID, createdLease.LEASE_NAME);

                return new Lease
                {
                    LeaseId = createdLease.LEASE_ID ?? string.Empty,
                    LeaseName = createdLease.LEASE_NAME ?? string.Empty,
                    FieldId = fieldId,
                    StartDate = leaseData.StartDate,
                    EndDate = leaseData.EndDate,
                    Status = "ACTIVE"
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error acquiring lease for field: {FieldId}", fieldId);
                throw;
            }
        }

        /// <summary>
        /// Manages lease operations using LeaseAcquisition service
        /// </summary>
        public async Task<Lease> ManageLeaseAsync(string fieldId, string leaseId, UpdateLease updateData, string userId)
        {
            try
            {
                _logger?.LogInformation("Managing lease: {LeaseId} for field: {FieldId}", leaseId, fieldId);
using Beep.OilandGas.Models.Data.ProspectIdentification;

                var metadata = await _metadata.GetTableMetadataAsync("LEASE");
                if (metadata == null)
                {
                    throw new InvalidOperationException("LEASE table metadata not found");
                }

                var entityType = typeof(LEASE);
                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, _connectionName, "LEASE", null);

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "LEASE_ID", Operator = "=", FilterValue = leaseId },
                    new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = _defaults.FormatIdForTable("LEASE", fieldId) }
                };

                var leases = await repo.GetAsync(filters);
                var lease = leases.FirstOrDefault() as LEASE;
                if (lease == null)
                {
                    throw new InvalidOperationException($"Lease {leaseId} not found for field {fieldId}");
                }

                // Update lease properties
                // Note: UpdateLease doesn't have LeaseName, only status and dates
                if (updateData.StartDate.HasValue)
                    lease.LEASE_EFF_DATE = updateData.StartDate.Value;
                if (updateData.EndDate.HasValue)
                    lease.LEASE_EXPIRY_DATE = updateData.EndDate.Value;

                if (lease is IPPDMEntity entity)
                    _commonColumnHandler.PrepareForUpdate(entity, userId);
                await repo.UpdateAsync(lease, userId);

                return new Lease
                {
                    LeaseId = lease.LEASE_ID ?? string.Empty,
                    LeaseName = lease.LEASE_NAME ?? string.Empty,
                    FieldId = fieldId,
                    StartDate = lease.LEASE_EFF_DATE,
                    EndDate = lease.LEASE_EXPIRY_DATE,
                    Status = "ACTIVE"
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error managing lease: {LeaseId}", leaseId);
                throw;
            }
        }

        #endregion
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Core.DTOs;
using Beep.OilandGas.PPDM39.Core.Interfaces;
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
    }
}

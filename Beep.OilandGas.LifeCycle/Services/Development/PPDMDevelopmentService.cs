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

namespace Beep.OilandGas.LifeCycle.Services.Development
{
    /// <summary>
    /// Service for Development phase data management, field-scoped
    /// </summary>
    public class PPDMDevelopmentService : IFieldDevelopmentService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly PPDMMappingService _mappingService;
        private readonly string _connectionName;
        private readonly ILogger<PPDMDevelopmentService>? _logger;

        public PPDMDevelopmentService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            PPDMMappingService mappingService,
            string connectionName = "PPDM39",
            ILogger<PPDMDevelopmentService>? logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _mappingService = mappingService ?? throw new ArgumentNullException(nameof(mappingService));
            _connectionName = connectionName ?? throw new ArgumentNullException(nameof(connectionName));
            _logger = logger;
        }

        public async Task<List<POOL>> GetPoolsForFieldAsync(string fieldId, List<AppFilter>? additionalFilters = null)
        {
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("POOL");
                if (metadata == null)
                {
                    _logger?.LogWarning("POOL table metadata not found");
                    return new List<POOL>();
                }

                var entityType = typeof(POOL);

                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, _connectionName, "POOL", null);

                var filters = new List<AppFilter>
                {
                    new AppFilter
                    {
                        FieldName = "FIELD_ID",
                        FilterValue = _defaults.FormatIdForTable("POOL", fieldId),
                        Operator = "="
                    }
                };

                if (additionalFilters != null)
                    filters.AddRange(additionalFilters);

                var results = await repo.GetAsync(filters);
                
                // Return PPDM models directly
                return results.Cast<POOL>().ToList();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error getting pools for field: {fieldId}");
                throw;
            }
        }

        public async Task<POOL> CreatePoolForFieldAsync(string fieldId, PoolRequest poolData, string userId)
        {
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("POOL");
                if (metadata == null)
                {
                    throw new InvalidOperationException("POOL table metadata not found");
                }

                var entityType = typeof(POOL);

                // Convert DTO to PPDM model
                var poolEntity = _mappingService.ConvertDTOToPPDMModel<POOL, PoolRequest>(poolData);
                
                // Set FIELD_ID automatically using reflection
                var fieldIdProp = entityType.GetProperty("FIELD_ID", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
                if (fieldIdProp != null && fieldIdProp.CanWrite)
                {
                    fieldIdProp.SetValue(poolEntity, _defaults.FormatIdForTable("POOL", fieldId));
                }

                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, _connectionName, "POOL", null);

                var result = await repo.InsertAsync(poolEntity, userId);
                
                // Return PPDM model directly
                return (POOL)result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error creating pool for field: {fieldId}");
                throw;
            }
        }

        public async Task<POOL> UpdatePoolForFieldAsync(string fieldId, string poolId, PoolRequest poolData, string userId)
        {
            try
            {
                var existingPool = await GetPoolForFieldAsync(fieldId, poolId);
                if (existingPool == null)
                {
                    throw new InvalidOperationException($"Pool {poolId} not found or does not belong to field {fieldId}");
                }

                var metadata = await _metadata.GetTableMetadataAsync("POOL");
                if (metadata == null)
                {
                    throw new InvalidOperationException("POOL table metadata not found");
                }

                var entityType = typeof(POOL);

                // Convert DTO to PPDM model
                var poolEntity = _mappingService.ConvertDTOToPPDMModel<POOL, PoolRequest>(poolData);

                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, _connectionName, "POOL", null);

                var formattedId = _defaults.FormatIdForTable("POOL", poolId);
                // Set the ID property on the entity before updating
                var idProp = entityType.GetProperty("POOL_ID");
                if (idProp != null)
                {
                    idProp.SetValue(poolEntity, formattedId);
                }
                var result = await repo.UpdateAsync(poolEntity, userId);
                
                // Return PPDM model directly
                return (POOL)result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error updating pool {poolId} for field: {fieldId}");
                throw;
            }
        }

        public async Task<POOL?> GetPoolForFieldAsync(string fieldId, string poolId)
        {
            try
            {
                var pools = await GetPoolsForFieldAsync(fieldId, new List<AppFilter>
                {
                    new AppFilter
                    {
                        FieldName = "POOL_ID",
                        FilterValue = _defaults.FormatIdForTable("POOL", poolId),
                        Operator = "="
                    }
                });

                return pools.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error getting pool {poolId} for field: {fieldId}");
                throw;
            }
        }

        public async Task<List<WELL>> GetDevelopmentWellsForFieldAsync(string fieldId, List<AppFilter>? additionalFilters = null)
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
                        FilterValue = "DEVELOPMENT",
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
                _logger?.LogError(ex, $"Error getting development wells for field: {fieldId}");
                throw;
            }
        }

        public async Task<WELL> CreateDevelopmentWellForFieldAsync(string fieldId, DevelopmentWellRequest wellData, string userId)
        {
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("WELL");
                if (metadata == null)
                {
                    throw new InvalidOperationException("WELL table metadata not found");
                }

                var entityType = typeof(WELL);

                // Convert DTO to PPDM model
                var wellEntity = _mappingService.ConvertDTOToPPDMModel<WELL, DevelopmentWellRequest>(wellData);
                
                // Set FIELD_ID and WELL_TYPE automatically using reflection
                var fieldIdProp = entityType.GetProperty("FIELD_ID", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
                if (fieldIdProp != null && fieldIdProp.CanWrite)
                {
                    fieldIdProp.SetValue(wellEntity, _defaults.FormatIdForTable("WELL", fieldId));
                }
                var wellTypeProp = entityType.GetProperty("WELL_TYPE", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
                if (wellTypeProp != null && wellTypeProp.CanWrite)
                {
                    wellTypeProp.SetValue(wellEntity, "DEVELOPMENT");
                }

                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, _connectionName, "WELL", null);

                var result = await repo.InsertAsync(wellEntity, userId);
                
                // Return PPDM model directly
                return (WELL)result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error creating development well for field: {fieldId}");
                throw;
            }
        }

        public async Task<List<WELL>> GetWellboresForWellAsync(string fieldId, string wellId, List<AppFilter>? additionalFilters = null)
        {
            try
            {
                // First validate that the well belongs to the field
                var wells = await GetDevelopmentWellsForFieldAsync(fieldId, new List<AppFilter>
                {
                    new AppFilter
                    {
                        FieldName = "WELL_ID",
                        FilterValue = _defaults.FormatIdForTable("WELL", wellId),
                        Operator = "="
                    }
                });

                if (!wells.Any())
                {
                    throw new InvalidOperationException($"Well {wellId} not found or does not belong to field {fieldId}");
                }

                // Wellbores are WELL table records with specific well_level_type, linked via WELL_XREF
                // Get the parent well's UWI to query WELL_XREF
                var parentWell = wells.First();
                var parentWellType = typeof(WELL);
                var uwiProp = parentWellType.GetProperty("UWI", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
                if (uwiProp == null)
                {
                    _logger?.LogWarning("UWI property not found on WELL type");
                    return new List<WELL>();
                }

                var parentUwi = uwiProp.GetValue(parentWell)?.ToString();
                if (string.IsNullOrWhiteSpace(parentUwi))
                {
                    _logger?.LogWarning($"Parent well {wellId} does not have a UWI");
                    return new List<WELL>();
                }

                // Query WELL_XREF to find wellbores linked to this well by UWI
                var wellXrefMetadata = await _metadata.GetTableMetadataAsync("WELL_XREF");
                if (wellXrefMetadata == null)
                {
                    _logger?.LogWarning("WELL_XREF table metadata not found");
                    return new List<WELL>();
                }

                var wellXrefType = typeof(WELL_XREF);
                var wellXrefRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    wellXrefType, _connectionName, "WELL_XREF", null);

                var wellboreXrefType = _defaults.GetWellboreXrefType();

                // Query WELL_XREF by parent well's UWI and XREF_TYPE = wellbore
                var xrefFilters = new List<AppFilter>
                {
                    new AppFilter
                    {
                        FieldName = "UWI",
                        FilterValue = parentUwi,
                        Operator = "="
                    },
                    new AppFilter
                    {
                        FieldName = "XREF_TYPE",
                        FilterValue = wellboreXrefType,
                        Operator = "="
                    }
                };

                var xrefResults = await wellXrefRepo.GetAsync(xrefFilters);
                var wellXrefs = xrefResults.Cast<WELL_XREF>().ToList();

                if (!wellXrefs.Any())
                {
                    return new List<WELL>();
                }

                // Get the wellbore UWIs from the xref records
                // The UWI in WELL_XREF identifies the wellbore well (same UWI structure)
                var wellboreUwis = new HashSet<string>();
                foreach (var xref in wellXrefs)
                {
                    // For wellbores, the xref UWI might be the same or different structure
                    // Get UWI from xref - this should identify the wellbore well
                    var xrefUwiProp = wellXrefType.GetProperty("UWI", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
                    if (xrefUwiProp != null)
                    {
                        var uwi = xrefUwiProp.GetValue(xref)?.ToString();
                        if (!string.IsNullOrWhiteSpace(uwi) && uwi != parentUwi)
                        {
                            wellboreUwis.Add(uwi);
                        }
                    }
                }

                if (!wellboreUwis.Any())
                {
                    // If no distinct UWIs found, wellbores might share the same UWI structure
                    // In that case, query WELL table directly filtering by well_level_type
                    // For now, return empty list - the relationship structure may vary
                    return new List<WELL>();
                }

                // Now query WELL table for these wellbore UWIs
                var wellMetadata = await _metadata.GetTableMetadataAsync("WELL");
                if (wellMetadata == null)
                {
                    _logger?.LogWarning("WELL table metadata not found");
                    return new List<WELL>();
                }

                var wellEntityType = typeof(WELL);
                var wellRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    wellEntityType, _connectionName, "WELL", null);

                // Filter WELL table by wellbore UWIs
                var wellFilters = new List<AppFilter>();
                
                // Query each UWI (repository should handle this efficiently)
                foreach (var uwi in wellboreUwis)
                {
                    wellFilters.Add(new AppFilter
                    {
                        FieldName = "UWI",
                        FilterValue = uwi,
                        Operator = "="
                    });
                }

                // Note: Wellbores are WELL records with specific well_level_type
                // The well_level_type field on WELL table identifies them as wellbores
                // This is validated via WELL_XREF filtering above

                if (additionalFilters != null)
                    wellFilters.AddRange(additionalFilters);

                var wellResults = await wellRepo.GetAsync(wellFilters);
                
                // Return PPDM models directly
                return wellResults.Cast<WELL>().ToList();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error getting wellbores for well {wellId} in field: {fieldId}");
                throw;
            }
        }

        public async Task<List<FACILITY>> GetFacilitiesForFieldAsync(string fieldId, List<AppFilter>? additionalFilters = null)
        {
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("FACILITY");
                if (metadata == null)
                {
                    _logger?.LogWarning("FACILITY table metadata not found");
                    return new List<FACILITY>();
                }

                var entityType = typeof(FACILITY);

                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, _connectionName, "FACILITY", null);

                var filters = new List<AppFilter>
                {
                    new AppFilter
                    {
                        FieldName = "FIELD_ID",
                        FilterValue = _defaults.FormatIdForTable("FACILITY", fieldId),
                        Operator = "="
                    }
                };

                if (additionalFilters != null)
                    filters.AddRange(additionalFilters);

                var results = await repo.GetAsync(filters);
                
                // Return PPDM models directly
                return results.Cast<FACILITY>().ToList();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error getting facilities for field: {fieldId}");
                throw;
            }
        }

        public async Task<FACILITY> CreateFacilityForFieldAsync(string fieldId, FacilityRequest facilityData, string userId)
        {
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("FACILITY");
                if (metadata == null)
                {
                    throw new InvalidOperationException("FACILITY table metadata not found");
                }

                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}");
                if (entityType == null)
                {
                    throw new InvalidOperationException($"Entity type not found for FACILITY: {metadata.EntityTypeName}");
                }

                // Convert DTO to PPDM model
                var facilityEntity = _mappingService.ConvertDTOToPPDMModelRuntime(facilityData, typeof(FacilityRequest), entityType);
                
                // Set FIELD_ID automatically using reflection
                var fieldIdProp = entityType.GetProperty("FIELD_ID", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
                if (fieldIdProp != null && fieldIdProp.CanWrite)
                {
                    fieldIdProp.SetValue(facilityEntity, _defaults.FormatIdForTable("FACILITY", fieldId));
                }

                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, _connectionName, "FACILITY");

                var result = await repo.InsertAsync(facilityEntity, userId);
                
                // Convert PPDM model back to DTO
                return (FacilityResponse)_mappingService.ConvertPPDMModelToDTORuntime(result, typeof(FacilityResponse), entityType);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error creating facility for field: {fieldId}");
                throw;
            }
        }

        public async Task<List<PIPELINE>> GetPipelinesForFieldAsync(string fieldId, List<AppFilter>? additionalFilters = null)
        {
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("PIPELINE");
                if (metadata == null)
                {
                    _logger?.LogWarning("PIPELINE table metadata not found");
                    return new List<PIPELINE>();
                }

                var entityType = typeof(PIPELINE);

                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, _connectionName, "PIPELINE", null);

                var filters = new List<AppFilter>
                {
                    new AppFilter
                    {
                        FieldName = "FIELD_ID",
                        FilterValue = _defaults.FormatIdForTable("PIPELINE", fieldId),
                        Operator = "="
                    }
                };

                if (additionalFilters != null)
                    filters.AddRange(additionalFilters);

                var results = await repo.GetAsync(filters);
                
                // Return PPDM models directly
                return results.Cast<PIPELINE>().ToList();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error getting pipelines for field: {fieldId}");
                throw;
            }
        }

        public async Task<PIPELINE> CreatePipelineForFieldAsync(string fieldId, PipelineRequest pipelineData, string userId)
        {
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("PIPELINE");
                if (metadata == null)
                {
                    throw new InvalidOperationException("PIPELINE table metadata not found");
                }

                var entityType = typeof(PIPELINE);

                // Convert DTO to PPDM model
                var pipelineEntity = _mappingService.ConvertDTOToPPDMModel<PIPELINE, PipelineRequest>(pipelineData);
                
                // Set FIELD_ID automatically using reflection
                var fieldIdProp = entityType.GetProperty("FIELD_ID", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
                if (fieldIdProp != null && fieldIdProp.CanWrite)
                {
                    fieldIdProp.SetValue(pipelineEntity, _defaults.FormatIdForTable("PIPELINE", fieldId));
                }

                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, _connectionName, "PIPELINE", null);

                var result = await repo.InsertAsync(pipelineEntity, userId);
                
                // Return PPDM model directly
                return (PIPELINE)result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error creating pipeline for field: {fieldId}");
                throw;
            }
        }
    }
}

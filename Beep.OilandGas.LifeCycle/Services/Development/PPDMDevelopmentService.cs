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
using Beep.OilandGas.GasLift;
using Beep.OilandGas.GasLift.Calculations;
using Beep.OilandGas.Models.Data.GasLift;
using Beep.OilandGas.PipelineAnalysis;
using Beep.OilandGas.PipelineAnalysis.Calculations;

using Beep.OilandGas.CompressorAnalysis.Calculations;

using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Beep.OilandGas.Models.Data.PipelineAnalysis;
using Beep.OilandGas.Models.Data.CompressorAnalysis;
using Beep.OilandGas.PPDM.Models;
using Beep.OilandGas.Models.Data.LifeCycle;

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

        public async Task<FacilityResponse> CreateFacilityForFieldAsync(string fieldId, FacilityRequest facilityData, string userId)
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
                return _mappingService.ConvertPPDMModelToDTORuntime(result, typeof(FacilityResponse), entityType) as FacilityResponse 
                    ?? throw new InvalidOperationException("Failed to convert facility entity to FacilityResponse");
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

        #region Gas Lift Integration

        /// <summary>
        /// Analyzes gas lift potential for a well
        /// </summary>
        public async Task<GAS_LIFT_POTENTIAL_RESULT> AnalyzeGasLiftPotentialAsync(
            string fieldId,
            string wellId,
            decimal minGasInjectionRate = 100m,
            decimal maxGasInjectionRate = 5000m,
            int numberOfPoints = 50,
            string? userId = null)
        {
            try
            {
                _logger?.LogInformation("Analyzing gas lift potential for well {WellId} in field {FieldId}", wellId, fieldId);

                // Get well properties from PPDM
                var wellProperties = await GetGasLiftWellPropertiesAsync(fieldId, wellId);

                // Perform gas lift potential analysis
                var result = GasLiftPotentialCalculator.AnalyzeGasLiftPotential(
                    wellProperties,
                    minGasInjectionRate,
                    maxGasInjectionRate,
                    numberOfPoints);

                // Store results in WELL_EQUIPMENT table
                await StoreGasLiftPotentialResultsAsync(wellId, result, userId ?? "SYSTEM");

                _logger?.LogInformation("Gas lift potential analysis completed for well {WellId}. Optimal injection rate: {Rate} Mscf/day, Max production: {Production} bbl/day",
                    wellId, result.OPTIMAL_GAS_INJECTION_RATE, result.MAXIMUM_PRODUCTION_RATE);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error analyzing gas lift potential for well {WellId}", wellId);
                throw;
            }
        }

        /// <summary>
        /// Designs gas lift valves for a well (US field units)
        /// </summary>
        public async Task<GAS_LIFT_VALVE_DESIGN_RESULT> DesignGasLiftValvesAsync(
            string fieldId,
            string wellId,
            decimal gasInjectionPressure,
            int numberOfValves = 5,
            bool useSIUnits = false,
            string? userId = null)
        {
            try
            {
                _logger?.LogInformation("Designing gas lift valves for well {WellId} in field {FieldId}", wellId, fieldId);

                // Get well properties from PPDM
                var wellProperties = await GetGasLiftWellPropertiesAsync(fieldId, wellId);

                // Design valves
                GAS_LIFT_VALVE_DESIGN_RESULT result;
                if (useSIUnits)
                {
                    result = GasLiftValveDesignCalculator.DesignValvesSI(
                        wellProperties, gasInjectionPressure, numberOfValves);
                }
                else
                {
                    result = GasLiftValveDesignCalculator.DesignValvesUS(
                        wellProperties, gasInjectionPressure, numberOfValves);
                }

                // Store valve design in WELL_EQUIPMENT table
                await StoreGasLiftValveDesignAsync(wellId, result, userId ?? "SYSTEM");

                _logger?.LogInformation("Gas lift valve design completed for well {WellId}. Designed {Count} valves",
                    wellId, result.Valves.Count);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error designing gas lift valves for well {WellId}", wellId);
                throw;
            }
        }

        /// <summary>
        /// Calculates gas lift valve spacing
        /// </summary>
        public async Task<GasLiftValveSpacingResult> CalculateGasLiftValveSpacingAsync(
            string fieldId,
            string wellId,
            decimal gasInjectionPressure,
            int numberOfValves = 5,
            string spacingMethod = "EQUAL_PRESSURE_DROP", // EQUAL_PRESSURE_DROP or EQUAL_DEPTH
            string? userId = null)
        {
            try
            {
                _logger?.LogInformation("Calculating gas lift valve spacing for well {WellId} in field {FieldId}", wellId, fieldId);

                // Get well properties from PPDM
                var wellProperties = await GetGasLiftWellPropertiesAsync(fieldId, wellId);

                // Calculate valve spacing
                GasLiftValveSpacingResult result;
                if (spacingMethod == "EQUAL_DEPTH")
                {
                    result = GasLiftValveSpacingCalculator.CalculateEqualDepthSpacing(
                        wellProperties, gasInjectionPressure, numberOfValves);
                }
                else
                {
                    result = GasLiftValveSpacingCalculator.CalculateEqualPressureDropSpacing(
                        wellProperties, gasInjectionPressure, numberOfValves);
                }

                // Store spacing results
                await StoreGasLiftValveSpacingAsync(wellId, result, spacingMethod, userId ?? "SYSTEM");

                _logger?.LogInformation("Gas lift valve spacing calculated for well {WellId}. Method: {Method}",
                    wellId, spacingMethod);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error calculating gas lift valve spacing for well {WellId}", wellId);
                throw;
            }
        }

        #endregion

        #region Gas Lift Helper Methods

        /// <summary>
        /// Retrieves well properties from PPDM for gas lift analysis
        /// </summary>
        private async Task<GAS_LIFT_WELL_PROPERTIES> GetGasLiftWellPropertiesAsync(string fieldId, string wellId)
        {
            try
            {
                // Get well data from PPDM
                var wellMetadata = await _metadata.GetTableMetadataAsync("WELL");
                if (wellMetadata == null)
                {
                    throw new InvalidOperationException("WELL table metadata not found");
                }

                var wellRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(WELL), _connectionName, "WELL", null);

                var wellFilters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "WELL_ID", Operator = "=", FilterValue = wellId },
                    new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = fieldId }
                };

                var wells = await wellRepo.GetAsync(wellFilters);
                var wellsList = wells?.ToList() ?? new List<object>();
                if (wellsList.Count == 0)
                {
                    throw new InvalidOperationException($"Well {wellId} not found in field {fieldId}");
                }

                var well = wellsList[0] as WELL;
                if (well == null)
                {
                    throw new InvalidOperationException($"Failed to retrieve well {wellId}");
                }

                // Get wellbore/tubing data from WELL_EQUIPMENT or WELLBORE
                // This is a simplified implementation - in production, you would retrieve actual values
                var wellProperties = new GAS_LIFT_WELL_PROPERTIES
                {
                    WellDepth = !string.IsNullOrEmpty(well.FINAL_TD_OUOM) && well.FINAL_TD_OUOM == "FT" 
                        ? well.FINAL_TD 
                        : 10000m, // Default 10,000 feet
                    TubingDiameter = 3, // Default 3" - would retrieve from WELL_EQUIPMENT
                    CasingDiameter = 7.0m, // Default - would retrieve from WELL_EQUIPMENT
                    WellheadPressure = 100m, // Default - would retrieve from well test or production data
                    BottomHolePressure = 2000m, // Default - would retrieve from well test or reservoir data
                    WellheadTemperature = 520m, // Default 60Â°F = 520Â°R
                    BottomHoleTemperature = 580m, // Default 120Â°F = 580Â°R
                    OilGravity = 35m, // Default 35 API - would retrieve from reservoir/fluid data
                    WaterCut = 0.3m, // Default 30% - would retrieve from production data
                    GasOilRatio = 500m, // Default 500 scf/bbl - would retrieve from production data
                    GasSpecificGravity = 0.65m, // Default - would retrieve from gas analysis
                    DesiredProductionRate = 2000m // Default 2000 bbl/day
                };

                _logger?.LogWarning("Using default values for some well properties. For accurate analysis, provide well properties in request or ensure PPDM data is complete.");

                return wellProperties;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving well properties for gas lift analysis");
                throw;
            }
        }

        /// <summary>
        /// Stores gas lift potential analysis results in WELL_EQUIPMENT table
        /// </summary>
        private async Task StoreGasLiftPotentialResultsAsync(
            string wellId,
            GAS_LIFT_POTENTIAL_RESULT result,
            string userId)
        {
            try
            {
                var equipmentMetadata = await _metadata.GetTableMetadataAsync("WELL_EQUIPMENT");
                if (equipmentMetadata == null)
                {
                    _logger?.LogWarning("WELL_EQUIPMENT table metadata not found, skipping storage");
                    return;
                }

                var equipmentRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(Dictionary<string, object>), _connectionName, "WELL_EQUIPMENT", null);

                var equipmentRecord = new Dictionary<string, object>
                {
                    ["WELL_EQUIPMENT_ID"] = Guid.NewGuid().ToString(),
                    ["WELL_ID"] = wellId,
                    ["EQUIPMENT_TYPE"] = "GAS_LIFT_SYSTEM",
                    ["EQUIPMENT_NAME"] = "Gas Lift Potential Analysis",
                    ["DESCRIPTION"] = $"Optimal injection rate: {result.OPTIMAL_GAS_INJECTION_RATE:F2} Mscf/day, Max production: {result.MAXIMUM_PRODUCTION_RATE:F2} bbl/day",
                    ["EQUIPMENT_DATA_JSON"] = JsonSerializer.Serialize(new
                    {
                        OptimalGasInjectionRate = result.OPTIMAL_GAS_INJECTION_RATE,
                        MaximumProductionRate = result.MAXIMUM_PRODUCTION_RATE,
                        OptimalGasLiquidRatio = result.OPTIMAL_GAS_LIQUID_RATIO,
                        PerformancePoints = result.PerformancePoints.Select(p => new
                        {
                            p.GasInjectionRate,
                            p.ProductionRate,
                            p.GasLiquidRatio,
                            p.BOTTOM_HOLE_PRESSURE
                        })
                    })
                };

                await equipmentRepo.InsertAsync(equipmentRecord, userId);
                _logger?.LogInformation("Stored gas lift potential results for well {WellId}", wellId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error storing gas lift potential results for well {WellId}", wellId);
                // Don't throw - storage failure shouldn't fail the operation
            }
        }

        /// <summary>
        /// Stores gas lift valve design in WELL_EQUIPMENT table
        /// </summary>
        private async Task StoreGasLiftValveDesignAsync(
            string wellId,
            GAS_LIFT_VALVE_DESIGN_RESULT result,
            string userId)
        {
            try
            {
                var equipmentMetadata = await _metadata.GetTableMetadataAsync("WELL_EQUIPMENT");
                if (equipmentMetadata == null)
                {
                    _logger?.LogWarning("WELL_EQUIPMENT table metadata not found, skipping storage");
                    return;
                }

                var equipmentRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(Dictionary<string, object>), _connectionName, "WELL_EQUIPMENT", null);

                // Store each valve as a separate equipment record
                foreach (var valve in result.Valves)
                {
                    var equipmentRecord = new Dictionary<string, object>
                    {
                        ["WELL_EQUIPMENT_ID"] = Guid.NewGuid().ToString(),
                        ["WELL_ID"] = wellId,
                        ["EQUIPMENT_TYPE"] = "GAS_LIFT_VALVE",
                        ["EQUIPMENT_NAME"] = $"Gas Lift Valve at {valve.Depth:F0} ft",
                        ["DESCRIPTION"] = $"Port size: {valve.PortSize:F3} in, Opening pressure: {valve.OpeningPressure:F2} psia",
                        ["EQUIPMENT_DATA_JSON"] = JsonSerializer.Serialize(new
                        {
                            valve.Depth,
                            valve.PortSize,
                            valve.OpeningPressure,
                            valve.ClosingPressure,
                            valve.ValveType,
                            valve.Temperature,
                            valve.GasInjectionRate
                        })
                    };

                    await equipmentRepo.InsertAsync(equipmentRecord, userId);
                }

                _logger?.LogInformation("Stored gas lift valve design for well {WellId} ({Count} valves)", wellId, result.Valves.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error storing gas lift valve design for well {WellId}", wellId);
                // Don't throw - storage failure shouldn't fail the operation
            }
        }

        /// <summary>
        /// Stores gas lift valve spacing results
        /// </summary>
        private async Task StoreGasLiftValveSpacingAsync(
            string wellId,
            GasLiftValveSpacingResult result,
            string spacingMethod,
            string userId)
        {
            try
            {
                var equipmentMetadata = await _metadata.GetTableMetadataAsync("WELL_EQUIPMENT");
                if (equipmentMetadata == null)
                {
                    _logger?.LogWarning("WELL_EQUIPMENT table metadata not found, skipping storage");
                    return;
                }

                var equipmentRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(Dictionary<string, object>), _connectionName, "WELL_EQUIPMENT", null);

                var equipmentRecord = new Dictionary<string, object>
                {
                    ["WELL_EQUIPMENT_ID"] = Guid.NewGuid().ToString(),
                    ["WELL_ID"] = wellId,
                    ["EQUIPMENT_TYPE"] = "GAS_LIFT_VALVE_SPACING",
                    ["EQUIPMENT_NAME"] = "Gas Lift Valve Spacing",
                    ["DESCRIPTION"] = $"Spacing method: {spacingMethod}, {result.NumberOfValves} valves",
                    ["EQUIPMENT_DATA_JSON"] = JsonSerializer.Serialize(new
                    {
                        SpacingMethod = spacingMethod,
                        TotalDepthCoverage = result.TotalDepthCoverage,
                        NumberOfValves = result.NumberOfValves,
                        ValveDepths = result.ValveDepths,
                        OpeningPressures = result.OpeningPressures
                    })
                };

                await equipmentRepo.InsertAsync(equipmentRecord, userId);
                _logger?.LogInformation("Stored gas lift valve spacing for well {WellId}", wellId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error storing gas lift valve spacing for well {WellId}", wellId);
                // Don't throw - storage failure shouldn't fail the operation
            }
        }

        #endregion

        #region Pipeline Analysis Integration

        /// <summary>
        /// Analyzes pipeline capacity for gas or liquid pipelines
        /// </summary>
        public async Task<PIPELINE_CAPACITY_RESULT> AnalyzePipelineCapacityAsync(
            string fieldId,
            string pipelineId,
            string pipelineType = "GAS", // GAS or LIQUID
            decimal? gasFlowRate = null,
            decimal? liquidFlowRate = null,
            decimal? gasSpecificGravity = null,
            decimal? liquidSpecificGravity = null,
            string? userId = null)
        {
            try
            {
                _logger?.LogInformation("Analyzing pipeline capacity for pipeline {PipelineId} in field {FieldId}, type: {Type}",
                    pipelineId, fieldId, pipelineType);

                // Get pipeline properties from PPDM
                var PIPELINE_PROPERTIES = await GetPipelinePropertiesFromPPDMAsync(fieldId, pipelineId);

                PIPELINE_CAPACITY_RESULT result;
                if (pipelineType.ToUpper() == "GAS")
                {
                    // Gas pipeline capacity
                    var gasFlowProperties = new GAS_PIPELINE_FLOW_PROPERTIES
                    {
                        Pipeline = PIPELINE_PROPERTIES,
                        GasFlowRate = gasFlowRate ?? 1000m, // Default
                        GasSpecificGravity = gasSpecificGravity ?? 0.65m,
                        GasMolecularWeight = (gasSpecificGravity ?? 0.65m) * 28.9645m,
                        BasePressure = 14.7m,
                        BaseTemperature = 520m
                    };

                    result = PipelineCapacityCalculator.CalculateGasPipelineCapacity(gasFlowProperties);
                }
                else
                {
                    // Liquid pipeline capacity
                    var liquidFlowProperties = new LIQUID_PIPELINE_FLOW_PROPERTIES
                    {
                        Pipeline = PIPELINE_PROPERTIES,
                        LiquidFlowRate = liquidFlowRate ?? 1000m, // Default
                        LiquidSpecificGravity = liquidSpecificGravity ?? 0.85m,
                        LiquidViscosity = 1.0m
                    };

                    result = PipelineCapacityCalculator.CalculateLiquidPipelineCapacity(liquidFlowProperties);
                }

                // Store results in PIPELINE table
                await StorePipelineCapacityResultsAsync(pipelineId, result, pipelineType, userId ?? "SYSTEM");

                _logger?.LogInformation("Pipeline capacity analysis completed for pipeline {PipelineId}. Max flow rate: {FlowRate}, Pressure drop: {PressureDrop}",
                    pipelineId, result.MAXIMUM_FLOW_RATE, result.PRESSURE_DROP);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error analyzing pipeline capacity for pipeline {PipelineId}", pipelineId);
                throw;
            }
        }

        /// <summary>
        /// Analyzes pipeline flow (flow rate or pressure drop) for gas or liquid pipelines
        /// </summary>
        public async Task<PIPELINE_FLOW_ANALYSIS_RESULT> AnalyzePipelineFlowAsync(
            string fieldId,
            string pipelineId,
            string pipelineType = "GAS", // GAS or LIQUID
            decimal? gasFlowRate = null,
            decimal? liquidFlowRate = null,
            decimal? gasSpecificGravity = null,
            decimal? liquidSpecificGravity = null,
            string? userId = null)
        {
            try
            {
                _logger?.LogInformation("Analyzing pipeline flow for pipeline {PipelineId} in field {FieldId}, type: {Type}",
                    pipelineId, fieldId, pipelineType);

                // Get pipeline properties from PPDM
                var PIPELINE_PROPERTIES = await GetPipelinePropertiesFromPPDMAsync(fieldId, pipelineId);

                PIPELINE_FLOW_ANALYSIS_RESULT result;
                if (pipelineType.ToUpper() == "GAS")
                {
                    // Gas pipeline flow
                    var gasFlowProperties = new GAS_PIPELINE_FLOW_PROPERTIES
                    {
                        Pipeline = PIPELINE_PROPERTIES,
                        GasFlowRate = gasFlowRate ?? 1000m,
                        GasSpecificGravity = gasSpecificGravity ?? 0.65m,
                        GasMolecularWeight = (gasSpecificGravity ?? 0.65m) * 28.9645m,
                        BasePressure = 14.7m,
                        BaseTemperature = 520m
                    };

                    result = PipelineFlowCalculator.CalculateGasFlow(gasFlowProperties);
                }
                else
                {
                    // Liquid pipeline flow
                    var liquidFlowProperties = new LIQUID_PIPELINE_FLOW_PROPERTIES
                    {
                        Pipeline = PIPELINE_PROPERTIES,
                        LiquidFlowRate = liquidFlowRate ?? 1000m,
                        LiquidSpecificGravity = liquidSpecificGravity ?? 0.85m,
                        LiquidViscosity = 1.0m
                    };

                    result = PipelineFlowCalculator.CalculateLiquidFlow(liquidFlowProperties);
                }

                // Store results in PIPELINE table
                await StorePipelineFlowResultsAsync(pipelineId, result, pipelineType, userId ?? "SYSTEM");

                _logger?.LogInformation("Pipeline flow analysis completed for pipeline {PipelineId}. Flow rate: {FlowRate}, Pressure drop: {PressureDrop}",
                    pipelineId, result.FLOW_RATE, result.PRESSURE_DROP);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error analyzing pipeline flow for pipeline {PipelineId}", pipelineId);
                throw;
            }
        }

        #endregion

        #region Pipeline Analysis Helper Methods

        /// <summary>
        /// Retrieves pipeline properties from PPDM for analysis
        /// </summary>
        private async Task<PIPELINE_PROPERTIES> GetPipelinePropertiesFromPPDMAsync(string fieldId, string pipelineId)
        {
            try
            {
                // Get pipeline data from PPDM
                var pipelineMetadata = await _metadata.GetTableMetadataAsync("PIPELINE");
                if (pipelineMetadata == null)
                {
                    throw new InvalidOperationException("PIPELINE table metadata not found");
                }

                var pipelineRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(PIPELINE), _connectionName, "PIPELINE", null);

                var pipelineFilters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "PIPELINE_ID", Operator = "=", FilterValue = pipelineId },
                    new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = fieldId }
                };

                var pipelines = await pipelineRepo.GetAsync(pipelineFilters);
                var pipelinesList = pipelines?.ToList() ?? new List<object>();
                if (pipelinesList.Count == 0)
                {
                    throw new InvalidOperationException($"Pipeline {pipelineId} not found in field {fieldId}");
                }

                var pipeline = pipelinesList[0] as PIPELINE;
                if (pipeline == null)
                {
                    throw new InvalidOperationException($"Failed to retrieve pipeline {pipelineId}");
                }

                // Map PPDM pipeline to PIPELINE_PROPERTIES
                // This is a simplified implementation - in production, you would retrieve actual values from PIPELINE and related tables
                var PIPELINE_PROPERTIES = new PIPELINE_PROPERTIES
                {
                    Diameter = !string.IsNullOrEmpty(pipeline.DIAMETER_OUOM) && pipeline.DIAMETER_OUOM == "IN"
                        ? (pipeline.DIAMETER ?? 12m)
                        : 12m, // Default 12 inches
                    Length = !string.IsNullOrEmpty(pipeline.LENGTH_OUOM) && pipeline.LENGTH_OUOM == "FT"
                        ? (pipeline.LENGTH ?? 50000m)
                        : 50000m, // Default 50,000 feet
                    Roughness = 0.00015m, // Default for steel pipe
                    ElevationChange = 0m, // Would retrieve from pipeline route data
                    InletPressure = 1000m, // Default - would retrieve from operational data
                    OutletPressure = 500m, // Default - would retrieve from operational data
                    AverageTemperature = 540m // Default 80Â°F = 540Â°R
                };

                _logger?.LogWarning("Using default values for some pipeline properties. For accurate analysis, provide pipeline properties in request or ensure PPDM data is complete.");

                return PIPELINE_PROPERTIES;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving pipeline properties for analysis");
                throw;
            }
        }

        /// <summary>
        /// Stores pipeline capacity analysis results in PIPELINE table
        /// </summary>
        private async Task StorePipelineCapacityResultsAsync(
            string pipelineId,
            PIPELINE_CAPACITY_RESULT result,
            string pipelineType,
            string userId)
        {
            try
            {
                var pipelineRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(PIPELINE), _connectionName, "PIPELINE", null);

                // Update pipeline with capacity results
                // Note: In production, you might want to store in a separate PIPELINE_ANALYSIS table
                var updateData = new Dictionary<string, object>
                {
                    ["PIPELINE_ID"] = pipelineId,
                    ["CAPACITY_ANALYSIS_JSON"] = JsonSerializer.Serialize(new
                    {
                        PipelineType = pipelineType,
                        MaximumFlowRate = result.MAXIMUM_FLOW_RATE,
                        PressureDrop = result.PRESSURE_DROP,
                        FlowVelocity = result.FLOW_VELOCITY,
                        ReynoldsNumber = result.REYNOLDS_NUMBER,
                        FrictionFactor = result.FRICTION_FACTOR,
                        PressureGradient = result.PRESSURE_GRADIENT,
                        OutletPressure = result.OUTLET_PRESSURE,
                        AnalysisDate = DateTime.UtcNow
                    })
                };

                await pipelineRepo.UpdateAsync(updateData, userId);
                _logger?.LogInformation("Stored pipeline capacity results for pipeline {PipelineId}", pipelineId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error storing pipeline capacity results for pipeline {PipelineId}", pipelineId);
                // Don't throw - storage failure shouldn't fail the operation
            }
        }

        /// <summary>
        /// Stores pipeline flow analysis results in PIPELINE table
        /// </summary>
        private async Task StorePipelineFlowResultsAsync(
            string pipelineId,
            PIPELINE_FLOW_ANALYSIS_RESULT result,
            string pipelineType,
            string userId)
        {
            try
            {
                var pipelineRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(PIPELINE), _connectionName, "PIPELINE", null);

                // Update pipeline with flow results
                var updateData = new Dictionary<string, object>
                {
                    ["PIPELINE_ID"] = pipelineId,
                    ["FLOW_ANALYSIS_JSON"] = JsonSerializer.Serialize(new
                    {
                        PipelineType = pipelineType,
                        FlowRate = result.FLOW_RATE,
                        PressureDrop = result.PRESSURE_DROP,
                        FlowVelocity = result.FLOW_VELOCITY,
                        ReynoldsNumber = result.REYNOLDS_NUMBER,
                        FrictionFactor = result.FRICTION_FACTOR,
                        FlowRegime = result.FLOW_REGIME,
                        AnalysisDate = DateTime.UtcNow
                    })
                };

                await pipelineRepo.UpdateAsync(updateData, userId);
                _logger?.LogInformation("Stored pipeline flow results for pipeline {PipelineId}", pipelineId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error storing pipeline flow results for pipeline {PipelineId}", pipelineId);
                // Don't throw - storage failure shouldn't fail the operation
            }
        }

        #endregion

        #region Compressor Analysis Integration

        /// <summary>
        /// Analyzes compressor power requirements for centrifugal or reciprocating compressors
        /// </summary>
        public async Task<COMPRESSOR_POWER_RESULT> AnalyzeCompressorPowerAsync(
            string fieldId,
            string facilityId,
            string compressorType = "CENTRIFUGAL", // CENTRIFUGAL or RECIPROCATING
            decimal? suctionPressure = null,
            decimal? dischargePressure = null,
            decimal? gasFlowRate = null,
            decimal? gasSpecificGravity = null,
            bool useSIUnits = false,
            string? userId = null)
        {
            try
            {
                _logger?.LogInformation("Analyzing compressor power for facility {FacilityId} in field {FieldId}, type: {Type}",
                    facilityId, fieldId, compressorType);

                // Get compressor operating conditions from PPDM or request
                var operatingConditions = await GetCompressorOperatingConditionsAsync(
                    fieldId, facilityId, suctionPressure, dischargePressure, gasFlowRate, gasSpecificGravity);

                COMPRESSOR_POWER_RESULT result;
                if (compressorType.ToUpper() == "RECIPROCATING")
                {
                    // Reciprocating compressor
                    var compressorProperties = await GetReciprocatingCompressorPropertiesAsync(
                        facilityId, operatingConditions);
                    result = ReciprocatingCompressorCalculator.CalculatePower(compressorProperties, useSIUnits);
                }
                else
                {
                    // Centrifugal compressor
                    var compressorProperties = await GetCentrifugalCompressorPropertiesAsync(
                        facilityId, operatingConditions);
                    result = CentrifugalCompressorCalculator.CalculatePower(compressorProperties, useSIUnits);
                }

                // Store results in FACILITY_EQUIPMENT table
                await StoreCompressorPowerResultsAsync(facilityId, result, compressorType, userId ?? "SYSTEM");

                _logger?.LogInformation("Compressor power analysis completed for facility {FacilityId}. Brake HP: {BHP}, Motor HP: {MotorHP}",
                    facilityId, result.BRAKE_HORSEPOWER, result.MOTOR_HORSEPOWER);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error analyzing compressor power for facility {FacilityId}", facilityId);
                throw;
            }
        }

        /// <summary>
        /// Analyzes compressor pressure requirements
        /// </summary>
        public async Task<COMPRESSOR_PRESSURE_RESULT> AnalyzeCompressorPressureAsync(
            string fieldId,
            string facilityId,
            decimal requiredFlowRate,
            decimal? maxPower = null,
            decimal? compressorEfficiency = null,
            decimal? suctionPressure = null,
            decimal? gasSpecificGravity = null,
            string? userId = null)
        {
            try
            {
                _logger?.LogInformation("Analyzing compressor pressure for facility {FacilityId} in field {FieldId}",
                    facilityId, fieldId);

                // Get compressor operating conditions
                var operatingConditions = await GetCompressorOperatingConditionsAsync(
                    fieldId, facilityId, suctionPressure, null, requiredFlowRate, gasSpecificGravity);

                // Calculate required pressure
                var result = CompressorPressureCalculator.CalculateRequiredPressure(
                    operatingConditions,
                    requiredFlowRate,
                    maxPower ?? 1000m,
                    compressorEfficiency ?? 0.75m);

                // Store results in FACILITY_EQUIPMENT table
                await StoreCompressorPressureResultsAsync(facilityId, result, userId ?? "SYSTEM");

                _logger?.LogInformation("Compressor pressure analysis completed for facility {FacilityId}. Required discharge pressure: {Pressure} psia",
                    facilityId, result.REQUIRED_DISCHARGE_PRESSURE);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error analyzing compressor pressure for facility {FacilityId}", facilityId);
                throw;
            }
        }

        #endregion

        #region Compressor Analysis Helper Methods

        /// <summary>
        /// Retrieves compressor operating conditions from PPDM
        /// </summary>
        private async Task<COMPRESSOR_OPERATING_CONDITIONS> GetCompressorOperatingConditionsAsync(
            string fieldId,
            string facilityId,
            decimal? suctionPressure,
            decimal? dischargePressure,
            decimal? gasFlowRate,
            decimal? gasSpecificGravity)
        {
            try
            {
                // Get facility/equipment data from PPDM
                // This is a simplified implementation - in production, you would retrieve actual values
                var operatingConditions = new COMPRESSOR_OPERATING_CONDITIONS
                {
                    SuctionPressure = suctionPressure ?? 100m, // Default 100 psia
                    DischargePressure = dischargePressure ?? 500m, // Default 500 psia
                    SuctionTemperature = 520m, // Default 60Â°F = 520Â°R
                    DischargeTemperature = 600m, // Default 140Â°F = 600Â°R
                    GasFlowRate = gasFlowRate ?? 1000m, // Default 1000 Mscf/day
                    GasSpecificGravity = gasSpecificGravity ?? 0.65m,
                    GasMolecularWeight = (gasSpecificGravity ?? 0.65m) * 28.9645m,
                    CompressorEfficiency = 0.75m,
                    MechanicalEfficiency = 0.95m
                };

                _logger?.LogWarning("Using default values for some compressor operating conditions. For accurate analysis, provide values in request or ensure PPDM data is complete.");

                return operatingConditions;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving compressor operating conditions");
                throw;
            }
        }

        /// <summary>
        /// Gets centrifugal compressor properties from PPDM
        /// </summary>
        private async Task<CENTRIFUGAL_COMPRESSOR_PROPERTIES> GetCentrifugalCompressorPropertiesAsync(
            string facilityId,
            COMPRESSOR_OPERATING_CONDITIONS operatingConditions)
        {
            // This is a simplified implementation
            return new CENTRIFUGAL_COMPRESSOR_PROPERTIES
            {
                OperatingConditions = operatingConditions,
                PolytropicEfficiency = 0.75m,
                SpecificHeatRatio = 1.3m,
                NumberOfStages = 1,
                Speed = 3600 // Default 3600 RPM
            };
        }

        /// <summary>
        /// Gets reciprocating compressor properties from PPDM
        /// </summary>
        private async Task<RECIPROCATING_COMPRESSOR_PROPERTIES> GetReciprocatingCompressorPropertiesAsync(
            string facilityId,
            COMPRESSOR_OPERATING_CONDITIONS operatingConditions)
        {
            // This is a simplified implementation
            return new RECIPROCATING_COMPRESSOR_PROPERTIES
            {
                OperatingConditions = operatingConditions,
                CylinderDiameter = 12m, // Default 12 inches
                StrokeLength = 6m, // Default 6 inches
                RotationalSpeed = 300m, // Default 300 RPM
                NumberOfCylinders = 2,
                VolumetricEfficiency = 0.85m,
                ClearanceFactor = (int)0.05m
            };
        }

        /// <summary>
        /// Stores compressor power analysis results in FACILITY_EQUIPMENT table
        /// </summary>
        private async Task StoreCompressorPowerResultsAsync(
            string facilityId,
            COMPRESSOR_POWER_RESULT result,
            string compressorType,
            string userId)
        {
            try
            {
                var equipmentMetadata = await _metadata.GetTableMetadataAsync("FACILITY_EQUIPMENT");
                if (equipmentMetadata == null)
                {
                    _logger?.LogWarning("FACILITY_EQUIPMENT table metadata not found, skipping storage");
                    return;
                }

                var equipmentRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(Dictionary<string, object>), _connectionName, "FACILITY_EQUIPMENT", null);

                var equipmentRecord = new Dictionary<string, object>
                {
                    ["FACILITY_EQUIPMENT_ID"] = Guid.NewGuid().ToString(),
                    ["FACILITY_ID"] = facilityId,
                    ["EQUIPMENT_TYPE"] = "COMPRESSOR",
                    ["EQUIPMENT_NAME"] = $"{compressorType} Compressor Power Analysis",
                    ["DESCRIPTION"] = $"Brake HP: {result.BRAKE_HORSEPOWER:F2}, Motor HP: {result.MOTOR_HORSEPOWER:F2}",
                    ["EQUIPMENT_DATA_JSON"] = JsonSerializer.Serialize(new
                    {
                        CompressorType = compressorType,
                        TheoreticalPower = result.THEORETICAL_POWER,
                        BrakeHorsepower = result.BRAKE_HORSEPOWER,
                        MotorHorsepower = result.MOTOR_HORSEPOWER,
                        PowerConsumptionKW = result.POWER_CONSUMPTION_KW,
                        CompressionRatio = result.COMPRESSION_RATIO,
                        PolytropicHead = result.POLYTROPIC_HEAD,
                        AdiabaticHead = result.ADIABATIC_HEAD,
                        AnalysisDate = DateTime.UtcNow
                    })
                };

                await equipmentRepo.InsertAsync(equipmentRecord, userId);
                _logger?.LogInformation("Stored compressor power results for facility {FacilityId}", facilityId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error storing compressor power results for facility {FacilityId}", facilityId);
                // Don't throw - storage failure shouldn't fail the operation
            }
        }

        /// <summary>
        /// Stores compressor pressure analysis results in FACILITY_EQUIPMENT table
        /// </summary>
        private async Task StoreCompressorPressureResultsAsync(
            string facilityId,
            COMPRESSOR_PRESSURE_RESULT result,
            string userId)
        {
            try
            {
                var equipmentMetadata = await _metadata.GetTableMetadataAsync("FACILITY_EQUIPMENT");
                if (equipmentMetadata == null)
                {
                    _logger?.LogWarning("FACILITY_EQUIPMENT table metadata not found, skipping storage");
                    return;
                }

                var equipmentRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(Dictionary<string, object>), _connectionName, "FACILITY_EQUIPMENT", null);

                var equipmentRecord = new Dictionary<string, object>
                {
                    ["FACILITY_EQUIPMENT_ID"] = Guid.NewGuid().ToString(),
                    ["FACILITY_ID"] = facilityId,
                    ["EQUIPMENT_TYPE"] = "COMPRESSOR",
                    ["EQUIPMENT_NAME"] = "Compressor Pressure Analysis",
                    ["DESCRIPTION"] = $"Required discharge pressure: {result.REQUIRED_DISCHARGE_PRESSURE:F2} psia",
                    ["EQUIPMENT_DATA_JSON"] = JsonSerializer.Serialize(new
                    {
                        RequiredDischargePressure = result.REQUIRED_DISCHARGE_PRESSURE,
                        CompressionRatio = result.COMPRESSION_RATIO,
                        RequiredPower = result.REQUIRED_POWER,
                        DischargeTemperature = result.DISCHARGE_TEMPERATURE,
                        AnalysisDate = DateTime.UtcNow
                    })
                };

                await equipmentRepo.InsertAsync(equipmentRecord, userId);
                _logger?.LogInformation("Stored compressor pressure results for facility {FacilityId}", facilityId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error storing compressor pressure results for facility {FacilityId}", facilityId);
                // Don't throw - storage failure shouldn't fail the operation
            }
        }

        #endregion

        #region Development Planning Integration

        /// <summary>
        /// Plans development for a field using DevelopmentPlanning service
        /// </summary>
        public async Task<DevelopmentPlan> PlanDevelopmentAsync(string fieldId, CreateDevelopmentPlan planData, string userId)
        {
            try
            {
                _logger?.LogInformation("Planning development for field: {FieldId}", fieldId);

                // Create development plan entity in PPDM
                // For now, store plan data in a generic table or use existing structures
                // In full implementation, would use DevelopmentPlanService

                return new DevelopmentPlan
                {
                    PlanId = Guid.NewGuid().ToString(),
                    FieldId = fieldId,
                    PlanName = planData.PlanName ?? "Development Plan",
                    Status = "DRAFT",
                    CreatedDate = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error planning development for field: {FieldId}", fieldId);
                throw;
            }
        }

        /// <summary>
        /// Designs a well using DevelopmentPlanning service
        /// </summary>
        public async Task<WellPlan> DesignWellAsync(string fieldId, string planId, CreateWellPlan wellPlanData, string userId)
        {
            try
            {
                _logger?.LogInformation("Designing well for plan: {PlanId} in field: {FieldId}", planId, fieldId);

                // Create well design
                // In full implementation, would use DevelopmentPlanService to add well plan to development plan

                return new WellPlan
                {
                    WellPlanId = Guid.NewGuid().ToString(),
                    PlanId = planId,
                    WellName = wellPlanData.WELL_NAME ?? "Well",
                    WellType = wellPlanData.WELL_TYPE ?? "PRODUCTION",
                    Status = "DESIGNED",
                    CreatedDate = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error designing well for plan: {PlanId}", planId);
                throw;
            }
        }

        #endregion

        #region Drilling and Construction Integration

        /// <summary>
        /// Executes drilling operation using DrillingAndConstruction service
        /// </summary>
        public async Task<DRILLING_OPERATION> ExecuteDrillingAsync(string fieldId, string wellId, CREATE_DRILLING_OPERATION drillingData, string userId)
        {
            try
            {
                _logger?.LogInformation("Executing drilling for well: {WellId} in field: {FieldId}", wellId, fieldId);

                // Create drilling operation in PPDM DRILLING_OPERATION table
                var metadata = await _metadata.GetTableMetadataAsync("DRILLING_OPERATION");
                if (metadata == null)
                {
                    throw new InvalidOperationException("DRILLING_OPERATION table metadata not found");
                }

                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}") ?? typeof(object);
                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, _connectionName, "DRILLING_OPERATION", null);

                // Create drilling operation entity
                var drillingOp = Activator.CreateInstance(entityType);
                var wellIdProp = entityType.GetProperty("WELL_ID");
                var operationDateProp = entityType.GetProperty("OPERATION_DATE") ?? entityType.GetProperty("DRILLING_DATE");
                var statusProp = entityType.GetProperty("STATUS") ?? entityType.GetProperty("OPERATION_STATUS");

                if (wellIdProp != null)
                    wellIdProp.SetValue(drillingOp, _defaults.FormatIdForTable("DRILLING_OPERATION", wellId));
                if (operationDateProp != null)
                    operationDateProp.SetValue(drillingOp, drillingData.PlannedSpudDate ?? DateTime.UtcNow);
                if (statusProp != null)
                    statusProp.SetValue(drillingOp, "IN_PROGRESS");

                if (drillingOp is IPPDMEntity entity)
                    _commonColumnHandler.PrepareForInsert(entity, userId);
                var result = await repo.InsertAsync(drillingOp, userId);

                var operationId = result.GetType().GetProperty("ROW_ID")?.GetValue(result)?.ToString() 
                    ?? result.GetType().GetProperty("DRILLING_OPERATION_ID")?.GetValue(result)?.ToString()
                    ?? Guid.NewGuid().ToString();

                _logger?.LogInformation("Drilling operation created: {OperationId} for well: {WellId}", operationId, wellId);

                return new DRILLING_OPERATION
                {
                    OperationId = operationId,
                    WellUWI = wellId,
                    SpudDate = drillingData.PlannedSpudDate ?? DateTime.UtcNow,
                    Status = "IN_PROGRESS"
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error executing drilling for well: {WellId}", wellId);
                throw;
            }
        }

        /// <summary>
        /// Constructs facility using DrillingAndConstruction service
        /// </summary>
        public async Task<FacilityResponse> ConstructFacilityAsync(string fieldId, string facilityId, FacilityRequest facilityData, string userId)
        {
            try
            {
                _logger?.LogInformation("Constructing facility: {FacilityId} in field: {FieldId}", facilityId, fieldId);


                // Use existing CreateFacilityForFieldAsync method
                return await CreateFacilityForFieldAsync(fieldId, facilityData, userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error constructing facility: {FacilityId}", facilityId);
                throw;
            }
        }

        #endregion
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Production;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.ProductionForecasting;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.ChokeAnalysis;
using Beep.OilandGas.ChokeAnalysis.Calculations;

using Beep.OilandGas.SuckerRodPumping.Calculations;

using Beep.OilandGas.PPDM.Models;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Beep.OilandGas.Models.Data.ChokeAnalysis;
using Beep.OilandGas.Models.Data.SuckerRodPumping;

namespace Beep.OilandGas.LifeCycle.Services.Production
{
    /// <summary>
    /// Service for Production & Reserves data management
    /// Implements both IPPDMProductionService and IFieldProductionService
    /// </summary>
    public class PPDMProductionService : IPPDMProductionService, IFieldProductionService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly PPDMMappingService _mappingService;
        private readonly string _connectionName;
        private readonly ILogger<PPDMProductionService>? _logger;

        public PPDMProductionService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            PPDMMappingService mappingService,
            string connectionName = "PPDM39",
            ILogger<PPDMProductionService>? logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _mappingService = mappingService ?? throw new ArgumentNullException(nameof(mappingService));
            _connectionName = connectionName;
            _logger = logger;
        }

        public async Task<List<FIELD>> GetFieldsAsync(List<AppFilter>? filters = null)
        {
            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(FIELD), _connectionName, "FIELD");

            var results = await repo.GetAsync(filters ?? new List<AppFilter>());
            return results.Cast<FIELD>().ToList();
        }

        public async Task<List<POOL>> GetPoolsAsync(List<AppFilter>? filters = null)
        {
            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(POOL), _connectionName, "POOL");

            var results = await repo.GetAsync(filters ?? new List<AppFilter>());
            return results.Cast<POOL>().ToList();
        }

        public async Task<List<PDEN_VOL_SUMMARY>> GetProductionAsync(List<AppFilter>? filters = null)
        {
            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(PDEN_VOL_SUMMARY), _connectionName, "PDEN_VOL_SUMMARY");

            var results = await repo.GetAsync(filters ?? new List<AppFilter>());
            return results.Cast<PDEN_VOL_SUMMARY>().ToList();
        }

        public async Task<List<RESERVE_ENTITY>> GetReservesAsync(List<AppFilter>? filters = null)
        {
            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(RESERVE_ENTITY), _connectionName, "RESERVE_ENTITY");

            var results = await repo.GetAsync(filters ?? new List<AppFilter>());
            return results.Cast<RESERVE_ENTITY>().ToList();
        }

        public async Task<List<PDEN_VOL_SUMMARY>> GetProductionReportingAsync(List<AppFilter>? filters = null)
        {
            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(PDEN_VOL_SUMMARY), _connectionName, "PDEN_VOL_SUMMARY");

            var results = await repo.GetAsync(filters ?? new List<AppFilter>());
            return results.Cast<PDEN_VOL_SUMMARY>().ToList();
        }

        // ============================================
        // IFieldProductionService Implementation
        // ============================================

        public async Task<List<ProductionResponse>> GetProductionForFieldAsync(string fieldId, List<AppFilter>? additionalFilters = null)
        {
            try
            {
                // Production is linked through PDEN (Production Entity) which has a direct FIELD_ID column.
                // Query PDEN by FIELD_ID, then query PDEN_VOL_SUMMARY by PDEN_ID.
                var pdenRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(PDEN), _connectionName, "PDEN");

                var pdenRecords = await pdenRepo.GetAsync(new List<AppFilter>
                {
                    new AppFilter { FieldName = "FIELD_ID", FilterValue = _defaults.FormatIdForTable("PDEN", fieldId), Operator = "=" },
                    new AppFilter { FieldName = "ACTIVE_IND", FilterValue = "Y", Operator = "=" }
                });

                if (!pdenRecords.Any())
                    return new List<ProductionResponse>();

                var pdenIds = pdenRecords.OfType<PDEN>()
                    .Select(p => p.PDEN_ID)
                    .Where(id => !string.IsNullOrEmpty(id))
                    .ToList();

                if (!pdenIds.Any())
                    return new List<ProductionResponse>();

                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(PDEN_VOL_SUMMARY), _connectionName, "PDEN_VOL_SUMMARY");

                // Filter by PDEN_IDs (OR condition)
                var filters = new List<AppFilter>();
                foreach (var pdenId in pdenIds)
                {
                    filters.Add(new AppFilter
                    {
                        FieldName = "PDEN_ID",
                        FilterValue = _defaults.FormatIdForTable("PDEN_VOL_SUMMARY", pdenId),
                        Operator = "="
                    });
                }

                if (additionalFilters != null)
                    filters.AddRange(additionalFilters);

                var results = await repo.GetAsync(filters);

                // Convert PPDM models to DTOs
                var dtoList = _mappingService.ConvertPPDMModelListToDTOListRuntime(results, typeof(ProductionResponse), typeof(PDEN_VOL_SUMMARY));
                return dtoList.Cast<ProductionResponse>().ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error getting production for field: {fieldId}", ex);
            }
        }

        public async Task<ProductionResponse> CreateProductionForFieldAsync(string fieldId, ProductionRequest productionData, string userId)
        {
            // Convert DTO to PPDM model (PDEN_VOL_SUMMARY; FIELD_ID is carried on PDEN, not here)
            var productionEntity = _mappingService.ConvertDTOToPPDMModel<PDEN_VOL_SUMMARY, ProductionRequest>(productionData);

            var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(PDEN_VOL_SUMMARY), _connectionName, "PDEN_VOL_SUMMARY");

            var result = await repo.InsertAsync(productionEntity, userId);

            // Convert PPDM model back to DTO
            return _mappingService.ConvertPPDMModelToDTO<ProductionResponse, PDEN_VOL_SUMMARY>((PDEN_VOL_SUMMARY)result);
        }

        public async Task<List<ProductionResponse>> GetProductionByPoolForFieldAsync(string fieldId, string? poolId = null)
        {
            try
            {
                // Get production for field, then filter by pool if specified
                var production = await GetProductionForFieldAsync(fieldId);

                if (string.IsNullOrEmpty(poolId))
                    return production;

                // Filter by pool ID (assuming PDEN_VOL_SUMMARY has POOL_ID field)
                var formattedPoolId = _defaults.FormatIdForTable("PDEN_VOL_SUMMARY", poolId);
                return production.Where(p => p.PoolId == formattedPoolId).ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error getting production by pool for field: {fieldId}", ex);
            }
        }

        public async Task<List<ReservesResponse>> GetReservesForFieldAsync(string fieldId, List<AppFilter>? additionalFilters = null)
        {
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("RESERVE_ENTITY");
                if (metadata == null)
                    return new List<ReservesResponse>();

                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(RESERVE_ENTITY), _connectionName, "RESERVE_ENTITY");

                // Filter by field ID (assuming RESERVE_ENTITY has FIELD_ID or is linked through pool/well)
                var filters = new List<AppFilter>
                {
                    new AppFilter
                    {
                        FieldName = "FIELD_ID",
                        FilterValue = _defaults.FormatIdForTable("RESERVE_ENTITY", fieldId),
                        Operator = "="
                    }
                };

                if (additionalFilters != null)
                    filters.AddRange(additionalFilters);

                var results = await repo.GetAsync(filters);
                
                // Convert PPDM models to DTOs
                var dtoList = _mappingService.ConvertPPDMModelListToDTOListRuntime(results, typeof(ReservesResponse), typeof(RESERVE_ENTITY));
                return dtoList.Cast<ReservesResponse>().ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error getting reserves for field: {fieldId}", ex);
            }
        }

        /// <summary>
        /// Returns all WELL_ACTIVITY records for every well belonging to the given field.
        /// Used to populate intervention candidate lists.
        /// </summary>
        public async Task<List<WELL_ACTIVITY>> GetWellActivitiesForFieldAsync(string fieldId)
        {
            try
            {
                var wellRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(WELL), _connectionName, "WELL");
                var wellFilters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "ASSIGNED_FIELD", Operator = "=", FilterValue = fieldId },
                    new AppFilter { FieldName = "ACTIVE_IND",     Operator = "=", FilterValue = "Y" }
                };
                var wellResults = await wellRepo.GetAsync(wellFilters);
                var wells = wellResults.OfType<WELL>().ToList();

                var activities = new List<WELL_ACTIVITY>();
                var actRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(WELL_ACTIVITY), _connectionName, "WELL_ACTIVITY");

                foreach (var well in wells)
                {
                    if (string.IsNullOrEmpty(well.UWI)) continue;
                    var actFilters = new List<AppFilter>
                    {
                        new AppFilter { FieldName = "UWI",        Operator = "=", FilterValue = well.UWI },
                        new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                    };
                    var actResults = await actRepo.GetAsync(actFilters);
                    activities.AddRange(actResults.OfType<WELL_ACTIVITY>());
                }
                return activities;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error getting well activities for field {fieldId}", ex);
            }
        }

        /// <summary>
        /// Records a decision (Approved / Deferred / Rejected) for a well intervention
        /// by inserting a new WELL_ACTIVITY record with the decision as REMARK.
        /// </summary>
        public async Task RecordInterventionDecisionAsync(string uwi, string decisionActivityTypeId,
            string remark, string userId)
        {
            try
            {
                var actRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(WELL_ACTIVITY), _connectionName, "WELL_ACTIVITY");

                var activity = new WELL_ACTIVITY
                {
                    UWI              = uwi,
                    SOURCE           = "SYSTEM",
                    ACTIVITY_OBS_NO  = (decimal)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    ACTIVITY_TYPE_ID = decisionActivityTypeId,
                    ACTIVE_IND       = "Y",
                    REMARK           = remark,
                };
                await actRepo.InsertAsync(activity, userId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error recording intervention decision for well {uwi}", ex);
            }
        }

        /// <summary>
        /// Returns a lightweight summary for the Production Dashboard:
        /// well counts by status, latest aggregate volume, open activity count.
        /// </summary>
        public async Task<ProductionDashboardSummary> GetProductionDashboardSummaryAsync(string fieldId)
        {
            try
            {
                var wellRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(WELL), _connectionName, "WELL");
                var wellFilters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "ASSIGNED_FIELD", Operator = "=", FilterValue = fieldId },
                    new AppFilter { FieldName = "ACTIVE_IND",     Operator = "=", FilterValue = "Y" }
                };
                var wellResults = await wellRepo.GetAsync(wellFilters);
                var wells = wellResults.OfType<WELL>().ToList();

                var actRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(WELL_ACTIVITY), _connectionName, "WELL_ACTIVITY");

                int openActivities = 0;
                foreach (var well in wells)
                {
                    if (string.IsNullOrEmpty(well.UWI)) continue;
                    var actFilters = new List<AppFilter>
                    {
                        new AppFilter { FieldName = "UWI",        Operator = "=", FilterValue = well.UWI },
                        new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                    };
                    var acts = await actRepo.GetAsync(actFilters);
                    openActivities += acts.Count();
                }

                return new ProductionDashboardSummary
                {
                    TotalWells      = wells.Count,
                    OpenWorkOrders  = openActivities,
                    FieldId         = fieldId,
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error getting production dashboard summary for field {fieldId}", ex);
            }
        }

        /// <summary>
        /// Returns per-well status rows for the Production Dashboard wells grid.
        /// Pulls WELL + latest WELL_TEST data for oil/gas rates and WELL_STATUS for status.
        /// </summary>
        public async Task<List<ProductionWellStatusDto>> GetProductionWellStatusAsync(string fieldId)
        {
            try
            {
                var wellRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(WELL), _connectionName, "WELL");
                var wellFilters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "ASSIGNED_FIELD", Operator = "=", FilterValue = fieldId },
                    new AppFilter { FieldName = "ACTIVE_IND",     Operator = "=", FilterValue = "Y" }
                };
                var wellResults = await wellRepo.GetAsync(wellFilters);
                var wells = wellResults.OfType<WELL>().ToList();

                var testRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(WELL_TEST), _connectionName, "WELL_TEST");

                var dtoList = new List<ProductionWellStatusDto>();
                foreach (var well in wells)
                {
                    if (string.IsNullOrEmpty(well.UWI)) continue;
                    var testFilters = new List<AppFilter>
                    {
                        new AppFilter { FieldName = "UWI", Operator = "=", FilterValue = well.UWI }
                    };
                    var testResults = await testRepo.GetAsync(testFilters);
                    var latestTest = testResults.OfType<WELL_TEST>()
                        .OrderByDescending(t => t.TEST_DATE)
                        .FirstOrDefault();

                    dtoList.Add(new ProductionWellStatusDto
                    {
                        WellId    = well.UWI,
                        WellName  = well.WELL_NAME ?? well.UWI,
                        Status    = well.CURRENT_STATUS ?? "UNKNOWN",
                        OilRate   = latestTest != null ? (double)(latestTest.OIL_FLOW_AMOUNT) : 0,
                        GasRate   = latestTest != null ? (double)(latestTest.GAS_FLOW_AMOUNT) : 0,
                        WaterCut  = latestTest != null ? (double)(latestTest.WATER_CUT_PERCENT) : 0,
                        LastTestDate = latestTest?.TEST_DATE,
                    });
                }
                return dtoList;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error getting production well status for field {fieldId}", ex);
            }
        }

        public async Task<List<WellTestResponse>> GetWellTestsForWellAsync(string fieldId, string wellId, List<AppFilter>? additionalFilters = null)
        {
            try
            {
                // Validate well belongs to field first by checking WELL table
                var wellRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(WELL), _connectionName, "WELL");

                var formattedWellId = _defaults.FormatIdForTable("WELL", wellId);
                var well = await wellRepo.GetByIdAsync(formattedWellId);

                if (well == null)
                    throw new InvalidOperationException($"Well {wellId} not found");

                // Validate well belongs to field using ASSIGNED_FIELD (WELL's field link column)
                var wellEntity = well as WELL;
                if (wellEntity != null)
                {
                    var formattedFieldId = _defaults.FormatIdForTable("WELL", fieldId);
                    if (!string.IsNullOrEmpty(wellEntity.ASSIGNED_FIELD) &&
                        !string.Equals(wellEntity.ASSIGNED_FIELD, formattedFieldId, StringComparison.OrdinalIgnoreCase))
                        throw new InvalidOperationException($"Well {wellId} does not belong to field {fieldId}");
                }

                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(WELL_TEST), _connectionName, "WELL_TEST");

                var filters = new List<AppFilter>
                {
                    new AppFilter
                    {
                        FieldName = "UWI",
                        FilterValue = _defaults.FormatIdForTable("WELL_TEST", wellId),
                        Operator = "="
                    }
                };

                if (additionalFilters != null)
                    filters.AddRange(additionalFilters);

                var results = await repo.GetAsync(filters);

                // Convert PPDM models to DTOs
                var dtoList = _mappingService.ConvertPPDMModelListToDTOListRuntime(results, typeof(WellTestResponse), typeof(WELL_TEST));
                return dtoList.Cast<WellTestResponse>().ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error getting well tests for well {wellId} in field: {fieldId}", ex);
            }
        }

        public async Task<List<ProductionForecastResponse>> GetProductionForecastsForFieldAsync(string fieldId, List<AppFilter>? additionalFilters = null)
        {
            try
            {
                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(PRODUCTION_FORECAST), _connectionName, "PRODUCTION_FORECAST");

                var filters = new List<AppFilter>
                {
                    new AppFilter
                    {
                        FieldName = "FIELD_ID",
                        FilterValue = _defaults.FormatIdForTable("PRODUCTION_FORECAST", fieldId),
                        Operator = "="
                    }
                };

                if (additionalFilters != null)
                    filters.AddRange(additionalFilters);

                var results = await repo.GetAsync(filters);

                // Convert PPDM models to DTOs
                var dtoList = _mappingService.ConvertPPDMModelListToDTOListRuntime(results, typeof(ProductionForecastResponse), typeof(PRODUCTION_FORECAST));
                return dtoList.Cast<ProductionForecastResponse>().ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error getting production forecasts for field: {fieldId}", ex);
            }
        }

        public async Task<ProductionForecastResponse> CreateProductionForecastForFieldAsync(string fieldId, ProductionForecastRequest forecastData, string userId)
        {
            try
            {
                // Convert DTO to PPDM model
                var forecastEntity = _mappingService.ConvertDTOToPPDMModel<PRODUCTION_FORECAST, ProductionForecastRequest>(forecastData);
                forecastEntity.FIELD_ID = _defaults.FormatIdForTable("PRODUCTION_FORECAST", fieldId);

                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(PRODUCTION_FORECAST), _connectionName, "PRODUCTION_FORECAST");

                var result = await repo.InsertAsync(forecastEntity, userId);

                // Convert PPDM model back to DTO
                return _mappingService.ConvertPPDMModelToDTO<ProductionForecastResponse, PRODUCTION_FORECAST>((PRODUCTION_FORECAST)result);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error creating production forecast for field: {fieldId}", ex);
            }
        }

        public async Task<List<ProductionResponse>> GetFacilityProductionForFieldAsync(string fieldId, List<AppFilter>? additionalFilters = null)
        {
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("PRODUCTION_FACILITY");
                if (metadata == null)
                    return new List<ProductionResponse>();

                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}");
                if (entityType == null)
                    return new List<ProductionResponse>();

                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, _connectionName, "PRODUCTION_FACILITY");

                // Get facilities for field first, then get production for those facilities
                var facilityRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(FACILITY), _connectionName, "FACILITY");

                var facilities = await facilityRepo.GetAsync(new List<AppFilter>
                {
                    new AppFilter
                    {
                        FieldName = "PRIMARY_FIELD_ID",
                        FilterValue = _defaults.FormatIdForTable("FACILITY", fieldId),
                        Operator = "="
                    }
                });

                // Extract facility IDs using typed access
                var facilityIds = facilities.OfType<FACILITY>()
                    .Select(f => f.FACILITY_ID)
                    .Where(id => !string.IsNullOrEmpty(id))
                    .ToList();

                if (!facilityIds.Any())
                    return new List<ProductionResponse>();

                var filters = new List<AppFilter>();
                foreach (var facilityId in facilityIds)
                {
                    filters.Add(new AppFilter
                    {
                        FieldName = "FACILITY_ID",
                        FilterValue = _defaults.FormatIdForTable("PRODUCTION_FACILITY", facilityId),
                        Operator = "="
                    });
                }

                if (additionalFilters != null)
                    filters.AddRange(additionalFilters);

                var results = await repo.GetAsync(filters);
                
                // Convert PPDM models to DTOs (using ProductionResponse since facility production is still production data)
                var dtoList = _mappingService.ConvertPPDMModelListToDTOListRuntime(results, typeof(ProductionResponse), entityType);
                return dtoList.Cast<ProductionResponse>().ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error getting facility production for field: {fieldId}", ex);
            }
        }

        #region Choke Analysis Integration

        /// <summary>
        /// Analyzes choke flow for a well (downhole or uphole)
        /// </summary>
        public async Task<CHOKE_FLOW_RESULT> AnalyzeChokeFlowAsync(
            string fieldId,
            string wellId,
            string chokeLocation = "DOWNHOLE", // DOWNHOLE or UPHOLE
            decimal? chokeDiameter = null,
            decimal? upstreamPressure = null,
            decimal? downstreamPressure = null,
            decimal? gasFlowRate = null,
            decimal? gasSpecificGravity = null,
            string? userId = null)
        {
            try
            {
                _logger?.LogInformation("Analyzing choke flow for well {WellId} in field {FieldId}, location: {Location}",
                    wellId, fieldId, chokeLocation);

                // Get choke and gas properties from PPDM or request
                var CHOKE_PROPERTIES = await GetChokePropertiesAsync(wellId, chokeDiameter);
                var gasProperties = await GetGasChokePropertiesAsync(
                    fieldId, wellId, upstreamPressure, downstreamPressure, gasFlowRate, gasSpecificGravity);

                // Calculate choke flow
                CHOKE_FLOW_RESULT result;
                if (chokeLocation.ToUpper() == "UPHOLE")
                {
                    result = GasChokeCalculator.CalculateUpholeChokeFlow(CHOKE_PROPERTIES, gasProperties);
                }
                else
                {
                    result = GasChokeCalculator.CalculateDownholeChokeFlow(CHOKE_PROPERTIES, gasProperties);
                }

                // Store results in WELL_EQUIPMENT table
                await StoreChokeFlowResultsAsync(wellId, result, chokeLocation, userId ?? "SYSTEM");

                _logger?.LogInformation("Choke flow analysis completed for well {WellId}. Flow rate: {FlowRate} Mscf/day, Flow regime: {Regime}",
                    wellId, result.FLOW_RATE, result.FLOW_REGIME);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error analyzing choke flow for well {WellId}", wellId);
                throw;
            }
        }

        /// <summary>
        /// Calculates required choke size for a target flow rate
        /// </summary>
        public async Task<CHOKE_PROPERTIES> CalculateChokeSizingAsync(
            string fieldId,
            string wellId,
            decimal targetFlowRate,
            string chokeLocation = "DOWNHOLE",
            decimal? upstreamPressure = null,
            decimal? downstreamPressure = null,
            decimal? gasSpecificGravity = null,
            string? userId = null)
        {
            try
            {
                _logger?.LogInformation("Calculating choke sizing for well {WellId} in field {FieldId}, target flow rate: {FlowRate} Mscf/day",
                    wellId, fieldId, targetFlowRate);

                // Get gas properties
                var gasProperties = await GetGasChokePropertiesAsync(
                    fieldId, wellId, upstreamPressure, downstreamPressure, targetFlowRate, gasSpecificGravity);

                // Iterate to find optimal choke diameter
                decimal minDiameter = 0.125m; // 1/8 inch
                decimal maxDiameter = 2.0m; // 2 inches
                decimal optimalDiameter = 0.5m; // Start with 1/2 inch

                for (int i = 0; i < 20; i++) // Max 20 iterations
                {
                    var CHOKE_PROPERTIES = new CHOKE_PROPERTIES
                    {
                        CHOKE_DIAMETER = optimalDiameter,
                        CHOKE_TYPE = ChokeType.Bean.ToString(),
                        DISCHARGE_COEFFICIENT = 0.85m
                    };

                    CHOKE_FLOW_RESULT result;
                    if (chokeLocation.ToUpper() == "UPHOLE")
                    {
                        result = GasChokeCalculator.CalculateUpholeChokeFlow(CHOKE_PROPERTIES, gasProperties);
                    }
                    else
                    {
                        result = GasChokeCalculator.CalculateDownholeChokeFlow(CHOKE_PROPERTIES, gasProperties);
                    }

                    decimal error = Math.Abs(result.FLOW_RATE - targetFlowRate);
                    if (error < 1.0m) // Within 1 Mscf/day
                    {
                        break;
                    }

                    // Adjust diameter based on flow rate difference
                    if (result.FLOW_RATE < targetFlowRate)
                    {
                        minDiameter = optimalDiameter;
                        optimalDiameter = (optimalDiameter + maxDiameter) / 2m;
                    }
                    else
                    {
                        maxDiameter = optimalDiameter;
                        optimalDiameter = (minDiameter + optimalDiameter) / 2m;
                    }
                }

                var finalChoke = new CHOKE_PROPERTIES
                {
                    CHOKE_DIAMETER = optimalDiameter,
                    CHOKE_TYPE = ChokeType.Bean.ToString(),
                    DISCHARGE_COEFFICIENT = 0.85m
                };

                // Store sizing results
                await StoreChokeSizingResultsAsync(wellId, finalChoke, targetFlowRate, chokeLocation, userId ?? "SYSTEM");

                _logger?.LogInformation("Choke sizing completed for well {WellId}. Optimal diameter: {Diameter} inches",
                    wellId, optimalDiameter);

                return finalChoke;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error calculating choke sizing for well {WellId}", wellId);
                throw;
            }
        }

        #endregion

        #region Choke Analysis Helper Methods

        /// <summary>
        /// Retrieves choke properties from PPDM WELL_EQUIPMENT table (falls back to defaults if not found).
        /// </summary>
        private async Task<CHOKE_PROPERTIES> GetChokePropertiesAsync(string wellId, decimal? chokeDiameter)
        {
            try
            {
                // Query WELL_EQUIPMENT for stored choke records (written by StoreChokeFlowResultsAsync / StoreChokeSizingResultsAsync)
                var equipMeta = await _metadata.GetTableMetadataAsync("WELL_EQUIPMENT");
                if (equipMeta != null)
                {
                    var equipRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                        typeof(WELL_EQUIPMENT), _connectionName, "WELL_EQUIPMENT", null);

                    var filters = new List<AppFilter>
                    {
                        new AppFilter { FieldName = "UWI", Operator = "=", FilterValue = _defaults.FormatIdForTable("WELL_EQUIPMENT", wellId) },
                        new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                    };

                    var equipRecords = await equipRepo.GetAsync(filters);

                    // Look for a choke record that has JSON in REMARK with choke diameter
                    foreach (var rec in equipRecords ?? Enumerable.Empty<object>())
                    {
                        var remark = (rec as WELL_EQUIPMENT)?.REMARK;
                        if (string.IsNullOrEmpty(remark) || !remark.Contains("ChokeDiameter")) continue;

                        try
                        {
                            using var doc = System.Text.Json.JsonDocument.Parse(remark);
                            var root = doc.RootElement;
                            decimal storedDiameter = 0;
                            if (root.TryGetProperty("ChokeDiameter", out var dProp))
                                storedDiameter = dProp.GetDecimal();

                            if (storedDiameter > 0)
                            {
                                _logger?.LogInformation("Retrieved choke diameter {Diameter} from PPDM for well {WellId}", storedDiameter, wellId);
                                return new CHOKE_PROPERTIES
                                {
                                    CHOKE_DIAMETER = chokeDiameter ?? storedDiameter,
                                    CHOKE_TYPE = ChokeType.Bean.ToString(),
                                    DISCHARGE_COEFFICIENT = 0.85m
                                };
                            }
                        }
                        catch { /* skip malformed remark */ }
                    }
                }

                // Fallback: use caller-supplied diameter or engineering default
                _logger?.LogWarning("No stored choke data found in PPDM for well {WellId}. Using {Diameter}\" bean choke defaults.", wellId, chokeDiameter ?? 0.5m);
                return new CHOKE_PROPERTIES
                {
                    CHOKE_DIAMETER = chokeDiameter ?? 0.5m,
                    CHOKE_TYPE = ChokeType.Bean.ToString(),
                    DISCHARGE_COEFFICIENT = 0.85m
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving choke properties for well {WellId}", wellId);
                throw;
            }
        }

        /// <summary>
        /// Retrieves gas properties for choke analysis, pulling live data from PPDM_VOL_SUMMARY where available.
        /// </summary>
        private async Task<GAS_CHOKE_PROPERTIES> GetGasChokePropertiesAsync(
            string fieldId,
            string wellId,
            decimal? upstreamPressure,
            decimal? downstreamPressure,
            decimal? gasFlowRate,
            decimal? gasSpecificGravity)
        {
            try
            {
                // Attempt to pull the most recent gas production volume from PDEN_VOL_SUMMARY
                decimal ppdmGasVolume = 0;
                decimal ppdmInjectionPressure = 0;

                var pdenMeta = await _metadata.GetTableMetadataAsync("PDEN_VOL_SUMMARY");
                if (pdenMeta != null)
                {
                    var pdenRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                        typeof(PDEN_VOL_SUMMARY), _connectionName, "PDEN_VOL_SUMMARY", null);

                    var pdenFilters = new List<AppFilter>
                    {
                        new AppFilter { FieldName = "PDEN_ID", Operator = "=", FilterValue = _defaults.FormatIdForTable("PDEN_VOL_SUMMARY", wellId) },
                        new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                    };

                    var pdenRecords = await pdenRepo.GetAsync(pdenFilters);

                    // Pick the record with the latest EFFECTIVE_DATE
                    var latestRecord = pdenRecords?.OfType<PDEN_VOL_SUMMARY>()
                        .Where(r => r.EFFECTIVE_DATE.HasValue)
                        .OrderByDescending(r => r.EFFECTIVE_DATE!.Value)
                        .FirstOrDefault();

                    if (latestRecord != null)
                    {
                        if (latestRecord.GAS_CUM_VOLUME > 0) ppdmGasVolume = latestRecord.GAS_CUM_VOLUME;
                        // INJECTION_PRESSURE is not in PDEN_VOL_SUMMARY — ppdmInjectionPressure stays 0
                    }
                }

                if (ppdmGasVolume > 0 || ppdmInjectionPressure > 0)
                    _logger?.LogInformation("Using PPDM production data for gas choke analysis on well {WellId}: GasVolume={GasVolume}, InjPressure={InjPressure}",
                        wellId, ppdmGasVolume, ppdmInjectionPressure);
                else
                    _logger?.LogWarning("No PPDM production data found for well {WellId}; using request defaults for gas choke analysis.", wellId);

                return new GAS_CHOKE_PROPERTIES
                {
                    UPSTREAM_PRESSURE = upstreamPressure ?? (ppdmInjectionPressure > 0 ? ppdmInjectionPressure : 1000m),
                    DOWNSTREAM_PRESSURE = downstreamPressure ?? 500m,
                    TEMPERATURE = 540m, // 80°F = 540°R — not stored in PPDM_VOL_SUMMARY
                    GAS_SPECIFIC_GRAVITY = gasSpecificGravity ?? 0.65m,
                    FLOW_RATE = gasFlowRate ?? (ppdmGasVolume > 0 ? ppdmGasVolume : 1000m),
                    Z_FACTOR = 0.9m
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving gas properties for choke analysis on well {WellId}", wellId);
                throw;
            }
        }

        /// <summary>
        /// Stores choke flow analysis results in WELL_EQUIPMENT table
        /// </summary>
        private async Task StoreChokeFlowResultsAsync(
            string wellId,
            CHOKE_FLOW_RESULT result,
            string chokeLocation,
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
                    ["EQUIPMENT_TYPE"] = "CHOKE",
                    ["EQUIPMENT_NAME"] = $"{chokeLocation} Choke Flow Analysis",
                    ["DESCRIPTION"] = $"Flow rate: {result.FLOW_RATE:F2} Mscf/day, Flow regime: {result.FLOW_REGIME}",
                    ["EQUIPMENT_DATA_JSON"] = JsonSerializer.Serialize(new
                    {
                        ChokeLocation = chokeLocation,
                        FlowRate = result.FLOW_RATE,
                        UpstreamPressure = result.UPSTREAM_PRESSURE,
                        DownstreamPressure = result.DOWNSTREAM_PRESSURE,
                        PressureRatio = result.PRESSURE_RATIO,
                        FlowRegime = result.FLOW_REGIME.ToString(),
                        CriticalPressureRatio = result.CRITICAL_PRESSURE_RATIO,
                        AnalysisDate = DateTime.UtcNow
                    })
                };

                await equipmentRepo.InsertAsync(equipmentRecord, userId);
                _logger?.LogInformation("Stored choke flow results for well {WellId}", wellId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error storing choke flow results for well {WellId}", wellId);
                // Don't throw - storage failure shouldn't fail the operation
            }
        }

        /// <summary>
        /// Stores choke sizing results in WELL_EQUIPMENT table
        /// </summary>
        private async Task StoreChokeSizingResultsAsync(
            string wellId,
            CHOKE_PROPERTIES CHOKE_PROPERTIES,
            decimal targetFlowRate,
            string chokeLocation,
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
                    ["EQUIPMENT_TYPE"] = "CHOKE",
                    ["EQUIPMENT_NAME"] = $"{chokeLocation} Choke Sizing",
                    ["DESCRIPTION"] = $"Optimal diameter: {CHOKE_PROPERTIES.CHOKE_DIAMETER:F3} inches for {targetFlowRate:F2} Mscf/day",
                    ["EQUIPMENT_DATA_JSON"] = JsonSerializer.Serialize(new
                    {
                        ChokeLocation = chokeLocation,
                        ChokeDiameter = CHOKE_PROPERTIES.CHOKE_DIAMETER,
                        ChokeType = CHOKE_PROPERTIES.CHOKE_TYPE.ToString(),
                        DischargeCoefficient = CHOKE_PROPERTIES.DISCHARGE_COEFFICIENT,
                        ChokeArea = CHOKE_PROPERTIES.CHOKE_AREA,
                        TargetFlowRate = targetFlowRate,
                        AnalysisDate = DateTime.UtcNow
                    })
                };

                await equipmentRepo.InsertAsync(equipmentRecord, userId);
                _logger?.LogInformation("Stored choke sizing results for well {WellId}", wellId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error storing choke sizing results for well {WellId}", wellId);
                // Don't throw - storage failure shouldn't fail the operation
            }
        }

        #endregion

        #region Sucker Rod Pumping Integration

        /// <summary>
        /// Analyzes sucker rod load for a well
        /// </summary>
        public async Task<SUCKER_ROD_LOAD_RESULT> AnalyzeSuckerRodLoadAsync(
            string fieldId,
            string wellId,
            decimal? strokeLength = null,
            decimal? strokesPerMinute = null,
            decimal? rodDiameter = null,
            decimal? pumpDiameter = null,
            string? userId = null)
        {
            try
            {
                _logger?.LogInformation("Analyzing sucker rod load for well {WellId} in field {FieldId}",
                    wellId, fieldId);

                // Get sucker rod system properties from PPDM
                var systemProperties = await GetSuckerRodSystemPropertiesAsync(
                    fieldId, wellId, strokeLength, strokesPerMinute, rodDiameter, pumpDiameter);
                var rodString = await GetSuckerRodStringAsync(wellId, systemProperties);

                // Calculate loads
                var result = SuckerRodLoadCalculator.CalculateLoads(systemProperties, rodString);

                // Store results in WELL_EQUIPMENT table
                await StoreSuckerRodLoadResultsAsync(wellId, result, userId ?? "SYSTEM");

                _logger?.LogInformation("Sucker rod load analysis completed for well {WellId}. Peak load: {PeakLoad} lbs, Stress range: {StressRange} psi",
                    wellId, result.PEAK_LOAD, result.STRESS_RANGE);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error analyzing sucker rod load for well {WellId}", wellId);
                throw;
            }
        }

        /// <summary>
        /// Analyzes sucker rod power requirements and production rate
        /// </summary>
        public async Task<SUCKER_ROD_FLOW_RATE_POWER_RESULT> AnalyzeSuckerRodPowerAsync(
            string fieldId,
            string wellId,
            decimal? strokeLength = null,
            decimal? strokesPerMinute = null,
            decimal? rodDiameter = null,
            decimal? pumpDiameter = null,
            string? userId = null)
        {
            try
            {
                _logger?.LogInformation("Analyzing sucker rod power for well {WellId} in field {FieldId}",
                    wellId, fieldId);

                // Get system properties
                var systemProperties = await GetSuckerRodSystemPropertiesAsync(
                    fieldId, wellId, strokeLength, strokesPerMinute, rodDiameter, pumpDiameter);
                var rodString = await GetSuckerRodStringAsync(wellId, systemProperties);

                // Calculate loads first
                var loadResult = SuckerRodLoadCalculator.CalculateLoads(systemProperties, rodString);

                // Calculate flow rate and power
                var result = SuckerRodFlowRatePowerCalculator.CalculateFlowRateAndPower(
                    systemProperties, loadResult);

                // Store results in WELL_EQUIPMENT table
                await StoreSuckerRodPowerResultsAsync(wellId, result, userId ?? "SYSTEM");

                _logger?.LogInformation("Sucker rod power analysis completed for well {WellId}. Production rate: {Rate} bbl/day, Motor HP: {MotorHP}",
                    wellId, result.PRODUCTION_RATE, result.MOTOR_HORSEPOWER);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error analyzing sucker rod power for well {WellId}", wellId);
                throw;
            }
        }

        #endregion

        #region Sucker Rod Pumping Helper Methods

        /// <summary>
        /// Retrieves sucker rod system properties, pulling well depth from the WELL table and
        /// tubing install depth from WELL_EQUIPMENT before falling back to engineering defaults.
        /// </summary>
        private async Task<SUCKER_ROD_SYSTEM_PROPERTIES> GetSuckerRodSystemPropertiesAsync(
            string fieldId,
            string wellId,
            decimal? strokeLength,
            decimal? strokesPerMinute,
            decimal? rodDiameter,
            decimal? pumpDiameter)
        {
            try
            {
                decimal wellDepth = 5000m;
                decimal tubingDiameter = 2.875m;

                // --- Query WELL table for actual well depth ---
                var wellMeta = await _metadata.GetTableMetadataAsync("WELL");
                if (wellMeta != null)
                {
                    var wellRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                        typeof(WELL), _connectionName, "WELL", null);

                    var wellFilters = new List<AppFilter>
                    {
                        new AppFilter { FieldName = "UWI", Operator = "=", FilterValue = _defaults.FormatIdForTable("WELL", wellId) },
                        new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                    };

                    var wellRecords = await wellRepo.GetAsync(wellFilters);
                    var wellEntity = wellRecords?.OfType<WELL>().FirstOrDefault();
                    if (wellEntity != null)
                    {
                        decimal rawDepth = wellEntity.FINAL_TD > 0 ? wellEntity.FINAL_TD
                            : wellEntity.DRILL_TD > 0 ? wellEntity.DRILL_TD
                            : 0;

                        if (rawDepth > 0)
                        {
                            // Convert to feet if stored in metres
                            wellDepth = string.Equals(wellEntity.FINAL_TD_OUOM, "M", StringComparison.OrdinalIgnoreCase)
                                ? rawDepth * 3.28084m
                                : rawDepth;
                            _logger?.LogInformation("Retrieved well depth {Depth} ft from PPDM for well {WellId}", wellDepth, wellId);
                        }
                    }
                }

                // --- Query WELL_EQUIPMENT for tubing depth (INSTALL_BASE_DEPTH) as proxy for tubing setting depth ---
                var equipMeta = await _metadata.GetTableMetadataAsync("WELL_EQUIPMENT");
                if (equipMeta != null)
                {
                    var equipRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                        typeof(WELL_EQUIPMENT), _connectionName, "WELL_EQUIPMENT", null);

                    var equipFilters = new List<AppFilter>
                    {
                        new AppFilter { FieldName = "UWI", Operator = "=", FilterValue = _defaults.FormatIdForTable("WELL_EQUIPMENT", wellId) },
                        new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                    };

                    var equipRecords = await equipRepo.GetAsync(equipFilters);

                    // Pick the record with the deepest install base depth (likely tubing)
                    decimal deepestInstall = 0;
                    foreach (var rec in equipRecords ?? Enumerable.Empty<object>())
                    {
                        var baseDep = (rec as WELL_EQUIPMENT)?.INSTALL_BASE_DEPTH;
                        if (baseDep is decimal bd && bd > deepestInstall)
                            deepestInstall = bd;
                    }

                    // Use deepest install depth as tubing setting depth reference
                    if (deepestInstall > 0 && deepestInstall < wellDepth)
                        wellDepth = deepestInstall; // Pump set at tubing shoe
                }

                if (wellDepth == 5000m)
                    _logger?.LogWarning("Could not retrieve well depth from PPDM for well {WellId}; using 5000 ft default.", wellId);

                // --- Query WELL_TUBULAR for tubing OD ---
                var tubularRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(WELL_TUBULAR), _connectionName, "WELL_TUBULAR", null);
                var tubularRecords = await tubularRepo.GetAsync(new List<AppFilter>
                {
                    new AppFilter { FieldName = "UWI", Operator = "=", FilterValue = _defaults.FormatIdForTable("WELL_TUBULAR", wellId) },
                    new AppFilter { FieldName = "TUBING_TYPE", Operator = "=", FilterValue = "TUBING" },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                });
                var tubularRecord = tubularRecords?.OfType<WELL_TUBULAR>()
                    .Where(t => t.OUTSIDE_DIAMETER > 0)
                    .OrderByDescending(t => t.OUTSIDE_DIAMETER)
                    .FirstOrDefault();
                if (tubularRecord?.OUTSIDE_DIAMETER > 0)
                    tubingDiameter = tubularRecord.OUTSIDE_DIAMETER;

                // --- Query WELL_TEST for fluid and pressure properties ---
                var testRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(WELL_TEST), _connectionName, "WELL_TEST", null);
                var testRecords = await testRepo.GetAsync(new List<AppFilter>
                {
                    new AppFilter { FieldName = "UWI", Operator = "=", FilterValue = _defaults.FormatIdForTable("WELL_TEST", wellId) },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                });
                var latestTest = testRecords?.OfType<WELL_TEST>()
                    .OrderByDescending(t => t.TEST_DATE ?? t.EFFECTIVE_DATE)
                    .FirstOrDefault();

                return new SUCKER_ROD_SYSTEM_PROPERTIES
                {
                    WELL_DEPTH = wellDepth,
                    TUBING_DIAMETER = tubingDiameter,
                    ROD_DIAMETER = rodDiameter ?? 0.875m,
                    PUMP_DIAMETER = pumpDiameter ?? 1.5m,
                    STROKE_LENGTH = strokeLength ?? 60m,
                    STROKES_PER_MINUTE = strokesPerMinute ?? 10m,
                    WELLHEAD_PRESSURE = latestTest?.FLOW_PRESSURE > 0 ? latestTest.FLOW_PRESSURE : 100m,
                    BOTTOM_HOLE_PRESSURE = latestTest?.STATIC_PRESSURE > 0 ? latestTest.STATIC_PRESSURE : 2000m,
                    OIL_GRAVITY = latestTest?.OIL_GRAVITY > 0 ? latestTest.OIL_GRAVITY : 35m,
                    WATER_CUT = latestTest?.WATER_CUT_PERCENT > 0 ? latestTest.WATER_CUT_PERCENT / 100m : 0.3m,
                    GAS_OIL_RATIO = latestTest?.GOR > 0 ? latestTest.GOR : 500m,
                    GAS_SPECIFIC_GRAVITY = latestTest?.GAS_GRAVITY > 0 ? latestTest.GAS_GRAVITY : 0.65m,
                    ROD_DENSITY = 490m,
                    PUMP_EFFICIENCY = 0.85m
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving sucker rod system properties for well {WellId}", wellId);
                throw;
            }
        }

        /// <summary>
        /// Gets sucker rod string configuration from PPDM WELL_EQUIPMENT table.
        /// Each WELL_EQUIPMENT record with distinct INSTALL_TOP_DEPTH/INSTALL_BASE_DEPTH becomes a rod section.
        /// Falls back to a single-section string when no equipment records are found.
        /// </summary>
        private async Task<SUCKER_ROD_STRING> GetSuckerRodStringAsync(
            string wellId,
            SUCKER_ROD_SYSTEM_PROPERTIES systemProperties)
        {
            try
            {
                var equipMeta = await _metadata.GetTableMetadataAsync("WELL_EQUIPMENT");
                if (equipMeta != null)
                {
                    var equipRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                        typeof(WELL_EQUIPMENT), _connectionName, "WELL_EQUIPMENT", null);

                    var filters = new List<AppFilter>
                    {
                        new AppFilter { FieldName = "UWI", Operator = "=", FilterValue = _defaults.FormatIdForTable("WELL_EQUIPMENT", wellId) },
                        new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                    };

                    var records = await equipRepo.GetAsync(filters);

                    // Build a rod section for each equipment record that has valid depth interval
                    var sections = new List<ROD_SECTION>();
                    foreach (var rec in (records ?? Enumerable.Empty<object>())
                        .OfType<WELL_EQUIPMENT>()
                        .OrderBy(e => e.INSTALL_TOP_DEPTH))
                    {
                        if (rec.INSTALL_BASE_DEPTH > rec.INSTALL_TOP_DEPTH)
                        {
                            sections.Add(new ROD_SECTION
                            {
                                DIAMETER = systemProperties.ROD_DIAMETER,
                                LENGTH = rec.INSTALL_BASE_DEPTH - rec.INSTALL_TOP_DEPTH,
                                DENSITY = systemProperties.ROD_DENSITY
                            });
                        }
                    }

                    if (sections.Count > 0)
                    {
                        var totalLen = sections.Sum(s => s.LENGTH);
                        _logger?.LogInformation("Built {Count}-section rod string ({TotalLength} ft) from PPDM WELL_EQUIPMENT for well {WellId}",
                            sections.Count, totalLen, wellId);
                        return new SUCKER_ROD_STRING { TOTAL_LENGTH = totalLen, SECTIONS = sections };
                    }
                }

                // Fallback: single uniform section spanning the full well depth
                _logger?.LogWarning("No WELL_EQUIPMENT depth intervals found for well {WellId}; using single-section rod string.", wellId);
                return new SUCKER_ROD_STRING
                {
                    TOTAL_LENGTH = systemProperties.WELL_DEPTH,
                    SECTIONS = new List<ROD_SECTION>
                    {
                        new ROD_SECTION
                        {
                            DIAMETER = systemProperties.ROD_DIAMETER,
                            LENGTH = systemProperties.WELL_DEPTH,
                            DENSITY = systemProperties.ROD_DENSITY
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error building sucker rod string for well {WellId}", wellId);
                // Safe fallback so analysis can still proceed
                return new SUCKER_ROD_STRING
                {
                    TOTAL_LENGTH = systemProperties.WELL_DEPTH,
                    SECTIONS = new List<ROD_SECTION>
                    {
                        new ROD_SECTION
                        {
                            DIAMETER = systemProperties.ROD_DIAMETER,
                            LENGTH = systemProperties.WELL_DEPTH,
                            DENSITY = systemProperties.ROD_DENSITY
                        }
                    }
                };
            }
        }

        /// <summary>
        /// Stores sucker rod load analysis results in WELL_EQUIPMENT table
        /// </summary>
        private async Task StoreSuckerRodLoadResultsAsync(
            string wellId,
            SUCKER_ROD_LOAD_RESULT result,
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
                    ["EQUIPMENT_TYPE"] = "SUCKER_ROD_PUMP",
                    ["EQUIPMENT_NAME"] = "Sucker Rod Load Analysis",
                    ["DESCRIPTION"] = $"Peak load: {result.PEAK_LOAD:F2} lbs, Stress range: {result.STRESS_RANGE:F2} psi",
                    ["EQUIPMENT_DATA_JSON"] = JsonSerializer.Serialize(new
                    {
                        PeakLoad = result.PEAK_LOAD,
                        MinimumLoad = result.MINIMUM_LOAD,
                        PolishedRodLoad = result.POLISHED_ROD_LOAD,
                        RodStringWeight = result.ROD_STRING_WEIGHT,
                        FluidLoad = result.FLUID_LOAD,
                        DynamicLoad = result.DYNAMIC_LOAD,
                        LoadRange = result.LOAD_RANGE,
                        StressRange = result.STRESS_RANGE,
                        MaximumStress = result.MAXIMUM_STRESS,
                        LoadFactor = result.LOAD_FACTOR,
                        AnalysisDate = DateTime.UtcNow
                    })
                };

                await equipmentRepo.InsertAsync(equipmentRecord, userId);
                _logger?.LogInformation("Stored sucker rod load results for well {WellId}", wellId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error storing sucker rod load results for well {WellId}", wellId);
                // Don't throw - storage failure shouldn't fail the operation
            }
        }

        /// <summary>
        /// Stores sucker rod power analysis results in WELL_EQUIPMENT table
        /// </summary>
        private async Task StoreSuckerRodPowerResultsAsync(
            string wellId,
            SUCKER_ROD_FLOW_RATE_POWER_RESULT result,
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
                    ["EQUIPMENT_TYPE"] = "SUCKER_ROD_PUMP",
                    ["EQUIPMENT_NAME"] = "Sucker Rod Power Analysis",
                    ["DESCRIPTION"] = $"Production rate: {result.PRODUCTION_RATE:F2} bbl/day, Motor HP: {result.MOTOR_HORSEPOWER:F2}",
                    ["EQUIPMENT_DATA_JSON"] = JsonSerializer.Serialize(new
                    {
                        ProductionRate = result.PRODUCTION_RATE,
                        PumpDisplacement = result.PUMP_DISPLACEMENT,
                        VolumetricEfficiency = result.VOLUMETRIC_EFFICIENCY,
                        PolishedRodHorsepower = result.POLISHED_ROD_HORSEPOWER,
                        HydraulicHorsepower = result.HYDRAULIC_HORSEPOWER,
                        FrictionHorsepower = result.FRICTION_HORSEPOWER,
                        TotalHorsepower = result.TOTAL_HORSEPOWER,
                        MotorHorsepower = result.MOTOR_HORSEPOWER,
                        SystemEfficiency = result.SYSTEM_EFFICIENCY,
                        EnergyConsumption = result.ENERGY_CONSUMPTION,
                        AnalysisDate = DateTime.UtcNow
                    })
                };

                await equipmentRepo.InsertAsync(equipmentRecord, userId);
                _logger?.LogInformation("Stored sucker rod power results for well {WellId}", wellId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error storing sucker rod power results for well {WellId}", wellId);
                // Don't throw - storage failure shouldn't fail the operation
            }
        }

        #endregion

        #region Production Operations Integration

        /// <summary>
        /// Manages production operations using ProductionOperations service
        /// </summary>
        public async Task<ProductionResponse> ManageProductionOperationsAsync(string fieldId, string wellId, ProductionRequest productionData, string userId)
        {
            try
            {
                _logger?.LogInformation("Managing production operations for well: {WellId} in field: {FieldId}", wellId, fieldId);

                // Use existing CreateProductionForFieldAsync method
                return await CreateProductionForFieldAsync(fieldId, productionData, userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error managing production operations for well: {WellId}", wellId);
                throw;
            }
        }

        /// <summary>
        /// Optimizes production using ProductionOperations service
        /// </summary>
        public async Task<ProductionResponse> OptimizeProductionAsync(string fieldId, string wellId, Dictionary<string, object> optimizationParameters, string userId)
        {
            try
            {
                _logger?.LogInformation("Optimizing production for well: {WellId} in field: {FieldId}", wellId, fieldId);

                // Get current production
                var production = await GetProductionForFieldAsync(fieldId, new List<AppFilter>
                {
                    new AppFilter { FieldName = "WELL_ID", Operator = "=", FilterValue = wellId }
                });

                var currentProd = production.FirstOrDefault();
                if (currentProd == null)
                {
                    throw new InvalidOperationException($"No production data found for well {wellId}");
                }

                // Record the optimization event in FACILITY_STATUS so it is traceable
                var facilityMeta = await _metadata.GetTableMetadataAsync("FACILITY_STATUS");
                if (facilityMeta != null)
                {
                    var fsRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                        typeof(FACILITY_STATUS), _connectionName, "FACILITY_STATUS", null);

                    var paramRemark = optimizationParameters != null && optimizationParameters.Any()
                        ? string.Join("; ", optimizationParameters.Select(kv => $"{kv.Key}={kv.Value}"))
                        : "standard";

                    var fs = new FACILITY_STATUS
                    {
                        FACILITY_ID = _defaults.FormatIdForTable("FACILITY_STATUS", wellId),
                        FACILITY_TYPE = "WELL",
                        STATUS_ID = Guid.NewGuid().ToString("N").Substring(0, 16),
                        STATUS_TYPE = "PROD_OPTIMIZE",
                        STATUS = "OPTIMIZED",
                        START_TIME = DateTime.UtcNow,
                        REMARK = $"FieldId:{fieldId}; Params:{paramRemark}",
                        ACTIVE_IND = "Y",
                        PPDM_GUID = Guid.NewGuid().ToString()
                    };
                    if (fs is Beep.OilandGas.PPDM.Models.IPPDMEntity e) _commonColumnHandler.PrepareForInsert(e, userId);
                    await fsRepo.InsertAsync(fs, userId);
                }

                return currentProd;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error optimizing production for well: {WellId}", wellId);
                throw;
            }
        }

        #endregion

        #region Enhanced Recovery Integration

        /// <summary>
        /// Plans enhanced recovery operation using EnhancedRecovery service
        /// </summary>
        public async Task<EnhancedRecoveryOperation> PlanEnhancedRecoveryAsync(string fieldId, CreateEnhancedRecoveryOperation eorData, string userId)
        {
            try
            {
                _logger?.LogInformation("Planning enhanced recovery for field: {FieldId}", fieldId);

                var operationId = Guid.NewGuid().ToString();
                var eorMethod = eorData.EorMethod ?? eorData.EORType ?? "WATER_FLOODING";
                var startDate = eorData.StartDate ?? eorData.PlannedStartDate ?? DateTime.UtcNow;

                var facilityMeta = await _metadata.GetTableMetadataAsync("FACILITY_STATUS");
                if (facilityMeta != null)
                {
                    var fsRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                        typeof(FACILITY_STATUS), _connectionName, "FACILITY_STATUS", null);
                    var fs = new FACILITY_STATUS
                    {
                        FACILITY_ID = _defaults.FormatIdForTable("FACILITY_STATUS", fieldId),
                        FACILITY_TYPE = "FIELD",
                        STATUS_ID = Guid.NewGuid().ToString("N").Substring(0, 16),
                        STATUS_TYPE = "EOR_PLAN",
                        STATUS = eorMethod,
                        START_TIME = startDate,
                        REMARK = $"OperationId:{operationId}; EorMethod:{eorMethod}",
                        ACTIVE_IND = "Y",
                        PPDM_GUID = Guid.NewGuid().ToString()
                    };
                    if (fs is IPPDMEntity e) _commonColumnHandler.PrepareForInsert(e, userId);
                    await fsRepo.InsertAsync(fs, userId);
                }

                return new EnhancedRecoveryOperation
                {
                    OperationId = operationId,
                    FieldId = fieldId,
                    EorMethod = eorMethod,
                    Status = "PLANNED",
                    StartDate = startDate
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error planning enhanced recovery for field: {FieldId}", fieldId);
                throw;
            }
        }

        /// <summary>
        /// Executes enhanced recovery operation using EnhancedRecovery service
        /// </summary>
        public async Task<EnhancedRecoveryOperation> ExecuteEnhancedRecoveryAsync(string fieldId, string operationId, Dictionary<string, object> executionParameters, string userId)
        {
            try
            {
                _logger?.LogInformation("Executing enhanced recovery operation: {OperationId} for field: {FieldId}", operationId, fieldId);

                var facilityMeta = await _metadata.GetTableMetadataAsync("FACILITY_STATUS");
                if (facilityMeta != null)
                {
                    var fsRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                        typeof(FACILITY_STATUS), _connectionName, "FACILITY_STATUS", null);
                    var fs = new FACILITY_STATUS
                    {
                        FACILITY_ID = _defaults.FormatIdForTable("FACILITY_STATUS", fieldId),
                        FACILITY_TYPE = "FIELD",
                        STATUS_ID = Guid.NewGuid().ToString("N").Substring(0, 16),
                        STATUS_TYPE = "EOR_EXEC",
                        STATUS = "EXECUTING",
                        START_TIME = DateTime.UtcNow,
                        REMARK = $"OperationId:{operationId}",
                        ACTIVE_IND = "Y",
                        PPDM_GUID = Guid.NewGuid().ToString()
                    };
                    if (fs is IPPDMEntity e) _commonColumnHandler.PrepareForInsert(e, userId);
                    await fsRepo.InsertAsync(fs, userId);
                }

                return new EnhancedRecoveryOperation
                {
                    OperationId = operationId,
                    FieldId = fieldId,
                    Status = "EXECUTING",
                    StartDate = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error executing enhanced recovery operation: {OperationId}", operationId);
                throw;
            }
        }

        #endregion
    }
}




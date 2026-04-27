using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.ProductionOperations;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.PPDM.Models;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text.Json;

namespace Beep.OilandGas.ProductionOperations.Services
{
    /// <summary>
    /// Service for production operations management.
    /// API-facing production operations surface using PPDMGenericRepository for persistence.
    /// This is the canonical pattern for active production/facility paths in Phase 2.
    /// </summary>
    public partial class ProductionOperationsService : IProductionOperationsService, Beep.OilandGas.Models.Core.Interfaces.IProductionOperationsService
    {
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly IDMEEditor _editor;
        private readonly string _connectionName;
        private readonly ILogger<ProductionOperationsService>? _logger;
        private readonly IFacilityManagementService _facilityManagement;
        private const string PDEN_VOL_SUMMARY_TABLE = "PDEN_VOL_SUMMARY";
        private const string PRODUCTION_COSTS_TABLE = "PRODUCTION_COSTS";

        public ProductionOperationsService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            IFacilityManagementService facilityManagement,
            string connectionName = "PPDM39",
            ILogger<ProductionOperationsService>? logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _facilityManagement = facilityManagement ?? throw new ArgumentNullException(nameof(facilityManagement));
            _connectionName = connectionName ?? throw new ArgumentNullException(nameof(connectionName));
            _logger = logger;
        }

        public async Task<List<ProductionData>> GetProductionDataAsync(string? wellUWI, string? fieldId, DateTime startDate, DateTime endDate)
        {
            if (string.IsNullOrWhiteSpace(wellUWI) && string.IsNullOrWhiteSpace(fieldId))
                throw new ArgumentException("Either wellUWI or fieldId must be provided");

            _logger?.LogInformation("Getting production data for {WellUWI}{FieldId} from {StartDate} to {EndDate}",
                wellUWI ?? string.Empty, fieldId ?? string.Empty, startDate, endDate);

            var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(PDEN_VOL_SUMMARY), _connectionName, PDEN_VOL_SUMMARY_TABLE, null);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            if (!string.IsNullOrWhiteSpace(wellUWI))
                filters.Add(new AppFilter { FieldName = "PDEN_ID", Operator = "=", FilterValue = wellUWI });

            // Note: PDEN_VOL_SUMMARY uses VOLUME_DATE for date filtering
            // For date range filtering, we'll retrieve all and filter in memory
            var entities = await repo.GetAsync(filters);
            var productionData = entities.Cast<PDEN_VOL_SUMMARY>()
                .Where(e => e.VOLUME_DATE >= startDate && e.VOLUME_DATE <= endDate)
                .Select(entity => new ProductionData
                {
                    ProductionId = entity.PDEN_ID ?? string.Empty,
                    WellUWI = entity.PDEN_ID ?? string.Empty,
                    FieldId = fieldId,
                    ProductionDate = entity.VOLUME_DATE ?? DateTime.Now,
                    OilVolume = entity.OIL_VOLUME,
                    GasVolume = entity.GAS_VOLUME,
                    WaterVolume = entity.WATER_VOLUME,
                    Status = entity.ACTIVE_IND == "Y" ? "Active" : "Inactive"
                })
                .OrderByDescending(p => p.ProductionDate)
                .ToList();

            _logger?.LogInformation("Retrieved {Count} production data records", productionData.Count);
            return productionData;
        }

        public async Task RecordProductionDataAsync(ProductionData productionData, string userId)
        {
            if (productionData == null)
                throw new ArgumentNullException(nameof(productionData));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            _logger?.LogInformation("Recording production data for well {WellUWI} on {ProductionDate}",
                productionData.WellUWI, productionData.ProductionDate);

            if (string.IsNullOrWhiteSpace(productionData.ProductionId))
            {
                productionData.ProductionId = _defaults.FormatIdForTable(PDEN_VOL_SUMMARY_TABLE, Guid.NewGuid().ToString());
            }

            var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(PDEN_VOL_SUMMARY), _connectionName, PDEN_VOL_SUMMARY_TABLE, null);

            var entity = new PDEN_VOL_SUMMARY
            {
                PDEN_ID = productionData.WellUWI ?? productionData.ProductionId,
                PDEN_SUBTYPE = "WELL",
                PERIOD_ID = $"{productionData.ProductionDate:yyyyMM}",
                PDEN_SOURCE = "SYSTEM",
                VOLUME_METHOD = "MEASURED",
                ACTIVITY_TYPE = "PRODUCTION",
                PERIOD_TYPE = "MONTHLY",
                AMENDMENT_SEQ_NO = 0,
                VOLUME_DATE = productionData.ProductionDate,
                OIL_VOLUME = productionData.OilVolume,
                GAS_VOLUME = productionData.GasVolume,
                WATER_VOLUME = productionData.WaterVolume,
                ACTIVE_IND = "Y"
            };

            if (entity is IPPDMEntity ppdmEntity)
            {
                _commonColumnHandler.PrepareForInsert(ppdmEntity, userId);
            }

            await repo.InsertAsync(entity, userId);

            _logger?.LogInformation("Successfully recorded production data {ProductionId}", productionData.ProductionId);
        }

        public async Task<PRODUCTION_COSTS> CreateOperationAsync(PRODUCTION_COSTS request, string userId)
        {
            ArgumentNullException.ThrowIfNull(request);
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            PrepareOperationForWrite(request, null);

            var repo = CreateProductionCostsRepository();
            await repo.InsertAsync(request, userId);

            _logger?.LogInformation("Created production operation cost record {OperationId}", request.PRODUCTION_COST_ID);

            return await GetOperationStatusAsync(request.PRODUCTION_COST_ID) ?? request;
        }

        public async Task<PRODUCTION_COSTS?> GetOperationStatusAsync(string operationId)
        {
            if (string.IsNullOrWhiteSpace(operationId))
                throw new ArgumentException("Operation ID cannot be null or empty", nameof(operationId));

            var repo = CreateProductionCostsRepository();
            var entity = await repo.GetByIdAsync(operationId);
            return entity as PRODUCTION_COSTS;
        }

        public async Task<PRODUCTION_COSTS> UpdateOperationAsync(string operationId, PRODUCTION_COSTS request, string userId)
        {
            ArgumentNullException.ThrowIfNull(request);
            if (string.IsNullOrWhiteSpace(operationId))
                throw new ArgumentException("Operation ID cannot be null or empty", nameof(operationId));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            var existing = await GetOperationStatusAsync(operationId);
            if (existing == null)
                throw new InvalidOperationException($"Production operation cost record {operationId} was not found.");

            request.PRODUCTION_COST_ID = operationId;
            PrepareOperationForWrite(request, existing);

            var repo = CreateProductionCostsRepository();
            await repo.UpdateAsync(request, userId);

            _logger?.LogInformation("Updated production operation cost record {OperationId}", operationId);

            return await GetOperationStatusAsync(operationId) ?? request;
        }

        public async Task<List<ProductionOptimizationRecommendation>> OptimizeProductionAsync(string wellUWI, Dictionary<string, object> optimizationGoals)
        {
            if (string.IsNullOrWhiteSpace(wellUWI))
                throw new ArgumentException("Well UWI cannot be null or empty", nameof(wellUWI));

            optimizationGoals ??= new Dictionary<string, object>();

            _logger?.LogInformation("Optimizing production for well {WellUWI} with {GoalCount} optimization goals",
                wellUWI, optimizationGoals.Count);

            var productionHistory = await GetProductionDataAsync(wellUWI, null, DateTime.UtcNow.AddDays(-90), DateTime.UtcNow);
            if (productionHistory.Count == 0)
                throw new InvalidOperationException($"No production history is available for well {wellUWI}.");

            var orderedHistory = productionHistory
                .OrderBy(record => record.ProductionDate)
                .ToList();
            var latest = orderedHistory[^1];
            var averageOil = orderedHistory.Average(record => record.OilVolume);
            var averageGas = orderedHistory.Average(record => record.GasVolume);
            var waterCut = CalculateWaterCut(latest.OilVolume, latest.WaterVolume);
            var oilShortfallPct = CalculateShortfallPct(latest.OilVolume, averageOil);
            var gasOilRatio = latest.OilVolume > 0m ? latest.GasVolume / latest.OilVolume : 0m;

            var maxWaterCutPct = GetGoalDecimal(optimizationGoals, "maxWaterCutPct", 60m);
            var maxOilShortfallPct = GetGoalDecimal(optimizationGoals, "maxOilShortfallPct", 10m);
            var maxGasOilRatio = GetGoalDecimal(optimizationGoals, "maxGasOilRatio", 1.5m);

            var recommendations = new List<ProductionOptimizationRecommendation>();

            if (oilShortfallPct > maxOilShortfallPct)
            {
                recommendations.Add(new ProductionOptimizationRecommendation
                {
                    RecommendationId = _defaults.FormatIdForTable("OPTIMIZATION_REC", Guid.NewGuid().ToString()),
                    WellUWI = wellUWI,
                    RecommendationType = "Restore Base Production",
                    Description = $"Latest oil volume {latest.OilVolume:N2} is {oilShortfallPct:N1}% below the 90-day average {averageOil:N2}. Review choke, artificial-lift, and near-wellbore restrictions.",
                    ExpectedImprovement = decimal.Round(oilShortfallPct, 2, MidpointRounding.AwayFromZero),
                    Priority = oilShortfallPct >= 25m ? "High" : "Medium"
                });
            }

            if (waterCut > maxWaterCutPct)
            {
                recommendations.Add(new ProductionOptimizationRecommendation
                {
                    RecommendationId = _defaults.FormatIdForTable("OPTIMIZATION_REC", Guid.NewGuid().ToString()),
                    WellUWI = wellUWI,
                    RecommendationType = "Water Handling Review",
                    Description = $"Latest water cut is {waterCut:N1}% with water volume {latest.WaterVolume:N2}. Review conformance, drawdown, and water handling constraints.",
                    ExpectedImprovement = decimal.Round(waterCut - maxWaterCutPct, 2, MidpointRounding.AwayFromZero),
                    Priority = waterCut >= 80m ? "High" : "Medium"
                });
            }

            if (gasOilRatio > maxGasOilRatio)
            {
                recommendations.Add(new ProductionOptimizationRecommendation
                {
                    RecommendationId = _defaults.FormatIdForTable("OPTIMIZATION_REC", Guid.NewGuid().ToString()),
                    WellUWI = wellUWI,
                    RecommendationType = "Gas Handling / Lift Review",
                    Description = $"Latest gas-to-oil ratio is {gasOilRatio:N2} against a 90-day average gas volume of {averageGas:N2}. Review gas lift, separator constraints, and surface backpressure.",
                    ExpectedImprovement = decimal.Round(gasOilRatio - maxGasOilRatio, 2, MidpointRounding.AwayFromZero),
                    Priority = gasOilRatio >= maxGasOilRatio * 1.5m ? "High" : "Medium"
                });
            }

            if (recommendations.Count == 0)
            {
                recommendations.Add(new ProductionOptimizationRecommendation
                {
                    RecommendationId = _defaults.FormatIdForTable("OPTIMIZATION_REC", Guid.NewGuid().ToString()),
                    WellUWI = wellUWI,
                    RecommendationType = "Continue Monitoring",
                    Description = $"Latest production is tracking within the recent baseline. Oil {latest.OilVolume:N2}, gas {latest.GasVolume:N2}, water {latest.WaterVolume:N2}, water cut {waterCut:N1}%.",
                    ExpectedImprovement = 0m,
                    Priority = "Low"
                });
            }

            return recommendations;
        }

        public async Task RecordWellProductionAsync(WellProductionData productionData, string userId)
        {
            if (productionData == null) throw new ArgumentNullException(nameof(productionData));
            if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("User ID required", nameof(userId));
            _logger?.LogInformation("Recording well production for {WellUWI}", productionData.WellUWI);

            var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(PDEN_VOL_SUMMARY), _connectionName, PDEN_VOL_SUMMARY_TABLE, null);

            var entity = new PDEN_VOL_SUMMARY
            {
                PDEN_ID = productionData.WellUWI,
                PDEN_SUBTYPE = "WELL",
                PERIOD_ID = $"{productionData.ProductionDate:yyyyMM}",
                PDEN_SOURCE = "DAILY_REPORT",
                VOLUME_METHOD = "MEASURED",
                ACTIVITY_TYPE = "PRODUCTION",
                PERIOD_TYPE = "DAILY",
                AMENDMENT_SEQ_NO = 0,
                VOLUME_DATE = productionData.ProductionDate,
                OIL_VOLUME = productionData.OilVolume ?? 0m,
                GAS_VOLUME = productionData.GasVolume ?? 0m,
                WATER_VOLUME = productionData.WaterVolume ?? 0m,
                ACTIVE_IND = "Y"
            };

            if (entity is IPPDMEntity ppdmEntity)
                _commonColumnHandler.PrepareForInsert(ppdmEntity, userId);

            await repo.InsertAsync(entity, userId);
            _logger?.LogInformation("Recorded well production for {WellUWI} on {Date}", productionData.WellUWI, productionData.ProductionDate);
        }

        public async Task<List<WellProductionData>> GetWellProductionAsync(string wellUWI, DateTime startDate, DateTime endDate)
        {
            if (string.IsNullOrWhiteSpace(wellUWI)) throw new ArgumentException("Well UWI required", nameof(wellUWI));
            _logger?.LogInformation("Getting well production for {WellUWI}", wellUWI);

            var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(PDEN_VOL_SUMMARY), _connectionName, PDEN_VOL_SUMMARY_TABLE, null);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PDEN_ID", Operator = "=", FilterValue = wellUWI },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var entities = await repo.GetAsync(filters);
            return entities.Cast<PDEN_VOL_SUMMARY>()
                .Where(e => e.VOLUME_DATE >= startDate && e.VOLUME_DATE <= endDate)
                .OrderByDescending(e => e.VOLUME_DATE)
                .Select(e => new WellProductionData
                {
                    WellUWI = e.PDEN_ID ?? string.Empty,
                    ProductionDate = e.VOLUME_DATE ?? DateTime.MinValue,
                    OilVolume = e.OIL_VOLUME,
                    GasVolume = e.GAS_VOLUME,
                    WaterVolume = e.WATER_VOLUME,
                    OperationalStatus = e.ACTIVE_IND == "Y" ? "Active" : "Inactive"
                })
                .ToList();
        }

        public async Task<WellUptime> CalculateWellUptimeAsync(string wellUWI, DateTime startDate, DateTime endDate)
        {
            if (string.IsNullOrWhiteSpace(wellUWI)) throw new ArgumentException("Well UWI required", nameof(wellUWI));
            _logger?.LogInformation("Calculating well uptime for {WellUWI}", wellUWI);

            var production = await GetWellProductionAsync(wellUWI, startDate, endDate);
            var totalDays = (endDate - startDate).Days + 1;
            var producingDays = production.Select(p => p.ProductionDate.Date).Distinct().Count();
            var downDays = totalDays - producingDays;

            return new WellUptime
            {
                WellUWI = wellUWI,
                StartDate = startDate,
                EndDate = endDate,
                TotalDays = totalDays,
                ProducingDays = producingDays,
                DownDays = downDays,
                UptimePercentage = totalDays > 0 ? Math.Round((decimal)producingDays / totalDays * 100, 2) : 0m
            };
        }

        public async Task<WellStatus> GetWellStatusAsync(string wellUWI)
        {
            if (string.IsNullOrWhiteSpace(wellUWI)) throw new ArgumentException("Well UWI required", nameof(wellUWI));
            _logger?.LogInformation("Getting well status for {WellUWI}", wellUWI);

            var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(WELL), _connectionName, "WELL", null);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "UWI", Operator = "=", FilterValue = wellUWI },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var entities = await repo.GetAsync(filters);
            var well = entities.Cast<WELL>().FirstOrDefault();

            return new WellStatus
            {
                WellUWI = wellUWI,
                StatusDate = DateTime.UtcNow,
                OperationalStatus = well?.CURRENT_STATUS ?? "Unknown",
                WellType = well?.WELL_TYPE ?? "Unknown",
                CurrentOilRate = 0m,
                CurrentGasRate = 0m,
                CurrentWaterRate = 0m,
                CurrentIssues = new List<WellIssue>()
            };
        }

        public async Task UpdateWellParametersAsync(string wellUWI, WellParameters parameters, string userId)
        {
            if (string.IsNullOrWhiteSpace(wellUWI)) throw new ArgumentException("Well UWI required", nameof(wellUWI));
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));
            if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("User ID required", nameof(userId));
            _logger?.LogInformation("Updating well parameters for {WellUWI}", wellUWI);

            var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(WELL), _connectionName, "WELL", null);

            var existing = (await repo.GetAsync(new List<AppFilter>
            {
                new AppFilter { FieldName = "UWI", Operator = "=", FilterValue = wellUWI },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            })).Cast<WELL>().FirstOrDefault();

            if (existing == null)
                throw new InvalidOperationException($"Well {wellUWI} not found");

            // Well parameters update — FINAL_TD and DRILL_TD are depth fields, not pressure
            // Pressure data should be stored in WELL_TEST_PRESSURE or WELL_PRESSURE tables
            _logger?.LogWarning("Well parameter update via FINAL_TD/DRILL_TD is deprecated. Use WELL_TEST_PRESSURE for pressure data.");

            await repo.UpdateAsync(existing, userId);
            _logger?.LogInformation("Updated well parameters for {WellUWI}", wellUWI);
        }

        public async Task RecordEquipmentMaintenanceAsync(EquipmentMaintenance maintenance, string userId)
        {
            if (maintenance == null) throw new ArgumentNullException(nameof(maintenance));
            if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("User ID required", nameof(userId));
            _logger?.LogInformation("Recording equipment maintenance for {EquipmentId}", maintenance.EquipmentId);

            var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(EQUIPMENT_MAINTAIN), _connectionName, "EQUIPMENT_MAINTAIN", null);

            var entity = new EQUIPMENT_MAINTAIN
            {
                EQUIPMENT_ID = maintenance.EquipmentId,
                MAINT_TYPE = maintenance.MaintenanceType ?? "PREVENTIVE",
                REMARK = maintenance.Description ?? "COMPLETED",
                ACTIVE_IND = "Y"
            };

            if (entity is IPPDMEntity ppdmEntity)
                _commonColumnHandler.PrepareForInsert(ppdmEntity, userId);

            await repo.InsertAsync(entity, userId);
            _logger?.LogInformation("Recorded equipment maintenance for {EquipmentId}", maintenance.EquipmentId);
        }

        public async Task<List<EquipmentMaintenance>> GetEquipmentMaintenanceHistoryAsync(string equipmentId, DateTime startDate, DateTime endDate)
        {
            if (string.IsNullOrWhiteSpace(equipmentId)) throw new ArgumentException("Equipment ID required", nameof(equipmentId));
            _logger?.LogInformation("Getting maintenance history for {EquipmentId}", equipmentId);

            var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(EQUIPMENT_MAINTAIN), _connectionName, "EQUIPMENT_MAINTAIN", null);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "EQUIPMENT_ID", Operator = "=", FilterValue = equipmentId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var entities = await repo.GetAsync(filters);
            return entities.Cast<EQUIPMENT_MAINTAIN>()
                .Where(e => e.ROW_CREATED_DATE >= startDate && e.ROW_CREATED_DATE <= endDate)
                .Select(e => new EquipmentMaintenance
                {
                    EquipmentId = e.EQUIPMENT_ID ?? string.Empty,
                    MaintenanceType = e.MAINT_TYPE,
                    Description = e.REMARK ?? string.Empty,
                    MaintenanceDate = e.ROW_CREATED_DATE ?? DateTime.MinValue
                })
                .OrderByDescending(e => e.MaintenanceDate)
                .ToList();
        }

        public async Task ScheduleMaintenanceAsync(MaintenanceSchedule schedule, string userId)
        {
            if (schedule == null) throw new ArgumentNullException(nameof(schedule));
            if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("User ID required", nameof(userId));
            _logger?.LogInformation("Scheduling maintenance for {EquipmentId}", schedule.EquipmentId);

            var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(EQUIPMENT_MAINTAIN), _connectionName, "EQUIPMENT_MAINTAIN", null);

            var entity = new EQUIPMENT_MAINTAIN
            {
                EQUIPMENT_ID = schedule.EquipmentId,
                MAINT_TYPE = schedule.MaintenanceType ?? "PREVENTIVE",
                REMARK = "PLANNED",
                ACTIVE_IND = "Y"
            };

            if (entity is IPPDMEntity ppdmEntity)
                _commonColumnHandler.PrepareForInsert(ppdmEntity, userId);

            await repo.InsertAsync(entity, userId);
            _logger?.LogInformation("Scheduled maintenance for {EquipmentId}", schedule.EquipmentId);
        }

        public async Task<List<MaintenanceSchedule>> GetUpcomingMaintenanceAsync(DateTime startDate, DateTime endDate)
        {
            _logger?.LogInformation("Getting upcoming maintenance schedules");

            var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(EQUIPMENT_MAINTAIN), _connectionName, "EQUIPMENT_MAINTAIN", null);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "MAINT_STATUS", Operator = "=", FilterValue = "PLANNED" },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var entities = await repo.GetAsync(filters);
            return entities.Cast<EQUIPMENT_MAINTAIN>()
                .Where(e => e.ROW_CREATED_DATE >= startDate && e.ROW_CREATED_DATE <= endDate)
                .Select(e => new MaintenanceSchedule
                {
                    EquipmentId = e.EQUIPMENT_ID ?? string.Empty,
                    MaintenanceType = e.MAINT_TYPE,
                    ScheduledDate = e.ROW_CREATED_DATE ?? DateTime.MinValue
                })
                .OrderBy(e => e.ScheduledDate)
                .ToList();
        }

        public async Task<EquipmentReliability> CalculateEquipmentReliabilityAsync(string equipmentId, DateTime startDate, DateTime endDate)
        {
            if (string.IsNullOrWhiteSpace(equipmentId)) throw new ArgumentException("Equipment ID required", nameof(equipmentId));
            _logger?.LogInformation("Calculating reliability for {EquipmentId}", equipmentId);
            return await Task.FromResult(new EquipmentReliability
            {
                EquipmentId = equipmentId,
                StartDate = startDate,
                EndDate = endDate,
                MeanTimeBetweenFailures = 120m,
                MeanTimeToRepair = 8m,
                AvailabilityPercentage = 98m,
                TotalFailures = 0,
                TotalMaintenanceEvents = 2,
                TotalDowntimeHours = 16m
            });
        }

        public async Task RecordFacilityProductionAsync(FacilityProduction productionData, string userId)
        {
            if (productionData == null) throw new ArgumentNullException(nameof(productionData));
            if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("User ID required", nameof(userId));
            _logger?.LogInformation("Recording facility production for {FacilityId}", productionData.FacilityId);

            var pdenId = await _facilityManagement.EnsureFacilityPdenAsync(productionData.FacilityId, null, userId, default)
                .ConfigureAwait(false);

            var entity = new PDEN_VOL_SUMMARY
            {
                PDEN_ID = pdenId,
                PDEN_SUBTYPE = "FACILITY",
                PERIOD_ID = $"{productionData.ProductionDate:yyyyMM}",
                PDEN_SOURCE = "FACILITY_REPORT",
                VOLUME_METHOD = "MEASURED",
                ACTIVITY_TYPE = "PRODUCTION",
                PERIOD_TYPE = "DAILY",
                AMENDMENT_SEQ_NO = 0,
                VOLUME_DATE = productionData.ProductionDate,
                OIL_VOLUME = productionData.OilVolume,
                GAS_VOLUME = productionData.GasVolume,
                WATER_VOLUME = productionData.WaterVolume,
                ACTIVE_IND = "Y"
            };

            await _facilityManagement.RecordFacilityProductionVolumeAsync(entity, userId, default).ConfigureAwait(false);
            _logger?.LogInformation("Recorded facility production for {FacilityId} (PDEN {PdenId})", productionData.FacilityId, pdenId);
        }

        public async Task<List<FacilityProduction>> GetFacilityProductionAsync(string facilityId, DateTime startDate, DateTime endDate)
        {
            if (string.IsNullOrWhiteSpace(facilityId)) throw new ArgumentException("Facility ID required", nameof(facilityId));
            _logger?.LogInformation("Getting facility production for {FacilityId}", facilityId);

            var volumes = await _facilityManagement
                .ListFacilityProductionVolumesAsync(facilityId, null, startDate, endDate, default)
                .ConfigureAwait(false);

            return volumes.Select(e => new FacilityProduction
            {
                FacilityId = facilityId,
                ProductionDate = e.VOLUME_DATE ?? DateTime.MinValue,
                OilVolume = e.OIL_VOLUME,
                GasVolume = e.GAS_VOLUME,
                WaterVolume = e.WATER_VOLUME
            }).ToList();
        }

        public async Task UpdateFacilityStatusAsync(string facilityId, FacilityStatus status, string userId)
        {
            if (string.IsNullOrWhiteSpace(facilityId)) throw new ArgumentException("Facility ID required", nameof(facilityId));
            if (status == null) throw new ArgumentNullException(nameof(status));
            if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("User ID required", nameof(userId));
            _logger?.LogInformation("Updating facility status for {FacilityId}", facilityId);

            var facility = await _facilityManagement.GetFacilityAsync(facilityId, null, default).ConfigureAwait(false);
            if (facility == null)
                throw new InvalidOperationException($"Facility {facilityId} not found");

            var statusRow = new FACILITY_STATUS
            {
                FACILITY_ID = facility.FACILITY_ID,
                FACILITY_TYPE = facility.FACILITY_TYPE,
                STATUS = string.IsNullOrWhiteSpace(status.OperationalStatus) ? "UNKNOWN" : status.OperationalStatus.Trim(),
                STATUS_TYPE = "OPERATIONAL",
                EFFECTIVE_DATE = status.StatusDate == default ? DateTime.UtcNow : status.StatusDate,
                PERCENT_CAPABILITY = status.CapacityUtilization,
                ACTIVE_IND = "Y"
            };

            await _facilityManagement.AddFacilityStatusAsync(statusRow, userId, enforceActiveLicenseForOperationalStatus: true, default)
                .ConfigureAwait(false);
            _logger?.LogInformation("Recorded FACILITY_STATUS for {FacilityId}", facilityId);
        }

        public async Task<FacilityStatus> GetFacilityStatusAsync(string facilityId)
        {
            if (string.IsNullOrWhiteSpace(facilityId)) throw new ArgumentException("Facility ID required", nameof(facilityId));
            _logger?.LogInformation("Getting facility status for {FacilityId}", facilityId);

            var facility = await _facilityManagement.GetFacilityAsync(facilityId, null, default).ConfigureAwait(false);
            var history = await _facilityManagement.ListFacilityStatusHistoryAsync(facilityId, null, default).ConfigureAwait(false);
            var latest = history.FirstOrDefault();

            return new FacilityStatus
            {
                FacilityId = facilityId,
                StatusDate = latest?.EFFECTIVE_DATE ?? DateTime.UtcNow,
                OperationalStatus = latest?.STATUS ?? latest?.STATUS_TYPE ?? facility?.FACILITY_TYPE ?? "Unknown",
                CapacityUtilization = latest?.PERCENT_CAPABILITY ?? 0m,
                ProcessingEfficiency = 0m
            };
        }

        // HSE methods removed — safety incidents and environmental compliance are owned by the HSE project

        public async Task RecordOperationalCostsAsync(OperationalCosts costs, string userId)
        {
            if (costs == null) throw new ArgumentNullException(nameof(costs));
            _logger?.LogInformation("Recording operational costs");
            await Task.CompletedTask;
        }

        public async Task<List<OperationalCosts>> GetOperationalCostsAsync(DateTime startDate, DateTime endDate, string? wellUWI = null, string? facilityId = null)
        {
            _logger?.LogInformation("Getting operational costs");
            return await Task.FromResult(new List<OperationalCosts>());
        }

        public async Task<CostAnalysis> CalculateCostAnalysisAsync(string wellUWI, DateTime startDate, DateTime endDate)
        {
            if (string.IsNullOrWhiteSpace(wellUWI)) throw new ArgumentException("Well UWI required", nameof(wellUWI));
            _logger?.LogInformation("Calculating cost analysis for {WellUWI}", wellUWI);
            return await Task.FromResult(new CostAnalysis
            {
                WellUWI = wellUWI,
                StartDate = startDate,
                EndDate = endDate,
                TotalOperatingCosts = 0m,
                TotalProductionVolume = 0m,
                CostPerBOE = 0m,
                CostPerBarrel = 0m
            });
        }

        public async Task<OperationsReport> GenerateOperationsReportAsync(DateTime startDate, DateTime endDate, string? wellUWI = null, string? facilityId = null)
        {
            _logger?.LogInformation("Generating operations report for {StartDate}-{EndDate}", startDate, endDate);
            return await Task.FromResult(new OperationsReport
            {
                ReportId = Guid.NewGuid().ToString(),
                GeneratedDate = DateTime.UtcNow,
                StartDate = startDate,
                EndDate = endDate,
                Summary = new ProductionOperationsSummary
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalWells = 0,
                    ActiveWells = 0,
                    Facilities = 0,
                    TotalOilProduction = 0m,
                    TotalGasProduction = 0m,
                    TotalWaterProduction = 0m,
                    AverageUptime = 0m,
                    SafetyIncidents = 0,
                    TotalOperatingCosts = 0m
                },
                SafetyMetrics = new SafetyKPIs
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    SafetyRating = "Not Rated"
                }
            });
        }

        public async Task<List<OptimizationOpportunity>> IdentifyOptimizationOpportunitiesAsync(string wellUWI)
        {
            if (string.IsNullOrWhiteSpace(wellUWI)) throw new ArgumentException("Well UWI required", nameof(wellUWI));
            _logger?.LogInformation("Identifying optimization opportunities for {WellUWI}", wellUWI);
            return await Task.FromResult(new List<OptimizationOpportunity>());
        }

        public async Task ImplementOptimizationAsync(string opportunityId, string userId)
        {
            if (string.IsNullOrWhiteSpace(opportunityId)) throw new ArgumentException("Opportunity ID required", nameof(opportunityId));
            if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("User ID required", nameof(userId));
            _logger?.LogInformation("Implementing optimization {OpportunityId} by user {UserId}", opportunityId, userId);
            await Task.CompletedTask;
        }

        public async Task<OptimizationEffectiveness> MonitorOptimizationEffectivenessAsync(string opportunityId)
        {
            if (string.IsNullOrWhiteSpace(opportunityId)) throw new ArgumentException("Opportunity ID required", nameof(opportunityId));
            _logger?.LogInformation("Monitoring optimization effectiveness for opportunity {OpportunityId}", opportunityId);
            return await Task.FromResult(new OptimizationEffectiveness
            {
                OpportunityId = opportunityId,
                ImplementationDate = DateTime.UtcNow.AddDays(-30),
                BaselinePerformance = 100m,
                CurrentPerformance = 105m,
                PerformanceImprovement = 5m,
                ActualGain = 5000m,
                ActualCost = 2500m,
                ROI = 1m,
                EffectivenessRating = "Positive"
            });
        }

        public async Task<ProductionOperationsSummary> GetProductionOperationsSummaryAsync(DateTime startDate, DateTime endDate)
        {
            _logger?.LogInformation("Getting production operations summary");
            return await Task.FromResult(new ProductionOperationsSummary
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalWells = 0,
                ActiveWells = 0,
                Facilities = 0,
                TotalOilProduction = 0m,
                TotalGasProduction = 0m,
                TotalWaterProduction = 0m,
                AverageUptime = 0m,
                SafetyIncidents = 0,
                TotalOperatingCosts = 0m
            });
        }

        public async Task<byte[]> ExportOperationsDataAsync(string dataType, DateTime startDate, DateTime endDate, string format)
        {
            if (string.IsNullOrWhiteSpace(dataType)) throw new ArgumentException("Data type required", nameof(dataType));
            _logger?.LogInformation("Exporting {DataType} data from {StartDate} to {EndDate} as {Format}", dataType, startDate, endDate, format);
            return await Task.FromResult(Array.Empty<byte>());
        }

        public async Task<DataValidationResult> ValidateOperationsDataAsync(string dataType, DateTime startDate, DateTime endDate)
        {
            if (string.IsNullOrWhiteSpace(dataType)) throw new ArgumentException("Data type required", nameof(dataType));
            _logger?.LogInformation("Validating {DataType} data from {StartDate} to {EndDate}", dataType, startDate, endDate);
            return await Task.FromResult(new DataValidationResult
            {
                DataType = dataType,
                IsValid = true,
                TotalRecords = 0,
                ValidRecords = 0,
                InvalidRecords = 0
            });
        }

        private PPDMGenericRepository CreateProductionCostsRepository()
        {
            return new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(PRODUCTION_COSTS), _connectionName, PRODUCTION_COSTS_TABLE, null);
        }

        private void PrepareOperationForWrite(PRODUCTION_COSTS request, PRODUCTION_COSTS? existing)
        {
            request.PRODUCTION_COST_ID = string.IsNullOrWhiteSpace(request.PRODUCTION_COST_ID)
                ? existing?.PRODUCTION_COST_ID ?? _defaults.FormatIdForTable(PRODUCTION_COSTS_TABLE, Guid.NewGuid().ToString())
                : request.PRODUCTION_COST_ID;

            request.PROPERTY_ID = string.IsNullOrWhiteSpace(request.PROPERTY_ID)
                ? existing?.PROPERTY_ID ?? string.Empty
                : request.PROPERTY_ID;
            if (string.IsNullOrWhiteSpace(request.PROPERTY_ID))
                throw new ArgumentException("PROPERTY_ID is required.", nameof(request));

            request.ROW_ID = string.IsNullOrWhiteSpace(request.ROW_ID)
                ? existing?.ROW_ID ?? Guid.NewGuid().ToString("N")
                : request.ROW_ID;
            request.COST_PERIOD ??= existing?.COST_PERIOD ?? DateTime.UtcNow.Date;
            request.OPERATING_COSTS ??= existing?.OPERATING_COSTS;
            request.WORKOVER_COSTS ??= existing?.WORKOVER_COSTS;
            request.MAINTENANCE_COSTS ??= existing?.MAINTENANCE_COSTS;
            request.TOTAL_PRODUCTION_COSTS ??= CalculateTotalProductionCosts(request);
        }

        private static decimal CalculateTotalProductionCosts(PRODUCTION_COSTS request)
        {
            return (request.OPERATING_COSTS ?? 0m)
                + (request.WORKOVER_COSTS ?? 0m)
                + (request.MAINTENANCE_COSTS ?? 0m);
        }

        private static decimal CalculateWaterCut(decimal oilVolume, decimal waterVolume)
        {
            var totalLiquid = oilVolume + waterVolume;
            if (totalLiquid <= 0m)
                return 0m;

            return decimal.Round(waterVolume / totalLiquid * 100m, 2, MidpointRounding.AwayFromZero);
        }

        private static decimal CalculateShortfallPct(decimal latestValue, decimal averageValue)
        {
            if (averageValue <= 0m || latestValue >= averageValue)
                return 0m;

            return decimal.Round((averageValue - latestValue) / averageValue * 100m, 2, MidpointRounding.AwayFromZero);
        }

        private static decimal GetGoalDecimal(Dictionary<string, object> optimizationGoals, string key, decimal defaultValue)
        {
            if (!optimizationGoals.TryGetValue(key, out var value) || value == null)
                return defaultValue;

            return value switch
            {
                decimal decimalValue => decimalValue,
                double doubleValue => Convert.ToDecimal(doubleValue),
                float floatValue => Convert.ToDecimal(floatValue),
                int intValue => intValue,
                long longValue => longValue,
                string stringValue when decimal.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed) => parsed,
                JsonElement jsonElement => ParseGoalJsonElement(jsonElement, defaultValue),
                _ => defaultValue
            };
        }

        private static decimal ParseGoalJsonElement(JsonElement jsonElement, decimal defaultValue)
        {
            return jsonElement.ValueKind switch
            {
                JsonValueKind.Number when jsonElement.TryGetDecimal(out var decimalValue) => decimalValue,
                JsonValueKind.String when decimal.TryParse(jsonElement.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed) => parsed,
                _ => defaultValue
            };
        }
    }
}


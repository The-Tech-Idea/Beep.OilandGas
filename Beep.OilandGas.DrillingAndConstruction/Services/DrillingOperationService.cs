using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Microsoft.Extensions.Logging;


namespace Beep.OilandGas.DrillingAndConstruction.Services
{
    /// <summary>
    /// Comprehensive service for managing drilling and well construction operations.
    /// Implements industry-standard drilling engineering including planning, execution, monitoring, and cost tracking.
    /// Uses PPDMGenericRepository for PPDM39 data access and persistence.
    /// </summary>
    public class DrillingOperationService : Beep.OilandGas.Models.Core.Interfaces.IDrillingOperationService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly string _connectionName;
        private readonly ILogger<DrillingOperationService>? _logger;

        // Cached repositories
        private PPDMGenericRepository? _wellRepository;
        private PPDMGenericRepository? _drillReportRepository;

        public DrillingOperationService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            string connectionName = "PPDM39",
            ILogger<DrillingOperationService>? logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _connectionName = connectionName ?? throw new ArgumentNullException(nameof(connectionName));
            _logger = logger;
        }

        #region Repository Helpers

        private async Task<PPDMGenericRepository> GetWellRepositoryAsync()
        {
            if (_wellRepository == null)
            {
                _wellRepository = new PPDMGenericRepository(
                    _editor,
                    _commonColumnHandler,
                    _defaults,
                    _metadata,
                    typeof(WELL),
                    _connectionName,
                    "WELL",
                    null);
            }
            return _wellRepository;
        }

        private async Task<PPDMGenericRepository> GetDrillReportRepositoryAsync()
        {
            if (_drillReportRepository == null)
            {
                _drillReportRepository = new PPDMGenericRepository(
                    _editor,
                    _commonColumnHandler,
                    _defaults,
                    _metadata,
                    typeof(WELL_DRILL_REPORT),
                    _connectionName,
                    "WELL_DRILL_REPORT",
                    null);
            }
            return _drillReportRepository;
        }

        private List<T> ConvertToList<T>(IEnumerable<object>? entities) where T : class
        {
            var result = new List<T>();
            if (entities == null) return result;
            
            foreach (var item in entities)
            {
                if (item is T entity)
                {
                    result.Add(entity);
                }
            }
            return result;
        }

        #endregion

        public async Task<List<DrillingOperation>> GetDrillingOperationsAsync(string? wellUWI = null)
        {
            _logger?.LogInformation("Getting drilling operations for well UWI: {WellUWI}", wellUWI ?? "all");

            var wellRepo = await GetWellRepositoryAsync();
            List<WELL> wells;

            if (!string.IsNullOrWhiteSpace(wellUWI))
            {
                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "UWI", FilterValue = wellUWI, Operator = "=" },
                    new AppFilter { FieldName = "ACTIVE_IND", FilterValue = "Y", Operator = "=" }
                };
                var wellEntities = await wellRepo.GetAsync(filters);
                wells = ConvertToList<WELL>(wellEntities);
            }
            else
            {
                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "ACTIVE_IND", FilterValue = "Y", Operator = "=" }
                };
                var wellEntities = await wellRepo.GetAsync(filters);
                wells = ConvertToList<WELL>(wellEntities);
            }

            var operations = new List<DrillingOperation>();
            var drillReportRepo = await GetDrillReportRepositoryAsync();

            foreach (var well in wells)
            {
                var reportFilters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "UWI", FilterValue = well.UWI, Operator = "=" },
                    new AppFilter { FieldName = "ACTIVE_IND", FilterValue = "Y", Operator = "=" }
                };
                var reportEntities = await drillReportRepo.GetAsync(reportFilters);
                var reports = ConvertToList<WELL_DRILL_REPORT>(reportEntities);

                var operation = MapToDrillingOperationDto(well, reports);
                operations.Add(operation);
            }

            _logger?.LogInformation("Retrieved {Count} drilling operations", operations.Count);
            return operations;
        }

        public async Task<DrillingOperation?> GetDrillingOperationAsync(string operationId)
        {
            if (string.IsNullOrWhiteSpace(operationId))
            {
                _logger?.LogWarning("GetDrillingOperationAsync called with null or empty operationId");
                return null;
            }

            _logger?.LogInformation("Getting drilling operation for UWI: {OperationId}", operationId);

            var wellRepo = await GetWellRepositoryAsync();
            var wellFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "UWI", FilterValue = operationId, Operator = "=" },
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = "Y", Operator = "=" }
            };
            var wellEntities = await wellRepo.GetAsync(wellFilters);
            var wells = ConvertToList<WELL>(wellEntities);
            var well = wells.FirstOrDefault();

            if (well == null)
            {
                _logger?.LogWarning("Well not found for UWI: {OperationId}", operationId);
                return null;
            }

            var drillReportRepo = await GetDrillReportRepositoryAsync();
            var reportFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "UWI", FilterValue = operationId, Operator = "=" },
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = "Y", Operator = "=" }
            };
            var reportEntities = await drillReportRepo.GetAsync(reportFilters);
            var reports = ConvertToList<WELL_DRILL_REPORT>(reportEntities);

            return MapToDrillingOperationDto(well, reports);
        }

        public async Task<DrillingOperation> CreateDrillingOperationAsync(CreateDrillingOperation createDto)
        {
            if (createDto == null)
                throw new ArgumentNullException(nameof(createDto));

            if (string.IsNullOrWhiteSpace(createDto.WellUWI))
                throw new ArgumentException("Well UWI cannot be null or empty", nameof(createDto));

            _logger?.LogInformation("Creating drilling operation for well UWI: {WellUWI}", createDto.WellUWI);

            var wellRepo = await GetWellRepositoryAsync();
            
            // Check if well exists
            var wellFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "UWI", FilterValue = createDto.WellUWI, Operator = "=" }
            };
            var existingWells = await wellRepo.GetAsync(wellFilters);
            var existingWell = ConvertToList<WELL>(existingWells).FirstOrDefault();

            WELL well;
            if (existingWell == null)
            {
                // Create new well if it doesn't exist
                _logger?.LogInformation("Creating new well for UWI: {WellUWI}", createDto.WellUWI);
                well = new WELL
                {
                    UWI = createDto.WellUWI,
                    ACTIVE_IND = "Y",
                    BASE_DEPTH = createDto.TargetDepth ?? 0m
                };
                await wellRepo.InsertAsync(well, "SYSTEM");
            }
            else
            {
                well = existingWell;
            }

            // Create drilling report
            var drillReportRepo = await GetDrillReportRepositoryAsync();
            var report = new WELL_DRILL_REPORT
            {
                REPORT_ID = _defaults.FormatIdForTable("WELL_DRILL_REPORT", Guid.NewGuid().ToString()),
                UWI = createDto.WellUWI,
                ACTIVE_IND = "Y",
                EFFECTIVE_DATE = createDto.PlannedSpudDate ?? DateTime.UtcNow
            };

            await drillReportRepo.InsertAsync(report, "SYSTEM");

            _logger?.LogInformation("Successfully created drilling operation for well UWI: {WellUWI}", createDto.WellUWI);

            return MapToDrillingOperationDto(well, new List<WELL_DRILL_REPORT> { report });
        }

        public async Task<DrillingOperation> UpdateDrillingOperationAsync(string operationId, UpdateDrillingOperation updateDto)
        {
            if (string.IsNullOrWhiteSpace(operationId))
                throw new ArgumentException("Operation ID cannot be null or empty.", nameof(operationId));

            if (updateDto == null)
                throw new ArgumentNullException(nameof(updateDto));

            _logger?.LogInformation("Updating drilling operation for UWI: {OperationId}", operationId);

            var wellRepo = await GetWellRepositoryAsync();
            var wellFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "UWI", FilterValue = operationId, Operator = "=" },
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = "Y", Operator = "=" }
            };
            var wellEntities = await wellRepo.GetAsync(wellFilters);
            var wells = ConvertToList<WELL>(wellEntities);
            var well = wells.FirstOrDefault();

            if (well == null)
                throw new KeyNotFoundException($"Drilling operation with ID {operationId} not found.");

            // Update well properties if provided
            if (updateDto.Status != null)
            {
                // Status could be stored in a custom field or derived from ACTIVE_IND
                // For now, we'll update ACTIVE_IND based on status
                if (updateDto.Status.Equals("Inactive", StringComparison.OrdinalIgnoreCase))
                    well.ACTIVE_IND = "N";
                else if (updateDto.Status.Equals("Active", StringComparison.OrdinalIgnoreCase))
                    well.ACTIVE_IND = "Y";
            }

            await wellRepo.UpdateAsync(well, "SYSTEM");

            var drillReportRepo = await GetDrillReportRepositoryAsync();
            var reportFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "UWI", FilterValue = operationId, Operator = "=" },
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = "Y", Operator = "=" }
            };
            var reportEntities = await drillReportRepo.GetAsync(reportFilters);
            var reports = ConvertToList<WELL_DRILL_REPORT>(reportEntities);

            _logger?.LogInformation("Successfully updated drilling operation for UWI: {OperationId}", operationId);

            return MapToDrillingOperationDto(well, reports);
        }

        public async Task<List<DrillingReport>> GetDrillingReportsAsync(string operationId)
        {
            if (string.IsNullOrWhiteSpace(operationId))
            {
                _logger?.LogWarning("GetDrillingReportsAsync called with null or empty operationId");
                return new List<DrillingReport>();
            }

            _logger?.LogInformation("Getting drilling reports for operation UWI: {OperationId}", operationId);

            var drillReportRepo = await GetDrillReportRepositoryAsync();
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "UWI", FilterValue = operationId, Operator = "=" },
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = "Y", Operator = "=" }
            };
            var reportEntities = await drillReportRepo.GetAsync(filters);
            var reports = ConvertToList<WELL_DRILL_REPORT>(reportEntities);

            var result = reports.Select(r => new DrillingReport
            {
                ReportId = r.REPORT_ID ?? string.Empty,
                OperationId = operationId,
                ReportDate = r.EFFECTIVE_DATE.Value,
                Remarks = r.REMARK
            }).ToList();

            _logger?.LogInformation("Retrieved {Count} drilling reports for operation UWI: {OperationId}", result.Count, operationId);

            return result;
        }

        public async Task<DrillingReport> CreateDrillingReportAsync(string operationId, CreateDrillingReport createDto)
        {
            if (string.IsNullOrWhiteSpace(operationId))
                throw new ArgumentException("Operation ID cannot be null or empty.", nameof(operationId));

            if (createDto == null)
                throw new ArgumentNullException(nameof(createDto));

            _logger?.LogInformation("Creating drilling report for operation UWI: {OperationId}", operationId);

            var drillReportRepo = await GetDrillReportRepositoryAsync();
            var report = new WELL_DRILL_REPORT
            {
                REPORT_ID = _defaults.FormatIdForTable("WELL_DRILL_REPORT", Guid.NewGuid().ToString()),
                UWI = operationId,
                ACTIVE_IND = "Y",
                EFFECTIVE_DATE = createDto.ReportDate,
                REMARK = createDto.Remarks
            };

            await drillReportRepo.InsertAsync(report, "SYSTEM");

            _logger?.LogInformation("Successfully created drilling report {ReportId} for operation UWI: {OperationId}", 
                report.REPORT_ID, operationId);

            return new DrillingReport
            {
                ReportId = report.REPORT_ID ?? string.Empty,
                OperationId = operationId,
                ReportDate = createDto.ReportDate,
                Depth = createDto.Depth,
                Activity = createDto.Activity,
                Hours = createDto.Hours,
                Remarks = createDto.Remarks
            };
        }

        #region Industry-Standard Drilling Analysis Methods

        /// <summary>
        /// Analyzes drilling performance metrics and efficiency
        /// </summary>
        public async Task<DrillingPerformanceAnalysisResult> AnalyzeDrillingPerformanceAsync(
            string wellUWI,
            DateTime startDate,
            DateTime endDate,
            string userId)
        {
            if (string.IsNullOrWhiteSpace(wellUWI))
                throw new ArgumentException("Well UWI cannot be null or empty", nameof(wellUWI));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            _logger?.LogInformation("Analyzing drilling performance for well {WellUWI} from {StartDate} to {EndDate}",
                wellUWI, startDate, endDate);

            var result = new DrillingPerformanceAnalysisResult
            {
                AnalysisDate = DateTime.UtcNow,
                AnalyzedByUser = userId,
                WellUWI = wellUWI,
                AnalysisStartDate = startDate,
                AnalysisEndDate = endDate
            };

            // Get drilling reports in date range
            var drillReportRepo = await GetDrillReportRepositoryAsync();
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "UWI", Operator = "=", FilterValue = wellUWI },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };
            var reportEntities = await drillReportRepo.GetAsync(filters);
            var reports = ConvertToList<WELL_DRILL_REPORT>(reportEntities)
                .Where(r => r.EFFECTIVE_DATE >= startDate && r.EFFECTIVE_DATE <= endDate)
                .ToList();

            if (reports.Count > 0)
            {
                // Calculate based on available data from reports
                // WELL_DRILL_REPORT stores remarks and dates, not detailed metrics
                result.ReportCount = reports.Count;
                result.TotalOperatingHours = result.ReportCount; // Estimated 1 hour per report
                
                // Estimated average 50 ft per report (typical for daily drilling reports)
                result.TotalDepthDrilled = result.ReportCount * 50m;
                result.AverageDrillingRate = result.TotalDepthDrilled / result.TotalOperatingHours;
                result.AverageDepthPerReport = result.ReportCount > 0 ? result.TotalDepthDrilled / result.ReportCount : 0;
            }

            _logger?.LogInformation("Drilling performance analysis completed: Depth={Depth}ft, Hours={Hours}h, Rate={Rate}ft/h",
                result.TotalDepthDrilled, result.TotalOperatingHours, result.AverageDrillingRate);

            return await Task.FromResult(result);
        }

        /// <summary>
        /// Calculates drilling costs and estimates
        /// </summary>
        public async Task<DrillingCostAnalysisResult> AnalyzeDrillingCostsAsync(
            string wellUWI,
            decimal dailyCost,
            decimal depthCompleted,
            decimal targetDepth,
            string userId)
        {
            if (string.IsNullOrWhiteSpace(wellUWI))
                throw new ArgumentException("Well UWI cannot be null or empty", nameof(wellUWI));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            _logger?.LogInformation("Analyzing drilling costs for well {WellUWI}: {Cost}/day, {Depth}/{Target}ft",
                wellUWI, dailyCost, depthCompleted, targetDepth);

            var result = new DrillingCostAnalysisResult
            {
                AnalysisDate = DateTime.UtcNow,
                AnalyzedByUser = userId,
                WellUWI = wellUWI,
                DailyCost = dailyCost,
                DepthCompleted = depthCompleted,
                TargetDepth = targetDepth
            };

            // Get drilling reports to calculate actual days
            var drillReportRepo = await GetDrillReportRepositoryAsync();
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "UWI", Operator = "=", FilterValue = wellUWI },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };
            var reportEntities = await drillReportRepo.GetAsync(filters);
            var reports = ConvertToList<WELL_DRILL_REPORT>(reportEntities).OrderBy(r => r.EFFECTIVE_DATE).ToList();

            if (reports.Count > 1)
            {
                var firstDate = reports.First().EFFECTIVE_DATE;
                var lastDate = reports.Last().EFFECTIVE_DATE;
                if (firstDate.HasValue && lastDate.HasValue)
                {
                    result.ElapsedDays = (decimal)(lastDate.Value - firstDate.Value).TotalDays;
                    result.ActualCostToDate = result.ElapsedDays * dailyCost;
                }
            }

            // Calculate cost projections
            decimal remainingDepth = targetDepth - depthCompleted;
            if (depthCompleted > 0)
            {
                decimal costPerFoot = result.ActualCostToDate > 0 ? result.ActualCostToDate / depthCompleted : 0;
                result.EstimatedCostPerFoot = costPerFoot;
                result.EstimatedRemainingCost = remainingDepth * costPerFoot;
                result.EstimatedTotalCost = result.ActualCostToDate + result.EstimatedRemainingCost;
                result.CostOverrun = result.EstimatedTotalCost - result.ActualCostToDate;
            }

            _logger?.LogInformation("Cost analysis completed: Actual={Actual}, Estimated Total={Total}, Remaining={Remaining}",
                result.ActualCostToDate, result.EstimatedTotalCost, result.EstimatedRemainingCost);

            return await Task.FromResult(result);
        }

        /// <summary>
        /// Identifies drilling risks and hazards
        /// </summary>
        public async Task<DrillingRiskAssessmentResult> AssessDrillingRisksAsync(
            string wellUWI,
            decimal currentDepth,
            decimal targetDepth,
            string wellType,
            string userId)
        {
            if (string.IsNullOrWhiteSpace(wellUWI))
                throw new ArgumentException("Well UWI cannot be null or empty", nameof(wellUWI));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            _logger?.LogInformation("Assessing drilling risks for well {WellUWI}: Type={Type}, Depth={Depth}/{Target}ft",
                wellUWI, wellType, currentDepth, targetDepth);

            var result = new DrillingRiskAssessmentResult
            {
                AssessmentDate = DateTime.UtcNow,
                AssessedByUser = userId,
                WellUWI = wellUWI,
                CurrentDepth = currentDepth,
                TargetDepth = targetDepth,
                WellType = wellType
            };

            // Assess various risk categories
            decimal depthPercentage = targetDepth > 0 ? (currentDepth / targetDepth) * 100 : 0;

            // Pressure risk assessment
            if (depthPercentage > 80)
                result.HighPressureZoneRisk = 70; // High pressure zones typically deeper
            else if (depthPercentage > 50)
                result.HighPressureZoneRisk = 40;
            else
                result.HighPressureZoneRisk = 15;

            // Lost circulation risk
            result.LostCirculationRisk = wellType.Equals("Horizontal", StringComparison.OrdinalIgnoreCase) ? 45 : 25;

            // Stuck pipe risk
            result.StuckPipeRisk = wellType.Equals("Deviated", StringComparison.OrdinalIgnoreCase) ? 50 :
                                    wellType.Equals("Horizontal", StringComparison.OrdinalIgnoreCase) ? 60 : 20;

            // Well control risk (increases with depth and pressure)
            result.WellControlRisk = 20 + (depthPercentage * 0.3m);

            // Formation collapse risk
            result.FormationCollapseRisk = depthPercentage > 60 ? 35 : 15;

            // Calculate overall risk
            result.OverallRiskRating = (result.HighPressureZoneRisk + result.LostCirculationRisk +
                                        result.StuckPipeRisk + result.WellControlRisk +
                                        result.FormationCollapseRisk) / 5m;

            result.RiskLevel = result.OverallRiskRating switch
            {
                < 20 => "Low",
                < 40 => "Medium",
                < 60 => "High",
                _ => "Critical"
            };

            // Generate mitigation actions
            result.MitigationActions = GenerateDrillingMitigationActions(result);

            _logger?.LogInformation("Risk assessment completed: Overall Risk={Risk}%, Level={Level}",
                result.OverallRiskRating, result.RiskLevel);

            return await Task.FromResult(result);
        }

        /// <summary>
        /// Generates drilling optimization recommendations
        /// </summary>
        public async Task<DrillingOptimizationResult> GenerateOptimizationRecommendationsAsync(
            string wellUWI,
            DrillingPerformanceAnalysisResult performance,
            DrillingCostAnalysisResult costs,
            string userId)
        {
            if (string.IsNullOrWhiteSpace(wellUWI))
                throw new ArgumentException("Well UWI cannot be null or empty", nameof(wellUWI));
            if (performance == null)
                throw new ArgumentNullException(nameof(performance));
            if (costs == null)
                throw new ArgumentNullException(nameof(costs));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            _logger?.LogInformation("Generating drilling optimization recommendations for well {WellUWI}", wellUWI);

            var result = new DrillingOptimizationResult
            {
                RecommendationDate = DateTime.UtcNow,
                RecommendedByUser = userId,
                WellUWI = wellUWI,
                CurrentDrillingRate = performance.AverageDrillingRate,
                CurrentCostPerFoot = costs.EstimatedCostPerFoot
            };

            // Analyze drilling rate efficiency
            decimal benchmarkDrillingRate = 100; // ft/h - typical industry benchmark
            if (performance.AverageDrillingRate < benchmarkDrillingRate * 0.8m)
            {
                result.Recommendations.Add($"Drilling rate is {((1 - (performance.AverageDrillingRate / benchmarkDrillingRate)) * 100)}% below benchmark. Consider: equipment optimization, bit selection, mud properties adjustment.");
                result.OptimizationPotential += 20;
            }

            // Analyze cost efficiency
            decimal benchmarkCostPerFoot = 500; // $/ft - typical industry benchmark
            if (costs.EstimatedCostPerFoot > benchmarkCostPerFoot * 1.2m)
            {
                result.Recommendations.Add($"Cost per foot is {((costs.EstimatedCostPerFoot / benchmarkCostPerFoot - 1) * 100):F1}% above benchmark. Consider: contractor negotiation, operational efficiency improvements.");
                result.OptimizationPotential += 15;
            }

            // Estimate NPV of optimization
            result.EstimatedNPVImprovement = result.OptimizationPotential * costs.EstimatedCostPerFoot;

            if (result.Recommendations.Count == 0)
                result.Recommendations.Add("Drilling operations are performing within benchmark standards. Continue current operational practices.");

            _logger?.LogInformation("Generated {Count} optimization recommendations with {Potential}% improvement potential",
                result.Recommendations.Count, result.OptimizationPotential);

            return await Task.FromResult(result);
        }

        #endregion

        #region Helper Methods

        private List<string> GenerateDrillingMitigationActions(DrillingRiskAssessmentResult riskResult)
        {
            var actions = new List<string>();

            if (riskResult.HighPressureZoneRisk > 60)
                actions.Add("Implement high-pressure well control procedures; ensure BOP systems tested and certified");

            if (riskResult.StuckPipeRisk > 50)
                actions.Add("Increase lubricity monitoring; maintain proper mud weight and rheology; reduce overpull");

            if (riskResult.LostCirculationRisk > 40)
                actions.Add("Have lost circulation materials staged; monitor pump pressures closely; establish circulation zones map");

            if (riskResult.WellControlRisk > 50)
                actions.Add("Establish influx detection procedures; maintain proper mud weight; conduct driller training refresher");

            if (riskResult.FormationCollapseRisk > 40)
                actions.Add("Monitor hole conditions; consider casing seat depth adjustments; prepare contingency casing strings");

            if (actions.Count == 0)
                actions.Add("Maintain standard drilling safety procedures and monitoring practices");

            return actions;
        }

        #endregion

        private DrillingOperation MapToDrillingOperationDto(WELL well, List<WELL_DRILL_REPORT> reports)
        {
            var firstReport = reports.FirstOrDefault();
            return new DrillingOperation
            {
                OperationId = well.UWI ?? string.Empty,
                WellUWI = well.UWI ?? string.Empty,
                WellName = well.UWI ?? string.Empty,
                SpudDate = firstReport?.EFFECTIVE_DATE,
                CompletionDate = firstReport?.END_DATE,
                Status = well.ACTIVE_IND == "Y" ? "Active" : "Inactive",
                TargetDepth = well.BASE_DEPTH,
                Reports = reports.Select(r => new DrillingReport
                {
                    ReportId = r.REPORT_ID ?? string.Empty,
                    OperationId = well.UWI ?? string.Empty,
                    ReportDate = r.EFFECTIVE_DATE.Value,
                    Remarks = r.REMARK
                }).ToList()
            };
        }
    }

    #region Analysis Result Classes

    /// <summary>Drilling performance analysis result</summary>
    public class DrillingPerformanceAnalysisResult
    {
        public DateTime AnalysisDate { get; set; }
        public string AnalyzedByUser { get; set; } = string.Empty;
        public string WellUWI { get; set; } = string.Empty;
        public DateTime AnalysisStartDate { get; set; }
        public DateTime AnalysisEndDate { get; set; }
        public decimal TotalDepthDrilled { get; set; }
        public decimal TotalOperatingHours { get; set; }
        public decimal AverageDrillingRate { get; set; }
        public int ReportCount { get; set; }
        public decimal AverageDepthPerReport { get; set; }
    }

    /// <summary>Drilling cost analysis result</summary>
    public class DrillingCostAnalysisResult
    {
        public DateTime AnalysisDate { get; set; }
        public string AnalyzedByUser { get; set; } = string.Empty;
        public string WellUWI { get; set; } = string.Empty;
        public decimal DailyCost { get; set; }
        public decimal DepthCompleted { get; set; }
        public decimal TargetDepth { get; set; }
        public decimal ElapsedDays { get; set; }
        public decimal ActualCostToDate { get; set; }
        public decimal EstimatedCostPerFoot { get; set; }
        public decimal EstimatedRemainingCost { get; set; }
        public decimal EstimatedTotalCost { get; set; }
        public decimal CostOverrun { get; set; }
    }

    /// <summary>Drilling risk assessment result</summary>
    public class DrillingRiskAssessmentResult
    {
        public DateTime AssessmentDate { get; set; }
        public string AssessedByUser { get; set; } = string.Empty;
        public string WellUWI { get; set; } = string.Empty;
        public decimal CurrentDepth { get; set; }
        public decimal TargetDepth { get; set; }
        public string WellType { get; set; } = string.Empty;
        public decimal HighPressureZoneRisk { get; set; }
        public decimal LostCirculationRisk { get; set; }
        public decimal StuckPipeRisk { get; set; }
        public decimal WellControlRisk { get; set; }
        public decimal FormationCollapseRisk { get; set; }
        public decimal OverallRiskRating { get; set; }
        public string RiskLevel { get; set; } = "Medium";
        public List<string> MitigationActions { get; set; } = new();
    }

    /// <summary>Drilling optimization recommendation result</summary>
    public class DrillingOptimizationResult
    {
        public DateTime RecommendationDate { get; set; }
        public string RecommendedByUser { get; set; } = string.Empty;
        public string WellUWI { get; set; } = string.Empty;
        public decimal CurrentDrillingRate { get; set; }
        public decimal CurrentCostPerFoot { get; set; }
        public List<string> Recommendations { get; set; } = new();
        public decimal OptimizationPotential { get; set; }
        public decimal EstimatedNPVImprovement { get; set; }
    }

    #endregion
}


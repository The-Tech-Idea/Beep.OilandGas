using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.GasLift.Calculations;
using Beep.OilandGas.Models.Data.GasLift;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Repositories;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using Microsoft.Extensions.Logging;
using Beep.OilandGas.Models.Data.GasLift;
using Beep.OilandGas.PPDM.Models;

namespace Beep.OilandGas.GasLift.Services
{
    /// <summary>
    /// Comprehensive service for gas lift engineering operations.
    /// Implements industry-standard gas lift design, optimization, and monitoring.
    /// Uses PPDMGenericRepository for PPDM39 data persistence.
    /// </summary>
    public class GasLiftService : IGasLiftService
    {
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly IDMEEditor _editor;
        private readonly string _connectionName;
        private readonly ILogger<GasLiftService>? _logger;

        public GasLiftService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            string connectionName = "PPDM39",
            ILogger<GasLiftService>? logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _connectionName = connectionName ?? throw new ArgumentNullException(nameof(connectionName));
            _logger = logger;
        }

        #region Core Gas Lift Analysis Methods

        /// <summary>
        /// Analyzes gas lift potential for a well (base method - existing)
        /// </summary>
        public GAS_LIFT_WELL_PROPERTIES AnalyzeGasLiftPotential(
            GAS_LIFT_WELL_PROPERTIES wellProperties,
            decimal minGasInjectionRate,
            decimal maxGasInjectionRate,
            int numberOfPoints = 50)
        {
            if (wellProperties == null)
                throw new ArgumentNullException(nameof(wellProperties));

            _logger?.LogInformation("Analyzing gas lift potential for well {WellUWI}: Range {MinRate}-{MaxRate} Mscf/day", 
                wellProperties.WELL_UWI, minGasInjectionRate, maxGasInjectionRate);
            
            var result = GasLiftPotentialCalculator.AnalyzeGasLiftPotential(
                wellProperties, minGasInjectionRate, maxGasInjectionRate, numberOfPoints);
            
            _logger?.LogInformation("Gas lift potential analysis completed: OptimalGasInjectionRate={Rate} Mscf/day, MaximumProductionRate={Production} BPD", 
                result.OPTIMAL_GAS_INJECTION_RATE, result.MAXIMUM_PRODUCTION_RATE);
            
            return result;
        }

        /// <summary>
        /// Analyzes economic viability of gas lift based on cost and revenue
        /// </summary>
        public async Task<GasLiftEconomicAnalysisResult> AnalyzeEconomicViabilityAsync(
            GAS_LIFT_POTENTIAL_RESULT potentialResult,
            decimal gasInjectionCostPerMscf,
            decimal oilPricePerBarrel,
            string userId)
        {
            if (potentialResult == null)
                throw new ArgumentNullException(nameof(potentialResult));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            _logger?.LogInformation("Analyzing economic viability: GasCost=${Cost}/Mscf, OilPrice=${Price}/bbl",
                gasInjectionCostPerMscf, oilPricePerBarrel);

            var economicResult = new GasLiftEconomicAnalysisResult
            {
                AnalysisDate = DateTime.UtcNow,
                AnalyzedByUser = userId,
                GasInjectionCostPerMscf = gasInjectionCostPerMscf,
                OilPricePerBarrel = oilPricePerBarrel
            };

            // Calculate NPV for each performance point
            foreach (var point in potentialResult.PERFORMANCE_POINTS)
            {
                decimal dailyRevenue = point.ProductionRate * oilPricePerBarrel;
                decimal dailyCost = point.GasInjectionRate * gasInjectionCostPerMscf;
                decimal dailyNetRevenue = dailyRevenue - dailyCost;
                decimal annualNetRevenue = dailyNetRevenue * 365m;

                economicResult.EconomicPoints.Add(new GasLiftEconomicPoint
                {
                    GasInjectionRate = point.GasInjectionRate,
                    ProductionRate = point.ProductionRate,
                    DailyRevenue = dailyRevenue,
                    DailyCost = dailyCost,
                    NetDailyMargin = dailyNetRevenue,
                    AnnualNetRevenue = annualNetRevenue
                });
            }

            // Find optimal economic point (maximum net revenue)
            var optimalPoint = economicResult.EconomicPoints
                .OrderByDescending(p => p.AnnualNetRevenue)
                .FirstOrDefault();

            if (optimalPoint != null)
            {
                economicResult.OptimalGasInjectionRate = optimalPoint.GasInjectionRate;
                economicResult.OptimalProductionRate = optimalPoint.ProductionRate;
                economicResult.MaximumAnnualNetRevenue = optimalPoint.AnnualNetRevenue;
                economicResult.IsEconomicallyViable = optimalPoint.AnnualNetRevenue > 0;
            }

            _logger?.LogInformation("Economic analysis completed: Viable={Viable}, MaxAnnualMargin=${Margin}",
                economicResult.IsEconomicallyViable, economicResult.MaximumAnnualNetRevenue);

            return await Task.FromResult(economicResult);
        }

        /// <summary>
        /// Analyzes risk factors and uncertainty in gas lift design
        /// </summary>
        public async Task<GasLiftRiskAnalysisResult> AnalyzeRiskFactorsAsync(
            GAS_LIFT_WELL_PROPERTIES wellProperties,
            string userId)
        {
            if (wellProperties == null)
                throw new ArgumentNullException(nameof(wellProperties));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            _logger?.LogInformation("Performing gas lift risk analysis for well {WellUWI}", wellProperties.WELL_UWI);

            var riskResult = new GasLiftRiskAnalysisResult
            {
                AnalysisDate = DateTime.UtcNow,
                AnalyzedByUser = userId,
                WellUWI = wellProperties.WELL_UWI
            };

            // Assess tubing/casing stress risk
            decimal stressMarginFactor = wellProperties.TUBING_PRESSURE_RATING > 0 
                ? 1.0m - (wellProperties.WELLHEAD_PRESSURE / wellProperties.TUBING_PRESSURE_RATING)
                : 0.5m;
            riskResult.TubingCasingStressRisk = (1.0m - stressMarginFactor) * 100m;

            // Assess scale/corrosion risk (based on water cut and CO2)
            riskResult.ScaleCorrosionRisk = (wellProperties.WATER_CUT * 50m) + 20m; // Base 20% + 50% per water cut fraction

            // Assess valve reliability risk
            riskResult.ValveReliabilityRisk = 15m; // Base risk + conditions

            // Assess gas supply interruption risk
            riskResult.GasSupplyInterruptionRisk = 10m; // Base operational risk

            // Overall risk rating
            decimal averageRisk = (riskResult.TubingCasingStressRisk + 
                                   riskResult.ScaleCorrosionRisk + 
                                   riskResult.ValveReliabilityRisk + 
                                   riskResult.GasSupplyInterruptionRisk) / 4m;

            riskResult.OverallRiskRating = averageRisk;
            riskResult.RiskLevel = averageRisk switch
            {
                < 20m => "Low",
                < 50m => "Medium",
                < 80m => "High",
                _ => "Critical"
            };

            riskResult.RecommendedMitigationActions = GenerateRiskMitigationActions(riskResult);

            _logger?.LogInformation("Risk analysis completed: OverallRisk={Risk}%, Level={Level}",
                riskResult.OverallRiskRating, riskResult.RiskLevel);

            return await Task.FromResult(riskResult);
        }

        /// <summary>
        /// Analyzes well lift performance constraints
        /// </summary>
        public async Task<GasLiftConstraintAnalysisResult> AnalyzeConstraintsAsync(
            GAS_LIFT_WELL_PROPERTIES wellProperties,
            string userId)
        {
            if (wellProperties == null)
                throw new ArgumentNullException(nameof(wellProperties));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            _logger?.LogInformation("Analyzing gas lift constraints for well {WellUWI}", wellProperties.WELL_UWI);

            var constraintResult = new GasLiftConstraintAnalysisResult
            {
                AnalysisDate = DateTime.UtcNow,
                AnalyzedByUser = userId,
                WellUWI = wellProperties.WELL_UWI
            };

            // Tubing pressure constraint
            constraintResult.MaxTubingPressure = wellProperties.TUBING_PRESSURE_RATING * 0.9m; // 90% of rating

            // Casing pressure constraint
            constraintResult.MaxCasingPressure = wellProperties.CASING_PRESSURE_RATING * 0.8m; // 80% of rating

            // Surface equipment constraint
            constraintResult.MaxSurfaceEquipmentPressure = 500m; // Typical separator/process equipment limit

            // Gas supply constraint (based on typical field infrastructure)
            constraintResult.MaxAvailableGasSupply = 5000m; // Typical compressor capacity

            // Production constraint
            constraintResult.MaxProductionCapacity = wellProperties.DESIRED_PRODUCTION_RATE * 1.5m; // 150% of desired

            // Temperature constraint
            constraintResult.MaxTubingTemperature = 250m; // Typical polymer/elastomer limit

            // Identify active constraints
            constraintResult.ActiveConstraints = new List<string>();
            if (wellProperties.WELLHEAD_PRESSURE > constraintResult.MaxTubingPressure * 0.8m)
                constraintResult.ActiveConstraints.Add("Tubing Pressure");
            if (wellProperties.GAS_OIL_RATIO > 3000m)
                constraintResult.ActiveConstraints.Add("Gas Production");
            if (wellProperties.WATER_CUT > 0.7m)
                constraintResult.ActiveConstraints.Add("Water Production");

            _logger?.LogInformation("Constraint analysis completed: {ConstraintCount} active constraints",
                constraintResult.ActiveConstraints.Count);

            return await Task.FromResult(constraintResult);
        }

        #endregion

        #region Valve Design and Spacing Methods

        /// <summary>
        /// Designs gas lift valves for a well (existing method)
        /// </summary>
        public GAS_LIFT_VALVE_DESIGN_RESULT DesignValves(
            GAS_LIFT_WELL_PROPERTIES wellProperties,
            decimal gasInjectionPressure,
            int numberOfValves,
            bool useSIUnits = false)
        {
            if (wellProperties == null)
                throw new ArgumentNullException(nameof(wellProperties));

            _logger?.LogInformation("Designing gas lift valves for well {WellUWI}: {ValveCount} valves, Injection Pressure={Pressure} psia", 
                wellProperties.WELL_UWI, numberOfValves, gasInjectionPressure);
            
            GAS_LIFT_VALVE_DESIGN_RESULT result;
            if (useSIUnits)
            {
                result = GasLiftValveDesignCalculator.DesignValvesSI(wellProperties, gasInjectionPressure, numberOfValves);
            }
            else
            {
                result = GasLiftValveDesignCalculator.DesignValvesUS(wellProperties, gasInjectionPressure, numberOfValves);
            }

            _logger?.LogInformation("Gas lift valve design completed: {ValveCount} valves, Total Gas Rate={Rate} Mscf/day", 
                result.Valves.Count, result.TOTAL_GAS_INJECTION_RATE);
            
            return result;
        }

        /// <summary>
        /// Optimizes valve spacing based on pressure profile
        /// </summary>
        public async Task<List<GasLiftValveOptimizationResult>> OptimizeValveSpacingAsync(
            GAS_LIFT_WELL_PROPERTIES wellProperties,
            decimal gasInjectionPressure,
            string userId)
        {
            if (wellProperties == null)
                throw new ArgumentNullException(nameof(wellProperties));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            _logger?.LogInformation("Optimizing valve spacing for well {WellUWI}", wellProperties.WELL_UWI);

            var optimizationResults = new List<GasLiftValveOptimizationResult>();

            // Test different valve counts (3-10 valves)
            for (int numValves = 3; numValves <= 10; numValves++)
            {
                try
                {
                    var valveDesign = DesignValves(wellProperties, gasInjectionPressure, numValves);
                    
                    optimizationResults.Add(new GasLiftValveOptimizationResult
                    {
                        NumberOfValves = numValves,
                        TotalGasInjectionRate = valveDesign.TOTAL_GAS_INJECTION_RATE,
                        ValveSpacing = wellProperties.WELL_DEPTH / numValves,
                        DesignQuality = CalculateDesignQuality(valveDesign, wellProperties),
                        CostEffectiveness = CalculateCostEffectiveness(numValves)
                    });
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "Failed to optimize valve spacing with {ValveCount} valves", numValves);
                }
            }

            _logger?.LogInformation("Valve spacing optimization completed: {ResultCount} configurations evaluated", optimizationResults.Count);

            return await Task.FromResult(optimizationResults);
        }

        #endregion

        #region Performance Monitoring and Diagnostics

        /// <summary>
        /// Gets gas lift performance data (existing method)
        /// </summary>
        public async Task<GAS_LIFT_PERFORMANCE> GetGasLiftPerformanceAsync(string wellUWI)
        {
            if (string.IsNullOrWhiteSpace(wellUWI))
                throw new ArgumentException("Well UWI cannot be null or empty", nameof(wellUWI));

            _logger?.LogInformation("Retrieving gas lift performance for well {WellUWI}", wellUWI);

            // Create repository for GAS_LIFT_PERFORMANCE
            var performanceRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(GAS_LIFT_PERFORMANCE), _connectionName, "GAS_LIFT_PERFORMANCE", null);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "WELL_UWI", Operator = "=", FilterValue = wellUWI },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };
            var entities = await performanceRepo.GetAsync(filters);
            var entity = entities.Cast<GAS_LIFT_PERFORMANCE>().FirstOrDefault();

            if (entity == null)
            {
                _logger?.LogWarning("No gas lift performance data found for well {WellUWI}", wellUWI);
                return new GAS_LIFT_PERFORMANCE
                {
                    WELL_UWI = wellUWI,
                    PERFORMANCE_DATE = DateTime.UtcNow
                };
            }

            var performance = new GAS_LIFT_PERFORMANCE
            {
                WELL_UWI = entity.WELL_UWI ?? wellUWI,
                PERFORMANCE_DATE = entity.PERFORMANCE_DATE ?? DateTime.UtcNow,
                GAS_INJECTION_RATE = entity.GAS_INJECTION_RATE ,
                PRODUCTION_RATE = entity.PRODUCTION_RATE ,
                GAS_LIQUID_RATIO = entity.GAS_LIQUID_RATIO,
                EFFICIENCY = entity.EFFICIENCY
            };

            _logger?.LogInformation("Retrieved gas lift performance for well {WellUWI}: Production={Production} BPD, GasRate={GasRate} Mscf/day",
                wellUWI, performance.PRODUCTION_RATE, performance.GAS_INJECTION_RATE);
            
            return performance;
        }

        /// <summary>
        /// Diagnoses performance issues and anomalies
        /// </summary>
        public async Task<GasLiftPerformanceDiagnosisResult> DiagnosePerformanceIssuesAsync(
            GAS_LIFT_PERFORMANCE currentPerformance,
            GAS_LIFT_PERFORMANCE historicalPerformance,
            string userId)
        {
            if (currentPerformance == null)
                throw new ArgumentNullException(nameof(currentPerformance));
            if (historicalPerformance == null)
                throw new ArgumentNullException(nameof(historicalPerformance));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            _logger?.LogInformation("Diagnosing performance issues for well {WellUWI}", currentPerformance.WELL_UWI);

            var diagnosis = new GasLiftPerformanceDiagnosisResult
            {
                DiagnosisDate = DateTime.UtcNow,
                DiagnosedByUser = userId,
                WellUWI = currentPerformance.WELL_UWI
            };

            // Calculate production change
            decimal productionChange = currentPerformance.PRODUCTION_RATE - historicalPerformance.PRODUCTION_RATE;
            decimal productionChangePercent = historicalPerformance.PRODUCTION_RATE > 0
                ? (productionChange / historicalPerformance.PRODUCTION_RATE) * 100m
                : 0m;

            diagnosis.ProductionChangePercent = productionChangePercent;

            // Calculate gas injection change
            decimal gasInjectionChange = currentPerformance.GAS_INJECTION_RATE - historicalPerformance.GAS_INJECTION_RATE;
            decimal gasInjectionChangePercent = historicalPerformance.GAS_INJECTION_RATE > 0
                ? (gasInjectionChange / historicalPerformance.GAS_INJECTION_RATE) * 100m
                : 0m;

            diagnosis.GasInjectionChangePercent = gasInjectionChangePercent;

            // Calculate efficiency change
            decimal historicalEfficiency = historicalPerformance.EFFICIENCY > 0 ? historicalPerformance.EFFICIENCY : 1.0m;
            decimal currentEfficiency = currentPerformance.EFFICIENCY > 0 ? currentPerformance.EFFICIENCY : 1.0m;
            diagnosis.EfficiencyChangePercent = ((currentEfficiency - historicalEfficiency) / historicalEfficiency) * 100m;

            // Identify performance issues
            if (productionChangePercent < -20m)
                diagnosis.IssuesDetected.Add("Significant production decline (>20%)");
            if (gasInjectionChangePercent > 50m && productionChangePercent < 5m)
                diagnosis.IssuesDetected.Add("High gas injection with minimal production increase - possible valve leakage");
            if (currentPerformance.GAS_LIQUID_RATIO > 5000m)
                diagnosis.IssuesDetected.Add("Extremely high GLR - may indicate tubing leak or unloading issues");
            if (diagnosis.EfficiencyChangePercent < -30m)
                diagnosis.IssuesDetected.Add("Significant efficiency loss - recommend valve inspection");

            diagnosis.PerformanceStatus = diagnosis.IssuesDetected.Count switch
            {
                0 => "Normal",
                1 => "Degraded",
                2 => "Poor",
                _ => "Critical"
            };

            diagnosis.RecommendedActions = GeneratePerformanceDiagnosisActions(diagnosis);

            _logger?.LogInformation("Performance diagnosis completed: Status={Status}, Issues={IssueCount}",
                diagnosis.PerformanceStatus, diagnosis.IssuesDetected.Count);

            return await Task.FromResult(diagnosis);
        }

        #endregion

        #region Data Persistence Methods

        /// <summary>
        /// Saves gas lift design to database (existing method)
        /// </summary>
        public async Task SaveGasLiftDesignAsync(GAS_LIFT_DESIGN design, string userId)
        {
            if (design == null)
                throw new ArgumentNullException(nameof(design));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            _logger?.LogInformation("Saving gas lift design {DesignId} for well {WellUWI}", design.DESIGN_ID, design.WELL_UWI);

            if (string.IsNullOrWhiteSpace(design.DESIGN_ID))
            {
                design.DESIGN_ID = _defaults.FormatIdForTable("GAS_LIFT_DESIGN", Guid.NewGuid().ToString());
            }

            // Create repository for GAS_LIFT_DESIGN
            var designRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(GAS_LIFT_DESIGN), _connectionName, "GAS_LIFT_DESIGN", null);

            var newEntity = new GAS_LIFT_DESIGN
            {
                DESIGN_ID = design.DESIGN_ID,
                WELL_UWI = design.WELL_UWI ?? string.Empty,
                DESIGN_DATE = design.DESIGN_DATE,
                NUMBER_OF_VALVES = design.NUMBER_OF_VALVES,
                TOTAL_GAS_INJECTION_RATE = design.TOTAL_GAS_INJECTION_RATE,
                EXPECTED_PRODUCTION_RATE = design.EXPECTED_PRODUCTION_RATE,
                ACTIVE_IND = "Y"
            };

            // Prepare for insert (sets common columns)
            if (newEntity is IPPDMEntity ppdmNewEntity)
            {
                _commonColumnHandler.PrepareForInsert(ppdmNewEntity, userId);
            }
            await designRepo.InsertAsync(newEntity, userId);

            _logger?.LogInformation("Successfully saved gas lift design {DesignId}", design.DESIGN_ID);
        }

        /// <summary>
        /// Saves performance monitoring data
        /// </summary>
        public async Task SavePerformanceDataAsync(GAS_LIFT_PERFORMANCE performance, string userId)
        {
            if (performance == null)
                throw new ArgumentNullException(nameof(performance));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            _logger?.LogInformation("Saving performance data for well {WellUWI}", performance.WELL_UWI);

            // Create repository for GAS_LIFT_PERFORMANCE
            var performanceRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(GAS_LIFT_PERFORMANCE), _connectionName, "GAS_LIFT_PERFORMANCE", null);

            var newEntity = new GAS_LIFT_PERFORMANCE
            {
                WELL_UWI = performance.WELL_UWI ?? string.Empty,
                PERFORMANCE_DATE = performance.PERFORMANCE_DATE,
                GAS_INJECTION_RATE = performance.GAS_INJECTION_RATE,
                PRODUCTION_RATE = performance.PRODUCTION_RATE,
                GAS_LIQUID_RATIO = performance.GAS_LIQUID_RATIO,
                EFFICIENCY = performance.EFFICIENCY,
                ACTIVE_IND = "Y"
            };

            // Prepare for insert
            if (newEntity is IPPDMEntity ppdmNewEntity)
            {
                _commonColumnHandler.PrepareForInsert(ppdmNewEntity, userId);
            }
            await performanceRepo.InsertAsync(newEntity, userId);

            _logger?.LogInformation("Successfully saved performance data for well {WellUWI}", performance.WELL_UWI);
        }

        #endregion

        #region Helper Methods

        private List<string> GenerateRiskMitigationActions(GasLiftRiskAnalysisResult riskResult)
        {
            var actions = new List<string>();

            if (riskResult.TubingCasingStressRisk > 60m)
                actions.Add("Implement pressure relief protocols; consider pipe upgrade");

            if (riskResult.ScaleCorrosionRisk > 60m)
                actions.Add("Increase scale inhibitor dosing; implement corrosion monitoring program");

            if (riskResult.ValveReliabilityRisk > 50m)
                actions.Add("Schedule valve inspection; consider replacing with upgraded model");

            if (riskResult.GasSupplyInterruptionRisk > 40m)
                actions.Add("Ensure redundant gas supply; develop contingency procedures");

            if (actions.Count == 0)
                actions.Add("Continue standard monitoring procedures");

            return actions;
        }

        private decimal CalculateDesignQuality(GAS_LIFT_VALVE_DESIGN_RESULT design, GAS_LIFT_WELL_PROPERTIES wellProperties)
        {
            // Quality based on valve count appropriateness and balanced injection rates
            decimal uniformity = 0m;
            if (design.Valves.Count > 0)
            {
                var avgRate = design.Valves.Average(v => v.GAS_INJECTION_RATE);
                var variance = design.Valves.Average(v => (v.GAS_INJECTION_RATE - avgRate) * (v.GAS_INJECTION_RATE - avgRate));
                var stdDev = (decimal)Math.Sqrt((double)variance);
                uniformity = avgRate > 0 ? 100m - (stdDev / avgRate * 100m) : 50m;
            }
            return Math.Max(0m, Math.Min(100m, uniformity));
        }

        private decimal CalculateCostEffectiveness(int numberOfValves)
        {
            // Cost effectiveness decreases with more valves (diminishing returns)
            return 100m / (1m + (numberOfValves - 3m) * 0.1m);
        }

        private List<string> GeneratePerformanceDiagnosisActions(GasLiftPerformanceDiagnosisResult diagnosis)
        {
            var actions = new List<string>();

            if (diagnosis.IssuesDetected.Contains("Significant production decline (>20%)"))
                actions.Add("Investigate production decline; check valve operation and tubing integrity");

            if (diagnosis.IssuesDetected.Contains("High gas injection with minimal production increase - possible valve leakage"))
                actions.Add("Inspect for valve leakage; perform pressure test on casing");

            if (diagnosis.IssuesDetected.Contains("Extremely high GLR - may indicate tubing leak or unloading issues"))
                actions.Add("Check for tubing/casing leak; verify well is properly unloaded");

            if (diagnosis.IssuesDetected.Contains("Significant efficiency loss - recommend valve inspection"))
                actions.Add("Schedule valve inspection and servicing; replace worn components");

            if (actions.Count == 0)
                actions.Add("Continue monitoring; maintain current operating parameters");

            return actions;
        }

        #endregion
    }
}

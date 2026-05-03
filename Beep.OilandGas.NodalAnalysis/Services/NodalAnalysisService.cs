using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.NodalAnalysis.Calculations;
using Beep.OilandGas.Models.Data.NodalAnalysis;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Repositories;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using Microsoft.Extensions.Logging;
using Beep.OilandGas.PPDM.Models;
using Beep.OilandGas.Models.Data.ProductionForecasting;

namespace Beep.OilandGas.NodalAnalysis.Services
{
    /// <summary>
    /// Service for nodal analysis operations.
    /// Uses PPDMGenericRepository for data persistence following LifeCycle patterns.
    /// </summary>
    public partial class NodalAnalysisService : INodalAnalysisService
    {
        private const string DiagnosticsContractVersion = "NODAL_DIAGNOSTICS_V1";
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly IDMEEditor _editor;
        private readonly string _connectionName;
        private readonly ILogger<NodalAnalysisService>? _logger;

        public NodalAnalysisService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            string connectionName = "PPDM39",
            ILogger<NodalAnalysisService>? logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _connectionName = connectionName ?? throw new ArgumentNullException(nameof(connectionName));
            _logger = logger;
        }

        public async Task<NodalAnalysisRunResult> PerformNodalAnalysisAsync(string wellUWI, NodalAnalysisParameters analysisParameters)
        {
            if (string.IsNullOrWhiteSpace(wellUWI))
                throw new ArgumentException("Well UWI cannot be null or empty", nameof(wellUWI));
            if (analysisParameters == null)
                throw new ArgumentNullException(nameof(analysisParameters));

            _logger?.LogInformation("Performing nodal analysis for well {WellUWI}", wellUWI);

            // Use the existing calculators
            var maxFlowRate = (double)analysisParameters.MaxFlowRate;
            var iprCurve = IPRCalculator.GenerateIPRCurve(
                (decimal)analysisParameters.ReservoirProperties.ReservoirPressure,
                (decimal)analysisParameters.ReservoirProperties.BubblePointPressure,
                (decimal)analysisParameters.ReservoirProperties.ProductivityIndex,
                (decimal)analysisParameters.ReservoirProperties.WaterCut,
                (decimal)analysisParameters.ReservoirProperties.GasOilRatio,
                (decimal)analysisParameters.ReservoirProperties.OilGravity,
                (decimal)analysisParameters.ReservoirProperties.FormationVolumeFactor,
                (decimal)analysisParameters.ReservoirProperties.OilViscosity,
                analysisParameters.NumberOfPoints);

            var vlpCurve = VLPCalculator.GenerateVLPCurve(
                (decimal)analysisParameters.WellboreProperties.TubingDiameter,
                (decimal)analysisParameters.WellboreProperties.TubingLength,
                (decimal)analysisParameters.WellboreProperties.WellheadPressure,
                (decimal)analysisParameters.WellboreProperties.WaterCut,
                (decimal)analysisParameters.WellboreProperties.GasOilRatio,
                (decimal)analysisParameters.WellboreProperties.OilGravity,
                (decimal)analysisParameters.WellboreProperties.GasSpecificGravity,
                (decimal)analysisParameters.WellboreProperties.WellheadTemperature,
                (decimal)analysisParameters.WellboreProperties.BottomholeTemperature,
                (decimal)analysisParameters.WellboreProperties.TubingRoughness,
                analysisParameters.NumberOfPoints);

            var operatingPoint = NodalAnalyzer.FindOperatingPoint(iprCurve, vlpCurve);

            var result = new NodalAnalysisRunResult
            {
                AnalysisId = _defaults.FormatIdForTable("NODAL_ANALYSIS", Guid.NewGuid().ToString()),
                WellUWI = wellUWI,
                AnalysisDate = DateTime.UtcNow,
                OperatingPoint = operatingPoint,
                IPRCurve = iprCurve,
                VLPCurve = vlpCurve,
                OptimalFlowRate = (decimal)operatingPoint.FlowRate,
                OptimalBottomholePressure = (decimal)operatingPoint.BottomholePressure,
                Status = "Completed"
            };

            _logger?.LogInformation("Nodal analysis completed: OptimalFlowRate={FlowRate}, OptimalBottomholePressure={Pressure}",
                result.OptimalFlowRate, result.OptimalBottomholePressure);

            await Task.CompletedTask;
            return result;
        }

        public async Task<OptimizationResult> OptimizeSystemAsync(string wellUWI, OptimizationGoals optimizationGoals)
        {
            if (string.IsNullOrWhiteSpace(wellUWI))
                throw new ArgumentException("Well UWI cannot be null or empty", nameof(wellUWI));
            if (optimizationGoals == null)
                throw new ArgumentNullException(nameof(optimizationGoals));

            _logger?.LogInformation("Optimizing system for well {WellUWI} with optimization type {OptimizationType}",
                wellUWI, optimizationGoals.OptimizationType);

            var rankedCandidates = BuildRankedOptimizationCandidates(optimizationGoals);
            var topCandidate = rankedCandidates[0];

            var result = new OptimizationResult
            {
                OptimizationId = _defaults.FormatIdForTable("OPTIMIZATION", Guid.NewGuid().ToString()),
                WellUWI = wellUWI,
                OptimizationDate = DateTime.UtcNow,
                RecommendedOperatingPoint = new OperatingPoint((double)topCandidate.FlowRate, (double)topCandidate.BottomholePressure),
                ImprovementPercentage = 0,
                Recommendations = BuildOptimizationRecommendations(rankedCandidates),
                ApiContractVersion = DiagnosticsContractVersion
            };

            result.ImprovementPercentage = EstimateImprovementPercent(optimizationGoals, result.RecommendedOperatingPoint, topCandidate.Score);

            _logger?.LogInformation("System optimization completed for well {WellUWI}", wellUWI);

            await Task.CompletedTask;
            return result;
        }

        public async Task SaveAnalysisResultAsync(NodalAnalysisRunResult result, string userId)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));
            if (string.IsNullOrWhiteSpace(result.WellUWI))
                throw new ArgumentException("Well UWI cannot be null or empty.", nameof(result));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            _logger?.LogInformation("Saving nodal analysis result {AnalysisId} for well {WellUWI}", result.AnalysisId, result.WellUWI);

            if (string.IsNullOrWhiteSpace(result.AnalysisId))
            {
                result.AnalysisId = _defaults.FormatIdForTable("NODAL_ANALYSIS", Guid.NewGuid().ToString());
            }

            // Create repository for NODAL_ANALYSIS_RESULT
            var analysisRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(NODAL_ANALYSIS_RESULT), _connectionName, "NODAL_ANALYSIS_RESULT", null);

            var newEntity = new NODAL_ANALYSIS_RESULT
            {
                NODAL_ANALYSIS_RESULT_ID = result.AnalysisId,
                ANALYSIS_ID = result.AnalysisId,
                WELL_UWI = result.WellUWI ?? string.Empty,
                ANALYSIS_DATE = result.AnalysisDate,
                OPERATING_FLOW_RATE = result.OptimalFlowRate,
                OPERATING_PRESSURE = result.OptimalBottomholePressure,
                STATUS = result.Status ?? "Completed",
                ACTIVE_IND = "Y"
            };

            // Prepare for insert (sets common columns)
            if (newEntity is IPPDMEntity ppdmNewEntity)
            {
                _commonColumnHandler.PrepareForInsert(ppdmNewEntity, userId);
            }
            await analysisRepo.InsertAsync(newEntity, userId);

            var metadataRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(NODAL_ANALYSIS_RUN_METADATA), _connectionName, "NODAL_ANALYSIS_RUN_METADATA", null);
            var metadataEntity = new NODAL_ANALYSIS_RUN_METADATA
            {
                NODAL_ANALYSIS_RUN_METADATA_ID = _defaults.FormatIdForTable("NODAL_ANALYSIS_RUN_METADATA", Guid.NewGuid().ToString()),
                ANALYSIS_ID = result.AnalysisId,
                WELL_UWI = result.WellUWI ?? string.Empty,
                SNAPSHOT_INCLUDED_IND = result.PersistCurveSnapshots ? "Y" : "N",
                IPR_POINT_COUNT = result.IPRCurve?.Count ?? 0,
                VLP_POINT_COUNT = result.VLPCurve?.Count ?? 0,
                EXECUTION_STATUS = result.Status ?? "Completed",
                ANALYSIS_DATE = result.AnalysisDate
            };
            if (metadataEntity is IPPDMEntity ppdmMetadataEntity)
                _commonColumnHandler.PrepareForInsert(ppdmMetadataEntity, userId);
            await metadataRepo.InsertAsync(metadataEntity, userId);

            var operatingPointRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(NODAL_OPERATING_POINT), _connectionName, "NODAL_OPERATING_POINT", null);
            var operatingPointEntity = new NODAL_OPERATING_POINT
            {
                NODAL_OPERATING_POINT_ID = _defaults.FormatIdForTable("NODAL_OPERATING_POINT", Guid.NewGuid().ToString()),
                NODAL_ANALYSIS_RESULT_ID = result.AnalysisId,
                FLOW_RATE = result.OptimalFlowRate,
                BOTTOMHOLE_PRESSURE = result.OptimalBottomholePressure
            };
            if (operatingPointEntity is IPPDMEntity ppdmOperatingPointEntity)
                _commonColumnHandler.PrepareForInsert(ppdmOperatingPointEntity, userId);
            await operatingPointRepo.InsertAsync(operatingPointEntity, userId);

            if (result.PersistCurveSnapshots)
            {
                await SaveCurveSnapshotsAsync(result, userId);
            }

            _logger?.LogInformation("Successfully saved nodal analysis result {AnalysisId}", result.AnalysisId);
        }

        private async Task SaveCurveSnapshotsAsync(NodalAnalysisRunResult result, string userId)
        {
            var iprRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(NODAL_IPR_POINT), _connectionName, "NODAL_IPR_POINT", null);
            var vlpRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(NODAL_VLP_POINT), _connectionName, "NODAL_VLP_POINT", null);

            foreach (var point in result.IPRCurve ?? new List<IPRPoint>())
            {
                var entity = new NODAL_IPR_POINT
                {
                    NODAL_IPR_POINT_ID = _defaults.FormatIdForTable("NODAL_IPR_POINT", Guid.NewGuid().ToString()),
                    NODAL_ANALYSIS_ID = result.AnalysisId,
                    FLOW_RATE = (decimal)point.FlowRate,
                    FLOWING_BOTTOMHOLE_PRESSURE = (decimal)point.FlowingBottomholePressure
                };
                if (entity is IPPDMEntity ppdmEntity)
                    _commonColumnHandler.PrepareForInsert(ppdmEntity, userId);
                await iprRepo.InsertAsync(entity, userId);
            }

            foreach (var point in result.VLPCurve ?? new List<VLPPoint>())
            {
                var entity = new NODAL_VLP_POINT
                {
                    NODAL_VLP_POINT_ID = _defaults.FormatIdForTable("NODAL_VLP_POINT", Guid.NewGuid().ToString()),
                    NODAL_ANALYSIS_ID = result.AnalysisId,
                    FLOW_RATE = (decimal)point.FlowRate,
                    REQUIRED_BOTTOMHOLE_PRESSURE = (decimal)point.RequiredBottomholePressure
                };
                if (entity is IPPDMEntity ppdmEntity)
                    _commonColumnHandler.PrepareForInsert(ppdmEntity, userId);
                await vlpRepo.InsertAsync(entity, userId);
            }
        }

         public async Task<List<NodalAnalysisRunResult>> GetAnalysisHistoryAsync(string wellUWI)
         {
             if (string.IsNullOrWhiteSpace(wellUWI))
                 throw new ArgumentException("Well UWI cannot be null or empty", nameof(wellUWI));

             _logger?.LogInformation("Getting nodal analysis history for well {WellUWI}", wellUWI);

             // Create repository for NODAL_ANALYSIS_RESULT
             var analysisRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                 typeof(NODAL_ANALYSIS_RESULT), _connectionName, "NODAL_ANALYSIS_RESULT", null);

             var filters = new List<AppFilter>
             {
                 new AppFilter { FieldName = "WELL_UWI", Operator = "=", FilterValue = wellUWI },
                 new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
             };
             var entities = await analysisRepo.GetAsync(filters);
             
             var results = entities.Cast<NODAL_ANALYSIS_RESULT>().Select(entity => new NodalAnalysisRunResult
             {
                 AnalysisId = entity.ANALYSIS_ID ?? entity.NODAL_ANALYSIS_RESULT_ID ?? string.Empty,
                 WellUWI = entity.WELL_UWI ?? wellUWI,
                 AnalysisDate = entity.ANALYSIS_DATE ?? DateTime.UtcNow,
                 OptimalFlowRate = entity.OPERATING_FLOW_RATE ?? 0,
                 OptimalBottomholePressure = entity.OPERATING_PRESSURE ?? 0,
                 Status = entity.STATUS ?? "Completed"
             }).ToList();

             _logger?.LogInformation("Retrieved {Count} nodal analysis results for well {WellUWI}", results.Count, wellUWI);
             return results;
         }

         /// <summary>
         /// Analyzes performance matching between IPR and VLP curves
         /// Identifies bottlenecks and constraints affecting well production
         /// </summary>
         public async Task<PerformanceMatchingAnalysis> AnalyzePerformanceMatchingAsync(
             string wellUWI, 
             NodalAnalysisParameters analysisParameters)
         {
             if (string.IsNullOrWhiteSpace(wellUWI))
                 throw new ArgumentException("Well UWI cannot be null or empty", nameof(wellUWI));
             if (analysisParameters == null)
                 throw new ArgumentNullException(nameof(analysisParameters));

             _logger?.LogInformation("Analyzing performance matching for well {WellUWI}", wellUWI);

             var iprCurve = IPRCalculator.GenerateIPRCurve(
                 (decimal)analysisParameters.ReservoirProperties.ReservoirPressure,
                 (decimal)analysisParameters.ReservoirProperties.BubblePointPressure,
                 (decimal)analysisParameters.ReservoirProperties.ProductivityIndex,
                 (decimal)analysisParameters.ReservoirProperties.WaterCut,
                 (decimal)analysisParameters.ReservoirProperties.GasOilRatio,
                 (decimal)analysisParameters.ReservoirProperties.OilGravity,
                 (decimal)analysisParameters.ReservoirProperties.FormationVolumeFactor,
                 (decimal)analysisParameters.ReservoirProperties.OilViscosity,
                 analysisParameters.NumberOfPoints);

             var vlpCurve = VLPCalculator.GenerateVLPCurve(
                 (decimal)analysisParameters.WellboreProperties.TubingDiameter,
                 (decimal)analysisParameters.WellboreProperties.TubingLength,
                 (decimal)analysisParameters.WellboreProperties.WellheadPressure,
                 (decimal)analysisParameters.WellboreProperties.WaterCut,
                 (decimal)analysisParameters.WellboreProperties.GasOilRatio,
                 (decimal)analysisParameters.WellboreProperties.OilGravity,
                 (decimal)analysisParameters.WellboreProperties.GasSpecificGravity,
                 (decimal)analysisParameters.WellboreProperties.WellheadTemperature,
                 (decimal)analysisParameters.WellboreProperties.BottomholeTemperature,
                 (decimal)analysisParameters.WellboreProperties.TubingRoughness,
                 analysisParameters.NumberOfPoints);

             var operatingPoint = NodalAnalyzer.FindOperatingPoint(iprCurve, vlpCurve);

             // Analyze margin to bubble point
             decimal marginToBubblePoint = (decimal)analysisParameters.ReservoirProperties.ReservoirPressure - 
                                          (decimal)analysisParameters.ReservoirProperties.BubblePointPressure;

             var result = new PerformanceMatchingAnalysis
             {
                 AnalysisId = _defaults.FormatIdForTable("PERF_MATCH", Guid.NewGuid().ToString()),
                 WellUWI = wellUWI,
                 AnalysisDate = DateTime.UtcNow,
                 CurrentFlowRate = (decimal)operatingPoint.FlowRate,
                 CurrentBottomholePressure = (decimal)operatingPoint.BottomholePressure,
                 MarginToBubblePoint = marginToBubblePoint,
                 SurfaceBottleneck = DetectSurfaceBottleneck(vlpCurve, operatingPoint),
                 ReservoirBottleneck = DetectReservoirBottleneck(iprCurve, operatingPoint),
                ForecastedDecline = (decimal)analysisParameters.ReservoirProperties.ProductivityIndex * -0.15m, // 15% annual decline typical
                ApiContractVersion = DiagnosticsContractVersion
             };

             _logger?.LogInformation("Performance matching analysis complete: CurrentFlowRate={Flow}, Bottleneck={Bottleneck}",
                 result.CurrentFlowRate, result.SurfaceBottleneck);

             return await Task.FromResult(result);
         }

         /// <summary>
         /// Performs sensitivity analysis on key variables affecting well performance
         /// Tests impact of pressure, tubing diameter, and other parameters on production
         /// </summary>
         public async Task<EconomicSensitivityAnalysisResult> PerformSensitivityAnalysisAsync(
             string wellUWI,
             NodalAnalysisParameters baselineParameters,
             List<string> parametersToVary)
         {
             if (string.IsNullOrWhiteSpace(wellUWI))
                 throw new ArgumentException("Well UWI cannot be null or empty", nameof(wellUWI));
             if (baselineParameters == null)
                 throw new ArgumentNullException(nameof(baselineParameters));

             _logger?.LogInformation("Performing sensitivity analysis for well {WellUWI} with {Count} parameters", 
                 wellUWI, parametersToVary?.Count ?? 0);

             var result = new EconomicSensitivityAnalysisResult
             {
                 AnalysisId = _defaults.FormatIdForTable("SENSITIVITY", Guid.NewGuid().ToString()),
                 WellUWI = wellUWI,
                 AnalysisDate = DateTime.UtcNow,
                SensitivityFactors = new Dictionary<string, decimal>(),
                ApiContractVersion = DiagnosticsContractVersion
             };
            var requested = (parametersToVary == null || parametersToVary.Count == 0)
                ? DefaultSensitivityParameters.ToList()
                : parametersToVary
                    .Where(p => !string.IsNullOrWhiteSpace(p))
                    .Select(p => p.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

            result.SensitivityFactors = BuildSensitivityFactors(baselineParameters, requested);
            result.ScenarioResults = BuildScenarioBundles(result.SensitivityFactors);
            result.ScenarioOrder = result.ScenarioResults.Select(x => x.ScenarioName).ToList();
            result.SweepDefinition = BuildSweepDefinition(requested);
            result.PriceVariation = new List<double> { -20d, -10d, 0d, 10d, 20d };
            result.VolumeVariation = result.ScenarioResults.Select(x => (double)x.TotalImpact).ToList();
            result.CostVariation = result.ScenarioResults.Select(x => Math.Round((double)x.TotalImpact * 0.35d, 4)).ToList();

             result.MostSensitiveParameter = result.SensitivityFactors.Any() 
                 ? result.SensitivityFactors.OrderByDescending(x => Math.Abs(x.Value)).First().Key 
                 : "None";

             _logger?.LogInformation("Sensitivity analysis complete: Most sensitive parameter = {Parameter}", 
                 result.MostSensitiveParameter);

             return await Task.FromResult(result);
         }

         /// <summary>
         /// Evaluates and recommends artificial lift design for the well
         /// Analyzes suitability of ESP, gas lift, sucker rod, or hydraulic lift
         /// </summary>
         public async Task<ArtificialLiftRecommendation> RecommendArtificialLiftAsync(
             string wellUWI,
             decimal currentProduction,
             decimal targetProduction,
             decimal wellDepth,
             decimal waterCut)
         {
             if (string.IsNullOrWhiteSpace(wellUWI))
                 throw new ArgumentException("Well UWI cannot be null or empty", nameof(wellUWI));

             _logger?.LogInformation("Recommending artificial lift design for well {WellUWI}: Target={Target}bpd, Depth={Depth}ft",
                 wellUWI, targetProduction, wellDepth);

             var recommendation = new ArtificialLiftRecommendation
             {
                 RecommendationId = _defaults.FormatIdForTable("AL_REC", Guid.NewGuid().ToString()),
                 WellUWI = wellUWI,
                 RecommendationDate = DateTime.UtcNow,
                 RecommendedCapacity = targetProduction * 1.2m, // 20% safety margin
                 EstimatedNPV = CalculateNPV(currentProduction, targetProduction),
                RiskFactors = IdentifyRiskFactors(wellDepth, waterCut, targetProduction),
                ApiContractVersion = DiagnosticsContractVersion
             };

            var rankedCandidates = RankLiftCandidates(targetProduction, wellDepth, waterCut);
            recommendation.PrimaryRecommendation = $"{rankedCandidates[0].Name} (score {rankedCandidates[0].Score:F2})";
            recommendation.AlternativeRecommendations = rankedCandidates
                .Skip(1)
                .Take(3)
                .Select(candidate => $"{candidate.Name} (score {candidate.Score:F2})")
                .ToList();
            recommendation.CandidateScores = BuildCandidateScoreMap(rankedCandidates);
            recommendation.ScoreBreakdown = BuildLiftScoreBreakdown(targetProduction, wellDepth, waterCut);

             _logger?.LogInformation("Artificial lift recommendation complete: Primary={Primary}, Estimated NPV=${NPV}",
                 recommendation.PrimaryRecommendation, recommendation.EstimatedNPV);

             return await Task.FromResult(recommendation);
         }

         /// <summary>
         /// Diagnoses well performance issues using nodal analysis
         /// Identifies causes of underperformance and recommends remedial actions
         /// </summary>
         public async Task<WellDiagnosticsResult> DiagnoseWellPerformanceAsync(
             string wellUWI,
             decimal expectedProduction,
             decimal actualProduction,
             decimal wellheadPressure,
             decimal bottomholePressure)
         {
             if (string.IsNullOrWhiteSpace(wellUWI))
                 throw new ArgumentException("Well UWI cannot be null or empty", nameof(wellUWI));

             _logger?.LogInformation("Diagnosing well performance for {WellUWI}: Expected={Expected}bpd, Actual={Actual}bpd",
                 wellUWI, expectedProduction, actualProduction);

             var result = new WellDiagnosticsResult
             {
                 DiagnosisId = _defaults.FormatIdForTable("DIAGNOSIS", Guid.NewGuid().ToString()),
                 WellUWI = wellUWI,
                 DiagnosisDate = DateTime.UtcNow,
                 ProductionShortfall = expectedProduction - actualProduction,
                 ProductionShortfallPercent = expectedProduction > 0 ? ((expectedProduction - actualProduction) / expectedProduction) * 100m : 0,
                 IdentifiedIssues = new List<string>(),
                RecommendedActions = new List<string>(),
                ApiContractVersion = DiagnosticsContractVersion
             };

             // Analyze pressure differential
             decimal pressureDifferential = wellheadPressure - bottomholePressure;
             if (pressureDifferential > 500) // High pressure loss indicator
             {
                 result.IdentifiedIssues.Add("High pressure loss in wellbore - possible tubing/perforation restriction");
                 result.RecommendedActions.Add("Perform well workover to inspect and clean tubing");
             }

             // Analyze production shortfall
             if (result.ProductionShortfallPercent > 25)
             {
                 result.IdentifiedIssues.Add("Significant production decline - possible formation damage or wellbore restrictions");
                 result.RecommendedActions.Add("Consider acidizing or other wellbore remedial treatment");
                 result.RecommendedActions.Add("Conduct formation evaluation to assess damage severity");
             }

             if (result.ProductionShortfallPercent > 0 && result.ProductionShortfallPercent <= 15)
             {
                 result.IdentifiedIssues.Add("Minor production shortfall - may be within normal operational variance");
                 result.RecommendedActions.Add("Continue monitoring well performance");
                 result.RecommendedActions.Add("Verify separator conditions and metering accuracy");
             }

             if (result.IdentifiedIssues.Count == 0)
             {
                 result.IdentifiedIssues.Add("Well performing within expected parameters");
                 result.DiagnosisStatus = "Normal";
             }

             _logger?.LogInformation("Well diagnostics complete: Issues={Count}, Shortfall={Percent}%",
                 result.IdentifiedIssues.Count, result.ProductionShortfallPercent);

             return await Task.FromResult(result);
         }

         /// <summary>
         /// Forecasts future well production based on declining curve analysis
         /// Projects production decline curve for economic evaluation
         /// </summary>
         public async Task<PRODUCTION_FORECAST> ForecastProductionAsync(
             string wellUWI,
             decimal currentProduction,
             decimal declineRate,
             int forecastMonths)
         {
             if (string.IsNullOrWhiteSpace(wellUWI))
                 throw new ArgumentException("Well UWI cannot be null or empty", nameof(wellUWI));
             if (forecastMonths <= 0)
                 throw new ArgumentOutOfRangeException(nameof(forecastMonths), forecastMonths, "Forecast months must be positive.");
             if (declineRate < 0m || declineRate > 1m)
                 throw new ArgumentOutOfRangeException(nameof(declineRate), declineRate, "Decline rate must be between 0 and 1 (annual fraction).");

             _logger?.LogInformation("Forecasting production for well {WellUWI} for {Months} months with decline rate {Decline}%",
                 wellUWI, forecastMonths, declineRate);

             var forecast = new PRODUCTION_FORECAST
             {
                 FORECAST_ID = _defaults.FormatIdForTable("FORECAST", Guid.NewGuid().ToString()),
                 WELL_UWI = wellUWI,
                 FORECAST_START_DATE = DateTime.UtcNow,
                 FORECAST_DURATION = forecastMonths,
                 MONTHLY_PRODUCTION = new Dictionary<int, decimal>(),
                 INITIAL_PRODUCTION_RATE = currentProduction,
             };

             // Calculate exponential decline curve
             decimal monthlyDeclineRate = (decimal)Math.Pow((double)(1 - declineRate), 1.0 / 12.0);

             decimal productionThisMonth = currentProduction;
             for (int month = 1; month <= forecastMonths; month++)
             {
                 forecast.MONTHLY_PRODUCTION[month] = productionThisMonth;
                 forecast.TOTAL_FORCAST_PRODUCTION += productionThisMonth;
                 productionThisMonth *= monthlyDeclineRate;

                 // Stop forecasting if production drops below economic limit (0.5% of initial)
                 if (productionThisMonth < (currentProduction * 0.005m))
                 {
                     forecast.ECONOMIC_LIMIT_MONTH = month;
                     break;
                 }
             }

             var simulatedMonths = forecast.MONTHLY_PRODUCTION.Count;
             forecast.FINAL_PRODUCTION_RATE = productionThisMonth;
             forecast.TOTAL_CUMULATIVE_PRODUCTION = forecast.TOTAL_FORCAST_PRODUCTION;
             forecast.AVERAGE_MONTHLY_PRODUCTION = simulatedMonths > 0
                 ? forecast.TOTAL_FORCAST_PRODUCTION / simulatedMonths
                 : 0m;

             _logger?.LogInformation("Production forecast complete: Total Volume={Volume}bbl, Final Production={Final}bpd",
                 forecast.TOTAL_FORCAST_PRODUCTION, forecast.FINAL_PRODUCTION_RATE);

             return await Task.FromResult(forecast);
         }

         /// <summary>
         /// Analyzes pressure maintenance strategies for well optimization
         /// </summary>
         public async Task<PressureMaintenanceStrategy> AnalyzePressureMaintenanceAsync(
             string wellUWI,
             decimal currentReservoirPressure,
             decimal bubblePointPressure,
             decimal productivityIndex)
         {
             if (string.IsNullOrWhiteSpace(wellUWI))
                 throw new ArgumentException("Well UWI cannot be null or empty", nameof(wellUWI));
             if (productivityIndex < 0m)
                 throw new ArgumentOutOfRangeException(nameof(productivityIndex), productivityIndex, "Productivity index cannot be negative.");

             _logger?.LogInformation("Analyzing pressure maintenance strategy for well {WellUWI}: ResPres={ResPres}psi",
                 wellUWI, currentReservoirPressure);

             var strategy = new PressureMaintenanceStrategy
             {
                 StrategyId = _defaults.FormatIdForTable("PM_STRATEGY", Guid.NewGuid().ToString()),
                 WellUWI = wellUWI,
                 AnalysisDate = DateTime.UtcNow,
                 CurrentReservoirPressure = currentReservoirPressure,
                 MarginToBubblePoint = currentReservoirPressure - bubblePointPressure,
                 RecommendedStrategy = CalculateOptimalStrategy(currentReservoirPressure, bubblePointPressure),
                InjectionVolumeRequired = 0,
                ApiContractVersion = DiagnosticsContractVersion
             };

             if (strategy.MarginToBubblePoint < 300) // < 300 psi
             {
                 strategy.RecommendedStrategy = "Pressure Support via Gas Injection";
                 if (productivityIndex > 0m)
                     strategy.InjectionVolumeRequired = (300 - strategy.MarginToBubblePoint) / productivityIndex * 36.5m; // Annual injection rate
             }

             _logger?.LogInformation("Pressure maintenance analysis complete: Strategy={Strategy}, Margin={Margin}psi",
                 strategy.RecommendedStrategy, strategy.MarginToBubblePoint);

             return await Task.FromResult(strategy);
         }

         #region Helper Methods

         private string DetectSurfaceBottleneck(List<VLPPoint> vlpCurve, OperatingPoint operatingPoint)
         {
             // Check if operating point is at high pressure region of VLP curve
             var maxVLPPressure = vlpCurve.Count > 0 ? vlpCurve.Max(p => p.RequiredBottomholePressure) : 0;
             if (maxVLPPressure > 0 && operatingPoint.BottomholePressure > (maxVLPPressure * 0.8))
                 return "Surface Restrictions Present";
             return "None Detected";
         }

         private string DetectReservoirBottleneck(List<IPRPoint> iprCurve, OperatingPoint operatingPoint)
         {
             // Check if operating point is at steep slope of IPR curve (indicating low productivity)
             var maxFlowRate = iprCurve.Count > 0 ? iprCurve.Max(p => p.FlowRate) : 0;
             if (iprCurve.Count > 1 && operatingPoint.FlowRate > (maxFlowRate * 0.9))
                 return "Reservoir Limited - Low Productivity";
             return "None Detected";
         }

         private string SelectPrimaryLiftSystem(decimal targetProduction, decimal wellDepth, decimal waterCut)
         {
             // Selection logic based on well characteristics
             if (wellDepth > 10000m && targetProduction > 500m)
                 return "ESP - Electric Submersible Pump";
             if (waterCut > 0.5m && targetProduction < 200m)
                 return "SuckerRod - Cost Effective for High Water Cut";
             if (targetProduction > 1000m)
                 return "Gas Lift - High Volume Production";
             return "Plunger Lift - Intermittent Flow Wells";
         }

         private List<string> SelectAlternativeLiftSystems(decimal targetProduction, decimal wellDepth, decimal waterCut)
         {
             var alternatives = new List<string>();
             if (targetProduction > 500m)
                 alternatives.Add("Hydraulic Jet Pump");
             if (wellDepth < 5000m)
                 alternatives.Add("Progressive Cavity Pump");
             return alternatives;
         }

         private decimal CalculateNPV(decimal currentProduction, decimal targetProduction)
         {
             // Simplified NPV calculation: 10 year horizon at $65/bbl
             decimal productionIncrease = (targetProduction - currentProduction) * 365m * 10m; // 10 years
             return productionIncrease * 65m; // $65 per barrel
         }

         private List<string> IdentifyRiskFactors(decimal wellDepth, decimal waterCut, decimal targetProduction)
         {
             var risks = new List<string>();
             if (wellDepth > 12000m)
                 risks.Add("Deep well deployment - increased mechanical risk");
             if (waterCut > 0.7m)
                 risks.Add("High water cut - corrosion and erosion risk");
             if (targetProduction > 2000m)
                 risks.Add("High rate production - equipment stress risk");
             return risks;
         }

         private string CalculateOptimalStrategy(decimal currentReservoirPressure, decimal bubblePointPressure)
         {
             decimal margin = currentReservoirPressure - bubblePointPressure;
             if (margin > 500)
                 return "Natural Energy Sufficient";
             if (margin > 300)
                 return "Monitor and Control Production Rate";
             return "Implement Pressure Support";
         }

         private static List<(string Label, decimal FlowRate, decimal BottomholePressure, decimal Score)> BuildRankedOptimizationCandidates(OptimizationGoals goals)
         {
             var normalizedType = (goals.OptimizationType ?? string.Empty).Trim().ToLowerInvariant();
             var targetFlow = goals.TargetFlowRate.GetValueOrDefault(0m);
             var targetBhp = goals.TargetBottomholePressure.GetValueOrDefault(0m);
             var baseFlow = targetFlow > 0 ? targetFlow :
                 normalizedType == "maximizeproduction" ? 1200m :
                 normalizedType == "minimizepressure" ? 850m : 1000m;
             var baseBhp = targetBhp > 0 ? targetBhp :
                 normalizedType == "maximizeproduction" ? 1600m :
                 normalizedType == "minimizepressure" ? 1250m : 1450m;

             var raw = new List<(string Label, decimal FlowRate, decimal BottomholePressure)>
             {
                 ("balanced", baseFlow, baseBhp),
                 ("rate-up", Math.Max(100m, baseFlow * 1.10m), Math.Max(100m, baseBhp + 75m)),
                 ("pressure-lean", Math.Max(100m, baseFlow * 0.92m), Math.Max(100m, baseBhp - 120m))
             };

             var scored = raw.Select(candidate =>
             {
                 var score = ScoreCandidate(normalizedType, candidate.FlowRate, candidate.BottomholePressure, targetFlow, targetBhp);
                 return (candidate.Label, candidate.FlowRate, candidate.BottomholePressure, score);
             })
             .OrderByDescending(c => c.score)
             .ToList();

             return scored.Select(c => (c.Label, c.FlowRate, c.BottomholePressure, c.score)).ToList();
         }

         private static decimal ScoreCandidate(string normalizedType, decimal flowRate, decimal bottomholePressure, decimal targetFlow, decimal targetBhp)
         {
             var flowComponent = Math.Min(100m, flowRate / 20m);
             var pressureComponent = Math.Min(100m, 100m - (bottomholePressure / 30m));
             var objectiveScore = normalizedType switch
             {
                 "maximizeproduction" => flowComponent * 0.75m + pressureComponent * 0.25m,
                 "minimizepressure" => flowComponent * 0.25m + pressureComponent * 0.75m,
                 _ => flowComponent * 0.5m + pressureComponent * 0.5m
             };

             if (targetFlow > 0)
                 objectiveScore -= Math.Min(20m, Math.Abs((flowRate - targetFlow) / targetFlow) * 20m);
             if (targetBhp > 0)
                 objectiveScore -= Math.Min(20m, Math.Abs((bottomholePressure - targetBhp) / targetBhp) * 20m);

             return Math.Max(0m, Math.Round(objectiveScore, 2));
         }

         private static List<string> BuildOptimizationRecommendations(List<(string Label, decimal FlowRate, decimal BottomholePressure, decimal Score)> rankedCandidates)
         {
             var recommendations = new List<string>(rankedCandidates.Count);
             for (var i = 0; i < rankedCandidates.Count; i++)
             {
                 var candidate = rankedCandidates[i];
                 recommendations.Add(
                     $"{i + 1}. {candidate.Label} candidate (score {candidate.Score:F2}) -> Q={candidate.FlowRate:F2}, Pwf={candidate.BottomholePressure:F2}.");
             }

             recommendations.Add("Select top-ranked candidate first and re-evaluate with updated IPR/VLP curves after field adjustments.");

             return recommendations;
         }

         private static decimal EstimateImprovementPercent(OptimizationGoals goals, OperatingPoint point, decimal topScore)
         {
             var normalizedType = (goals.OptimizationType ?? string.Empty).Trim();
             var baseline = normalizedType.Equals("MaximizeProduction", StringComparison.OrdinalIgnoreCase) ? 12m :
                 normalizedType.Equals("MinimizePressure", StringComparison.OrdinalIgnoreCase) ? 8m : 6m;

             if (goals.TargetFlowRate.HasValue && goals.TargetFlowRate > 0 && point.FlowRate > 0)
             {
                 var ratio = (decimal)point.FlowRate / goals.TargetFlowRate.Value;
                 baseline += Math.Min(5m, Math.Max(-3m, (ratio - 1m) * 10m));
             }

             baseline += Math.Min(6m, topScore / 25m);
             return Math.Max(0m, Math.Round(baseline, 2));
         }

         #endregion
     }
}

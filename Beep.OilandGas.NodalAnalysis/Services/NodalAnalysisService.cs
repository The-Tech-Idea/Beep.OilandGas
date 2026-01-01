using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.NodalAnalysis.Calculations;
using Beep.OilandGas.Models.NodalAnalysis;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.DTOs;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Repositories;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using Microsoft.Extensions.Logging;
using Beep.OilandGas.Models.Data.NodalAnalysis;

namespace Beep.OilandGas.NodalAnalysis.Services
{
    /// <summary>
    /// Service for nodal analysis operations.
    /// Uses PPDMGenericRepository for data persistence following LifeCycle patterns.
    /// </summary>
    public class NodalAnalysisService : INodalAnalysisService
    {
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

        public async Task<NodalAnalysisResultDto> PerformNodalAnalysisAsync(string wellUWI, NodalAnalysisParametersDto analysisParameters)
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

            var result = new NodalAnalysisResultDto
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

        public async Task<OptimizationResultDto> OptimizeSystemAsync(string wellUWI, OptimizationGoalsDto optimizationGoals)
        {
            if (string.IsNullOrWhiteSpace(wellUWI))
                throw new ArgumentException("Well UWI cannot be null or empty", nameof(wellUWI));
            if (optimizationGoals == null)
                throw new ArgumentNullException(nameof(optimizationGoals));

            _logger?.LogInformation("Optimizing system for well {WellUWI} with optimization type {OptimizationType}",
                wellUWI, optimizationGoals.OptimizationType);

            // TODO: Implement optimization logic based on goals
            var result = new OptimizationResultDto
            {
                OptimizationId = _defaults.FormatIdForTable("OPTIMIZATION", Guid.NewGuid().ToString()),
                WellUWI = wellUWI,
                OptimizationDate = DateTime.UtcNow,
                RecommendedOperatingPoint = new OperatingPoint(),
                ImprovementPercentage = 0,
                Recommendations = new List<string> { "Optimization logic to be implemented" }
            };

            _logger?.LogInformation("System optimization completed for well {WellUWI}", wellUWI);

            await Task.CompletedTask;
            return result;
        }

        public async Task SaveAnalysisResultAsync(NodalAnalysisResultDto result, string userId)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));
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

            _logger?.LogInformation("Successfully saved nodal analysis result {AnalysisId}", result.AnalysisId);
        }

        public async Task<List<NodalAnalysisResultDto>> GetAnalysisHistoryAsync(string wellUWI)
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
            
            var results = entities.Cast<NODAL_ANALYSIS_RESULT>().Select(entity => new NodalAnalysisResultDto
            {
                AnalysisId = entity.ANALYSIS_ID ?? string.Empty,
                WellUWI = entity.WELL_UWI ?? wellUWI,
                AnalysisDate = entity.ANALYSIS_DATE ?? DateTime.UtcNow,
                OptimalFlowRate = entity.OPERATING_FLOW_RATE ?? 0,
                OptimalBottomholePressure = entity.OPERATING_PRESSURE ?? 0,
                Status = entity.STATUS ?? "Completed"
            }).ToList();

            _logger?.LogInformation("Retrieved {Count} nodal analysis results for well {WellUWI}", results.Count, wellUWI);
            return results;
        }
    }
}

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ChokeAnalysis;
using Beep.OilandGas.Models.Data.CompressorAnalysis;
using Beep.OilandGas.Models.Data.PipelineAnalysis;
using Beep.OilandGas.Models.Data.WellTestAnalysis;
using Beep.OilandGas.Models.Data.GasLift;
using Beep.OilandGas.Models.Data.PumpPerformance;
using Beep.OilandGas.Models.Data.ChokeAnalysis;
using Beep.OilandGas.Models.Data.CompressorAnalysis;
using Beep.OilandGas.Models.Data.PipelineAnalysis;
using Beep.OilandGas.Models.Data.WellTestAnalysis;
using Beep.OilandGas.Models.Data.GasLift;
using Beep.OilandGas.Models.Data.PumpPerformance;
using Beep.OilandGas.Models.Data.ProspectIdentification;

namespace Beep.OilandGas.Client.App.Services.Analysis
{
    /// <summary>
    /// Service interface for Analysis operations
    /// Includes Choke, Compressor, Pipeline, WellTest, GasLift, PumpPerformance, Prospect
    /// </summary>
    public interface IAnalysisService
    {
        #region Choke

        Task<ChokeFlowResult> CalculateDownholeChokeFlowAsync(GasChokeProperties request, CancellationToken cancellationToken = default);
        Task<ChokeFlowResult> CalculateUpholeChokeFlowAsync(GasChokeProperties request, CancellationToken cancellationToken = default);
        Task<ChokeFlowResult> CalculateDownstreamPressureAsync(GasChokeProperties request, CancellationToken cancellationToken = default);
        Task<ChokeFlowResult> CalculateRequiredChokeSizeAsync(GasChokeProperties request, CancellationToken cancellationToken = default);
        Task<CHOKE_FLOW_RESULT> AnalyzeChokePerformanceAsync(CHOKE_PROPERTIES request, CancellationToken cancellationToken = default);

        #endregion

        #region Compressor

        Task<CompressorPowerResult> AnalyzeCompressorAsync(CompressorOperatingConditions request, CancellationToken cancellationToken = default);
        Task<COMPRESSOR_POWER_RESULT> DesignCentrifugalCompressorAsync(CENTRIFUGAL_COMPRESSOR_PROPERTIES request, CancellationToken cancellationToken = default);
        Task<COMPRESSOR_POWER_RESULT> DesignReciprocatingCompressorAsync(RECIPROCATING_COMPRESSOR_PROPERTIES request, CancellationToken cancellationToken = default);
        Task<COMPRESSOR_OPERATING_CONDITIONS> GetCompressorPerformanceAsync(string compressorId, CancellationToken cancellationToken = default);
        Task<CompressorPowerResult> CalculateCompressorPowerAsync(CompressorOperatingConditions request, CancellationToken cancellationToken = default);

        #endregion

        #region Pipeline

        Task<PipelineFlowAnalysisResult> AnalyzePipelineAsync(PipelineProperties request, CancellationToken cancellationToken = default);
        Task<PIPELINE_FLOW_ANALYSIS_RESULT> CalculatePressureDropAsync(GAS_PIPELINE_FLOW_PROPERTIES request, CancellationToken cancellationToken = default);
        Task<PIPELINE_CAPACITY_RESULT> GetFlowCapacityAsync(string pipelineId, CancellationToken cancellationToken = default);
        Task<PIPELINE_DESIGN> DesignPipelineAsync(PIPELINE_PROPERTIES request, CancellationToken cancellationToken = default);
        Task<PIPELINE_RISK_ASSESSMENT> GetPipelineIntegrityAsync(string pipelineId, CancellationToken cancellationToken = default);

        #endregion

        #region WellTest

        Task<WellTestAnalysisResult> AnalyzeBuildUpAsync(WellTestData request, CancellationToken cancellationToken = default);
        Task<WellTestAnalysisResult> AnalyzeDrawdownAsync(WellTestData request, CancellationToken cancellationToken = default);
        Task<WELL_TEST_ANALYSIS_RESULT> GetDerivativeAnalysisAsync(WELL_TEST_DATA request, CancellationToken cancellationToken = default);
        Task<WELL_TEST_ANALYSIS_RESULT> InterpretWellTestAsync(WELL_TEST_DATA request, CancellationToken cancellationToken = default);
        Task<List<WELL_TEST_DATA>> GetWellTestHistoryAsync(string wellId, CancellationToken cancellationToken = default);

        #endregion

        #region GasLift

        Task<GasLiftValveDesignResult> DesignGasLiftAsync(GasLiftWellProperties request, CancellationToken cancellationToken = default);
        Task<GasLiftPotentialResult> OptimizeInjectionAsync(GasLiftWellProperties request, CancellationToken cancellationToken = default);
        Task<GAS_LIFT_VALVE_DESIGN_RESULT> GetValveSpacingAsync(GAS_LIFT_WELL_PROPERTIES request, CancellationToken cancellationToken = default);
        Task<GAS_LIFT_PERFORMANCE> GetGasLiftPerformanceAsync(string wellId, CancellationToken cancellationToken = default);
        Task<GAS_LIFT_DESIGN> TroubleshootGasLiftAsync(GAS_LIFT_WELL_PROPERTIES request, CancellationToken cancellationToken = default);

        #endregion

        #region PumpPerformance

        Task<ESPDesignResult> AnalyzePumpPerformanceAsync(ESPDesignProperties request, CancellationToken cancellationToken = default);
        Task<ESP_DESIGN_RESULT> GetSystemCurveAsync(ESP_DESIGN_PROPERTIES request, CancellationToken cancellationToken = default);
        Task<ESP_DESIGN_RESULT> OptimizePumpAsync(ESP_DESIGN_PROPERTIES request, CancellationToken cancellationToken = default);
        Task<ESP_DESIGN_RESULT> SelectPumpAsync(ESP_DESIGN_PROPERTIES request, CancellationToken cancellationToken = default);
        Task<ESP_PUMP_POINT> GetPumpEfficiencyAsync(string pumpId, CancellationToken cancellationToken = default);

        #endregion

        #region Prospect

        Task<PROSPECT> IdentifyProspectAsync(PROSPECT request, CancellationToken cancellationToken = default);
        Task<PROSPECT_RISK_ASSESSMENT> EvaluateRiskAsync(PROSPECT request, CancellationToken cancellationToken = default);
        Task<PROSPECT_VOLUME_ESTIMATE> GetVolumetricsAsync(string prospectId, CancellationToken cancellationToken = default);
        Task<PROSPECT_RANKING> RankProspectsAsync(PROSPECT_PORTFOLIO request, CancellationToken cancellationToken = default);
        Task<List<PROSPECT>> GetProspectPortfolioAsync(string basinId, CancellationToken cancellationToken = default);

        #endregion
    }
}

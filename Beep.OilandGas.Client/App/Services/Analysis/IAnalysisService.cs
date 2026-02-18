using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ChokeAnalysis;
using Beep.OilandGas.Models.Data.CompressorAnalysis;
using Beep.OilandGas.Models.Data.PipelineAnalysis;
using Beep.OilandGas.Models.Data.WellTestAnalysis;
using Beep.OilandGas.Models.Data.GasLift;
using Beep.OilandGas.Models.Data.PumpPerformance;
using Beep.OilandGas.Models.Data.ProspectIdentification;
using Beep.OilandGas.Models.Data.Calculations;

namespace Beep.OilandGas.Client.App.Services.Analysis
{
    /// <summary>
    /// Service interface for Analysis operations
    /// Includes Choke, Compressor, Pipeline, WellTest, GasLift, PumpPerformance, Prospect
    /// </summary>
    public interface IAnalysisService
    {
        #region Choke

        Task<CHOKE_FLOW_RESULT> CalculateDownholeChokeFlowAsync(GAS_CHOKE_PROPERTIES request, CancellationToken cancellationToken = default);
        Task<CHOKE_FLOW_RESULT> CalculateUpholeChokeFlowAsync(GAS_CHOKE_PROPERTIES request, CancellationToken cancellationToken = default);
        Task<CHOKE_FLOW_RESULT> CalculateDownstreamPressureAsync(GAS_CHOKE_PROPERTIES request, CancellationToken cancellationToken = default);
        Task<CHOKE_FLOW_RESULT> CalculateRequiredChokeSizeAsync(GAS_CHOKE_PROPERTIES request, CancellationToken cancellationToken = default);
        Task<CHOKE_FLOW_RESULT> AnalyzeChokePerformanceAsync(CHOKE_PROPERTIES request, CancellationToken cancellationToken = default);

        #endregion

        #region Compressor

        Task<COMPRESSOR_POWER_RESULT> AnalyzeCompressorAsync(COMPRESSOR_OPERATING_CONDITIONS request, CancellationToken cancellationToken = default);
        Task<COMPRESSOR_POWER_RESULT> DesignCentrifugalCompressorAsync(CENTRIFUGAL_COMPRESSOR_PROPERTIES request, CancellationToken cancellationToken = default);
        Task<COMPRESSOR_POWER_RESULT> DesignReciprocatingCompressorAsync(RECIPROCATING_COMPRESSOR_PROPERTIES request, CancellationToken cancellationToken = default);
        Task<COMPRESSOR_OPERATING_CONDITIONS> GetCompressorPerformanceAsync(string compressorId, CancellationToken cancellationToken = default);
        Task<COMPRESSOR_POWER_RESULT> CalculateCompressorPowerAsync(COMPRESSOR_OPERATING_CONDITIONS request, CancellationToken cancellationToken = default);

        #endregion

        #region Pipeline

        Task<PIPELINE_FLOW_ANALYSIS_RESULT> AnalyzePipelineAsync(Beep.OilandGas.Models.Data.Calculations.AnalyzePipelineFlowRequest request, CancellationToken cancellationToken = default);
        Task<PIPELINE_FLOW_ANALYSIS_RESULT> CalculatePressureDropAsync(GAS_PIPELINE_FLOW_PROPERTIES request, CancellationToken cancellationToken = default);
        Task<PIPELINE_CAPACITY_RESULT> GetFlowCapacityAsync(string pipelineId, CancellationToken cancellationToken = default);
        Task<PIPELINE_DESIGN> DesignPipelineAsync(PIPELINE_PROPERTIES request, CancellationToken cancellationToken = default);
        Task<PIPELINE_RISK_ASSESSMENT> GetPipelineIntegrityAsync(string pipelineId, CancellationToken cancellationToken = default);

        #endregion

        #region WellTest

        Task<Beep.OilandGas.Models.Data.WellTestAnalysis.WELL_TEST_ANALYSIS_RESULT> AnalyzeBuildUpAsync(WELL_TEST_DATA request, CancellationToken cancellationToken = default);
        Task<Beep.OilandGas.Models.Data.WellTestAnalysis.WELL_TEST_ANALYSIS_RESULT> AnalyzeDrawdownAsync(WELL_TEST_DATA request, CancellationToken cancellationToken = default);
        Task<WELL_TEST_ANALYSIS_RESULT> GetDerivativeAnalysisAsync(WELL_TEST_DATA request, CancellationToken cancellationToken = default);
        Task<WELL_TEST_ANALYSIS_RESULT> InterpretWellTestAsync(WELL_TEST_DATA request, CancellationToken cancellationToken = default);
        Task<List<WELL_TEST_DATA>> GetWellTestHistoryAsync(string wellId, CancellationToken cancellationToken = default);

        #endregion

        #region GasLift

        Task<GAS_LIFT_VALVE_DESIGN_RESULT> DesignGasLiftAsync(GAS_LIFT_WELL_PROPERTIES request, CancellationToken cancellationToken = default);
        Task<GAS_LIFT_POTENTIAL_RESULT> OptimizeInjectionAsync(GAS_LIFT_WELL_PROPERTIES request, CancellationToken cancellationToken = default);
        Task<GAS_LIFT_VALVE_DESIGN_RESULT> GetValveSpacingAsync(GAS_LIFT_WELL_PROPERTIES request, CancellationToken cancellationToken = default);
        Task<GAS_LIFT_PERFORMANCE> GetGasLiftPerformanceAsync(string wellId, CancellationToken cancellationToken = default);
        Task<GAS_LIFT_DESIGN> TroubleshootGasLiftAsync(GAS_LIFT_WELL_PROPERTIES request, CancellationToken cancellationToken = default);

        #endregion

        #region PumpPerformance

        Task<ESP_DESIGN_RESULT> AnalyzePumpPerformanceAsync(ESP_DESIGN_PROPERTIES request, CancellationToken cancellationToken = default);
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


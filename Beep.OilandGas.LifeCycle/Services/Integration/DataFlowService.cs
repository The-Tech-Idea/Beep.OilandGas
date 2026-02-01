using Beep.OilandGas.LifeCycle.Services.Calculations;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Calculations;
using Beep.OilandGas.Models.Data.HydraulicPumps;
using Beep.OilandGas.Models.Data.PipelineAnalysis;
using Beep.OilandGas.Models.Data.PlungerLift;
using Beep.OilandGas.Models.Data.Pumps;
using Beep.OilandGas.Models.Data.SuckerRodPumping;
using Beep.OilandGas.Models.Data.WellTestAnalysis;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Beep.OilandGas.LifeCycle.Services.Integration
{
    /// <summary>
    /// Service that provides a simplified API layer over PPDMCalculationService for workflows.
    /// This service wraps PPDMCalculationService to provide easier-to-use methods for common analysis operations.
    /// </summary>
    public class DataFlowService
    {
        private readonly PPDMCalculationService _calculationService;
        private readonly ILogger<DataFlowService>? _logger;

        public DataFlowService(
            PPDMCalculationService calculationService,
            ILogger<DataFlowService>? logger = null)
        {
            _calculationService = calculationService ?? throw new ArgumentNullException(nameof(calculationService));
            _logger = logger;
        }

        /// <summary>
        /// Runs nodal analysis for a well using PPDM39 data.
        /// </summary>
        /// <param name="wellId">The well ID.</param>
        /// <param name="userId">The user ID performing the analysis.</param>
        /// <param name="additionalParameters">Optional additional parameters for the analysis.</param>
        /// <returns>The nodal analysis results including operating point and curves.</returns>
        public async Task<NodalAnalysisResult> RunNodalAnalysisAsync(
            string wellId,
            string userId,
            NodalAnalysisOptions? additionalParameters = null)
        {
            if (string.IsNullOrEmpty(wellId))
                throw new ArgumentException("Well ID cannot be null or empty.", nameof(wellId));

            _logger?.LogInformation("Running nodal analysis for well: {WellId}", wellId);

            var request = new NodalAnalysisRequest
            {
                WellUWI = wellId,
                UserId = userId,
                AdditionalParameters = additionalParameters
            };

            // Use PPDMCalculationService to perform the analysis
            var result = await _calculationService.PerformNodalAnalysisAsync(request);

            _logger?.LogInformation("Nodal analysis completed for well: {WellId}, Operating Flow Rate: {FlowRate} BPD", 
                wellId, result.OperatingPoint?.Rate);

            return result;
        }

        /// <summary>
        /// Runs DCA (Decline Curve Analysis) for a well, pool, or field.
        /// </summary>
        /// <param name="wellId">Optional well ID.</param>
        /// <param name="poolId">Optional pool ID.</param>
        /// <param name="fieldId">Optional field ID.</param>
        /// <param name="userId">The user ID performing the analysis.</param>
        /// <param name="calculationType">The DCA calculation type (e.g., "Hyperbolic", "Exponential", "Harmonic").</param>
        /// <param name="additionalParameters">Optional additional parameters for the analysis.</param>
        /// <returns>The DCA analysis results.</returns>
        public async Task<DCAResult> RunDCAAsync(
            string? wellId = null,
            string? poolId = null,
            string? fieldId = null,
            string userId = "system",
            string calculationType = "Hyperbolic",
            DcaAnalysisOptions? additionalParameters = null)
        {
            if (string.IsNullOrEmpty(wellId) && string.IsNullOrEmpty(poolId) && string.IsNullOrEmpty(fieldId))
                throw new ArgumentException("At least one of WellId, PoolId, or FieldId must be provided.");

            _logger?.LogInformation("Running DCA analysis - WellId: {WellId}, PoolId: {PoolId}, FieldId: {FieldId}, Type: {Type}", 
                wellId, poolId, fieldId, calculationType);

            var request = new DCARequest
            {
                WellId = wellId,
                PoolId = poolId,
                FieldId = fieldId,
                UserId = userId,
                CalculationType = calculationType,
                AdditionalParameters = additionalParameters
            };

            // Use PPDMCalculationService to perform the analysis
            var result = await _calculationService.PerformDCAAnalysisAsync(request);

            _logger?.LogInformation("DCA analysis completed - CalculationId: {CalculationId}, RÂ²: {RSquared}", 
                result.CalculationId, result.R2);

            return result;
        }

        /// <summary>
        /// Runs well test analysis for a well.
        /// </summary>
        /// <param name="wellId">The well ID.</param>
        /// <param name="testId">Optional test ID.</param>
        /// <param name="userId">The user ID performing the analysis.</param>
        /// <param name="additionalParameters">Optional additional parameters for the analysis.</param>
        /// <returns>The well test analysis results.</returns>
        public async Task<WELL_TEST_ANALYSIS_RESULT> RunWellTestAnalysisAsync(
            string wellId,
            string? testId = null,
            string userId = "system",
            WellTestAnalysisOptions? additionalParameters = null)
        {
            if (string.IsNullOrEmpty(wellId))
                throw new ArgumentException("Well ID cannot be null or empty.", nameof(wellId));

            _logger?.LogInformation("Running well test analysis for well: {WellId}, TestId: {TestId}", wellId, testId);

            var request = new WellTestAnalysisCalculationRequest
            {
                WellId = wellId,
                TestId = testId,
                UserId = userId,
                AdditionalParameters = additionalParameters
            };

            // Use PPDMCalculationService to perform the analysis
            var result = await _calculationService.PerformWellTestAnalysisAsync(request);

            _logger?.LogInformation("Well test analysis completed for well: {WellId}", wellId);

            return result;
        }

        /// <summary>
        /// Runs flash calculation for a well or facility.
        /// </summary>
        /// <param name="wellId">Optional well ID.</param>
        /// <param name="facilityId">Optional facility ID.</param>
        /// <param name="userId">The user ID performing the calculation.</param>
        /// <param name="additionalParameters">Optional additional parameters for the calculation.</param>
        /// <returns>The flash calculation results.</returns>
        public async Task<FlashCalculationResult> RunFlashCalculationAsync(
            string? wellId = null,
            string? facilityId = null,
            string userId = "system",
            FlashCalculationOptions? additionalParameters = null)
        {
            if (string.IsNullOrEmpty(wellId) && string.IsNullOrEmpty(facilityId))
                throw new ArgumentException("At least one of WellId or FacilityId must be provided.");

            _logger?.LogInformation("Running flash calculation - WellId: {WellId}, FacilityId: {FacilityId}", wellId, facilityId);

            var request = new FlashCalculationRequest
            {
                WellId = wellId,
                FacilityId = facilityId,
                UserId = userId,
                AdditionalParameters = additionalParameters
            };

            // Use PPDMCalculationService to perform the calculation
            var result = await _calculationService.PerformFlashCalculationAsync(request);

            _logger?.LogInformation("Flash calculation completed - CalculationId: {CalculationId}", result.CalculationId);

            return result;
        }

        /// <summary>
        /// Runs choke analysis for a well.
        /// </summary>
        /// <param name="wellId">The well ID.</param>
        /// <param name="userId">The user ID performing the analysis.</param>
        /// <param name="equipmentId">Optional equipment ID for specific choke equipment.</param>
        /// <param name="analysisType">Analysis type: DOWNHOLE, UPHOLE, SIZING, or PRESSURE.</param>
        /// <param name="additionalParameters">Optional additional parameters for the analysis.</param>
        /// <returns>The choke analysis results.</returns>
        public async Task<ChokeAnalysisResult> RunChokeAnalysisAsync(
            string wellId,
            string userId = "system",
            string? equipmentId = null,
            string analysisType = "DOWNHOLE",
            ChokeAnalysisOptions? additionalParameters = null)
        {
            if (string.IsNullOrEmpty(wellId))
                throw new ArgumentException("Well ID cannot be null or empty.", nameof(wellId));

            _logger?.LogInformation("Running choke analysis for well: {WellId}, AnalysisType: {AnalysisType}", wellId, analysisType);

            var request = new ChokeAnalysisRequest
            {
                WellId = wellId,
                EquipmentId = equipmentId,
                AnalysisType = analysisType,
                UserId = userId,
                AdditionalParameters = additionalParameters
            };

            var result = (ChokeAnalysisResult)await _calculationService.PerformChokeAnalysisAsync(request);

            _logger?.LogInformation("Choke analysis completed for well: {WellId}, Flow Rate: {FlowRate} Mscf/day", 
                wellId, result.FlowRate);

            return result;
        }

        /// <summary>
        /// Runs gas lift analysis for a well.
        /// </summary>
        /// <param name="wellId">The well ID.</param>
        /// <param name="userId">The user ID performing the analysis.</param>
        /// <param name="analysisType">Analysis type: POTENTIAL, VALVE_DESIGN, or VALVE_SPACING.</param>
        /// <param name="additionalParameters">Optional additional parameters for the analysis.</param>
        /// <returns>The gas lift analysis results.</returns>
        public async Task<GasLiftAnalysisResult> RunGasLiftAnalysisAsync(
            string wellId,
            string userId = "system",
            string analysisType = "POTENTIAL",
            GasLiftAnalysisOptions? additionalParameters = null)
        {
            if (string.IsNullOrEmpty(wellId))
                throw new ArgumentException("Well ID cannot be null or empty.", nameof(wellId));

            _logger?.LogInformation("Running gas lift analysis for well: {WellId}, AnalysisType: {AnalysisType}", wellId, analysisType);

            var request = new GasLiftAnalysisRequest
            {
                WellId = wellId,
                AnalysisType = analysisType,
                UserId = userId,
                AdditionalParameters = additionalParameters
            };

            var result = (GasLiftAnalysisResult)await _calculationService.PerformGasLiftAnalysisAsync(request);

            _logger?.LogInformation("Gas lift analysis completed for well: {WellId}, Optimal Injection Rate: {Rate} Mscf/day", 
                wellId, result.OptimalGasInjectionRate);

            return result;
        }

        /// <summary>
        /// Runs pump performance analysis for a well or facility.
        /// </summary>
        /// <param name="wellId">Optional well ID (for well pumps like ESP).</param>
        /// <param name="facilityId">Optional facility ID (for facility pumps).</param>
        /// <param name="equipmentId">Optional equipment ID.</param>
        /// <param name="userId">The user ID performing the analysis.</param>
        /// <param name="pumpType">Pump type: ESP, CENTRIFUGAL, POSITIVE_DISPLACEMENT, or JET.</param>
        /// <param name="analysisType">Analysis type: PERFORMANCE, DESIGN, EFFICIENCY, NPSH, or SYSTEM_CURVE.</param>
        /// <param name="additionalParameters">Optional additional parameters for the analysis.</param>
        /// <returns>The pump analysis results.</returns>
        public async Task<PumpAnalysisResult> RunPumpAnalysisAsync(
            string? wellId = null,
            string? facilityId = null,
            string? equipmentId = null,
            string userId = "system",
            string pumpType = "ESP",
            string analysisType = "PERFORMANCE",
            PumpAnalysisOptions? additionalParameters = null)
        {
            if (string.IsNullOrEmpty(wellId) && string.IsNullOrEmpty(facilityId))
                throw new ArgumentException("At least one of WellId or FacilityId must be provided.");

            _logger?.LogInformation("Running pump analysis - WellId: {WellId}, FacilityId: {FacilityId}, PumpType: {PumpType}", 
                wellId, facilityId, pumpType);

            var request = new PumpAnalysisRequest
            {
                WellId = wellId,
                FacilityId = facilityId,
                EquipmentId = equipmentId,
                PumpType = pumpType,
                AnalysisType = analysisType,
                UserId = userId,
                AdditionalParameters = additionalParameters
            };

            var result = (PumpAnalysisResult)await _calculationService.PerformPumpAnalysisAsync(request);

            _logger?.LogInformation("Pump analysis completed - CalculationId: {CalculationId}, Efficiency: {Efficiency}", 
                result.CalculationId, result.Efficiency);

            return result;
        }

        /// <summary>
        /// Runs sucker rod pumping analysis for a well.
        /// </summary>
        /// <param name="wellId">The well ID.</param>
        /// <param name="userId">The user ID performing the analysis.</param>
        /// <param name="equipmentId">Optional equipment ID for specific sucker rod system.</param>
        /// <param name="analysisType">Analysis type: LOAD, POWER, PUMP_CARD, or OPTIMIZATION.</param>
        /// <param name="additionalParameters">Optional additional parameters for the analysis.</param>
        /// <returns>The sucker rod analysis results.</returns>
        public async Task<SuckerRodAnalysisResult> RunSuckerRodAnalysisAsync(
            string wellId,
            string userId = "system",
            string? equipmentId = null,
            string analysisType = "LOAD",
            SuckerRodAnalysisOptions? additionalParameters = null)
        {
            if (string.IsNullOrEmpty(wellId))
                throw new ArgumentException("Well ID cannot be null or empty.", nameof(wellId));

            _logger?.LogInformation("Running sucker rod analysis for well: {WellId}, AnalysisType: {AnalysisType}", wellId, analysisType);

            var request = new SuckerRodAnalysisRequest
            {
                WellId = wellId,
                EquipmentId = equipmentId,
                AnalysisType = analysisType,
                UserId = userId,
                AdditionalParameters = additionalParameters
            };

            var result = (SuckerRodAnalysisResult)await _calculationService.PerformSuckerRodAnalysisAsync(request);

            _logger?.LogInformation("Sucker rod analysis completed for well: {WellId}, Production Rate: {Rate} bbl/day", 
                wellId, result.ProductionRate);

            return result;
        }

        /// <summary>
        /// Runs compressor analysis for a facility.
        /// </summary>
        /// <param name="facilityId">The facility ID.</param>
        /// <param name="userId">The user ID performing the analysis.</param>
        /// <param name="equipmentId">Optional equipment ID for specific compressor.</param>
        /// <param name="compressorType">Compressor type: CENTRIFUGAL or RECIPROCATING.</param>
        /// <param name="analysisType">Analysis type: POWER, PRESSURE, or EFFICIENCY.</param>
        /// <param name="additionalParameters">Optional additional parameters for the analysis.</param>
        /// <returns>The compressor analysis results.</returns>
        public async Task<CompressorAnalysisResult> RunCompressorAnalysisAsync(
            string facilityId,
            string userId = "system",
            string? equipmentId = null,
            string compressorType = "CENTRIFUGAL",
            string analysisType = "POWER",
            CompressorAnalysisOptions? additionalParameters = null)
        {
            if (string.IsNullOrEmpty(facilityId))
                throw new ArgumentException("Facility ID cannot be null or empty.", nameof(facilityId));

            _logger?.LogInformation("Running compressor analysis for facility: {FacilityId}, CompressorType: {CompressorType}", 
                facilityId, compressorType);

            var request = new CompressorAnalysisRequest
            {
                FacilityId = facilityId,
                EquipmentId = equipmentId,
                CompressorType = compressorType,
                AnalysisType = analysisType,
                UserId = userId,
                AdditionalParameters = additionalParameters
            };

            var result = (CompressorAnalysisResult)await _calculationService.PerformCompressorAnalysisAsync(request);

            _logger?.LogInformation("Compressor analysis completed for facility: {FacilityId}, Power Required: {Power} HP", 
                facilityId, result.PowerRequired);

            return result;
        }

        /// <summary>
        /// Runs pipeline analysis for a pipeline.
        /// </summary>
        /// <param name="pipelineId">The pipeline ID.</param>
        /// <param name="userId">The user ID performing the analysis.</param>
        /// <param name="pipelineType">Pipeline type: GAS or LIQUID.</param>
        /// <param name="analysisType">Analysis type: CAPACITY, FLOW_RATE, or PRESSURE_DROP.</param>
        /// <param name="additionalParameters">Optional additional parameters for the analysis.</param>
        /// <returns>The pipeline analysis results.</returns>
        public async Task<PIPELINE_ANALYSIS_RESULT> RunPipelineAnalysisAsync(
            string pipelineId,
            string userId = "system",
            string pipelineType = "GAS",
            string analysisType = "CAPACITY",
            PipelineAnalysisOptions? additionalParameters = null)
        {
            if (string.IsNullOrEmpty(pipelineId))
                throw new ArgumentException("Pipeline ID cannot be null or empty.", nameof(pipelineId));

            _logger?.LogInformation("Running pipeline analysis for pipeline: {PipelineId}, PipelineType: {PipelineType}", 
                pipelineId, pipelineType);

            var request = new PipelineAnalysisRequest
            {
                PipelineId = pipelineId,
                PipelineType = pipelineType,
                AnalysisType = analysisType,
                UserId = userId,
                AdditionalParameters = additionalParameters
            };

            var result = (PIPELINE_ANALYSIS_RESULT)await _calculationService.PerformPipelineAnalysisAsync(request);

            _logger?.LogInformation("Pipeline analysis completed for pipeline: {PipelineId}, Flow Rate: {FlowRate} Mscf/day", 
                pipelineId, result.FLOW_RATE);

            return result;
        }

        /// <summary>
        /// Runs plunger lift analysis for a well.
        /// </summary>
        /// <param name="wellId">The well ID.</param>
        /// <param name="userId">The user ID performing the analysis.</param>
        /// <param name="equipmentId">Optional equipment ID for specific plunger lift system.</param>
        /// <param name="analysisType">Analysis type: PERFORMANCE, OPTIMIZATION, or CYCLE_TIME.</param>
        /// <param name="additionalParameters">Optional additional parameters for the analysis.</param>
        /// <returns>The plunger lift analysis results.</returns>
        public async Task<PlungerLiftAnalysisResult> RunPlungerLiftAnalysisAsync(
            string wellId,
            string userId = "system",
            string? equipmentId = null,
            string analysisType = "PERFORMANCE",
            PlungerLiftAnalysisOptions? additionalParameters = null)
        {
            if (string.IsNullOrEmpty(wellId))
                throw new ArgumentException("Well ID cannot be null or empty.", nameof(wellId));

            _logger?.LogInformation("Running plunger lift analysis for well: {WellId}, AnalysisType: {AnalysisType}", wellId, analysisType);

            var request = new PlungerLiftAnalysisRequest
            {
                WellId = wellId,
                EquipmentId = equipmentId,
                AnalysisType = analysisType,
                UserId = userId,
                AdditionalParameters = additionalParameters
            };

            var result = (PlungerLiftAnalysisResult)await _calculationService.PerformPlungerLiftAnalysisAsync(request);

            _logger?.LogInformation("Plunger lift analysis completed for well: {WellId}, Production Rate: {Rate} bbl/day", 
                wellId, result.ProductionRate);

            return result;
        }

        /// <summary>
        /// Runs hydraulic pump analysis for a well.
        /// </summary>
        /// <param name="wellId">The well ID.</param>
        /// <param name="userId">The user ID performing the analysis.</param>
        /// <param name="equipmentId">Optional equipment ID for specific hydraulic pump.</param>
        /// <param name="analysisType">Analysis type: PERFORMANCE, DESIGN, or EFFICIENCY.</param>
        /// <param name="additionalParameters">Optional additional parameters for the analysis.</param>
        /// <returns>The hydraulic pump analysis results.</returns>
        public async Task<HydraulicPumpAnalysisResult> RunHydraulicPumpAnalysisAsync(
            string wellId,
            string userId = "system",
            string? equipmentId = null,
            string analysisType = "PERFORMANCE",
            HydraulicPumpAnalysisOptions? additionalParameters = null)
        {
            if (string.IsNullOrEmpty(wellId))
                throw new ArgumentException("Well ID cannot be null or empty.", nameof(wellId));

            _logger?.LogInformation("Running hydraulic pump analysis for well: {WellId}, AnalysisType: {AnalysisType}", wellId, analysisType);

            var request = new HydraulicPumpAnalysisRequest
            {
                WellId = wellId,
                EquipmentId = equipmentId,
                AnalysisType = analysisType,
                UserId = userId,
                AdditionalParameters = additionalParameters
            };

            var result = (HydraulicPumpAnalysisResult)await _calculationService.PerformHydraulicPumpAnalysisAsync(request);

            _logger?.LogInformation("Hydraulic pump analysis completed for well: {WellId}, Production Rate: {Rate} bbl/day", 
                wellId, result.ProductionRate);

            return result;
        }

        /// <summary>
        /// Note: Results are automatically stored by PPDMCalculationService.
        /// This method is kept for backward compatibility but does not need to be called.
        /// </summary>
        [Obsolete("Results are automatically stored by PPDMCalculationService. This method is no longer needed.")]
        public void StoreNodalAnalysisResult(NodalAnalysisResult result)
        {
            _logger?.LogWarning("StoreNodalAnalysisResult is obsolete. Results are automatically stored by PPDMCalculationService.");
            // Results are already stored by PPDMCalculationService, so this method does nothing
        }
    }
}

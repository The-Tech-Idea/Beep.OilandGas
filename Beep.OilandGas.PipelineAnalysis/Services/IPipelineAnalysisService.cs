using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Beep.OilandGas.PipelineAnalysis.Services
{
    /// <summary>
    /// Comprehensive pipeline analysis service interface
    /// Provides pipeline hydraulics, flow regime analysis, pressure drop, erosion, and optimization capabilities
    /// </summary>
    public interface IPipelineAnalysisService
    {
        #region Pipeline Hydraulics

        /// <summary>
        /// Analyzes pipeline flow conditions
        /// </summary>
        Task<PipelineAnalysisResultDto> AnalyzePipelineFlowAsync(string pipelineId, decimal flowRate, decimal inletPressure);

        /// <summary>
        /// Calculates pressure drop across pipeline segment
        /// </summary>
        Task<PressureDropResultDto> CalculatePressureDropAsync(string pipelineId, decimal flowRate);

        /// <summary>
        /// Determines flow regime (Laminar, Turbulent, Transitional)
        /// </summary>
        Task<FlowRegimeAnalysisDto> AnalyzeFlowRegimeAsync(string pipelineId, FlowRegimeRequestDto request);

        /// <summary>
        /// Calculates Beggs-Brill correlation for multiphase flow
        /// </summary>
        Task<MultiphaseFlowResultDto> CalculateBeggsbrillAsync(string pipelineId, MultiphaseFlowRequestDto request);

        /// <summary>
        /// Calculates Hagedorn-Brown correlation for multiphase flow
        /// </summary>
        Task<MultiphaseFlowResultDto> CalculateHagedornBrownAsync(string pipelineId, MultiphaseFlowRequestDto request);

        /// <summary>
        /// Calculates Duns-Ros correlation for vertical flow
        /// </summary>
        Task<MultiphaseFlowResultDto> CalculateDunsRosAsync(string pipelineId, MultiphaseFlowRequestDto request);

        /// <summary>
        /// Performs comprehensive pipeline sizing analysis
        /// </summary>
        Task<PipelineSizingDto> PerformPipelineSizingAsync(string pipelineId, PipelineSizingRequestDto request);

        #endregion

        #region Erosion and Corrosion Analysis

        /// <summary>
        /// Analyzes erosion potential
        /// </summary>
        Task<ErosionAnalysisDto> AnalyzeErosionAsync(string pipelineId, ErosionRequestDto request);

        /// <summary>
        /// Calculates erosion velocity
        /// </summary>
        Task<ErosionVelocityResultDto> CalculateErosionVelocityAsync(string pipelineId, ErosionVelocityRequestDto request);

        /// <summary>
        /// Analyzes corrosion risk factors
        /// </summary>
        Task<CorrosionAnalysisDto> AnalyzeCorrosionRiskAsync(string pipelineId, CorrosionRequestDto request);

        /// <summary>
        /// Estimates corrosion rate
        /// </summary>
        Task<CorrosionRateResultDto> EstimateCorrosionRateAsync(string pipelineId, CorrosionRateRequestDto request);

        /// <summary>
        /// Assesses pipeline material compatibility
        /// </summary>
        Task<MaterialCompatibilityDto> AssessMaterialCompatibilityAsync(string pipelineId, MaterialRequestDto request);

        #endregion

        #region Slug Flow Analysis

        /// <summary>
        /// Analyzes slug flow conditions
        /// </summary>
        Task<SlugFlowAnalysisDto> AnalyzeSlugFlowAsync(string pipelineId, SlugFlowRequestDto request);

        /// <summary>
        /// Predicts slug frequency
        /// </summary>
        Task<SlugFrequencyResultDto> PredictSlugFrequencyAsync(string pipelineId, SlugFrequencyRequestDto request);

        /// <summary>
        /// Analyzes slug formation mechanisms
        /// </summary>
        Task<SlugFormationAnalysisDto> AnalyzeSlugFormationAsync(string pipelineId, SlugFormationRequestDto request);

        /// <summary>
        /// Evaluates slug mitigation strategies
        /// </summary>
        Task<SlugMitigationDto> EvaluateSlugMitigationAsync(string pipelineId, List<string> mitigationStrategies);

        #endregion

        #region Leak Detection and Diagnostics

        /// <summary>
        /// Detects potential leaks
        /// </summary>
        Task<LeakDetectionResultDto> DetectLeaksAsync(string pipelineId, LeakDetectionRequestDto request);

        /// <summary>
        /// Analyzes pressure profile for anomalies
        /// </summary>
        Task<PressureAnomalyDto> AnalyzePressureAnomaliesAsync(string pipelineId, PressureAnomalyRequestDto request);

        /// <summary>
        /// Performs acoustic leak detection
        /// </summary>
        Task<AcousticDetectionResultDto> PerformAcousticLeakDetectionAsync(string pipelineId, AcousticRequestDto request);

        /// <summary>
        /// Diagnoses operational issues
        /// </summary>
        Task<DiagnosisResultDto> DiagnoseOperationalIssuesAsync(string pipelineId, DiagnosisRequestDto request);

        /// <summary>
        /// Calculates gas loss rate
        /// </summary>
        Task<GasLossRateDto> CalculateGasLossRateAsync(string pipelineId, decimal pressureDifference);

        #endregion

        #region Pipeline Integrity

        /// <summary>
        /// Assesses pipeline integrity
        /// </summary>
        Task<IntegrityAssessmentDto> AssessIntegrityAsync(string pipelineId, IntegrityRequestDto request);

        /// <summary>
        /// Performs stress analysis
        /// </summary>
        Task<StressAnalysisDto> PerformStressAnalysisAsync(string pipelineId, StressRequestDto request);

        /// <summary>
        /// Evaluates wall thickness
        /// </summary>
        Task<WallThicknessAnalysisDto> EvaluateWallThicknessAsync(string pipelineId, WallThicknessRequestDto request);

        /// <summary>
        /// Assesses pipeline stability
        /// </summary>
        Task<StabilityAssessmentDto> AssessStabilityAsync(string pipelineId, StabilityRequestDto request);

        #endregion

        #region Optimization

        /// <summary>
        /// Optimizes pipeline flow parameters
        /// </summary>
        Task<OptimizationResultDto> OptimizeFlowParametersAsync(string pipelineId, OptimizationRequestDto request);

        /// <summary>
        /// Recommends optimal pipeline diameter
        /// </summary>
        Task<DiameterRecommendationDto> RecommendOptimalDiameterAsync(string pipelineId, DiameterRequestDto request);

        /// <summary>
        /// Optimizes flow rate for maximum efficiency
        /// </summary>
        Task<FlowRateOptimizationDto> OptimizeFlowRateAsync(string pipelineId, FlowOptimizationRequestDto request);

        /// <summary>
        /// Performs sensitivity analysis on pipeline parameters
        /// </summary>
        Task<SensitivityAnalysisDto> PerformSensitivityAnalysisAsync(string pipelineId, SensitivityRequestDto request);

        /// <summary>
        /// Recommends compressor/pump sizing
        /// </summary>
        Task<PumpCompressorSizingDto> RecommendPumpCompressorSizingAsync(string pipelineId, PumpCompressorRequestDto request);

        #endregion

        #region Hydraulic Simulation

        /// <summary>
        /// Performs steady-state hydraulic simulation
        /// </summary>
        Task<HydraulicSimulationResultDto> SimulateSteadyStateAsync(string pipelineId, SimulationRequestDto request);

        /// <summary>
        /// Performs transient analysis
        /// </summary>
        Task<TransientAnalysisResultDto> PerformTransientAnalysisAsync(string pipelineId, TransientRequestDto request);

        /// <summary>
        /// Analyzes temperature effects on flow
        /// </summary>
        Task<TemperatureEffectDto> AnalyzeTemperatureEffectsAsync(string pipelineId, TemperatureRequestDto request);

        /// <summary>
        /// Evaluates pigging operations
        /// </summary>
        Task<PiggingAnalysisDto> EvaluatePiggingOperationsAsync(string pipelineId, PiggingRequestDto request);

        #endregion

        #region Data Management

        /// <summary>
        /// Saves pipeline analysis results
        /// </summary>
        Task SaveAnalysisResultsAsync(PipelineAnalysisResultDto results, string userId);

        /// <summary>
        /// Retrieves pipeline analysis history
        /// </summary>
        Task<List<PipelineAnalysisResultDto>> GetAnalysisHistoryAsync(string pipelineId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Updates pipeline configuration
        /// </summary>
        Task UpdatePipelineConfigurationAsync(PipelineConfigurationDto config, string userId);

        /// <summary>
        /// Retrieves pipeline configuration
        /// </summary>
        Task<PipelineConfigurationDto?> GetPipelineConfigurationAsync(string pipelineId);

        #endregion

        #region Reporting

        /// <summary>
        /// Generates comprehensive pipeline analysis report
        /// </summary>
        Task<PipelineReportDto> GenerateAnalysisReportAsync(string pipelineId, ReportRequestDto request);

        /// <summary>
        /// Generates pressure profile report
        /// </summary>
        Task<PressureProfileReportDto> GeneratePressureProfileReportAsync(string pipelineId, PressureReportRequestDto request);

        /// <summary>
        /// Generates integrity assessment report
        /// </summary>
        Task<IntegrityReportDto> GenerateIntegrityReportAsync(string pipelineId, IntegrityReportRequestDto request);

        /// <summary>
        /// Exports analysis data
        /// </summary>
        Task<byte[]> ExportAnalysisDataAsync(string pipelineId, DateTime startDate, DateTime endDate, string format = "CSV");

        #endregion
    }

    #region Pipeline Hydraulics DTOs

    /// <summary>
    /// Pipeline analysis result DTO
    /// </summary>
    public class PipelineAnalysisResultDto
    {
        public string AnalysisId { get; set; } = string.Empty;
        public string PipelineId { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }
        public decimal FlowRate { get; set; }
        public decimal InletPressure { get; set; }
        public decimal OutletPressure { get; set; }
        public decimal PressureDrop { get; set; }
        public decimal Velocity { get; set; }
        public string FlowRegime { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public List<string> Recommendations { get; set; } = new();
    }

    /// <summary>
    /// Pressure drop result DTO
    /// </summary>
    public class PressureDropResultDto
    {
        public decimal PressureDrop { get; set; }
        public decimal FrictionFactor { get; set; }
        public decimal ReynoldsNumber { get; set; }
        public string FlowRegime { get; set; } = string.Empty;
        public decimal FrictionalLoss { get; set; }
        public decimal AccelerationLoss { get; set; }
        public decimal ElevationChange { get; set; }
    }

    /// <summary>
    /// Flow regime analysis DTO
    /// </summary>
    public class FlowRegimeAnalysisDto
    {
        public string PipelineId { get; set; } = string.Empty;
        public string FlowRegime { get; set; } = string.Empty;
        public decimal ReynoldsNumber { get; set; }
        public decimal FroudeNumber { get; set; }
        public bool IsMultiphaseFlow { get; set; }
        public string FlowPattern { get; set; } = string.Empty;
        public List<string> StabilityCharacteristics { get; set; } = new();
    }

    /// <summary>
    /// Flow regime request DTO
    /// </summary>
    public class FlowRegimeRequestDto
    {
        public string PipelineId { get; set; } = string.Empty;
        public decimal FlowRate { get; set; }
        public decimal PipelineDiameter { get; set; }
        public decimal FluidViscosity { get; set; }
        public decimal FluidDensity { get; set; }
        public decimal GasVolumetricFraction { get; set; }
    }

    /// <summary>
    /// Multiphase flow result DTO
    /// </summary>
    public class MultiphaseFlowResultDto
    {
        public string PipelineId { get; set; } = string.Empty;
        public decimal PressureDrop { get; set; }
        public decimal HolupFraction { get; set; }
        public decimal FluidVelocity { get; set; }
        public decimal GasVelocity { get; set; }
        public string FlowPattern { get; set; } = string.Empty;
        public decimal DensityMixture { get; set; }
        public string CalculationMethod { get; set; } = string.Empty;
    }

    /// <summary>
    /// Multiphase flow request DTO
    /// </summary>
    public class MultiphaseFlowRequestDto
    {
        public string PipelineId { get; set; } = string.Empty;
        public decimal OilRate { get; set; }
        public decimal GasRate { get; set; }
        public decimal WaterRate { get; set; }
        public decimal PipelineDiameter { get; set; }
        public decimal PipelineLength { get; set; }
        public decimal PipelineInclination { get; set; }
        public decimal Pressure { get; set; }
        public decimal Temperature { get; set; }
    }

    /// <summary>
    /// Pipeline sizing DTO
    /// </summary>
    public class PipelineSizingDto
    {
        public string PipelineId { get; set; } = string.Empty;
        public decimal RecommendedDiameter { get; set; }
        public decimal MinimumDiameter { get; set; }
        public decimal MaximumDiameter { get; set; }
        public decimal OptimalVelocity { get; set; }
        public decimal PressureDropAtOptimalVelocity { get; set; }
        public string RecommendedMaterial { get; set; } = string.Empty;
        public decimal EstimatedCapitalCost { get; set; }
        public decimal EstimatedOperatingCost { get; set; }
    }

    /// <summary>
    /// Pipeline sizing request DTO
    /// </summary>
    public class PipelineSizingRequestDto
    {
        public string PipelineId { get; set; } = string.Empty;
        public decimal DesignFlowRate { get; set; }
        public decimal MaximumPressureDrop { get; set; }
        public decimal DesignPressure { get; set; }
        public decimal DesignTemperature { get; set; }
        public List<string> OptimizationCriteria { get; set; } = new();
    }

    #endregion

    #region Erosion and Corrosion DTOs

    /// <summary>
    /// Erosion analysis DTO
    /// </summary>
    public class ErosionAnalysisDto
    {
        public string PipelineId { get; set; } = string.Empty;
        public decimal ErosionVelocity { get; set; }
        public decimal ActualVelocity { get; set; }
        public string ErosionRisk { get; set; } = string.Empty; // Low, Medium, High, Critical
        public decimal SafetyMargin { get; set; }
        public List<ErosionSpotDto> HighRiskSpots { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
    }

    /// <summary>
    /// Erosion spot DTO
    /// </summary>
    public class ErosionSpotDto
    {
        public string LocationDescription { get; set; } = string.Empty;
        public decimal ErosionRating { get; set; }
        public string MitigationStrategy { get; set; } = string.Empty;
    }

    /// <summary>
    /// Erosion request DTO
    /// </summary>
    public class ErosionRequestDto
    {
        public string PipelineId { get; set; } = string.Empty;
        public decimal FlowRate { get; set; }
        public decimal PipelineDiameter { get; set; }
        public string PipeMaterial { get; set; } = string.Empty;
        public decimal SolidConcentration { get; set; }
        public decimal ParticleSize { get; set; }
    }

    /// <summary>
    /// Erosion velocity result DTO
    /// </summary>
    public class ErosionVelocityResultDto
    {
        public decimal ErosionVelocity { get; set; }
        public decimal ActualVelocity { get; set; }
        public bool IsSafe { get; set; }
        public decimal VelocitySafetyFactor { get; set; }
    }

    /// <summary>
    /// Erosion velocity request DTO
    /// </summary>
    public class ErosionVelocityRequestDto
    {
        public string PipelineId { get; set; } = string.Empty;
        public string PipeMaterial { get; set; } = string.Empty;
        public decimal PipeDiameter { get; set; }
        public string FluidType { get; set; } = string.Empty;
    }

    /// <summary>
    /// Corrosion analysis DTO
    /// </summary>
    public class CorrosionAnalysisDto
    {
        public string PipelineId { get; set; } = string.Empty;
        public decimal CorrosionRisk { get; set; } // 0-100 scale
        public string RiskLevel { get; set; } = string.Empty; // Low, Medium, High, Critical
        public List<CorrosionFactorDto> CorrosionFactors { get; set; } = new();
        public List<string> MitigationStrategies { get; set; } = new();
        public string RecommendedMaterial { get; set; } = string.Empty;
    }

    /// <summary>
    /// Corrosion factor DTO
    /// </summary>
    public class CorrosionFactorDto
    {
        public string FactorName { get; set; } = string.Empty;
        public decimal ContributionPercent { get; set; }
        public string Severity { get; set; } = string.Empty;
    }

    /// <summary>
    /// Corrosion request DTO
    /// </summary>
    public class CorrosionRequestDto
    {
        public string PipelineId { get; set; } = string.Empty;
        public string PipeMaterial { get; set; } = string.Empty;
        public decimal CO2Concentration { get; set; }
        public decimal H2SConcentration { get; set; }
        public decimal Temperature { get; set; }
        public decimal Pressure { get; set; }
        public decimal pH { get; set; }
        public decimal WaterContent { get; set; }
    }

    /// <summary>
    /// Corrosion rate result DTO
    /// </summary>
    public class CorrosionRateResultDto
    {
        public decimal CorrosionRate { get; set; } // mm/year or mils/year
        public string Unit { get; set; } = "mm/year";
        public string CorrosionType { get; set; } = string.Empty;
        public decimal RemainingLifeYears { get; set; }
        public DateTime InspectionDueDate { get; set; }
    }

    /// <summary>
    /// Corrosion rate request DTO
    /// </summary>
    public class CorrosionRateRequestDto
    {
        public string PipelineId { get; set; } = string.Empty;
        public string PipeMaterial { get; set; } = string.Empty;
        public decimal CurrentWallThickness { get; set; }
        public DateTime LastInspectionDate { get; set; }
        public List<CorrosionDataPointDto> HistoricalData { get; set; } = new();
    }

    /// <summary>
    /// Corrosion data point DTO
    /// </summary>
    public class CorrosionDataPointDto
    {
        public DateTime MeasurementDate { get; set; }
        public decimal WallThickness { get; set; }
    }

    /// <summary>
    /// Material compatibility DTO
    /// </summary>
    public class MaterialCompatibilityDto
    {
        public string PipelineId { get; set; } = string.Empty;
        public string CurrentMaterial { get; set; } = string.Empty;
        public bool IsCompatible { get; set; }
        public string CompatibilityScore { get; set; } = string.Empty;
        public List<string> Concerns { get; set; } = new();
        public List<string> AlternativeMaterials { get; set; } = new();
    }

    /// <summary>
    /// Material request DTO
    /// </summary>
    public class MaterialRequestDto
    {
        public string PipelineId { get; set; } = string.Empty;
        public string ProposedMaterial { get; set; } = string.Empty;
        public string FluidType { get; set; } = string.Empty;
        public decimal DesignPressure { get; set; }
        public decimal DesignTemperature { get; set; }
    }

    #endregion

    #region Slug Flow DTOs

    /// <summary>
    /// Slug flow analysis DTO
    /// </summary>
    public class SlugFlowAnalysisDto
    {
        public string PipelineId { get; set; } = string.Empty;
        public bool IsSlugFlowPresent { get; set; }
        public decimal SlugFrequency { get; set; }
        public decimal SlugLength { get; set; }
        public decimal SlugVelocity { get; set; }
        public decimal PressureVariation { get; set; }
        public List<string> MitigationOptions { get; set; } = new();
    }

    /// <summary>
    /// Slug flow request DTO
    /// </summary>
    public class SlugFlowRequestDto
    {
        public string PipelineId { get; set; } = string.Empty;
        public decimal OilRate { get; set; }
        public decimal GasRate { get; set; }
        public decimal WaterRate { get; set; }
        public decimal PipelineDiameter { get; set; }
        public decimal PipelineInclination { get; set; }
        public decimal Temperature { get; set; }
    }

    /// <summary>
    /// Slug frequency result DTO
    /// </summary>
    public class SlugFrequencyResultDto
    {
        public decimal SlugFrequency { get; set; }
        public string FrequencyUnit { get; set; } = "slugs/hour";
        public decimal AverageSlugLength { get; set; }
        public decimal AverageSlugVelocity { get; set; }
        public decimal PressureVariation { get; set; }
    }

    /// <summary>
    /// Slug frequency request DTO
    /// </summary>
    public class SlugFrequencyRequestDto
    {
        public string PipelineId { get; set; } = string.Empty;
        public decimal OilRate { get; set; }
        public decimal GasRate { get; set; }
        public decimal PipelineDiameter { get; set; }
        public decimal PipelineInclination { get; set; }
    }

    /// <summary>
    /// Slug formation analysis DTO
    /// </summary>
    public class SlugFormationAnalysisDto
    {
        public string PipelineId { get; set; } = string.Empty;
        public List<SlugMechanismDto> FormationMechanisms { get; set; } = new();
        public string DominantMechanism { get; set; } = string.Empty;
        public decimal PredictedSlugFrequency { get; set; }
    }

    /// <summary>
    /// Slug mechanism DTO
    /// </summary>
    public class SlugMechanismDto
    {
        public string MechanismType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Probability { get; set; }
    }

    /// <summary>
    /// Slug formation request DTO
    /// </summary>
    public class SlugFormationRequestDto
    {
        public string PipelineId { get; set; } = string.Empty;
        public decimal OilRate { get; set; }
        public decimal GasRate { get; set; }
        public decimal Pressure { get; set; }
        public decimal Temperature { get; set; }
    }

    /// <summary>
    /// Slug mitigation DTO
    /// </summary>
    public class SlugMitigationDto
    {
        public string PipelineId { get; set; } = string.Empty;
        public List<MitigationStrategyDto> Strategies { get; set; } = new();
        public string BestStrategy { get; set; } = string.Empty;
        public string ImplementationCost { get; set; } = string.Empty;
    }

    /// <summary>
    /// Mitigation strategy DTO
    /// </summary>
    public class MitigationStrategyDto
    {
        public string StrategyName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Effectiveness { get; set; }
        public decimal EstimatedCost { get; set; }
    }

    #endregion

    #region Leak Detection DTOs

    /// <summary>
    /// Leak detection result DTO
    /// </summary>
    public class LeakDetectionResultDto
    {
        public string PipelineId { get; set; } = string.Empty;
        public bool LeakDetected { get; set; }
        public List<LeakLocationDto> LeakLocations { get; set; } = new();
        public decimal EstimatedLeakRate { get; set; }
        public DateTime DetectionTime { get; set; }
        public string RecommendedAction { get; set; } = string.Empty;
    }

    /// <summary>
    /// Leak location DTO
    /// </summary>
    public class LeakLocationDto
    {
        public string LocationDescription { get; set; } = string.Empty;
        public decimal Distance { get; set; }
        public decimal ConfidenceLevel { get; set; }
        public string Severity { get; set; } = string.Empty;
    }

    /// <summary>
    /// Leak detection request DTO
    /// </summary>
    public class LeakDetectionRequestDto
    {
        public string PipelineId { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<string> DetectionMethods { get; set; } = new();
    }

    /// <summary>
    /// Pressure anomaly DTO
    /// </summary>
    public class PressureAnomalyDto
    {
        public string PipelineId { get; set; } = string.Empty;
        public bool AnomalyDetected { get; set; }
        public List<AnomalyLocationDto> AnomalyLocations { get; set; } = new();
        public string AnomalyType { get; set; } = string.Empty;
    }

    /// <summary>
    /// Anomaly location DTO
    /// </summary>
    public class AnomalyLocationDto
    {
        public decimal Distance { get; set; }
        public decimal PressureDeviation { get; set; }
        public DateTime DetectionTime { get; set; }
    }

    /// <summary>
    /// Pressure anomaly request DTO
    /// </summary>
    public class PressureAnomalyRequestDto
    {
        public string PipelineId { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal AnomalyThreshold { get; set; }
    }

    /// <summary>
    /// Acoustic detection result DTO
    /// </summary>
    public class AcousticDetectionResultDto
    {
        public string PipelineId { get; set; } = string.Empty;
        public bool LeakDetected { get; set; }
        public List<AcousticSignalDto> SignalData { get; set; } = new();
        public string MostLikelyLocation { get; set; } = string.Empty;
    }

    /// <summary>
    /// Acoustic signal DTO
    /// </summary>
    public class AcousticSignalDto
    {
        public decimal Frequency { get; set; }
        public decimal Amplitude { get; set; }
        public string SignalType { get; set; } = string.Empty;
    }

    /// <summary>
    /// Acoustic request DTO
    /// </summary>
    public class AcousticRequestDto
    {
        public string PipelineId { get; set; } = string.Empty;
        public DateTime MeasurementDate { get; set; }
        public decimal FrequencyRange { get; set; }
    }

    /// <summary>
    /// Diagnosis result DTO
    /// </summary>
    public class DiagnosisResultDto
    {
        public string PipelineId { get; set; } = string.Empty;
        public List<DiagnosedIssueDto> IdentifiedIssues { get; set; } = new();
        public string OverallStatus { get; set; } = string.Empty;
        public List<string> RecommendedActions { get; set; } = new();
    }

    /// <summary>
    /// Diagnosed issue DTO
    /// </summary>
    public class DiagnosedIssueDto
    {
        public string IssueType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string RecommendedAction { get; set; } = string.Empty;
    }

    /// <summary>
    /// Diagnosis request DTO
    /// </summary>
    public class DiagnosisRequestDto
    {
        public string PipelineId { get; set; } = string.Empty;
        public List<string> SymptomCodes { get; set; } = new();
        public DateTime DiagnosisDate { get; set; }
    }

    /// <summary>
    /// Gas loss rate DTO
    /// </summary>
    public class GasLossRateDto
    {
        public string PipelineId { get; set; } = string.Empty;
        public decimal GasLossRate { get; set; }
        public string Unit { get; set; } = "SCFH";
        public decimal DailyLoss { get; set; }
        public decimal AnnualLoss { get; set; }
        public decimal EstimatedValue { get; set; }
    }

    #endregion

    #region Integrity DTOs

    /// <summary>
    /// Integrity assessment DTO
    /// </summary>
    public class IntegrityAssessmentDto
    {
        public string PipelineId { get; set; } = string.Empty;
        public string IntegrityStatus { get; set; } = string.Empty; // Excellent, Good, Fair, Poor, Critical
        public decimal IntegrityScore { get; set; }
        public List<IntegrityIssueDto> Issues { get; set; } = new();
        public List<string> InspectionRecommendations { get; set; } = new();
    }

    /// <summary>
    /// Integrity issue DTO
    /// </summary>
    public class IntegrityIssueDto
    {
        public string IssueType { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string RemediationAction { get; set; } = string.Empty;
    }

    /// <summary>
    /// Integrity request DTO
    /// </summary>
    public class IntegrityRequestDto
    {
        public string PipelineId { get; set; } = string.Empty;
        public DateTime LastInspectionDate { get; set; }
        public bool IncludeHistoricalData { get; set; } = true;
    }

    /// <summary>
    /// Stress analysis DTO
    /// </summary>
    public class StressAnalysisDto
    {
        public string PipelineId { get; set; } = string.Empty;
        public decimal MaximumStress { get; set; }
        public decimal AllowableStress { get; set; }
        public decimal SafetyFactor { get; set; }
        public bool IsSafe { get; set; }
        public List<StressLocationDto> HighStressAreas { get; set; } = new();
    }

    /// <summary>
    /// Stress location DTO
    /// </summary>
    public class StressLocationDto
    {
        public string LocationDescription { get; set; } = string.Empty;
        public decimal StressLevel { get; set; }
    }

    /// <summary>
    /// Stress request DTO
    /// </summary>
    public class StressRequestDto
    {
        public string PipelineId { get; set; } = string.Empty;
        public decimal OperatingPressure { get; set; }
        public decimal DesignPressure { get; set; }
        public decimal ExternalLoads { get; set; }
    }

    /// <summary>
    /// Wall thickness analysis DTO
    /// </summary>
    public class WallThicknessAnalysisDto
    {
        public string PipelineId { get; set; } = string.Empty;
        public decimal CurrentWallThickness { get; set; }
        public decimal MinimumRequiredThickness { get; set; }
        public decimal RemainingLife { get; set; }
        public bool IsAdequate { get; set; }
    }

    /// <summary>
    /// Wall thickness request DTO
    /// </summary>
    public class WallThicknessRequestDto
    {
        public string PipelineId { get; set; } = string.Empty;
        public decimal CurrentThickness { get; set; }
        public string PipeMaterial { get; set; } = string.Empty;
        public decimal OperatingPressure { get; set; }
    }

    /// <summary>
    /// Stability assessment DTO
    /// </summary>
    public class StabilityAssessmentDto
    {
        public string PipelineId { get; set; } = string.Empty;
        public bool IsStable { get; set; }
        public decimal StabilityScore { get; set; }
        public List<string> StabilityFactors { get; set; } = new();
    }

    /// <summary>
    /// Stability request DTO
    /// </summary>
    public class StabilityRequestDto
    {
        public string PipelineId { get; set; } = string.Empty;
        public decimal SoilBearingCapacity { get; set; }
        public decimal PipelineWeight { get; set; }
        public decimal DepthBelowGrade { get; set; }
    }

    #endregion

    #region Optimization DTOs

    /// <summary>
    /// Optimization result DTO
    /// </summary>
    public class OptimizationResultDto
    {
        public string PipelineId { get; set; } = string.Empty;
        public List<OptimizedParameterDto> OptimizedParameters { get; set; } = new();
        public decimal ExpectedCostSavings { get; set; }
        public decimal ProductionGainPercent { get; set; }
        public string Recommendations { get; set; } = string.Empty;
    }

    /// <summary>
    /// Optimized parameter DTO
    /// </summary>
    public class OptimizedParameterDto
    {
        public string ParameterName { get; set; } = string.Empty;
        public decimal CurrentValue { get; set; }
        public decimal OptimizedValue { get; set; }
        public string Unit { get; set; } = string.Empty;
    }

    /// <summary>
    /// Optimization request DTO
    /// </summary>
    public class OptimizationRequestDto
    {
        public string PipelineId { get; set; } = string.Empty;
        public List<string> OptimizationObjectives { get; set; } = new();
        public List<string> ConstraintParameters { get; set; } = new();
    }

    /// <summary>
    /// Diameter recommendation DTO
    /// </summary>
    public class DiameterRecommendationDto
    {
        public string PipelineId { get; set; } = string.Empty;
        public decimal RecommendedDiameter { get; set; }
        public decimal OptimalVelocity { get; set; }
        public decimal PressureDropPerMile { get; set; }
        public decimal AnnualCostSavings { get; set; }
    }

    /// <summary>
    /// Diameter request DTO
    /// </summary>
    public class DiameterRequestDto
    {
        public string PipelineId { get; set; } = string.Empty;
        public decimal DesignFlowRate { get; set; }
        public decimal PipelineLength { get; set; }
        public decimal MaxAllowablePressureDrop { get; set; }
    }

    /// <summary>
    /// Flow rate optimization DTO
    /// </summary>
    public class FlowRateOptimizationDto
    {
        public string PipelineId { get; set; } = string.Empty;
        public decimal OptimalFlowRate { get; set; }
        public decimal MaximumFlowRate { get; set; }
        public decimal MinimumFlowRate { get; set; }
        public decimal EnergyConsumption { get; set; }
    }

    /// <summary>
    /// Flow optimization request DTO
    /// </summary>
    public class FlowOptimizationRequestDto
    {
        public string PipelineId { get; set; } = string.Empty;
        public decimal DesignFlowRate { get; set; }
        public List<string> OptimizationCriteria { get; set; } = new();
    }

    /// <summary>
    /// Sensitivity analysis DTO
    /// </summary>
    public class SensitivityAnalysisDto
    {
        public string PipelineId { get; set; } = string.Empty;
        public List<SensitivityParameterResultDto> Parameters { get; set; } = new();
        public string AnalysisSummary { get; set; } = string.Empty;
    }

    /// <summary>
    /// Sensitivity parameter result DTO
    /// </summary>
    public class SensitivityParameterResultDto
    {
        public string ParameterName { get; set; } = string.Empty;
        public decimal BaseValue { get; set; }
        public List<SensitivityPointResultDto> Results { get; set; } = new();
    }

    /// <summary>
    /// Sensitivity point result DTO
    /// </summary>
    public class SensitivityPointResultDto
    {
        public decimal ParameterValue { get; set; }
        public decimal PressureDrop { get; set; }
        public decimal Cost { get; set; }
    }

    /// <summary>
    /// Sensitivity request DTO
    /// </summary>
    public class SensitivityRequestDto
    {
        public string PipelineId { get; set; } = string.Empty;
        public List<string> ParametersToAnalyze { get; set; } = new();
        public decimal VariationPercentage { get; set; } = 20m;
    }

    /// <summary>
    /// Pump/Compressor sizing DTO
    /// </summary>
    public class PumpCompressorSizingDto
    {
        public string PipelineId { get; set; } = string.Empty;
        public decimal RequiredHP { get; set; }
        public string RecommendedEquipmentType { get; set; } = string.Empty;
        public string RecommendedModel { get; set; } = string.Empty;
        public decimal DischargeFlow { get; set; }
        public decimal DischargeHead { get; set; }
        public decimal EstimatedCost { get; set; }
    }

    /// <summary>
    /// Pump/Compressor request DTO
    /// </summary>
    public class PumpCompressorRequestDto
    {
        public string PipelineId { get; set; } = string.Empty;
        public decimal DesignFlowRate { get; set; }
        public decimal InletPressure { get; set; }
        public decimal OutletPressure { get; set; }
        public string EquipmentType { get; set; } = string.Empty;
    }

    #endregion

    #region Simulation DTOs

    /// <summary>
    /// Hydraulic simulation result DTO
    /// </summary>
    public class HydraulicSimulationResultDto
    {
        public string PipelineId { get; set; } = string.Empty;
        public DateTime SimulationDate { get; set; }
        public List<SimulationNodeDto> Nodes { get; set; } = new();
        public decimal MaximumPressure { get; set; }
        public decimal MinimumPressure { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>
    /// Simulation node DTO
    /// </summary>
    public class SimulationNodeDto
    {
        public string NodeName { get; set; } = string.Empty;
        public decimal Distance { get; set; }
        public decimal Pressure { get; set; }
        public decimal Temperature { get; set; }
        public decimal VelocityPipe { get; set; }
    }

    /// <summary>
    /// Simulation request DTO
    /// </summary>
    public class SimulationRequestDto
    {
        public string PipelineId { get; set; } = string.Empty;
        public decimal InletFlowRate { get; set; }
        public decimal InletPressure { get; set; }
        public decimal InletTemperature { get; set; }
        public int NumberOfNodes { get; set; } = 10;
    }

    /// <summary>
    /// Transient analysis result DTO
    /// </summary>
    public class TransientAnalysisResultDto
    {
        public string PipelineId { get; set; } = string.Empty;
        public List<TransientEventDto> Events { get; set; } = new();
        public decimal MaximumPressureSpike { get; set; }
        public decimal MinimumPressureSpike { get; set; }
    }

    /// <summary>
    /// Transient event DTO
    /// </summary>
    public class TransientEventDto
    {
        public DateTime EventTime { get; set; }
        public decimal Pressure { get; set; }
        public string EventType { get; set; } = string.Empty;
    }

    /// <summary>
    /// Transient request DTO
    /// </summary>
    public class TransientRequestDto
    {
        public string PipelineId { get; set; } = string.Empty;
        public string InitialCondition { get; set; } = string.Empty;
        public string UpsetCondition { get; set; } = string.Empty;
    }

    /// <summary>
    /// Temperature effect DTO
    /// </summary>
    public class TemperatureEffectDto
    {
        public string PipelineId { get; set; } = string.Empty;
        public List<TemperatureScenarioDto> Scenarios { get; set; } = new();
        public string TemperatureImpact { get; set; } = string.Empty;
    }

    /// <summary>
    /// Temperature scenario DTO
    /// </summary>
    public class TemperatureScenarioDto
    {
        public decimal Temperature { get; set; }
        public decimal FluidDensity { get; set; }
        public decimal FluidViscosity { get; set; }
        public decimal PressureDrop { get; set; }
    }

    /// <summary>
    /// Temperature request DTO
    /// </summary>
    public class TemperatureRequestDto
    {
        public string PipelineId { get; set; } = string.Empty;
        public decimal MinTemperature { get; set; }
        public decimal MaxTemperature { get; set; }
        public int NumberOfSteps { get; set; } = 5;
    }

    /// <summary>
    /// Pigging analysis DTO
    /// </summary>
    public class PiggingAnalysisDto
    {
        public string PipelineId { get; set; } = string.Empty;
        public bool IsSuitableForPigging { get; set; }
        public string RecommendedPigType { get; set; } = string.Empty;
        public decimal PigRunTime { get; set; }
        public decimal PressureRequirement { get; set; }
        public List<string> Recommendations { get; set; } = new();
    }

    /// <summary>
    /// Pigging request DTO
    /// </summary>
    public class PiggingRequestDto
    {
        public string PipelineId { get; set; } = string.Empty;
        public decimal PipelineDiameter { get; set; }
        public decimal PipelineLength { get; set; }
        public string PipelineCondition { get; set; } = string.Empty;
    }

    #endregion

    #region Data Management DTOs

    /// <summary>
    /// Pipeline configuration DTO
    /// </summary>
    public class PipelineConfigurationDto
    {
        public string PipelineId { get; set; } = string.Empty;
        public decimal Diameter { get; set; }
        public decimal WallThickness { get; set; }
        public decimal Length { get; set; }
        public string Material { get; set; } = string.Empty;
        public decimal DesignPressure { get; set; }
        public decimal DesignTemperature { get; set; }
        public DateTime LastInspectionDate { get; set; }
        public decimal MaxAllowableWorkingPressure { get; set; }
    }

    #endregion

    #region Reporting DTOs

    /// <summary>
    /// Pipeline report DTO
    /// </summary>
    public class PipelineReportDto
    {
        public string ReportId { get; set; } = string.Empty;
        public string PipelineId { get; set; } = string.Empty;
        public DateTime GeneratedDate { get; set; }
        public string ReportType { get; set; } = string.Empty;
        public byte[] ReportContent { get; set; } = Array.Empty<byte>();
        public string ExecutiveSummary { get; set; } = string.Empty;
        public List<string> KeyFindings { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
    }

    /// <summary>
    /// Report request DTO
    /// </summary>
    public class ReportRequestDto
    {
        public string PipelineId { get; set; } = string.Empty;
        public string ReportType { get; set; } = "Comprehensive";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<string> IncludeSections { get; set; } = new();
        public string Format { get; set; } = "PDF";
    }

    /// <summary>
    /// Pressure profile report DTO
    /// </summary>
    public class PressureProfileReportDto
    {
        public string ReportId { get; set; } = string.Empty;
        public string PipelineId { get; set; } = string.Empty;
        public List<PressureProfilePointDto> ProfileData { get; set; } = new();
        public decimal MaximumPressure { get; set; }
        public decimal MinimumPressure { get; set; }
    }

    /// <summary>
    /// Pressure profile point DTO
    /// </summary>
    public class PressureProfilePointDto
    {
        public decimal Distance { get; set; }
        public decimal Pressure { get; set; }
        public DateTime MeasurementTime { get; set; }
    }

    /// <summary>
    /// Pressure report request DTO
    /// </summary>
    public class PressureReportRequestDto
    {
        public string PipelineId { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int DataPoints { get; set; } = 100;
    }

    /// <summary>
    /// Integrity report DTO
    /// </summary>
    public class IntegrityReportDto
    {
        public string ReportId { get; set; } = string.Empty;
        public string PipelineId { get; set; } = string.Empty;
        public string IntegrityStatus { get; set; } = string.Empty;
        public List<IntegrityFindingDto> Findings { get; set; } = new();
        public List<string> RemediationActions { get; set; } = new();
    }

    /// <summary>
    /// Integrity finding DTO
    /// </summary>
    public class IntegrityFindingDto
    {
        public string FindingType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string RemediationAction { get; set; } = string.Empty;
    }

    /// <summary>
    /// Integrity report request DTO
    /// </summary>
    public class IntegrityReportRequestDto
    {
        public string PipelineId { get; set; } = string.Empty;
        public DateTime EvaluationDate { get; set; }
        public bool IncludeHistoricalData { get; set; } = true;
    }

    #endregion
}

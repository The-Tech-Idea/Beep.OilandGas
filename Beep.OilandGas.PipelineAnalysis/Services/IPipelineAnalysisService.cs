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
        Task<PipelineAnalysisResult> AnalyzePipelineFlowAsync(string pipelineId, decimal flowRate, decimal inletPressure);

        /// <summary>
        /// Calculates pressure drop across pipeline segment
        /// </summary>
        Task<PressureDropResult> CalculatePressureDropAsync(string pipelineId, decimal flowRate);

        /// <summary>
        /// Determines flow regime (Laminar, Turbulent, Transitional)
        /// </summary>
        Task<FlowRegimeAnalysis> AnalyzeFlowRegimeAsync(string pipelineId, FlowRegimeRequest request);

        /// <summary>
        /// Calculates Beggs-Brill correlation for multiphase flow
        /// </summary>
        Task<MultiphaseFlowResult> CalculateBeggsbrillAsync(string pipelineId, MultiphaseFlowRequest request);

        /// <summary>
        /// Calculates Hagedorn-Brown correlation for multiphase flow
        /// </summary>
        Task<MultiphaseFlowResult> CalculateHagedornBrownAsync(string pipelineId, MultiphaseFlowRequest request);

        /// <summary>
        /// Calculates Duns-Ros correlation for vertical flow
        /// </summary>
        Task<MultiphaseFlowResult> CalculateDunsRosAsync(string pipelineId, MultiphaseFlowRequest request);

        /// <summary>
        /// Performs comprehensive pipeline sizing analysis
        /// </summary>
        Task<PipelineSizing> PerformPipelineSizingAsync(string pipelineId, PipelineSizingRequest request);

        #endregion

        #region Erosion and Corrosion Analysis

        /// <summary>
        /// Analyzes erosion potential
        /// </summary>
        Task<ErosionAnalysis> AnalyzeErosionAsync(string pipelineId, ErosionRequest request);

        /// <summary>
        /// Calculates erosion velocity
        /// </summary>
        Task<ErosionVelocityResult> CalculateErosionVelocityAsync(string pipelineId, ErosionVelocityRequest request);

        /// <summary>
        /// Analyzes corrosion risk factors
        /// </summary>
        Task<CorrosionAnalysis> AnalyzeCorrosionRiskAsync(string pipelineId, CorrosionRequest request);

        /// <summary>
        /// Estimates corrosion rate
        /// </summary>
        Task<CorrosionRateResult> EstimateCorrosionRateAsync(string pipelineId, CorrosionRateRequest request);

        /// <summary>
        /// Assesses pipeline material compatibility
        /// </summary>
        Task<MaterialCompatibility> AssessMaterialCompatibilityAsync(string pipelineId, MaterialRequest request);

        #endregion

        #region Slug Flow Analysis

        /// <summary>
        /// Analyzes slug flow conditions
        /// </summary>
        Task<SlugFlowAnalysis> AnalyzeSlugFlowAsync(string pipelineId, SlugFlowRequest request);

        /// <summary>
        /// Predicts slug frequency
        /// </summary>
        Task<SlugFrequencyResult> PredictSlugFrequencyAsync(string pipelineId, SlugFrequencyRequest request);

        /// <summary>
        /// Analyzes slug formation mechanisms
        /// </summary>
        Task<SlugFormationAnalysis> AnalyzeSlugFormationAsync(string pipelineId, SlugFormationRequest request);

        /// <summary>
        /// Evaluates slug mitigation strategies
        /// </summary>
        Task<SlugMitigation> EvaluateSlugMitigationAsync(string pipelineId, List<string> mitigationStrategies);

        #endregion

        #region Leak Detection and Diagnostics

        /// <summary>
        /// Detects potential leaks
        /// </summary>
        Task<LeakDetectionResult> DetectLeaksAsync(string pipelineId, LeakDetectionRequest request);

        /// <summary>
        /// Analyzes pressure profile for anomalies
        /// </summary>
        Task<PressureAnomaly> AnalyzePressureAnomaliesAsync(string pipelineId, PressureAnomalyRequest request);

        /// <summary>
        /// Performs acoustic leak detection
        /// </summary>
        Task<AcousticDetectionResult> PerformAcousticLeakDetectionAsync(string pipelineId, AcousticRequest request);

        /// <summary>
        /// Diagnoses operational issues
        /// </summary>
        Task<DiagnosisResult> DiagnoseOperationalIssuesAsync(string pipelineId, DiagnosisRequest request);

        /// <summary>
        /// Calculates gas loss rate
        /// </summary>
        Task<GasLossRate> CalculateGasLossRateAsync(string pipelineId, decimal pressureDifference);

        #endregion

        #region Pipeline Integrity

        /// <summary>
        /// Assesses pipeline integrity
        /// </summary>
        Task<IntegrityAssessment> AssessIntegrityAsync(string pipelineId, IntegrityRequest request);

        /// <summary>
        /// Performs stress analysis
        /// </summary>
        Task<StressAnalysis> PerformStressAnalysisAsync(string pipelineId, StressRequest request);

        /// <summary>
        /// Evaluates wall thickness
        /// </summary>
        Task<WallThicknessAnalysis> EvaluateWallThicknessAsync(string pipelineId, WallThicknessRequest request);

        /// <summary>
        /// Assesses pipeline stability
        /// </summary>
        Task<StabilityAssessment> AssessStabilityAsync(string pipelineId, StabilityRequest request);

        #endregion

        #region Optimization

        /// <summary>
        /// Optimizes pipeline flow parameters
        /// </summary>
        Task<OptimizationResult> OptimizeFlowParametersAsync(string pipelineId, OptimizationRequest request);

        /// <summary>
        /// Recommends optimal pipeline diameter
        /// </summary>
        Task<DiameterRecommendation> RecommendOptimalDiameterAsync(string pipelineId, DiameterRequest request);

        /// <summary>
        /// Optimizes flow rate for maximum efficiency
        /// </summary>
        Task<FlowRateOptimization> OptimizeFlowRateAsync(string pipelineId, FlowOptimizationRequest request);

        /// <summary>
        /// Performs sensitivity analysis on pipeline parameters
        /// </summary>
        Task<SensitivityAnalysis> PerformSensitivityAnalysisAsync(string pipelineId, SensitivityRequest request);

        /// <summary>
        /// Recommends compressor/pump sizing
        /// </summary>
        Task<PumpCompressorSizing> RecommendPumpCompressorSizingAsync(string pipelineId, PumpCompressorRequest request);

        #endregion

        #region Hydraulic Simulation

        /// <summary>
        /// Performs steady-state hydraulic simulation
        /// </summary>
        Task<HydraulicSimulationResult> SimulateSteadyStateAsync(string pipelineId, SimulationRequest request);

        /// <summary>
        /// Performs transient analysis
        /// </summary>
        Task<TransientAnalysisResult> PerformTransientAnalysisAsync(string pipelineId, TransientRequest request);

        /// <summary>
        /// Analyzes temperature effects on flow
        /// </summary>
        Task<TemperatureEffect> AnalyzeTemperatureEffectsAsync(string pipelineId, TemperatureRequest request);

        /// <summary>
        /// Evaluates pigging operations
        /// </summary>
        Task<PiggingAnalysis> EvaluatePiggingOperationsAsync(string pipelineId, PiggingRequest request);

        #endregion

        #region Data Management

        /// <summary>
        /// Saves pipeline analysis results
        /// </summary>
        Task SaveAnalysisResultsAsync(PipelineAnalysisResult results, string userId);

        /// <summary>
        /// Retrieves pipeline analysis history
        /// </summary>
        Task<List<PipelineAnalysisResult>> GetAnalysisHistoryAsync(string pipelineId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Updates pipeline configuration
        /// </summary>
        Task UpdatePipelineConfigurationAsync(PipelineConfiguration config, string userId);

        /// <summary>
        /// Retrieves pipeline configuration
        /// </summary>
        Task<PipelineConfiguration?> GetPipelineConfigurationAsync(string pipelineId);

        #endregion

        #region Reporting

        /// <summary>
        /// Generates comprehensive pipeline analysis report
        /// </summary>
        Task<PipelineReport> GenerateAnalysisReportAsync(string pipelineId, ReportRequest request);

        /// <summary>
        /// Generates pressure profile report
        /// </summary>
        Task<PressureProfileReport> GeneratePressureProfileReportAsync(string pipelineId, PressureReportRequest request);

        /// <summary>
        /// Generates integrity assessment report
        /// </summary>
        Task<IntegrityReport> GenerateIntegrityReportAsync(string pipelineId, IntegrityReportRequest request);

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
    public class PipelineAnalysisResult
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
    public class PressureDropResult
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
    public class FlowRegimeAnalysis
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
    public class FlowRegimeRequest
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
    public class MultiphaseFlowResult
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
    public class MultiphaseFlowRequest
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
    public class PipelineSizing
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
    public class PipelineSizingRequest
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
    public class ErosionAnalysis
    {
        public string PipelineId { get; set; } = string.Empty;
        public decimal ErosionVelocity { get; set; }
        public decimal ActualVelocity { get; set; }
        public string ErosionRisk { get; set; } = string.Empty; // Low, Medium, High, Critical
        public decimal SafetyMargin { get; set; }
        public List<ErosionSpot> HighRiskSpots { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
    }

    /// <summary>
    /// Erosion spot DTO
    /// </summary>
    public class ErosionSpot
    {
        public string LocationDescription { get; set; } = string.Empty;
        public decimal ErosionRating { get; set; }
        public string MitigationStrategy { get; set; } = string.Empty;
    }

    /// <summary>
    /// Erosion request DTO
    /// </summary>
    public class ErosionRequest
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
    public class ErosionVelocityResult
    {
        public decimal ErosionVelocity { get; set; }
        public decimal ActualVelocity { get; set; }
        public bool IsSafe { get; set; }
        public decimal VelocitySafetyFactor { get; set; }
    }

    /// <summary>
    /// Erosion velocity request DTO
    /// </summary>
    public class ErosionVelocityRequest
    {
        public string PipelineId { get; set; } = string.Empty;
        public string PipeMaterial { get; set; } = string.Empty;
        public decimal PipeDiameter { get; set; }
        public string FluidType { get; set; } = string.Empty;
    }

    /// <summary>
    /// Corrosion analysis DTO
    /// </summary>
    public class CorrosionAnalysis
    {
        public string PipelineId { get; set; } = string.Empty;
        public decimal CorrosionRisk { get; set; } // 0-100 scale
        public string RiskLevel { get; set; } = string.Empty; // Low, Medium, High, Critical
        public List<CorrosionFactor> CorrosionFactors { get; set; } = new();
        public List<string> MitigationStrategies { get; set; } = new();
        public string RecommendedMaterial { get; set; } = string.Empty;
    }

    /// <summary>
    /// Corrosion factor DTO
    /// </summary>
    public class CorrosionFactor
    {
        public string FactorName { get; set; } = string.Empty;
        public decimal ContributionPercent { get; set; }
        public string Severity { get; set; } = string.Empty;
    }

    /// <summary>
    /// Corrosion request DTO
    /// </summary>
    public class CorrosionRequest
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
    public class CorrosionRateResult
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
    public class CorrosionRateRequest
    {
        public string PipelineId { get; set; } = string.Empty;
        public string PipeMaterial { get; set; } = string.Empty;
        public decimal CurrentWallThickness { get; set; }
        public DateTime LastInspectionDate { get; set; }
        public List<CorrosionDataPoint> HistoricalData { get; set; } = new();
    }

    /// <summary>
    /// Corrosion data point DTO
    /// </summary>
    public class CorrosionDataPoint
    {
        public DateTime MeasurementDate { get; set; }
        public decimal WallThickness { get; set; }
    }

    /// <summary>
    /// Material compatibility DTO
    /// </summary>
    public class MaterialCompatibility
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
    public class MaterialRequest
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
    public class SlugFlowAnalysis
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
    public class SlugFlowRequest
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
    public class SlugFrequencyResult
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
    public class SlugFrequencyRequest
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
    public class SlugFormationAnalysis
    {
        public string PipelineId { get; set; } = string.Empty;
        public List<SlugMechanism> FormationMechanisms { get; set; } = new();
        public string DominantMechanism { get; set; } = string.Empty;
        public decimal PredictedSlugFrequency { get; set; }
    }

    /// <summary>
    /// Slug mechanism DTO
    /// </summary>
    public class SlugMechanism
    {
        public string MechanismType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Probability { get; set; }
    }

    /// <summary>
    /// Slug formation request DTO
    /// </summary>
    public class SlugFormationRequest
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
    public class SlugMitigation
    {
        public string PipelineId { get; set; } = string.Empty;
        public List<MitigationStrategy> Strategies { get; set; } = new();
        public string BestStrategy { get; set; } = string.Empty;
        public string ImplementationCost { get; set; } = string.Empty;
    }

    /// <summary>
    /// Mitigation strategy DTO
    /// </summary>
    public class MitigationStrategy
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
    public class LeakDetectionResult
    {
        public string PipelineId { get; set; } = string.Empty;
        public bool LeakDetected { get; set; }
        public List<LeakLocation> LeakLocations { get; set; } = new();
        public decimal EstimatedLeakRate { get; set; }
        public DateTime DetectionTime { get; set; }
        public string RecommendedAction { get; set; } = string.Empty;
    }

    /// <summary>
    /// Leak location DTO
    /// </summary>
    public class LeakLocation
    {
        public string LocationDescription { get; set; } = string.Empty;
        public decimal Distance { get; set; }
        public decimal ConfidenceLevel { get; set; }
        public string Severity { get; set; } = string.Empty;
    }

    /// <summary>
    /// Leak detection request DTO
    /// </summary>
    public class LeakDetectionRequest
    {
        public string PipelineId { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<string> DetectionMethods { get; set; } = new();
    }

    /// <summary>
    /// Pressure anomaly DTO
    /// </summary>
    public class PressureAnomaly
    {
        public string PipelineId { get; set; } = string.Empty;
        public bool AnomalyDetected { get; set; }
        public List<AnomalyLocation> AnomalyLocations { get; set; } = new();
        public string AnomalyType { get; set; } = string.Empty;
    }

    /// <summary>
    /// Anomaly location DTO
    /// </summary>
    public class AnomalyLocation
    {
        public decimal Distance { get; set; }
        public decimal PressureDeviation { get; set; }
        public DateTime DetectionTime { get; set; }
    }

    /// <summary>
    /// Pressure anomaly request DTO
    /// </summary>
    public class PressureAnomalyRequest
    {
        public string PipelineId { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal AnomalyThreshold { get; set; }
    }

    /// <summary>
    /// Acoustic detection result DTO
    /// </summary>
    public class AcousticDetectionResult
    {
        public string PipelineId { get; set; } = string.Empty;
        public bool LeakDetected { get; set; }
        public List<AcousticSignal> SignalData { get; set; } = new();
        public string MostLikelyLocation { get; set; } = string.Empty;
    }

    /// <summary>
    /// Acoustic signal DTO
    /// </summary>
    public class AcousticSignal
    {
        public decimal Frequency { get; set; }
        public decimal Amplitude { get; set; }
        public string SignalType { get; set; } = string.Empty;
    }

    /// <summary>
    /// Acoustic request DTO
    /// </summary>
    public class AcousticRequest
    {
        public string PipelineId { get; set; } = string.Empty;
        public DateTime MeasurementDate { get; set; }
        public decimal FrequencyRange { get; set; }
    }

    /// <summary>
    /// Diagnosis result DTO
    /// </summary>
    public class DiagnosisResult
    {
        public string PipelineId { get; set; } = string.Empty;
        public List<DiagnosedIssue> IdentifiedIssues { get; set; } = new();
        public string OverallStatus { get; set; } = string.Empty;
        public List<string> RecommendedActions { get; set; } = new();
    }

    /// <summary>
    /// Diagnosed issue DTO
    /// </summary>
    public class DiagnosedIssue
    {
        public string IssueType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string RecommendedAction { get; set; } = string.Empty;
    }

    /// <summary>
    /// Diagnosis request DTO
    /// </summary>
    public class DiagnosisRequest
    {
        public string PipelineId { get; set; } = string.Empty;
        public List<string> SymptomCodes { get; set; } = new();
        public DateTime DiagnosisDate { get; set; }
    }

    /// <summary>
    /// Gas loss rate DTO
    /// </summary>
    public class GasLossRate
    {
        public string PipelineId { get; set; } = string.Empty;
        public decimal LossRate { get; set; }
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
    public class IntegrityAssessment
    {
        public string PipelineId { get; set; } = string.Empty;
        public string IntegrityStatus { get; set; } = string.Empty; // Excellent, Good, Fair, Poor, Critical
        public decimal IntegrityScore { get; set; }
        public List<IntegrityIssue> Issues { get; set; } = new();
        public List<string> InspectionRecommendations { get; set; } = new();
    }

    /// <summary>
    /// Integrity issue DTO
    /// </summary>
    public class IntegrityIssue
    {
        public string IssueType { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string RemediationAction { get; set; } = string.Empty;
    }

    /// <summary>
    /// Integrity request DTO
    /// </summary>
    public class IntegrityRequest
    {
        public string PipelineId { get; set; } = string.Empty;
        public DateTime LastInspectionDate { get; set; }
        public bool IncludeHistoricalData { get; set; } = true;
    }

    /// <summary>
    /// Stress analysis DTO
    /// </summary>
    public class StressAnalysis
    {
        public string PipelineId { get; set; } = string.Empty;
        public decimal MaximumStress { get; set; }
        public decimal AllowableStress { get; set; }
        public decimal SafetyFactor { get; set; }
        public bool IsSafe { get; set; }
        public List<StressLocation> HighStressAreas { get; set; } = new();
    }

    /// <summary>
    /// Stress location DTO
    /// </summary>
    public class StressLocation
    {
        public string LocationDescription { get; set; } = string.Empty;
        public decimal StressLevel { get; set; }
    }

    /// <summary>
    /// Stress request DTO
    /// </summary>
    public class StressRequest
    {
        public string PipelineId { get; set; } = string.Empty;
        public decimal OperatingPressure { get; set; }
        public decimal DesignPressure { get; set; }
        public decimal ExternalLoads { get; set; }
    }

    /// <summary>
    /// Wall thickness analysis DTO
    /// </summary>
    public class WallThicknessAnalysis
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
    public class WallThicknessRequest
    {
        public string PipelineId { get; set; } = string.Empty;
        public decimal CurrentThickness { get; set; }
        public string PipeMaterial { get; set; } = string.Empty;
        public decimal OperatingPressure { get; set; }
    }

    /// <summary>
    /// Stability assessment DTO
    /// </summary>
    public class StabilityAssessment
    {
        public string PipelineId { get; set; } = string.Empty;
        public bool IsStable { get; set; }
        public decimal StabilityScore { get; set; }
        public List<string> StabilityFactors { get; set; } = new();
    }

    /// <summary>
    /// Stability request DTO
    /// </summary>
    public class StabilityRequest
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
    public class OptimizationResult
    {
        public string PipelineId { get; set; } = string.Empty;
        public List<OptimizedParameter> OptimizedParameters { get; set; } = new();
        public decimal ExpectedCostSavings { get; set; }
        public decimal ProductionGainPercent { get; set; }
        public string Recommendations { get; set; } = string.Empty;
    }

    /// <summary>
    /// Optimized parameter DTO
    /// </summary>
    public class OptimizedParameter
    {
        public string ParameterName { get; set; } = string.Empty;
        public decimal CurrentValue { get; set; }
        public decimal OptimizedValue { get; set; }
        public string Unit { get; set; } = string.Empty;
    }

    /// <summary>
    /// Optimization request DTO
    /// </summary>
    public class OptimizationRequest
    {
        public string PipelineId { get; set; } = string.Empty;
        public List<string> OptimizationObjectives { get; set; } = new();
        public List<string> ConstraintParameters { get; set; } = new();
    }

    /// <summary>
    /// Diameter recommendation DTO
    /// </summary>
    public class DiameterRecommendation
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
    public class DiameterRequest
    {
        public string PipelineId { get; set; } = string.Empty;
        public decimal DesignFlowRate { get; set; }
        public decimal PipelineLength { get; set; }
        public decimal MaxAllowablePressureDrop { get; set; }
    }

    /// <summary>
    /// Flow rate optimization DTO
    /// </summary>
    public class FlowRateOptimization
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
    public class FlowOptimizationRequest
    {
        public string PipelineId { get; set; } = string.Empty;
        public decimal DesignFlowRate { get; set; }
        public List<string> OptimizationCriteria { get; set; } = new();
    }

    /// <summary>
    /// Sensitivity analysis DTO
    /// </summary>
    public class SensitivityAnalysis
    {
        public string PipelineId { get; set; } = string.Empty;
        public List<SensitivityParameterResult> Parameters { get; set; } = new();
        public string AnalysisSummary { get; set; } = string.Empty;
    }

    /// <summary>
    /// Sensitivity parameter result DTO
    /// </summary>
    public class SensitivityParameterResult
    {
        public string ParameterName { get; set; } = string.Empty;
        public decimal BaseValue { get; set; }
        public List<SensitivityPointResult> Results { get; set; } = new();
    }

    /// <summary>
    /// Sensitivity point result DTO
    /// </summary>
    public class SensitivityPointResult
    {
        public decimal ParameterValue { get; set; }
        public decimal PressureDrop { get; set; }
        public decimal Cost { get; set; }
    }

    /// <summary>
    /// Sensitivity request DTO
    /// </summary>
    public class SensitivityRequest
    {
        public string PipelineId { get; set; } = string.Empty;
        public List<string> ParametersToAnalyze { get; set; } = new();
        public decimal VariationPercentage { get; set; } = 20m;
    }

    /// <summary>
    /// Pump/Compressor sizing DTO
    /// </summary>
    public class PumpCompressorSizing
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
    public class PumpCompressorRequest
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
    public class HydraulicSimulationResult
    {
        public string PipelineId { get; set; } = string.Empty;
        public DateTime SimulationDate { get; set; }
        public List<SimulationNode> Nodes { get; set; } = new();
        public decimal MaximumPressure { get; set; }
        public decimal MinimumPressure { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>
    /// Simulation node DTO
    /// </summary>
    public class SimulationNode
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
    public class SimulationRequest
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
    public class TransientAnalysisResult
    {
        public string PipelineId { get; set; } = string.Empty;
        public List<TransientEvent> Events { get; set; } = new();
        public decimal MaximumPressureSpike { get; set; }
        public decimal MinimumPressureSpike { get; set; }
    }

    /// <summary>
    /// Transient event DTO
    /// </summary>
    public class TransientEvent
    {
        public DateTime EventTime { get; set; }
        public decimal Pressure { get; set; }
        public string EventType { get; set; } = string.Empty;
    }

    /// <summary>
    /// Transient request DTO
    /// </summary>
    public class TransientRequest
    {
        public string PipelineId { get; set; } = string.Empty;
        public string InitialCondition { get; set; } = string.Empty;
        public string UpsetCondition { get; set; } = string.Empty;
    }

    /// <summary>
    /// Temperature effect DTO
    /// </summary>
    public class TemperatureEffect
    {
        public string PipelineId { get; set; } = string.Empty;
        public List<TemperatureScenario> Scenarios { get; set; } = new();
        public string TemperatureImpact { get; set; } = string.Empty;
    }

    /// <summary>
    /// Temperature scenario DTO
    /// </summary>
    public class TemperatureScenario
    {
        public decimal Temperature { get; set; }
        public decimal FluidDensity { get; set; }
        public decimal FluidViscosity { get; set; }
        public decimal PressureDrop { get; set; }
    }

    /// <summary>
    /// Temperature request DTO
    /// </summary>
    public class TemperatureRequest
    {
        public string PipelineId { get; set; } = string.Empty;
        public decimal MinTemperature { get; set; }
        public decimal MaxTemperature { get; set; }
        public int NumberOfSteps { get; set; } = 5;
    }

    /// <summary>
    /// Pigging analysis DTO
    /// </summary>
    public class PiggingAnalysis
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
    public class PiggingRequest
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
    public class PipelineConfiguration
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
    public class PipelineReport
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
    public class ReportRequest
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
    public class PressureProfileReport
    {
        public string ReportId { get; set; } = string.Empty;
        public string PipelineId { get; set; } = string.Empty;
        public List<PressureProfilePoint> ProfileData { get; set; } = new();
        public decimal MaximumPressure { get; set; }
        public decimal MinimumPressure { get; set; }
    }

    /// <summary>
    /// Pressure profile point DTO
    /// </summary>
    public class PressureProfilePoint
    {
        public decimal Distance { get; set; }
        public decimal Pressure { get; set; }
        public DateTime MeasurementTime { get; set; }
    }

    /// <summary>
    /// Pressure report request DTO
    /// </summary>
    public class PressureReportRequest
    {
        public string PipelineId { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int DataPoints { get; set; } = 100;
    }

    /// <summary>
    /// Integrity report DTO
    /// </summary>
    public class IntegrityReport
    {
        public string ReportId { get; set; } = string.Empty;
        public string PipelineId { get; set; } = string.Empty;
        public string IntegrityStatus { get; set; } = string.Empty;
        public List<IntegrityFinding> Findings { get; set; } = new();
        public List<string> RemediationActions { get; set; } = new();
    }

    /// <summary>
    /// Integrity finding DTO
    /// </summary>
    public class IntegrityFinding
    {
        public string FindingType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string RemediationAction { get; set; } = string.Empty;
    }

    /// <summary>
    /// Integrity report request DTO
    /// </summary>
    public class IntegrityReportRequest
    {
        public string PipelineId { get; set; } = string.Empty;
        public DateTime EvaluationDate { get; set; }
        public bool IncludeHistoricalData { get; set; } = true;
    }

    #endregion
}

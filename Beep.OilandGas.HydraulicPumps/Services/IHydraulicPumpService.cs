using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Beep.OilandGas.HydraulicPumps.Services
{
    /// <summary>
    /// Comprehensive hydraulic pump service interface
    /// Provides pump design, performance analysis, optimization, and monitoring capabilities
    /// </summary>
    public interface IHydraulicPumpService
    {
        #region Pump Design and Sizing

        /// <summary>
        /// Designs a hydraulic pump system for a well
        /// </summary>
        Task<PumpDesignResultDto> DesignPumpSystemAsync(string wellUWI, PumpDesignRequestDto request, string userId);

        /// <summary>
        /// Performs pump sizing calculations
        /// </summary>
        Task<PumpSizingResultDto> SizePumpAsync(string wellUWI, PumpSizingRequestDto request);

        /// <summary>
        /// Selects optimal pump type based on conditions
        /// </summary>
        Task<PumpTypeSelectionDto> SelectOptimalPumpTypeAsync(string wellUWI, PumpSelectionCriteriaDto criteria);

        /// <summary>
        /// Calculates power requirements for pump
        /// </summary>
        Task<PowerRequirementsDto> CalculatePowerRequirementsAsync(string wellUWI, PowerCalculationRequestDto request);

        /// <summary>
        /// Analyzes pump hydraulic balance
        /// </summary>
        Task<HydraulicBalanceDto> AnalyzeHydraulicBalanceAsync(string pumpId, BalanceRequestDto request);

        /// <summary>
        /// Performs pump rod string design
        /// </summary>
        Task<RodStringDesignDto> DesignRodStringAsync(string wellUWI, RodStringRequestDto request);

        #endregion

        #region Performance Analysis

        /// <summary>
        /// Analyzes pump performance
        /// </summary>
        Task<PumpPerformanceAnalysisDto> AnalyzePumpPerformanceAsync(string pumpId, PerformanceAnalysisRequestDto request);

        /// <summary>
        /// Calculates pump efficiency
        /// </summary>
        Task<PumpEfficiencyDto> CalculatePumpEfficiencyAsync(string pumpId, EfficiencyRequestDto request);

        /// <summary>
        /// Analyzes pump cavitation risk
        /// </summary>
        Task<CavitationAnalysisDto> AnalyzeCavitationRiskAsync(string pumpId, CavitationRequestDto request);

        /// <summary>
        /// Performs vibration analysis
        /// </summary>
        Task<VibrationAnalysisDto> AnalyzeVibrationAsync(string pumpId, VibrationRequestDto request);

        /// <summary>
        /// Analyzes pressure dynamics
        /// </summary>
        Task<PressureDynamicsDto> AnalyzePressureDynamicsAsync(string pumpId, PressureRequestDto request);

        /// <summary>
        /// Calculates flow rate characteristics
        /// </summary>
        Task<FlowCharacteristicsDto> CalculateFlowCharacteristicsAsync(string pumpId, FlowRequestDto request);

        #endregion

        #region Optimization

        /// <summary>
        /// Optimizes pump performance parameters
        /// </summary>
        Task<OptimizationResultDto> OptimizePumpParametersAsync(string pumpId, OptimizationRequestDto request);

        /// <summary>
        /// Recommends pump parameter adjustments
        /// </summary>
        Task<ParameterAdjustmentDto> RecommendParameterAdjustmentsAsync(string pumpId);

        /// <summary>
        /// Analyzes pump efficiency improvement opportunities
        /// </summary>
        Task<EfficiencyImprovementDto> IdentifyEfficiencyImprovementsAsync(string pumpId);

        /// <summary>
        /// Performs comparative pump analysis
        /// </summary>
        Task<PumpComparisonDto> ComparePumpsAsync(List<string> pumpIds, ComparisonCriteriaDto criteria);

        /// <summary>
        /// Recommends pump upgrade or replacement
        /// </summary>
        Task<PumpUpgradeRecommendationDto> RecommendPumpUpgradeAsync(string pumpId, UpgradeRequestDto request);

        #endregion

        #region Monitoring and Diagnostics

        /// <summary>
        /// Monitors pump performance in real-time
        /// </summary>
        Task<PumpMonitoringDataDto> MonitorPumpPerformanceAsync(string pumpId, MonitoringRequestDto request);

        /// <summary>
        /// Performs pump diagnostics
        /// </summary>
        Task<DiagnosticsResultDto> PerformPumpDiagnosticsAsync(string pumpId, DiagnosticsRequestDto request);

        /// <summary>
        /// Analyzes pump condition
        /// </summary>
        Task<ConditionAssessmentDto> AssessPumpConditionAsync(string pumpId);

        /// <summary>
        /// Detects pump operational anomalies
        /// </summary>
        Task<AnomalyDetectionDto> DetectOperationalAnomaliesAsync(string pumpId);

        /// <summary>
        /// Performs predictive maintenance analysis
        /// </summary>
        Task<PredictiveMaintenanceDto> AnalyzeMaintenanceRequirementsAsync(string pumpId, MaintenanceRequestDto request);

        #endregion

        #region Failure and Reliability Analysis

        /// <summary>
        /// Analyzes pump failure modes
        /// </summary>
        Task<FailureModeAnalysisDto> AnalyzeFailureModesAsync(string pumpId);

        /// <summary>
        /// Assesses pump reliability
        /// </summary>
        Task<ReliabilityAssessmentDto> AssessReliabilityAsync(string pumpId, ReliabilityRequestDto request);

        /// <summary>
        /// Calculates mean time between failures (MTBF)
        /// </summary>
        Task<MTBFCalculationDto> CalculateMTBFAsync(string pumpId);

        /// <summary>
        /// Analyzes pump wear patterns
        /// </summary>
        Task<WearAnalysisDto> AnalyzeWearPatternsAsync(string pumpId, WearRequestDto request);

        /// <summary>
        /// Identifies potential failure points
        /// </summary>
        Task<FailureRiskAssessmentDto> AssessFailureRiskAsync(string pumpId);

        #endregion

        #region Maintenance and Service

        /// <summary>
        /// Manages pump maintenance schedule
        /// </summary>
        Task<MaintenanceScheduleDto> GenerateMaintenanceScheduleAsync(string pumpId, ScheduleRequestDto request);

        /// <summary>
        /// Logs pump maintenance activities
        /// </summary>
        Task LogMaintenanceActivityAsync(string pumpId, MaintenanceActivityDto activity, string userId);

        /// <summary>
        /// Manages pump rebuild process
        /// </summary>
        Task<RebuildAnalysisDto> AnalyzeRebuildRequirementsAsync(string pumpId, RebuildRequestDto request);

        /// <summary>
        /// Tracks pump spare parts inventory
        /// </summary>
        Task<PartsInventoryDto> ManagePartsInventoryAsync(string pumpId, PartsRequestDto request);

        /// <summary>
        /// Estimates maintenance costs
        /// </summary>
        Task<MaintenanceCostEstimateDto> EstimateCostsAsync(string pumpId, CostEstimateRequestDto request);

        #endregion

        #region Hydraulic Fluid Management

        /// <summary>
        /// Analyzes hydraulic fluid condition
        /// </summary>
        Task<FluidAnalysisDto> AnalyzeHydraulicFluidAsync(string pumpId, FluidAnalysisRequestDto request);

        /// <summary>
        /// Recommends fluid changes
        /// </summary>
        Task<FluidChangeRecommendationDto> RecommendFluidChangeAsync(string pumpId);

        /// <summary>
        /// Tracks fluid contamination levels
        /// </summary>
        Task<ContaminationLevelDto> TrackFluidContaminationAsync(string pumpId, ContaminationRequestDto request);

        /// <summary>
        /// Manages fluid filtration system
        /// </summary>
        Task<FiltrationSystemDto> ManageFiltrationSystemAsync(string pumpId, FiltrationRequestDto request);

        #endregion

        #region Integration and Control Systems

        /// <summary>
        /// Integrates pump with SCADA systems
        /// </summary>
        Task<SCODAIntegrationDto> IntegrateSCADAAsync(string pumpId, SCADAConfigDto config);

        /// <summary>
        /// Manages pump control parameters
        /// </summary>
        Task<ControlParametersDto> ManageControlParametersAsync(string pumpId, ControlRequestDto request, string userId);

        /// <summary>
        /// Analyzes pump-wellbore interaction
        /// </summary>
        Task<WellboreInteractionDto> AnalyzePumpWellboreInteractionAsync(string wellUWI, InteractionRequestDto request);

        #endregion

        #region Data Management

        /// <summary>
        /// Saves pump design data
        /// </summary>
        Task SavePumpDesignAsync(PumpDesignResultDto design, string userId);

        /// <summary>
        /// Retrieves pump design data
        /// </summary>
        Task<PumpDesignResultDto?> GetPumpDesignAsync(string pumpId);

        /// <summary>
        /// Updates pump design data
        /// </summary>
        Task UpdatePumpDesignAsync(PumpDesignResultDto design, string userId);

        /// <summary>
        /// Retrieves pump operational history
        /// </summary>
        Task<List<PumpHistoryDto>> GetPumpHistoryAsync(string pumpId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Retrieves pump performance trends
        /// </summary>
        Task<PerformanceTrendsDto> GetPerformanceTrendsAsync(string pumpId, int monthsBack = 12);

        #endregion

        #region Reporting and Analysis

        /// <summary>
        /// Generates comprehensive pump report
        /// </summary>
        Task<PumpReportDto> GeneratePumpReportAsync(string pumpId, ReportRequestDto request);

        /// <summary>
        /// Generates performance summary report
        /// </summary>
        Task<PerformanceSummaryReportDto> GeneratePerformanceSummaryAsync(string pumpId, SummaryReportRequestDto request);

        /// <summary>
        /// Generates cost analysis report
        /// </summary>
        Task<CostAnalysisReportDto> GenerateCostAnalysisAsync(string pumpId, CostReportRequestDto request);

        /// <summary>
        /// Exports pump data
        /// </summary>
        Task<byte[]> ExportPumpDataAsync(string pumpId, string format = "CSV");

        #endregion
    }

    #region Design and Sizing DTOs

    /// <summary>
    /// Pump design result DTO
    /// </summary>
    public class PumpDesignResultDto
    {
        public string DesignId { get; set; } = string.Empty;
        public string WellUWI { get; set; } = string.Empty;
        public string PumpType { get; set; } = string.Empty;
        public DateTime DesignDate { get; set; }
        public decimal PumpDepth { get; set; }
        public decimal TubingSize { get; set; }
        public decimal CasingSize { get; set; }
        public decimal DesignFlowRate { get; set; }
        public decimal DesignPressure { get; set; }
        public decimal ExpectedEfficiency { get; set; }
        public decimal EstimatedPower { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<string> DesignRecommendations { get; set; } = new();
    }

    /// <summary>
    /// Pump design request DTO
    /// </summary>
    public class PumpDesignRequestDto
    {
        public string WellUWI { get; set; } = string.Empty;
        public string PumpType { get; set; } = string.Empty;
        public decimal WellDepth { get; set; }
        public decimal TubingSize { get; set; }
        public decimal CasingSize { get; set; }
        public decimal DesiredFlowRate { get; set; }
        public decimal ReservoirPressure { get; set; }
        public decimal WellheadPressure { get; set; }
        public string FluidType { get; set; } = string.Empty;
    }

    /// <summary>
    /// Pump sizing result DTO
    /// </summary>
    public class PumpSizingResultDto
    {
        public string PumpId { get; set; } = string.Empty;
        public decimal RecommendedDisplacement { get; set; }
        public decimal MaximumFlowRate { get; set; }
        public decimal MaximumPressure { get; set; }
        public decimal OptimalSpeed { get; set; }
        public string RecommendedPumpModel { get; set; } = string.Empty;
    }

    /// <summary>
    /// Pump sizing request DTO
    /// </summary>
    public class PumpSizingRequestDto
    {
        public string WellUWI { get; set; } = string.Empty;
        public decimal DesiredFlowRate { get; set; }
        public decimal MaxOperatingPressure { get; set; }
        public decimal FluidViscosity { get; set; }
    }

    /// <summary>
    /// Pump type selection DTO
    /// </summary>
    public class PumpTypeSelectionDto
    {
        public string WellUWI { get; set; } = string.Empty;
        public List<PumpTypeOptionDto> Options { get; set; } = new();
        public string RecommendedType { get; set; } = string.Empty;
        public string SelectionRationale { get; set; } = string.Empty;
    }

    /// <summary>
    /// Pump type option DTO
    /// </summary>
    public class PumpTypeOptionDto
    {
        public string PumpType { get; set; } = string.Empty;
        public decimal Efficiency { get; set; }
        public decimal Cost { get; set; }
        public decimal Reliability { get; set; }
        public List<string> Advantages { get; set; } = new();
        public List<string> Disadvantages { get; set; } = new();
    }

    /// <summary>
    /// Pump selection criteria DTO
    /// </summary>
    public class PumpSelectionCriteriaDto
    {
        public string WellUWI { get; set; } = string.Empty;
        public decimal FlowRate { get; set; }
        public decimal Pressure { get; set; }
        public string FluidType { get; set; } = string.Empty;
        public List<string> PriorityFactors { get; set; } = new();
    }

    /// <summary>
    /// Power requirements DTO
    /// </summary>
    public class PowerRequirementsDto
    {
        public string PumpId { get; set; } = string.Empty;
        public decimal HydraulicPower { get; set; }
        public decimal MechanicalPower { get; set; }
        public decimal TotalPowerRequired { get; set; }
        public decimal PowerEfficiency { get; set; }
        public string PowerUnit { get; set; } = "HP";
    }

    /// <summary>
    /// Power calculation request DTO
    /// </summary>
    public class PowerCalculationRequestDto
    {
        public string WellUWI { get; set; } = string.Empty;
        public decimal FlowRate { get; set; }
        public decimal DischargePressure { get; set; }
        public decimal SuctionPressure { get; set; }
    }

    /// <summary>
    /// Hydraulic balance DTO
    /// </summary>
    public class HydraulicBalanceDto
    {
        public string PumpId { get; set; } = string.Empty;
        public bool IsBalanced { get; set; }
        public decimal BalanceScore { get; set; }
        public List<BalanceFactorDto> Factors { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
    }

    /// <summary>
    /// Balance factor DTO
    /// </summary>
    public class BalanceFactorDto
    {
        public string FactorName { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>
    /// Balance request DTO
    /// </summary>
    public class BalanceRequestDto
    {
        public string PumpId { get; set; } = string.Empty;
        public decimal DischargeFlow { get; set; }
        public decimal DischargePressure { get; set; }
    }

    /// <summary>
    /// Rod string design DTO
    /// </summary>
    public class RodStringDesignDto
    {
        public string DesignId { get; set; } = string.Empty;
        public string WellUWI { get; set; } = string.Empty;
        public decimal RodSize { get; set; }
        public string RodGrade { get; set; } = string.Empty;
        public int RodSections { get; set; }
        public decimal TotalRodLength { get; set; }
        public decimal SafetyFactor { get; set; }
        public bool IsAdequate { get; set; }
    }

    /// <summary>
    /// Rod string request DTO
    /// </summary>
    public class RodStringRequestDto
    {
        public string WellUWI { get; set; } = string.Empty;
        public decimal WellDepth { get; set; }
        public decimal PumpingLoad { get; set; }
        public string DesiredGrade { get; set; } = string.Empty;
    }

    #endregion

    #region Performance Analysis DTOs

    /// <summary>
    /// Pump performance analysis DTO
    /// </summary>
    public class PumpPerformanceAnalysisDto
    {
        public string AnalysisId { get; set; } = string.Empty;
        public string PumpId { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }
        public decimal ActualFlowRate { get; set; }
        public decimal ActualPressure { get; set; }
        public decimal ActualEfficiency { get; set; }
        public decimal ActualPower { get; set; }
        public string PerformanceStatus { get; set; } = string.Empty;
        public List<string> Recommendations { get; set; } = new();
    }

    /// <summary>
    /// Performance analysis request DTO
    /// </summary>
    public class PerformanceAnalysisRequestDto
    {
        public string PumpId { get; set; } = string.Empty;
        public decimal MeasuredFlowRate { get; set; }
        public decimal MeasuredPressure { get; set; }
        public decimal MeasuredPower { get; set; }
    }

    /// <summary>
    /// Pump efficiency DTO
    /// </summary>
    public class PumpEfficiencyDto
    {
        public string PumpId { get; set; } = string.Empty;
        public decimal OverallEfficiency { get; set; }
        public decimal VolumetricEfficiency { get; set; }
        public decimal MechanicalEfficiency { get; set; }
        public decimal HydraulicEfficiency { get; set; }
        public string EfficiencyStatus { get; set; } = string.Empty;
    }

    /// <summary>
    /// Efficiency request DTO
    /// </summary>
    public class EfficiencyRequestDto
    {
        public string PumpId { get; set; } = string.Empty;
        public decimal ActualOutput { get; set; }
        public decimal TheoreticalOutput { get; set; }
    }

    /// <summary>
    /// Cavitation analysis DTO
    /// </summary>
    public class CavitationAnalysisDto
    {
        public string PumpId { get; set; } = string.Empty;
        public bool CavitationRisk { get; set; }
        public decimal NPSHAvailable { get; set; }
        public decimal NPSHRequired { get; set; }
        public decimal CavitationMargin { get; set; }
        public string RiskLevel { get; set; } = string.Empty;
    }

    /// <summary>
    /// Cavitation request DTO
    /// </summary>
    public class CavitationRequestDto
    {
        public string PumpId { get; set; } = string.Empty;
        public decimal Pressure { get; set; }
        public decimal Temperature { get; set; }
        public decimal SuctionLift { get; set; }
    }

    /// <summary>
    /// Vibration analysis DTO
    /// </summary>
    public class VibrationAnalysisDto
    {
        public string AnalysisId { get; set; } = string.Empty;
        public string PumpId { get; set; } = string.Empty;
        public decimal VibrationLevel { get; set; }
        public string FrequencyDomain { get; set; } = string.Empty;
        public bool IsAbnormal { get; set; }
        public List<string> IdentifiedAnomalies { get; set; } = new();
    }

    /// <summary>
    /// Vibration request DTO
    /// </summary>
    public class VibrationRequestDto
    {
        public string PumpId { get; set; } = string.Empty;
        public decimal MeasuredVibration { get; set; }
        public string MeasurementAxis { get; set; } = string.Empty;
    }

    /// <summary>
    /// Pressure dynamics DTO
    /// </summary>
    public class PressureDynamicsDto
    {
        public string PumpId { get; set; } = string.Empty;
        public decimal PeakPressure { get; set; }
        public decimal AveragePressure { get; set; }
        public decimal MinimumPressure { get; set; }
        public decimal PressureVariation { get; set; }
        public List<string> DynamicCharacteristics { get; set; } = new();
    }

    /// <summary>
    /// Pressure request DTO
    /// </summary>
    public class PressureRequestDto
    {
        public string PumpId { get; set; } = string.Empty;
        public decimal DischargePressure { get; set; }
        public decimal SuctionPressure { get; set; }
    }

    /// <summary>
    /// Flow characteristics DTO
    /// </summary>
    public class FlowCharacteristicsDto
    {
        public string PumpId { get; set; } = string.Empty;
        public decimal VolumetricFlowRate { get; set; }
        public decimal ActualFlowRate { get; set; }
        public decimal FlowVariation { get; set; }
        public string FlowRegime { get; set; } = string.Empty;
    }

    /// <summary>
    /// Flow request DTO
    /// </summary>
    public class FlowRequestDto
    {
        public string PumpId { get; set; } = string.Empty;
        public decimal PumpDisplacement { get; set; }
        public decimal PumpSpeed { get; set; }
    }

    #endregion

    #region Optimization DTOs

    /// <summary>
    /// Optimization result DTO
    /// </summary>
    public class OptimizationResultDto
    {
        public string PumpId { get; set; } = string.Empty;
        public List<OptimizedParameterDto> OptimizedParameters { get; set; } = new();
        public decimal ExpectedEfficiencyGain { get; set; }
        public decimal CostSavingsPerYear { get; set; }
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
        public string PumpId { get; set; } = string.Empty;
        public List<string> OptimizationObjectives { get; set; } = new();
        public List<string> ConstraintParameters { get; set; } = new();
    }

    /// <summary>
    /// Parameter adjustment DTO
    /// </summary>
    public class ParameterAdjustmentDto
    {
        public string PumpId { get; set; } = string.Empty;
        public List<AdjustmentDto> Adjustments { get; set; } = new();
        public string AdjustmentRationale { get; set; } = string.Empty;
    }

    /// <summary>
    /// Adjustment DTO
    /// </summary>
    public class AdjustmentDto
    {
        public string ParameterName { get; set; } = string.Empty;
        public decimal CurrentValue { get; set; }
        public decimal RecommendedValue { get; set; }
    }

    /// <summary>
    /// Efficiency improvement DTO
    /// </summary>
    public class EfficiencyImprovementDto
    {
        public string PumpId { get; set; } = string.Empty;
        public List<ImprovementOpportunityDto> Opportunities { get; set; } = new();
        public decimal TotalPotentialGain { get; set; }
    }

    /// <summary>
    /// Improvement opportunity DTO
    /// </summary>
    public class ImprovementOpportunityDto
    {
        public string OpportunityName { get; set; } = string.Empty;
        public decimal PotentialGain { get; set; }
        public decimal ImplementationCost { get; set; }
        public int PaybackMonths { get; set; }
    }

    /// <summary>
    /// Pump comparison DTO
    /// </summary>
    public class PumpComparisonDto
    {
        public List<ComparisonItemDto> Items { get; set; } = new();
        public string BestPerformer { get; set; } = string.Empty;
        public string ComparisonSummary { get; set; } = string.Empty;
    }

    /// <summary>
    /// Comparison item DTO
    /// </summary>
    public class ComparisonItemDto
    {
        public string PumpId { get; set; } = string.Empty;
        public decimal Efficiency { get; set; }
        public decimal Cost { get; set; }
        public decimal Reliability { get; set; }
    }

    /// <summary>
    /// Comparison criteria DTO
    /// </summary>
    public class ComparisonCriteriaDto
    {
        public List<string> CriteriaToCompare { get; set; } = new();
        public bool IncludeCostAnalysis { get; set; } = true;
    }

    /// <summary>
    /// Pump upgrade recommendation DTO
    /// </summary>
    public class PumpUpgradeRecommendationDto
    {
        public string PumpId { get; set; } = string.Empty;
        public bool UpgradeRecommended { get; set; }
        public string RecommendedUpgrade { get; set; } = string.Empty;
        public decimal UpgradeCost { get; set; }
        public decimal AnnualSavings { get; set; }
        public decimal PaybackPeriod { get; set; }
    }

    /// <summary>
    /// Upgrade request DTO
    /// </summary>
    public class UpgradeRequestDto
    {
        public string PumpId { get; set; } = string.Empty;
        public bool IncludeCostAnalysis { get; set; } = true;
    }

    #endregion

    #region Monitoring and Diagnostics DTOs

    /// <summary>
    /// Pump monitoring data DTO
    /// </summary>
    public class PumpMonitoringDataDto
    {
        public string PumpId { get; set; } = string.Empty;
        public DateTime MonitoringDate { get; set; }
        public decimal CurrentFlowRate { get; set; }
        public decimal CurrentPressure { get; set; }
        public decimal CurrentEfficiency { get; set; }
        public decimal CurrentTemperature { get; set; }
        public string OperationalStatus { get; set; } = string.Empty;
        public List<string> Alerts { get; set; } = new();
    }

    /// <summary>
    /// Monitoring request DTO
    /// </summary>
    public class MonitoringRequestDto
    {
        public string PumpId { get; set; } = string.Empty;
        public bool IncludeDetailedAnalysis { get; set; } = true;
    }

    /// <summary>
    /// Diagnostics result DTO
    /// </summary>
    public class DiagnosticsResultDto
    {
        public string DiagnosticsId { get; set; } = string.Empty;
        public string PumpId { get; set; } = string.Empty;
        public List<DiagnosticFindingDto> Findings { get; set; } = new();
        public string OverallStatus { get; set; } = string.Empty;
        public List<string> RecommendedActions { get; set; } = new();
    }

    /// <summary>
    /// Diagnostic finding DTO
    /// </summary>
    public class DiagnosticFindingDto
    {
        public string FindingType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string RecommendedAction { get; set; } = string.Empty;
    }

    /// <summary>
    /// Diagnostics request DTO
    /// </summary>
    public class DiagnosticsRequestDto
    {
        public string PumpId { get; set; } = string.Empty;
        public List<string> DiagnosticAspects { get; set; } = new();
    }

    /// <summary>
    /// Condition assessment DTO
    /// </summary>
    public class ConditionAssessmentDto
    {
        public string PumpId { get; set; } = string.Empty;
        public string OverallCondition { get; set; } = string.Empty;
        public decimal ConditionScore { get; set; }
        public List<ComponentConditionDto> Components { get; set; } = new();
        public DateTime NextInspectionDue { get; set; }
    }

    /// <summary>
    /// Component condition DTO
    /// </summary>
    public class ComponentConditionDto
    {
        public string ComponentName { get; set; } = string.Empty;
        public string Condition { get; set; } = string.Empty;
        public decimal ConditionScore { get; set; }
    }

    /// <summary>
    /// Anomaly detection DTO
    /// </summary>
    public class AnomalyDetectionDto
    {
        public string PumpId { get; set; } = string.Empty;
        public bool AnomaliesDetected { get; set; }
        public List<DetectedAnomalyDto> Anomalies { get; set; } = new();
        public DateTime AnalysisDate { get; set; }
    }

    /// <summary>
    /// Detected anomaly DTO
    /// </summary>
    public class DetectedAnomalyDto
    {
        public string AnomalyType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Severity { get; set; }
        public string RecommendedAction { get; set; } = string.Empty;
    }

    /// <summary>
    /// Predictive maintenance DTO
    /// </summary>
    public class PredictiveMaintenanceDto
    {
        public string PumpId { get; set; } = string.Empty;
        public List<MaintenancePredictionDto> Predictions { get; set; } = new();
        public DateTime NextMaintenanceDue { get; set; }
        public string MaintenanceUrgency { get; set; } = string.Empty;
    }

    /// <summary>
    /// Maintenance prediction DTO
    /// </summary>
    public class MaintenancePredictionDto
    {
        public string ComponentName { get; set; } = string.Empty;
        public DateTime PredictedFailureDate { get; set; }
        public string MaintenanceType { get; set; } = string.Empty;
        public string Urgency { get; set; } = string.Empty;
    }

    /// <summary>
    /// Maintenance request DTO
    /// </summary>
    public class MaintenanceRequestDto
    {
        public string PumpId { get; set; } = string.Empty;
        public bool IncludeComponentAnalysis { get; set; } = true;
        public int LookAheadDays { get; set; } = 90;
    }

    #endregion

    #region Failure and Reliability DTOs

    /// <summary>
    /// Failure mode analysis DTO
    /// </summary>
    public class FailureModeAnalysisDto
    {
        public string PumpId { get; set; } = string.Empty;
        public List<FailureModeDto> FailureModes { get; set; } = new();
        public string OverallRisk { get; set; } = string.Empty;
    }

    /// <summary>
    /// Failure mode DTO
    /// </summary>
    public class FailureModeDto
    {
        public string FailureType { get; set; } = string.Empty;
        public string Cause { get; set; } = string.Empty;
        public decimal Probability { get; set; }
        public string Impact { get; set; } = string.Empty;
    }

    /// <summary>
    /// Reliability assessment DTO
    /// </summary>
    public class ReliabilityAssessmentDto
    {
        public string PumpId { get; set; } = string.Empty;
        public decimal ReliabilityScore { get; set; }
        public string ReliabilityStatus { get; set; } = string.Empty;
        public decimal AvailabilityPercent { get; set; }
        public List<string> RiskFactors { get; set; } = new();
    }

    /// <summary>
    /// Reliability request DTO
    /// </summary>
    public class ReliabilityRequestDto
    {
        public string PumpId { get; set; } = string.Empty;
        public int HistoricalYears { get; set; } = 5;
    }

    /// <summary>
    /// MTBF calculation DTO
    /// </summary>
    public class MTBFCalculationDto
    {
        public string PumpId { get; set; } = string.Empty;
        public decimal CalculatedMTBF { get; set; }
        public string TimeUnit { get; set; } = "Hours";
        public int TotalFailures { get; set; }
        public int TotalOperatingHours { get; set; }
    }

    /// <summary>
    /// Wear analysis DTO
    /// </summary>
    public class WearAnalysisDto
    {
        public string PumpId { get; set; } = string.Empty;
        public List<WearItemDto> WearItems { get; set; } = new();
        public string OverallWearStatus { get; set; } = string.Empty;
        public DateTime EstimatedReplacement { get; set; }
    }

    /// <summary>
    /// Wear item DTO
    /// </summary>
    public class WearItemDto
    {
        public string ComponentName { get; set; } = string.Empty;
        public decimal WearPercent { get; set; }
        public decimal RemainingLife { get; set; }
        public string TimeUnit { get; set; } = string.Empty;
    }

    /// <summary>
    /// Wear request DTO
    /// </summary>
    public class WearRequestDto
    {
        public string PumpId { get; set; } = string.Empty;
        public bool IncludeHistoricalAnalysis { get; set; } = true;
    }

    /// <summary>
    /// Failure risk assessment DTO
    /// </summary>
    public class FailureRiskAssessmentDto
    {
        public string PumpId { get; set; } = string.Empty;
        public decimal FailureRisk { get; set; }
        public List<RiskPointDto> RiskPoints { get; set; } = new();
        public string OverallRiskLevel { get; set; } = string.Empty;
    }

    /// <summary>
    /// Risk point DTO
    /// </summary>
    public class RiskPointDto
    {
        public string ComponentName { get; set; } = string.Empty;
        public decimal RiskScore { get; set; }
        public string MitigationStrategy { get; set; } = string.Empty;
    }

    #endregion

    #region Maintenance and Service DTOs

    /// <summary>
    /// Maintenance schedule DTO
    /// </summary>
    public class MaintenanceScheduleDto
    {
        public string PumpId { get; set; } = string.Empty;
        public List<ScheduledMaintenanceDto> ScheduledItems { get; set; } = new();
        public DateTime NextMaintenanceDate { get; set; }
    }

    /// <summary>
    /// Scheduled maintenance DTO
    /// </summary>
    public class ScheduledMaintenanceDto
    {
        public string MaintenanceType { get; set; } = string.Empty;
        public DateTime ScheduledDate { get; set; }
        public decimal EstimatedDuration { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    /// <summary>
    /// Schedule request DTO
    /// </summary>
    public class ScheduleRequestDto
    {
        public string PumpId { get; set; } = string.Empty;
        public int MonthsAhead { get; set; } = 12;
    }

    /// <summary>
    /// Maintenance activity DTO
    /// </summary>
    public class MaintenanceActivityDto
    {
        public string PumpId { get; set; } = string.Empty;
        public DateTime ActivityDate { get; set; }
        public string ActivityType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal DurationHours { get; set; }
    }

    /// <summary>
    /// Rebuild analysis DTO
    /// </summary>
    public class RebuildAnalysisDto
    {
        public string PumpId { get; set; } = string.Empty;
        public bool RebuildRecommended { get; set; }
        public decimal RebuildCost { get; set; }
        public decimal ExpectedLife { get; set; }
        public List<string> RequiredComponents { get; set; } = new();
    }

    /// <summary>
    /// Rebuild request DTO
    /// </summary>
    public class RebuildRequestDto
    {
        public string PumpId { get; set; } = string.Empty;
        public bool IncludeCostAnalysis { get; set; } = true;
    }

    /// <summary>
    /// Parts inventory DTO
    /// </summary>
    public class PartsInventoryDto
    {
        public string PumpId { get; set; } = string.Empty;
        public List<PartItemDto> Parts { get; set; } = new();
        public string InventoryStatus { get; set; } = string.Empty;
    }

    /// <summary>
    /// Part item DTO
    /// </summary>
    public class PartItemDto
    {
        public string PartName { get; set; } = string.Empty;
        public int QuantityOnHand { get; set; }
        public int QuantityRequired { get; set; }
        public string PartNumber { get; set; } = string.Empty;
    }

    /// <summary>
    /// Parts request DTO
    /// </summary>
    public class PartsRequestDto
    {
        public string PumpId { get; set; } = string.Empty;
        public List<string> PartsToTrack { get; set; } = new();
    }

    /// <summary>
    /// Maintenance cost estimate DTO
    /// </summary>
    public class MaintenanceCostEstimateDto
    {
        public string PumpId { get; set; } = string.Empty;
        public decimal EstimatedLaborCost { get; set; }
        public decimal EstimatedPartsCost { get; set; }
        public decimal TotalEstimatedCost { get; set; }
        public DateTime CostEstimateDate { get; set; }
    }

    /// <summary>
    /// Cost estimate request DTO
    /// </summary>
    public class CostEstimateRequestDto
    {
        public string PumpId { get; set; } = string.Empty;
        public string MaintenanceType { get; set; } = string.Empty;
    }

    #endregion

    #region Hydraulic Fluid Management DTOs

    /// <summary>
    /// Fluid analysis DTO
    /// </summary>
    public class FluidAnalysisDto
    {
        public string PumpId { get; set; } = string.Empty;
        public string FluidType { get; set; } = string.Empty;
        public decimal Viscosity { get; set; }
        public decimal AcidNumber { get; set; }
        public decimal WaterContent { get; set; }
        public string FluidCondition { get; set; } = string.Empty;
        public List<string> Recommendations { get; set; } = new();
    }

    /// <summary>
    /// Fluid analysis request DTO
    /// </summary>
    public class FluidAnalysisRequestDto
    {
        public string PumpId { get; set; } = string.Empty;
        public bool IncludeContaminationAnalysis { get; set; } = true;
    }

    /// <summary>
    /// Fluid change recommendation DTO
    /// </summary>
    public class FluidChangeRecommendationDto
    {
        public string PumpId { get; set; } = string.Empty;
        public bool ChangeRecommended { get; set; }
        public DateTime RecommendedChangeDate { get; set; }
        public string RecommendedFluidType { get; set; } = string.Empty;
        public decimal RequiredVolume { get; set; }
    }

    /// <summary>
    /// Contamination level DTO
    /// </summary>
    public class ContaminationLevelDto
    {
        public string PumpId { get; set; } = string.Empty;
        public decimal ISO4406Rating { get; set; }
        public decimal ContaminationPercent { get; set; }
        public List<ContaminantDto> Contaminants { get; set; } = new();
        public string ContaminationStatus { get; set; } = string.Empty;
    }

    /// <summary>
    /// Contaminant DTO
    /// </summary>
    public class ContaminantDto
    {
        public string ContaminantType { get; set; } = string.Empty;
        public decimal Concentration { get; set; }
        public string Unit { get; set; } = string.Empty;
    }

    /// <summary>
    /// Contamination request DTO
    /// </summary>
    public class ContaminationRequestDto
    {
        public string PumpId { get; set; } = string.Empty;
        public bool IncludeParticleAnalysis { get; set; } = true;
    }

    /// <summary>
    /// Filtration system DTO
    /// </summary>
    public class FiltrationSystemDto
    {
        public string PumpId { get; set; } = string.Empty;
        public List<FilterStatusDto> Filters { get; set; } = new();
        public string FiltrationStatus { get; set; } = string.Empty;
        public DateTime NextFilterChangeDate { get; set; }
    }

    /// <summary>
    /// Filter status DTO
    /// </summary>
    public class FilterStatusDto
    {
        public string FilterName { get; set; } = string.Empty;
        public decimal BetaRating { get; set; }
        public decimal CloggingPercent { get; set; }
        public bool ChangeRequired { get; set; }
    }

    /// <summary>
    /// Filtration request DTO
    /// </summary>
    public class FiltrationRequestDto
    {
        public string PumpId { get; set; } = string.Empty;
        public List<string> FilterTypesToManage { get; set; } = new();
    }

    #endregion

    #region Integration and Control DTOs

    /// <summary>
    /// SCADA integration DTO
    /// </summary>
    public class SCODAIntegrationDto
    {
        public string PumpId { get; set; } = string.Empty;
        public bool IsIntegrated { get; set; }
        public List<IntegrationParameterDto> Parameters { get; set; } = new();
        public string IntegrationStatus { get; set; } = string.Empty;
    }

    /// <summary>
    /// Integration parameter DTO
    /// </summary>
    public class IntegrationParameterDto
    {
        public string ParameterName { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public string UpdateFrequency { get; set; } = string.Empty;
    }

    /// <summary>
    /// SCADA configuration DTO
    /// </summary>
    public class SCADAConfigDto
    {
        public string PumpId { get; set; } = string.Empty;
        public string SCODAServer { get; set; } = string.Empty;
        public List<string> ParametersToExport { get; set; } = new();
    }

    /// <summary>
    /// Control parameters DTO
    /// </summary>
    public class ControlParametersDto
    {
        public string PumpId { get; set; } = string.Empty;
        public List<ControlParamDto> Parameters { get; set; } = new();
        public string ControlMode { get; set; } = string.Empty;
    }

    /// <summary>
    /// Control parameter DTO
    /// </summary>
    public class ControlParamDto
    {
        public string ParameterName { get; set; } = string.Empty;
        public decimal CurrentValue { get; set; }
        public decimal MinValue { get; set; }
        public decimal MaxValue { get; set; }
    }

    /// <summary>
    /// Control request DTO
    /// </summary>
    public class ControlRequestDto
    {
        public string PumpId { get; set; } = string.Empty;
        public Dictionary<string, decimal> ParameterUpdates { get; set; } = new();
    }

    /// <summary>
    /// Wellbore interaction DTO
    /// </summary>
    public class WellboreInteractionDto
    {
        public string WellUWI { get; set; } = string.Empty;
        public decimal InteractionIntensity { get; set; }
        public List<InteractionFactorDto> Factors { get; set; } = new();
        public string OverallAssessment { get; set; } = string.Empty;
    }

    /// <summary>
    /// Interaction factor DTO
    /// </summary>
    public class InteractionFactorDto
    {
        public string FactorName { get; set; } = string.Empty;
        public decimal Impact { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// Interaction request DTO
    /// </summary>
    public class InteractionRequestDto
    {
        public string WellUWI { get; set; } = string.Empty;
        public bool IncludeDetailedAnalysis { get; set; } = true;
    }

    #endregion

    #region Data Management and Reporting DTOs

    /// <summary>
    /// Pump history DTO
    /// </summary>
    public class PumpHistoryDto
    {
        public DateTime EventDate { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string EventDescription { get; set; } = string.Empty;
        public string PerformedBy { get; set; } = string.Empty;
    }

    /// <summary>
    /// Performance trends DTO
    /// </summary>
    public class PerformanceTrendsDto
    {
        public string PumpId { get; set; } = string.Empty;
        public List<TrendPointDto> TrendData { get; set; } = new();
        public string OverallTrend { get; set; } = string.Empty;
    }

    /// <summary>
    /// Trend point DTO
    /// </summary>
    public class TrendPointDto
    {
        public DateTime MeasurementDate { get; set; }
        public decimal Efficiency { get; set; }
        public decimal FlowRate { get; set; }
        public decimal Pressure { get; set; }
    }

    /// <summary>
    /// Pump report DTO
    /// </summary>
    public class PumpReportDto
    {
        public string ReportId { get; set; } = string.Empty;
        public string PumpId { get; set; } = string.Empty;
        public DateTime GeneratedDate { get; set; }
        public byte[] ReportContent { get; set; } = Array.Empty<byte>();
        public string ExecutiveSummary { get; set; } = string.Empty;
        public List<string> KeyFindings { get; set; } = new();
    }

    /// <summary>
    /// Report request DTO
    /// </summary>
    public class ReportRequestDto
    {
        public string PumpId { get; set; } = string.Empty;
        public string ReportType { get; set; } = "Comprehensive";
        public List<string> IncludeSections { get; set; } = new();
        public string Format { get; set; } = "PDF";
    }

    /// <summary>
    /// Performance summary report DTO
    /// </summary>
    public class PerformanceSummaryReportDto
    {
        public string ReportId { get; set; } = string.Empty;
        public string PumpId { get; set; } = string.Empty;
        public decimal AverageEfficiency { get; set; }
        public decimal UptimePercent { get; set; }
        public int TotalRunningHours { get; set; }
        public string OverallPerformanceStatus { get; set; } = string.Empty;
    }

    /// <summary>
    /// Summary report request DTO
    /// </summary>
    public class SummaryReportRequestDto
    {
        public string PumpId { get; set; } = string.Empty;
        public int ReportPeriodMonths { get; set; } = 12;
    }

    /// <summary>
    /// Cost analysis report DTO
    /// </summary>
    public class CostAnalysisReportDto
    {
        public string ReportId { get; set; } = string.Empty;
        public string PumpId { get; set; } = string.Empty;
        public decimal TotalMaintenanceCost { get; set; }
        public decimal AnnualOperatingCost { get; set; }
        public decimal LifecycleCost { get; set; }
        public string CostTrend { get; set; } = string.Empty;
    }

    /// <summary>
    /// Cost report request DTO
    /// </summary>
    public class CostReportRequestDto
    {
        public string PumpId { get; set; } = string.Empty;
        public int AnalysisPeriodYears { get; set; } = 5;
    }

    #endregion
}

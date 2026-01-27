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
        Task<PumpDesignResult> DesignPumpSystemAsync(string wellUWI, PumpDesignRequest request, string userId);

        /// <summary>
        /// Performs pump sizing calculations
        /// </summary>
        Task<PumpSizingResult> SizePumpAsync(string wellUWI, PumpSizingRequest request);

        /// <summary>
        /// Selects optimal pump type based on conditions
        /// </summary>
        Task<PumpTypeSelection> SelectOptimalPumpTypeAsync(string wellUWI, PumpSelectionCriteria criteria);

        /// <summary>
        /// Calculates power requirements for pump
        /// </summary>
        Task<PowerRequirements> CalculatePowerRequirementsAsync(string wellUWI, PowerCalculationRequest request);

        /// <summary>
        /// Analyzes pump hydraulic balance
        /// </summary>
        Task<HydraulicBalance> AnalyzeHydraulicBalanceAsync(string pumpId, BalanceRequest request);

        /// <summary>
        /// Performs pump rod string design
        /// </summary>
        Task<RodStringDesign> DesignRodStringAsync(string wellUWI, RodStringRequest request);

        #endregion

        #region Performance Analysis

        /// <summary>
        /// Analyzes pump performance
        /// </summary>
        Task<PumpPerformanceAnalysis> AnalyzePumpPerformanceAsync(string pumpId, PerformanceAnalysisRequest request);

        /// <summary>
        /// Calculates pump efficiency
        /// </summary>
        Task<PumpEfficiency> CalculatePumpEfficiencyAsync(string pumpId, EfficiencyRequest request);

        /// <summary>
        /// Analyzes pump cavitation risk
        /// </summary>
        Task<CavitationAnalysis> AnalyzeCavitationRiskAsync(string pumpId, CavitationRequest request);

        /// <summary>
        /// Performs vibration analysis
        /// </summary>
        Task<VibrationAnalysis> AnalyzeVibrationAsync(string pumpId, VibrationRequest request);

        /// <summary>
        /// Analyzes pressure dynamics
        /// </summary>
        Task<PressureDynamics> AnalyzePressureDynamicsAsync(string pumpId, PressureRequest request);

        /// <summary>
        /// Calculates flow rate characteristics
        /// </summary>
        Task<FlowCharacteristics> CalculateFlowCharacteristicsAsync(string pumpId, FlowRequest request);

        #endregion

        #region Optimization

        /// <summary>
        /// Optimizes pump performance parameters
        /// </summary>
        Task<OptimizationResult> OptimizePumpParametersAsync(string pumpId, OptimizationRequest request);

        /// <summary>
        /// Recommends pump parameter adjustments
        /// </summary>
        Task<ParameterAdjustment> RecommendParameterAdjustmentsAsync(string pumpId);

        /// <summary>
        /// Analyzes pump efficiency improvement opportunities
        /// </summary>
        Task<EfficiencyImprovement> IdentifyEfficiencyImprovementsAsync(string pumpId);

        /// <summary>
        /// Performs comparative pump analysis
        /// </summary>
        Task<PumpComparison> ComparePumpsAsync(List<string> pumpIds, ComparisonCriteria criteria);

        /// <summary>
        /// Recommends pump upgrade or replacement
        /// </summary>
        Task<PumpUpgradeRecommendation> RecommendPumpUpgradeAsync(string pumpId, UpgradeRequest request);

        #endregion

        #region Monitoring and Diagnostics

        /// <summary>
        /// Monitors pump performance in real-time
        /// </summary>
        Task<PumpMonitoringData> MonitorPumpPerformanceAsync(string pumpId, MonitoringRequest request);

        /// <summary>
        /// Performs pump diagnostics
        /// </summary>
        Task<DiagnosticsResult> PerformPumpDiagnosticsAsync(string pumpId, DiagnosticsRequest request);

        /// <summary>
        /// Analyzes pump condition
        /// </summary>
        Task<ConditionAssessment> AssessPumpConditionAsync(string pumpId);

        /// <summary>
        /// Detects pump operational anomalies
        /// </summary>
        Task<AnomalyDetection> DetectOperationalAnomaliesAsync(string pumpId);

        /// <summary>
        /// Performs predictive maintenance analysis
        /// </summary>
        Task<PredictiveMaintenance> AnalyzeMaintenanceRequirementsAsync(string pumpId, MaintenanceRequest request);

        #endregion

        #region Failure and Reliability Analysis

        /// <summary>
        /// Analyzes pump failure modes
        /// </summary>
        Task<FailureModeAnalysis> AnalyzeFailureModesAsync(string pumpId);

        /// <summary>
        /// Assesses pump reliability
        /// </summary>
        Task<ReliabilityAssessment> AssessReliabilityAsync(string pumpId, ReliabilityRequest request);

        /// <summary>
        /// Calculates mean time between failures (MTBF)
        /// </summary>
        Task<MTBFCalculation> CalculateMTBFAsync(string pumpId);

        /// <summary>
        /// Analyzes pump wear patterns
        /// </summary>
        Task<WearAnalysis> AnalyzeWearPatternsAsync(string pumpId, WearRequest request);

        /// <summary>
        /// Identifies potential failure points
        /// </summary>
        Task<FailureRiskAssessment> AssessFailureRiskAsync(string pumpId);

        #endregion

        #region Maintenance and Service

        /// <summary>
        /// Manages pump maintenance schedule
        /// </summary>
        Task<MaintenanceSchedule> GenerateMaintenanceScheduleAsync(string pumpId, ScheduleRequest request);

        /// <summary>
        /// Logs pump maintenance activities
        /// </summary>
        Task LogMaintenanceActivityAsync(string pumpId, MaintenanceActivity activity, string userId);

        /// <summary>
        /// Manages pump rebuild process
        /// </summary>
        Task<RebuildAnalysis> AnalyzeRebuildRequirementsAsync(string pumpId, RebuildRequest request);

        /// <summary>
        /// Tracks pump spare parts inventory
        /// </summary>
        Task<PartsInventory> ManagePartsInventoryAsync(string pumpId, PartsRequest request);

        /// <summary>
        /// Estimates maintenance costs
        /// </summary>
        Task<MaintenanceCostEstimate> EstimateCostsAsync(string pumpId, CostEstimateRequest request);

        #endregion

        #region Hydraulic Fluid Management

        /// <summary>
        /// Analyzes hydraulic fluid condition
        /// </summary>
        Task<FluidAnalysis> AnalyzeHydraulicFluidAsync(string pumpId, FluidAnalysisRequest request);

        /// <summary>
        /// Recommends fluid changes
        /// </summary>
        Task<FluidChangeRecommendation> RecommendFluidChangeAsync(string pumpId);

        /// <summary>
        /// Tracks fluid contamination levels
        /// </summary>
        Task<ContaminationLevel> TrackFluidContaminationAsync(string pumpId, ContaminationRequest request);

        /// <summary>
        /// Manages fluid filtration system
        /// </summary>
        Task<FiltrationSystem> ManageFiltrationSystemAsync(string pumpId, FiltrationRequest request);

        #endregion

        #region Integration and Control Systems

        /// <summary>
        /// Integrates pump with SCADA systems
        /// </summary>
        Task<SCODAIntegration> IntegrateSCADAAsync(string pumpId, SCADAConfig config);

        /// <summary>
        /// Manages pump control parameters
        /// </summary>
        Task<ControlParameters> ManageControlParametersAsync(string pumpId, ControlRequest request, string userId);

        /// <summary>
        /// Analyzes pump-wellbore interaction
        /// </summary>
        Task<WellboreInteraction> AnalyzePumpWellboreInteractionAsync(string wellUWI, InteractionRequest request);

        #endregion

        #region Data Management

        /// <summary>
        /// Saves pump design data
        /// </summary>
        Task SavePumpDesignAsync(PumpDesignResult design, string userId);

        /// <summary>
        /// Retrieves pump design data
        /// </summary>
        Task<PumpDesignResult?> GetPumpDesignAsync(string pumpId);

        /// <summary>
        /// Updates pump design data
        /// </summary>
        Task UpdatePumpDesignAsync(PumpDesignResult design, string userId);

        /// <summary>
        /// Retrieves pump operational history
        /// </summary>
        Task<List<PumpHistory>> GetPumpHistoryAsync(string pumpId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Retrieves pump performance trends
        /// </summary>
        Task<PerformanceTrends> GetPerformanceTrendsAsync(string pumpId, int monthsBack = 12);

        #endregion

        #region Reporting and Analysis

        /// <summary>
        /// Generates comprehensive pump report
        /// </summary>
        Task<PumpReport> GeneratePumpReportAsync(string pumpId, ReportRequest request);

        /// <summary>
        /// Generates performance summary report
        /// </summary>
        Task<PerformanceSummaryReport> GeneratePerformanceSummaryAsync(string pumpId, SummaryReportRequest request);

        /// <summary>
        /// Generates cost analysis report
        /// </summary>
        Task<CostAnalysisReport> GenerateCostAnalysisAsync(string pumpId, CostReportRequest request);

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
    public class PumpDesignResult
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
    public class PumpDesignRequest
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
    public class PumpSizingResult
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
    public class PumpSizingRequest
    {
        public string WellUWI { get; set; } = string.Empty;
        public decimal DesiredFlowRate { get; set; }
        public decimal MaxOperatingPressure { get; set; }
        public decimal FluidViscosity { get; set; }
    }

    /// <summary>
    /// Pump type selection DTO
    /// </summary>
    public class PumpTypeSelection
    {
        public string WellUWI { get; set; } = string.Empty;
        public List<PumpTypeOption> Options { get; set; } = new();
        public string RecommendedType { get; set; } = string.Empty;
        public string SelectionRationale { get; set; } = string.Empty;
    }

    /// <summary>
    /// Pump type option DTO
    /// </summary>
    public class PumpTypeOption
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
    public class PumpSelectionCriteria
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
    public class PowerRequirements
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
    public class PowerCalculationRequest
    {
        public string WellUWI { get; set; } = string.Empty;
        public decimal FlowRate { get; set; }
        public decimal DischargePressure { get; set; }
        public decimal SuctionPressure { get; set; }
    }

    /// <summary>
    /// Hydraulic balance DTO
    /// </summary>
    public class HydraulicBalance
    {
        public string PumpId { get; set; } = string.Empty;
        public bool IsBalanced { get; set; }
        public decimal BalanceScore { get; set; }
        public List<BalanceFactor> Factors { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
    }

    /// <summary>
    /// Balance factor DTO
    /// </summary>
    public class BalanceFactor
    {
        public string FactorName { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>
    /// Balance request DTO
    /// </summary>
    public class BalanceRequest
    {
        public string PumpId { get; set; } = string.Empty;
        public decimal DischargeFlow { get; set; }
        public decimal DischargePressure { get; set; }
    }

    /// <summary>
    /// Rod string design DTO
    /// </summary>
    public class RodStringDesign
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
    public class RodStringRequest
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
    public class PumpPerformanceAnalysis
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
    /// Pump efficiency DTO
    /// </summary>
    public class PumpEfficiency
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
    public class EfficiencyRequest
    {
        public string PumpId { get; set; } = string.Empty;
        public decimal ActualOutput { get; set; }
        public decimal TheoreticalOutput { get; set; }
    }

    /// <summary>
    /// Cavitation analysis DTO
    /// </summary>
    public class CavitationAnalysis
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
    public class CavitationRequest
    {
        public string PumpId { get; set; } = string.Empty;
        public decimal Pressure { get; set; }
        public decimal Temperature { get; set; }
        public decimal SuctionLift { get; set; }
    }

    /// <summary>
    /// Vibration analysis DTO
    /// </summary>
    public class VibrationAnalysis
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
    public class VibrationRequest
    {
        public string PumpId { get; set; } = string.Empty;
        public decimal MeasuredVibration { get; set; }
        public string MeasurementAxis { get; set; } = string.Empty;
    }

    /// <summary>
    /// Pressure dynamics DTO
    /// </summary>
    public class PressureDynamics
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
    public class PressureRequest
    {
        public string PumpId { get; set; } = string.Empty;
        public decimal DischargePressure { get; set; }
        public decimal SuctionPressure { get; set; }
    }

    /// <summary>
    /// Flow characteristics DTO
    /// </summary>
    public class FlowCharacteristics
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
    public class FlowRequest
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
    public class OptimizationResult
    {
        public string PumpId { get; set; } = string.Empty;
        public List<OptimizedParameter> OptimizedParameters { get; set; } = new();
        public decimal ExpectedEfficiencyGain { get; set; }
        public decimal CostSavingsPerYear { get; set; }
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
        public string PumpId { get; set; } = string.Empty;
        public List<string> OptimizationObjectives { get; set; } = new();
        public List<string> ConstraintParameters { get; set; } = new();
    }

    /// <summary>
    /// Parameter adjustment DTO
    /// </summary>
    public class ParameterAdjustment
    {
        public string PumpId { get; set; } = string.Empty;
        public List<Adjustment> Adjustments { get; set; } = new();
        public string AdjustmentRationale { get; set; } = string.Empty;
    }

    /// <summary>
    /// Adjustment DTO
    /// </summary>
    public class Adjustment
    {
        public string ParameterName { get; set; } = string.Empty;
        public decimal CurrentValue { get; set; }
        public decimal RecommendedValue { get; set; }
    }

    /// <summary>
    /// Efficiency improvement DTO
    /// </summary>
    public class EfficiencyImprovement
    {
        public string PumpId { get; set; } = string.Empty;
        public List<ImprovementOpportunity> Opportunities { get; set; } = new();
        public decimal TotalPotentialGain { get; set; }
    }

    /// <summary>
    /// Improvement opportunity DTO
    /// </summary>
    public class ImprovementOpportunity
    {
        public string OpportunityName { get; set; } = string.Empty;
        public decimal PotentialGain { get; set; }
        public decimal ImplementationCost { get; set; }
        public int PaybackMonths { get; set; }
    }

    /// <summary>
    /// Pump comparison DTO
    /// </summary>
    public class PumpComparison
    {
        public List<ComparisonItem> Items { get; set; } = new();
        public string BestPerformer { get; set; } = string.Empty;
        public string ComparisonSummary { get; set; } = string.Empty;
    }

    /// <summary>
    /// Comparison item DTO
    /// </summary>
    public class ComparisonItem
    {
        public string PumpId { get; set; } = string.Empty;
        public decimal Efficiency { get; set; }
        public decimal Cost { get; set; }
        public decimal Reliability { get; set; }
    }

    /// <summary>
    /// Comparison criteria DTO
    /// </summary>
    public class ComparisonCriteria
    {
        public List<string> CriteriaToCompare { get; set; } = new();
        public bool IncludeCostAnalysis { get; set; } = true;
    }

    /// <summary>
    /// Pump upgrade recommendation DTO
    /// </summary>
    public class PumpUpgradeRecommendation
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
    public class UpgradeRequest
    {
        public string PumpId { get; set; } = string.Empty;
        public bool IncludeCostAnalysis { get; set; } = true;
    }

    #endregion

    #region Monitoring and Diagnostics DTOs

    /// <summary>
    /// Pump monitoring data DTO
    /// </summary>
    public class PumpMonitoringData
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
    public class MonitoringRequest
    {
        public string PumpId { get; set; } = string.Empty;
        public bool IncludeDetailedAnalysis { get; set; } = true;
    }

    /// <summary>
    /// Diagnostics result DTO
    /// </summary>
    public class DiagnosticsResult
    {
        public string DiagnosticsId { get; set; } = string.Empty;
        public string PumpId { get; set; } = string.Empty;
        public List<DiagnosticFinding> Findings { get; set; } = new();
        public string OverallStatus { get; set; } = string.Empty;
        public List<string> RecommendedActions { get; set; } = new();
    }

    /// <summary>
    /// Diagnostic finding DTO
    /// </summary>
    public class DiagnosticFinding
    {
        public string FindingType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string RecommendedAction { get; set; } = string.Empty;
    }

    /// <summary>
    /// Diagnostics request DTO
    /// </summary>
    public class DiagnosticsRequest
    {
        public string PumpId { get; set; } = string.Empty;
        public List<string> DiagnosticAspects { get; set; } = new();
    }

    /// <summary>
    /// Condition assessment DTO
    /// </summary>
    public class ConditionAssessment
    {
        public string PumpId { get; set; } = string.Empty;
        public string OverallCondition { get; set; } = string.Empty;
        public decimal ConditionScore { get; set; }
        public List<ComponentCondition> Components { get; set; } = new();
        public DateTime NextInspectionDue { get; set; }
    }

    /// <summary>
    /// Component condition DTO
    /// </summary>
    public class ComponentCondition
    {
        public string ComponentName { get; set; } = string.Empty;
        public string Condition { get; set; } = string.Empty;
        public decimal ConditionScore { get; set; }
    }

    /// <summary>
    /// Anomaly detection DTO
    /// </summary>
    public class AnomalyDetection
    {
        public string PumpId { get; set; } = string.Empty;
        public bool AnomaliesDetected { get; set; }
        public List<DetectedAnomaly> Anomalies { get; set; } = new();
        public DateTime AnalysisDate { get; set; }
    }

    /// <summary>
    /// Detected anomaly DTO
    /// </summary>
    public class DetectedAnomaly
    {
        public string AnomalyType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Severity { get; set; }
        public string RecommendedAction { get; set; } = string.Empty;
    }

    /// <summary>
    /// Predictive maintenance DTO
    /// </summary>
    public class PredictiveMaintenance
    {
        public string PumpId { get; set; } = string.Empty;
        public List<MaintenancePrediction> Predictions { get; set; } = new();
        public DateTime NextMaintenanceDue { get; set; }
        public string MaintenanceUrgency { get; set; } = string.Empty;
    }

    /// <summary>
    /// Maintenance prediction DTO
    /// </summary>
    public class MaintenancePrediction
    {
        public string ComponentName { get; set; } = string.Empty;
        public DateTime PredictedFailureDate { get; set; }
        public string MaintenanceType { get; set; } = string.Empty;
        public string Urgency { get; set; } = string.Empty;
    }

    /// <summary>
    /// Maintenance request DTO
    /// </summary>
    public class MaintenanceRequest
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
    public class FailureModeAnalysis
    {
        public string PumpId { get; set; } = string.Empty;
        public List<FailureMode> FailureModes { get; set; } = new();
        public string OverallRisk { get; set; } = string.Empty;
    }

    /// <summary>
    /// Failure mode DTO
    /// </summary>
    public class FailureMode
    {
        public string FailureType { get; set; } = string.Empty;
        public string Cause { get; set; } = string.Empty;
        public decimal Probability { get; set; }
        public string Impact { get; set; } = string.Empty;
    }

    /// <summary>
    /// Reliability assessment DTO
    /// </summary>
    public class ReliabilityAssessment
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
    public class ReliabilityRequest
    {
        public string PumpId { get; set; } = string.Empty;
        public int HistoricalYears { get; set; } = 5;
    }

    /// <summary>
    /// MTBF calculation DTO
    /// </summary>
    public class MTBFCalculation
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
    public class WearAnalysis
    {
        public string PumpId { get; set; } = string.Empty;
        public List<WearItem> WearItems { get; set; } = new();
        public string OverallWearStatus { get; set; } = string.Empty;
        public DateTime EstimatedReplacement { get; set; }
    }

    /// <summary>
    /// Wear item DTO
    /// </summary>
    public class WearItem
    {
        public string ComponentName { get; set; } = string.Empty;
        public decimal WearPercent { get; set; }
        public decimal RemainingLife { get; set; }
        public string TimeUnit { get; set; } = string.Empty;
    }

    /// <summary>
    /// Wear request DTO
    /// </summary>
    public class WearRequest
    {
        public string PumpId { get; set; } = string.Empty;
        public bool IncludeHistoricalAnalysis { get; set; } = true;
    }

    /// <summary>
    /// Failure risk assessment DTO
    /// </summary>
    public class FailureRiskAssessment
    {
        public string PumpId { get; set; } = string.Empty;
        public decimal FailureRisk { get; set; }
        public List<RiskPoint> RiskPoints { get; set; } = new();
        public string OverallRiskLevel { get; set; } = string.Empty;
    }

    /// <summary>
    /// Risk point DTO
    /// </summary>
    public class RiskPoint
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
    public class MaintenanceSchedule
    {
        public string PumpId { get; set; } = string.Empty;
        public List<ScheduledMaintenance> ScheduledItems { get; set; } = new();
        public DateTime NextMaintenanceDate { get; set; }
    }

    /// <summary>
    /// Scheduled maintenance DTO
    /// </summary>
    public class ScheduledMaintenance
    {
        public string MaintenanceType { get; set; } = string.Empty;
        public DateTime ScheduledDate { get; set; }
        public decimal EstimatedDuration { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    /// <summary>
    /// Schedule request DTO
    /// </summary>
    public class ScheduleRequest
    {
        public string PumpId { get; set; } = string.Empty;
        public int MonthsAhead { get; set; } = 12;
    }

    /// <summary>
    /// Maintenance activity DTO
    /// </summary>
    public class MaintenanceActivity
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
    public class RebuildAnalysis
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
    public class RebuildRequest
    {
        public string PumpId { get; set; } = string.Empty;
        public bool IncludeCostAnalysis { get; set; } = true;
    }

    /// <summary>
    /// Parts inventory DTO
    /// </summary>
    public class PartsInventory
    {
        public string PumpId { get; set; } = string.Empty;
        public List<PartItem> Parts { get; set; } = new();
        public string InventoryStatus { get; set; } = string.Empty;
    }

    /// <summary>
    /// Part item DTO
    /// </summary>
    public class PartItem
    {
        public string PartName { get; set; } = string.Empty;
        public int QuantityOnHand { get; set; }
        public int QuantityRequired { get; set; }
        public string PartNumber { get; set; } = string.Empty;
    }

    /// <summary>
    /// Parts request DTO
    /// </summary>
    public class PartsRequest
    {
        public string PumpId { get; set; } = string.Empty;
        public List<string> PartsToTrack { get; set; } = new();
    }

    /// <summary>
    /// Maintenance cost estimate DTO
    /// </summary>
    public class MaintenanceCostEstimate
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
    public class CostEstimateRequest
    {
        public string PumpId { get; set; } = string.Empty;
        public string MaintenanceType { get; set; } = string.Empty;
    }

    #endregion

    #region Hydraulic Fluid Management DTOs

    /// <summary>
    /// Fluid analysis DTO
    /// </summary>
    public class FluidAnalysis
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
    public class FluidAnalysisRequest
    {
        public string PumpId { get; set; } = string.Empty;
        public bool IncludeContaminationAnalysis { get; set; } = true;
    }

    /// <summary>
    /// Fluid change recommendation DTO
    /// </summary>
    public class FluidChangeRecommendation
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
    public class ContaminationLevel
    {
        public string PumpId { get; set; } = string.Empty;
        public decimal ISO4406Rating { get; set; }
        public decimal ContaminationPercent { get; set; }
        public List<Contaminant> Contaminants { get; set; } = new();
        public string ContaminationStatus { get; set; } = string.Empty;
    }

    /// <summary>
    /// Contaminant DTO
    /// </summary>
    public class Contaminant
    {
        public string ContaminantType { get; set; } = string.Empty;
        public decimal Concentration { get; set; }
        public string Unit { get; set; } = string.Empty;
    }

    /// <summary>
    /// Contamination request DTO
    /// </summary>
    public class ContaminationRequest
    {
        public string PumpId { get; set; } = string.Empty;
        public bool IncludeParticleAnalysis { get; set; } = true;
    }

    /// <summary>
    /// Filtration system DTO
    /// </summary>
    public class FiltrationSystem
    {
        public string PumpId { get; set; } = string.Empty;
        public List<FilterStatus> Filters { get; set; } = new();
        public string FiltrationStatus { get; set; } = string.Empty;
        public DateTime NextFilterChangeDate { get; set; }
    }

    /// <summary>
    /// Filter status DTO
    /// </summary>
    public class FilterStatus
    {
        public string FilterName { get; set; } = string.Empty;
        public decimal BetaRating { get; set; }
        public decimal CloggingPercent { get; set; }
        public bool ChangeRequired { get; set; }
    }

    /// <summary>
    /// Filtration request DTO
    /// </summary>
    public class FiltrationRequest
    {
        public string PumpId { get; set; } = string.Empty;
        public List<string> FilterTypesToManage { get; set; } = new();
    }

    #endregion

    #region Integration and Control DTOs

    /// <summary>
    /// SCADA integration DTO
    /// </summary>
    public class SCODAIntegration
    {
        public string PumpId { get; set; } = string.Empty;
        public bool IsIntegrated { get; set; }
        public List<IntegrationParameter> Parameters { get; set; } = new();
        public string IntegrationStatus { get; set; } = string.Empty;
    }

    /// <summary>
    /// Integration parameter DTO
    /// </summary>
    public class IntegrationParameter
    {
        public string ParameterName { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public string UpdateFrequency { get; set; } = string.Empty;
    }

    /// <summary>
    /// SCADA configuration DTO
    /// </summary>
    public class SCADAConfig
    {
        public string PumpId { get; set; } = string.Empty;
        public string SCODAServer { get; set; } = string.Empty;
        public List<string> ParametersToExport { get; set; } = new();
    }

    /// <summary>
    /// Control parameters DTO
    /// </summary>
    public class ControlParameters
    {
        public string PumpId { get; set; } = string.Empty;
        public List<ControlParam> Parameters { get; set; } = new();
        public string ControlMode { get; set; } = string.Empty;
    }

    /// <summary>
    /// Control parameter DTO
    /// </summary>
    public class ControlParam
    {
        public string ParameterName { get; set; } = string.Empty;
        public decimal CurrentValue { get; set; }
        public decimal MinValue { get; set; }
        public decimal MaxValue { get; set; }
    }

    /// <summary>
    /// Control request DTO
    /// </summary>
    public class ControlRequest
    {
        public string PumpId { get; set; } = string.Empty;
        public Dictionary<string, decimal> ParameterUpdates { get; set; } = new();
    }

    /// <summary>
    /// Wellbore interaction DTO
    /// </summary>
    public class WellboreInteraction
    {
        public string WellUWI { get; set; } = string.Empty;
        public decimal InteractionIntensity { get; set; }
        public List<InteractionFactor> Factors { get; set; } = new();
        public string OverallAssessment { get; set; } = string.Empty;
    }

    /// <summary>
    /// Interaction factor DTO
    /// </summary>
    public class InteractionFactor
    {
        public string FactorName { get; set; } = string.Empty;
        public decimal Impact { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// Interaction request DTO
    /// </summary>
    public class InteractionRequest
    {
        public string WellUWI { get; set; } = string.Empty;
        public bool IncludeDetailedAnalysis { get; set; } = true;
    }

    #endregion

    #region Data Management and Reporting DTOs

    /// <summary>
    /// Pump history DTO
    /// </summary>
    public class PumpHistory
    {
        public DateTime EventDate { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string EventDescription { get; set; } = string.Empty;
        public string PerformedBy { get; set; } = string.Empty;
    }

    /// <summary>
    /// Performance trends DTO
    /// </summary>
    public class PerformanceTrends
    {
        public string PumpId { get; set; } = string.Empty;
        public List<TrendPoint> TrendData { get; set; } = new();
        public string OverallTrend { get; set; } = string.Empty;
    }

    /// <summary>
    /// Trend point DTO
    /// </summary>
    public class TrendPoint
    {
        public DateTime MeasurementDate { get; set; }
        public decimal Efficiency { get; set; }
        public decimal FlowRate { get; set; }
        public decimal Pressure { get; set; }
    }

    /// <summary>
    /// Pump report DTO
    /// </summary>
    public class PumpReport
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
    public class ReportRequest
    {
        public string PumpId { get; set; } = string.Empty;
        public string ReportType { get; set; } = "Comprehensive";
        public List<string> IncludeSections { get; set; } = new();
        public string Format { get; set; } = "PDF";
    }

    /// <summary>
    /// Performance summary report DTO
    /// </summary>
    public class PerformanceSummaryReport
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
    public class SummaryReportRequest
    {
        public string PumpId { get; set; } = string.Empty;
        public int ReportPeriodMonths { get; set; } = 12;
    }

    /// <summary>
    /// Cost analysis report DTO
    /// </summary>
    public class CostAnalysisReport
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
    public class CostReportRequest
    {
        public string PumpId { get; set; } = string.Empty;
        public int AnalysisPeriodYears { get; set; } = 5;
    }

    #endregion
}

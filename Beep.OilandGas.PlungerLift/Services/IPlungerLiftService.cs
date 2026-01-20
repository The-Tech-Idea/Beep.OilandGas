using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Beep.OilandGas.PlungerLift.Services
{
    /// <summary>
    /// Comprehensive plunger lift service interface
    /// Provides design, optimization, performance analysis, and monitoring capabilities
    /// </summary>
    public interface IPlungerLiftService
    {
        #region System Design

        /// <summary>
        /// Designs a plunger lift system for a well
        /// </summary>
        Task<PlungerLiftDesign> DesignPlungerLiftSystemAsync(string wellUWI, PlungerLiftWellProperties wellProperties);

        /// <summary>
        /// Optimizes plunger lift design parameters
        /// </summary>
        Task<PlungerLiftDesign> OptimizeDesignAsync(string wellUWI, PlungerLiftOptimizationRequest request);

        /// <summary>
        /// Selects optimal plunger type based on well conditions
        /// </summary>
        Task<PlungerTypeSelection> SelectPlungerTypeAsync(string wellUWI, PlungerSelectionCriteria criteria);

        /// <summary>
        /// Calculates optimal cycle time for production
        /// </summary>
        Task<CycleTimeCalculation> CalculateCycleTimeAsync(string wellUWI, CycleTimeRequest request);

        #endregion

        #region Performance Analysis

        /// <summary>
        /// Analyzes plunger lift system performance
        /// </summary>
        Task<PlungerLiftPerformance> AnalyzePerformanceAsync(string wellUWI);

        /// <summary>
        /// Calculates expected production rate
        /// </summary>
        Task<ProductionRate> CalculateProductionRateAsync(string wellUWI, ProductionRateRequest request);

        /// <summary>
        /// Performs efficiency analysis
        /// </summary>
        Task<EfficiencyAnalysis> AnalyzeEfficiencyAsync(string wellUWI, EfficiencyAnalysisRequest request);

        /// <summary>
        /// Calculates system energy requirements
        /// </summary>
        Task<EnergyRequirements> CalculateEnergyRequirementsAsync(string wellUWI, EnergyRequest request);

        #endregion

        #region Valve and Equipment Analysis

        /// <summary>
        /// Analyzes plunger valve performance
        /// </summary>
        Task<ValvePerformance> AnalyzeValvePerformanceAsync(string wellUWI, ValveAnalysisRequest request);

        /// <summary>
        /// Calculates valve sizing requirements
        /// </summary>
        Task<ValveSizing> CalculateValveSizingAsync(string wellUWI, ValveSizingRequest request);

        /// <summary>
        /// Performs tubing analysis for plunger lift
        /// </summary>
        Task<TubingAnalysis> AnalyzeTubingAsync(string wellUWI, TubingAnalysisRequest request);

        /// <summary>
        /// Analyzes casing performance for plunger lift operations
        /// </summary>
        Task<CasingAnalysis> AnalyzeCasingAsync(string wellUWI, CasingAnalysisRequest request);

        #endregion

        #region Production Optimization

        /// <summary>
        /// Identifies optimization opportunities
        /// </summary>
        Task<List<OptimizationOpportunity>> IdentifyOptimizationOpportunitiesAsync(string wellUWI);

        /// <summary>
        /// Recommends parameter adjustments
        /// </summary>
        Task<ParameterAdjustment> RecommendParameterAdjustmentsAsync(string wellUWI, PerformanceData currentPerformance);

        /// <summary>
        /// Performs sensitivity analysis on design parameters
        /// </summary>
        Task<SensitivityAnalysis> PerformSensitivityAnalysisAsync(string wellUWI, SensitivityRequest request);

        /// <summary>
        /// Compares different plunger lift designs
        /// </summary>
        Task<DesignComparison> CompareDesignsAsync(List<PlungerLiftDesign> designs);

        #endregion

        #region Acoustic and Monitoring

        /// <summary>
        /// Performs acoustic telemetry analysis
        /// </summary>
        Task<AcousticTelemetry> AnalyzeAcousticTelemetryAsync(string wellUWI, AcousticDataRequest request);

        /// <summary>
        /// Analyzes real-time production data
        /// </summary>
        Task<ProductionMonitoring> MonitorProductionAsync(string wellUWI, MonitoringRequest request);

        /// <summary>
        /// Detects operational issues
        /// </summary>
        Task<IssueDetection> DetectOperationalIssuesAsync(string wellUWI);

        /// <summary>
        /// Performs predictive maintenance analysis
        /// </summary>
        Task<PredictiveMaintenance> PerformPredictiveMaintenanceAsync(string wellUWI, MaintenanceRequest request);

        #endregion

        #region Artificial Lift Comparison

        /// <summary>
        /// Compares plunger lift with other artificial lift methods
        /// </summary>
        Task<ArtificialLiftComparison> CompareWithOtherMethodsAsync(string wellUWI, ComparisonRequest request);

        /// <summary>
        /// Evaluates plunger lift feasibility
        /// </summary>
        Task<FeasibilityAssessment> AssessFeasibilityAsync(string wellUWI, FeasibilityRequest request);

        /// <summary>
        /// Performs cost analysis for plunger lift
        /// </summary>
        Task<CostAnalysis> PerformCostAnalysisAsync(string wellUWI, CostAnalysisRequest request);

        #endregion

        #region Data Management

        /// <summary>
        /// Saves plunger lift design to database
        /// </summary>
        Task SavePlungerLiftDesignAsync(PlungerLiftDesign design, string userId);

        /// <summary>
        /// Retrieves plunger lift design
        /// </summary>
        Task<PlungerLiftDesign?> GetPlungerLiftDesignAsync(string wellUWI);

        /// <summary>
        /// Updates plunger lift design
        /// </summary>
        Task UpdatePlungerLiftDesignAsync(PlungerLiftDesign design, string userId);

        /// <summary>
        /// Saves performance monitoring data
        /// </summary>
        Task SavePerformanceDataAsync(PerformanceData performanceData, string userId);

        /// <summary>
        /// Retrieves performance data
        /// </summary>
        Task<List<PerformanceData>> GetPerformanceDataAsync(string wellUWI, DateTime startDate, DateTime endDate);

        #endregion

        #region Reporting and Export

        /// <summary>
        /// Generates plunger lift design report
        /// </summary>
        Task<PlungerLiftReport> GenerateDesignReportAsync(string wellUWI, ReportRequest request);

        /// <summary>
        /// Exports performance data
        /// </summary>
        Task<byte[]> ExportPerformanceDataAsync(string wellUWI, DateTime startDate, DateTime endDate, string format = "CSV");

        /// <summary>
        /// Generates technical specifications
        /// </summary>
        Task<TechnicalSpecifications> GenerateTechnicalSpecificationsAsync(string wellUWI, PlungerLiftDesign design);

        #endregion
    }

    #region Design DTOs

    /// <summary>
    /// Plunger lift design DTO
    /// </summary>
    public class PlungerLiftDesign
    {
        public string DesignId { get; set; } = string.Empty;
        public string WellUWI { get; set; } = string.Empty;
        public DateTime DesignDate { get; set; }
        public int PlungerType { get; set; }
        public decimal OperatingPressure { get; set; }
        public decimal MinimumPressure { get; set; }
        public decimal MaximumPressure { get; set; }
        public int CycleTime { get; set; } // in minutes
        public decimal TubingSize { get; set; } // inches
        public decimal CasingSize { get; set; } // inches
        public string Status { get; set; } = string.Empty;
        public List<string> DesignNotes { get; set; } = new();
    }

    /// <summary>
    /// Plunger lift well properties DTO
    /// </summary>
    public class PlungerLiftWellProperties
    {
        public string WellUWI { get; set; } = string.Empty;
        public decimal WellheadPressure { get; set; }
        public decimal ReservoirPressure { get; set; }
        public decimal ReservoirTemperature { get; set; }
        public decimal OilGravity { get; set; }
        public decimal GasGravity { get; set; }
        public decimal GOR { get; set; }
        public decimal WaterCut { get; set; }
        public decimal TubingSize { get; set; }
        public decimal CasingSize { get; set; }
        public decimal WellDepth { get; set; }
        public string WellType { get; set; } = string.Empty;
    }

    /// <summary>
    /// Plunger lift optimization request DTO
    /// </summary>
    public class PlungerLiftOptimizationRequest
    {
        public string WellUWI { get; set; } = string.Empty;
        public string OptimizationObjective { get; set; } = "MaximizeProduction";
        public List<string> ConstraintParameters { get; set; } = new();
        public int MaxIterations { get; set; } = 100;
    }

    /// <summary>
    /// Plunger type selection DTO
    /// </summary>
    public class PlungerTypeSelection
    {
        public string WellUWI { get; set; } = string.Empty;
        public List<PlungerTypeOption> Options { get; set; } = new();
        public string RecommendedType { get; set; } = string.Empty;
        public string RecommendationRationale { get; set; } = string.Empty;
    }

    /// <summary>
    /// Plunger type option DTO
    /// </summary>
    public class PlungerTypeOption
    {
        public string PlungerType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal EstimatedCost { get; set; }
        public decimal OperatingEfficiency { get; set; }
        public List<string> Advantages { get; set; } = new();
        public List<string> Disadvantages { get; set; } = new();
    }

    /// <summary>
    /// Cycle time calculation DTO
    /// </summary>
    public class CycleTimeCalculation
    {
        public string WellUWI { get; set; } = string.Empty;
        public int OptimalCycleTime { get; set; } // minutes
        public int MinimumCycleTime { get; set; }
        public int MaximumCycleTime { get; set; }
        public decimal ExpectedProductionRate { get; set; }
        public decimal EnergyCosts { get; set; }
        public string RecommendedCycleTime { get; set; } = string.Empty;
    }

    /// <summary>
    /// Cycle time request DTO
    /// </summary>
    public class CycleTimeRequest
    {
        public string WellUWI { get; set; } = string.Empty;
        public decimal TargetProductionRate { get; set; }
        public decimal MaximumOperatingPressure { get; set; }
        public decimal MinimumOperatingPressure { get; set; }
    }

    #endregion

    #region Performance DTOs

    /// <summary>
    /// Plunger lift performance DTO
    /// </summary>
    public class PlungerLiftPerformance
    {
        public string PerformanceId { get; set; } = string.Empty;
        public string WellUWI { get; set; } = string.Empty;
        public DateTime PerformanceDate { get; set; }
        public decimal ProductionRate { get; set; }
        public int CycleTime { get; set; }
        public decimal Efficiency { get; set; }
        public decimal AveragePressure { get; set; }
        public int OperatingHours { get; set; }
        public int DowntimeHours { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>
    /// Production rate DTO
    /// </summary>
    public class ProductionRate
    {
        public string WellUWI { get; set; } = string.Empty;
        public decimal OilRate { get; set; }
        public decimal GasRate { get; set; }
        public decimal WaterRate { get; set; }
        public decimal TotalRate { get; set; }
        public string Unit { get; set; } = "STB/day";
        public DateTime CalculationDate { get; set; }
    }

    /// <summary>
    /// Production rate request DTO
    /// </summary>
    public class ProductionRateRequest
    {
        public string WellUWI { get; set; } = string.Empty;
        public int CycleTime { get; set; }
        public decimal ReservoirPressure { get; set; }
        public decimal WellheadPressure { get; set; }
    }

    /// <summary>
    /// Efficiency analysis DTO
    /// </summary>
    public class EfficiencyAnalysis
    {
        public string AnalysisId { get; set; } = string.Empty;
        public string WellUWI { get; set; } = string.Empty;
        public decimal ThermalEfficiency { get; set; }
        public decimal VolumetricEfficiency { get; set; }
        public decimal OverallEfficiency { get; set; }
        public decimal EnergyInput { get; set; }
        public decimal UsefulOutput { get; set; }
        public List<EfficiencyLoss> Losses { get; set; } = new();
    }

    /// <summary>
    /// Efficiency loss DTO
    /// </summary>
    public class EfficiencyLoss
    {
        public string LossType { get; set; } = string.Empty;
        public decimal LossPercentage { get; set; }
        public string Description { get; set; } = string.Empty;
        public string MitigationStrategy { get; set; } = string.Empty;
    }

    /// <summary>
    /// Efficiency analysis request DTO
    /// </summary>
    public class EfficiencyAnalysisRequest
    {
        public string WellUWI { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IncludeLossAnalysis { get; set; } = true;
    }

    /// <summary>
    /// Energy requirements DTO
    /// </summary>
    public class EnergyRequirements
    {
        public string WellUWI { get; set; } = string.Empty;
        public decimal DailyEnergyUsage { get; set; } // kWh
        public decimal MonthlyEnergyUsage { get; set; }
        public decimal AnnualEnergyUsage { get; set; }
        public decimal EnergyCostPerDay { get; set; }
        public decimal CompressorPowerRequired { get; set; }
        public string EnergySource { get; set; } = string.Empty;
    }

    /// <summary>
    /// Energy request DTO
    /// </summary>
    public class EnergyRequest
    {
        public string WellUWI { get; set; } = string.Empty;
        public decimal CycleTime { get; set; }
        public decimal AirPressure { get; set; }
        public decimal CompressorEfficiency { get; set; }
    }

    /// <summary>
    /// Performance data DTO
    /// </summary>
    public class PerformanceData
    {
        public string DataId { get; set; } = string.Empty;
        public string WellUWI { get; set; } = string.Empty;
        public DateTime MeasurementDate { get; set; }
        public decimal OilVolume { get; set; }
        public decimal GasVolume { get; set; }
        public decimal WaterVolume { get; set; }
        public decimal WellheadPressure { get; set; }
        public decimal AverageCycleTime { get; set; }
        public int OperatingHours { get; set; }
        public string Comments { get; set; } = string.Empty;
    }

    #endregion

    #region Equipment DTOs

    /// <summary>
    /// Valve performance DTO
    /// </summary>
    public class ValvePerformance
    {
        public string AnalysisId { get; set; } = string.Empty;
        public string WellUWI { get; set; } = string.Empty;
        public decimal OpeningPressure { get; set; }
        public decimal ClosingPressure { get; set; }
        public int CyclesPerDay { get; set; }
        public decimal LeakageRate { get; set; }
        public string ValveCondition { get; set; } = string.Empty;
        public List<string> Recommendations { get; set; } = new();
    }

    /// <summary>
    /// Valve analysis request DTO
    /// </summary>
    public class ValveAnalysisRequest
    {
        public string WellUWI { get; set; } = string.Empty;
        public decimal TubingPressure { get; set; }
        public decimal CasingPressure { get; set; }
        public int CyclesPerDay { get; set; }
    }

    /// <summary>
    /// Valve sizing DTO
    /// </summary>
    public class ValveSizing
    {
        public string WellUWI { get; set; } = string.Empty;
        public decimal RequiredValveSize { get; set; }
        public decimal FlowCapacity { get; set; }
        public decimal PressureRating { get; set; }
        public string RecommendedValveType { get; set; } = string.Empty;
    }

    /// <summary>
    /// Valve sizing request DTO
    /// </summary>
    public class ValveSizingRequest
    {
        public string WellUWI { get; set; } = string.Empty;
        public decimal MaximumFlowRate { get; set; }
        public decimal OperatingPressure { get; set; }
    }

    /// <summary>
    /// Tubing analysis DTO
    /// </summary>
    public class TubingAnalysis
    {
        public string AnalysisId { get; set; } = string.Empty;
        public string WellUWI { get; set; } = string.Empty;
        public decimal TubingSize { get; set; }
        public decimal WallThickness { get; set; }
        public decimal Grade { get; set; }
        public decimal MaximumWorkingPressure { get; set; }
        public bool IsAdequate { get; set; }
        public List<string> Issues { get; set; } = new();
    }

    /// <summary>
    /// Tubing analysis request DTO
    /// </summary>
    public class TubingAnalysisRequest
    {
        public string WellUWI { get; set; } = string.Empty;
        public decimal TubingSize { get; set; }
        public decimal MaximumOperatingPressure { get; set; }
    }

    /// <summary>
    /// Casing analysis DTO
    /// </summary>
    public class CasingAnalysis
    {
        public string AnalysisId { get; set; } = string.Empty;
        public string WellUWI { get; set; } = string.Empty;
        public decimal CasingSize { get; set; }
        public decimal Grade { get; set; }
        public decimal MaximumWorkingPressure { get; set; }
        public decimal CollapsePressure { get; set; }
        public bool IsAdequate { get; set; }
        public List<string> Concerns { get; set; } = new();
    }

    /// <summary>
    /// Casing analysis request DTO
    /// </summary>
    public class CasingAnalysisRequest
    {
        public string WellUWI { get; set; } = string.Empty;
        public decimal CasingSize { get; set; }
        public decimal MaximumOperatingPressure { get; set; }
    }

    #endregion

    #region Optimization & Comparison DTOs

    /// <summary>
    /// Optimization opportunity DTO
    /// </summary>
    public class OptimizationOpportunity
    {
        public string OpportunityId { get; set; } = string.Empty;
        public string WellUWI { get; set; } = string.Empty;
        public string OpportunityType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal ExpectedProductionIncrease { get; set; }
        public decimal EstimatedCost { get; set; }
        public decimal PaybackPeriod { get; set; }
        public string Priority { get; set; } = string.Empty;
    }

    /// <summary>
    /// Parameter adjustment DTO
    /// </summary>
    public class ParameterAdjustment
    {
        public string AdjustmentId { get; set; } = string.Empty;
        public string WellUWI { get; set; } = string.Empty;
        public List<ParameterChange> Adjustments { get; set; } = new();
        public decimal ExpectedProductionImprovement { get; set; }
        public string Rationale { get; set; } = string.Empty;
    }

    /// <summary>
    /// Parameter change DTO
    /// </summary>
    public class ParameterChange
    {
        public string ParameterName { get; set; } = string.Empty;
        public decimal CurrentValue { get; set; }
        public decimal RecommendedValue { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string Rationale { get; set; } = string.Empty;
    }

    /// <summary>
    /// Sensitivity analysis DTO
    /// </summary>
    public class SensitivityAnalysis
    {
        public string AnalysisId { get; set; } = string.Empty;
        public string WellUWI { get; set; } = string.Empty;
        public List<SensitivityParameter> Parameters { get; set; } = new();
        public string AnalysisSummary { get; set; } = string.Empty;
    }

    /// <summary>
    /// Sensitivity parameter DTO
    /// </summary>
    public class SensitivityParameter
    {
        public string ParameterName { get; set; } = string.Empty;
        public decimal BaseValue { get; set; }
        public decimal MinValue { get; set; }
        public decimal MaxValue { get; set; }
        public List<SensitivityPoint> Results { get; set; } = new();
    }

    /// <summary>
    /// Sensitivity point DTO
    /// </summary>
    public class SensitivityPoint
    {
        public decimal ParameterValue { get; set; }
        public decimal ProductionRate { get; set; }
        public decimal Efficiency { get; set; }
        public decimal Cost { get; set; }
    }

    /// <summary>
    /// Design comparison DTO
    /// </summary>
    public class DesignComparison
    {
        public string ComparisonId { get; set; } = string.Empty;
        public List<DesignComparisonItem> Designs { get; set; } = new();
        public string BestDesign { get; set; } = string.Empty;
        public string ComparisonSummary { get; set; } = string.Empty;
    }

    /// <summary>
    /// Design comparison item DTO
    /// </summary>
    public class DesignComparisonItem
    {
        public string DesignId { get; set; } = string.Empty;
        public string DesignName { get; set; } = string.Empty;
        public decimal ExpectedProduction { get; set; }
        public decimal EstimatedCost { get; set; }
        public decimal Efficiency { get; set; }
        public List<string> Advantages { get; set; } = new();
        public List<string> Disadvantages { get; set; } = new();
    }

    #endregion

    #region Monitoring & Maintenance DTOs

    /// <summary>
    /// Acoustic telemetry DTO
    /// </summary>
    public class AcousticTelemetry
    {
        public string AnalysisId { get; set; } = string.Empty;
        public string WellUWI { get; set; } = string.Empty;
        public DateTime MeasurementDate { get; set; }
        public decimal SignalFrequency { get; set; }
        public decimal SignalAmplitude { get; set; }
        public string PlungerDetection { get; set; } = string.Empty;
        public int CycleCount { get; set; }
        public List<string> Anomalies { get; set; } = new();
    }

    /// <summary>
    /// Acoustic data request DTO
    /// </summary>
    public class AcousticDataRequest
    {
        public string WellUWI { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IncludeAnomalyDetection { get; set; } = true;
    }

    /// <summary>
    /// Production monitoring DTO
    /// </summary>
    public class ProductionMonitoring
    {
        public string MonitoringId { get; set; } = string.Empty;
        public string WellUWI { get; set; } = string.Empty;
        public DateTime MonitoringDate { get; set; }
        public decimal DailyProduction { get; set; }
        public decimal AverageRate { get; set; }
        public int OperatingHours { get; set; }
        public string OperationalStatus { get; set; } = string.Empty;
        public List<string> Alerts { get; set; } = new();
    }

    /// <summary>
    /// Monitoring request DTO
    /// </summary>
    public class MonitoringRequest
    {
        public string WellUWI { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IncludeAlerts { get; set; } = true;
    }

    /// <summary>
    /// Issue detection DTO
    /// </summary>
    public class IssueDetection
    {
        public string IssueId { get; set; } = string.Empty;
        public string WellUWI { get; set; } = string.Empty;
        public List<OperationalIssue> Issues { get; set; } = new();
        public string OverallStatus { get; set; } = string.Empty;
        public List<string> Recommendations { get; set; } = new();
    }

    /// <summary>
    /// Operational issue DTO
    /// </summary>
    public class OperationalIssue
    {
        public string IssueType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public DateTime DetectionTime { get; set; }
        public string RecommendedAction { get; set; } = string.Empty;
    }

    /// <summary>
    /// Predictive maintenance DTO
    /// </summary>
    public class PredictiveMaintenance
    {
        public string MaintenanceId { get; set; } = string.Empty;
        public string WellUWI { get; set; } = string.Empty;
        public List<MaintenancePrediction> Predictions { get; set; } = new();
        public DateTime AnalysisDate { get; set; }
        public string OverallHealth { get; set; } = string.Empty;
    }

    /// <summary>
    /// Maintenance prediction DTO
    /// </summary>
    public class MaintenancePrediction
    {
        public string ComponentName { get; set; } = string.Empty;
        public string EstimatedCondition { get; set; } = string.Empty;
        public int RemainingLife { get; set; } // days
        public DateTime RecommendedMaintenanceDate { get; set; }
        public string MaintenanceType { get; set; } = string.Empty;
    }

    /// <summary>
    /// Maintenance request DTO
    /// </summary>
    public class MaintenanceRequest
    {
        public string WellUWI { get; set; } = string.Empty;
        public bool IncludeComponentAnalysis { get; set; } = true;
        public int LookAheadPeriod { get; set; } = 90; // days
    }

    #endregion

    #region Comparison & Feasibility DTOs

    /// <summary>
    /// Artificial lift comparison DTO
    /// </summary>
    public class ArtificialLiftComparison
    {
        public string ComparisonId { get; set; } = string.Empty;
        public string WellUWI { get; set; } = string.Empty;
        public List<LiftMethodComparison> Methods { get; set; } = new();
        public string RecommendedMethod { get; set; } = string.Empty;
        public string ComparisonSummary { get; set; } = string.Empty;
    }

    /// <summary>
    /// Lift method comparison DTO
    /// </summary>
    public class LiftMethodComparison
    {
        public string MethodName { get; set; } = string.Empty;
        public decimal EstimatedCost { get; set; }
        public decimal ProductionPotential { get; set; }
        public decimal OperatingCost { get; set; }
        public decimal Reliability { get; set; }
        public List<string> Pros { get; set; } = new();
        public List<string> Cons { get; set; } = new();
    }

    /// <summary>
    /// Feasibility assessment DTO
    /// </summary>
    public class FeasibilityAssessment
    {
        public string AssessmentId { get; set; } = string.Empty;
        public string WellUWI { get; set; } = string.Empty;
        public bool IsFeasible { get; set; }
        public decimal FeasibilityScore { get; set; }
        public List<FeasibilityFactor> Factors { get; set; } = new();
        public string AssessmentSummary { get; set; } = string.Empty;
    }

    /// <summary>
    /// Feasibility factor DTO
    /// </summary>
    public class FeasibilityFactor
    {
        public string FactorName { get; set; } = string.Empty;
        public bool IsFavorable { get; set; }
        public decimal Score { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// Feasibility request DTO
    /// </summary>
    public class FeasibilityRequest
    {
        public string WellUWI { get; set; } = string.Empty;
        public List<string> EvaluationCriteria { get; set; } = new();
    }

    /// <summary>
    /// Cost analysis DTO
    /// </summary>
    public class CostAnalysis
    {
        public string AnalysisId { get; set; } = string.Empty;
        public string WellUWI { get; set; } = string.Empty;
        public decimal InitialCost { get; set; }
        public decimal AnnualOperatingCost { get; set; }
        public decimal MaintenanceCost { get; set; }
        public decimal AnnualRevenue { get; set; }
        public decimal PaybackPeriod { get; set; }
        public decimal NPV { get; set; }
        public string CostAnalysisSummary { get; set; } = string.Empty;
    }

    /// <summary>
    /// Cost analysis request DTO
    /// </summary>
    public class CostAnalysisRequest
    {
        public string WellUWI { get; set; } = string.Empty;
        public decimal OilPrice { get; set; }
        public decimal GasPrice { get; set; }
        public decimal DiscountRate { get; set; }
    }

    #endregion

    #region Comparison Request DTOs

    /// <summary>
    /// Comparison request DTO
    /// </summary>
    public class ComparisonRequest
    {
        public string WellUWI { get; set; } = string.Empty;
        public List<string> LiftMethods { get; set; } = new();
        public bool IncludeCostAnalysis { get; set; } = true;
    }

    #endregion

    #region Request DTOs

    /// <summary>
    /// Plunger selection criteria DTO
    /// </summary>
    public class PlungerSelectionCriteria
    {
        public string WellUWI { get; set; } = string.Empty;
        public decimal TubingSize { get; set; }
        public decimal CasingSize { get; set; }
        public decimal ReservoirPressure { get; set; }
        public decimal ReservoirTemperature { get; set; }
        public decimal GOR { get; set; }
        public decimal WaterCut { get; set; }
        public bool IncludePartialStroke { get; set; }
        public List<string> PriorityCriteria { get; set; } = new();
    }

    /// <summary>
    /// Sensitivity analysis request DTO
    /// </summary>
    public class SensitivityRequest
    {
        public string WellUWI { get; set; } = string.Empty;
        public List<string> ParametersToAnalyze { get; set; } = new();
        public decimal VariationPercentage { get; set; } = 20m;
        public int NumberOfSteps { get; set; } = 10;
        public bool IncludeInteractions { get; set; } = false;
    }

    #endregion

    #region Report DTOs

    /// <summary>
    /// Plunger lift report DTO
    /// </summary>
    public class PlungerLiftReport
    {
        public string ReportId { get; set; } = string.Empty;
        public string WellUWI { get; set; } = string.Empty;
        public DateTime GeneratedDate { get; set; }
        public string ReportType { get; set; } = string.Empty;
        public byte[] ReportContent { get; set; } = Array.Empty<byte>();
        public List<byte[]> Charts { get; set; } = new();
        public string ExecutiveSummary { get; set; } = string.Empty;
        public List<string> Recommendations { get; set; } = new();
    }

    /// <summary>
    /// Report request DTO
    /// </summary>
    public class ReportRequest
    {
        public string WellUWI { get; set; } = string.Empty;
        public string ReportType { get; set; } = "Comprehensive";
        public List<string> IncludeSections { get; set; } = new();
        public bool IncludeCharts { get; set; } = true;
        public string Format { get; set; } = "PDF";
    }

    /// <summary>
    /// Technical specifications DTO
    /// </summary>
    public class TechnicalSpecifications
    {
        public string SpecId { get; set; } = string.Empty;
        public string WellUWI { get; set; } = string.Empty;
        public List<SpecificationItem> Specifications { get; set; } = new();
        public string Notes { get; set; } = string.Empty;
    }

    /// <summary>
    /// Specification item DTO
    /// </summary>
    public class SpecificationItem
    {
        public string ItemName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
    }

    #endregion
}

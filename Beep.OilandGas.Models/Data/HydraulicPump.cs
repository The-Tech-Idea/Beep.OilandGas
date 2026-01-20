using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    #region Design and Sizing DTOs

    /// <summary>DTO for hydraulic pump design result.</summary>
    public class PumpDesignResult : ModelEntityBase
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
        public string Status { get; set; } = "Designed";
        public List<string> DesignRecommendations { get; set; } = new();
    }

    /// <summary>DTO for pump design request.</summary>
    public class PumpDesignRequest : ModelEntityBase
    {
        public string PumpType { get; set; } = string.Empty;
        public decimal WellDepth { get; set; }
        public decimal DesiredFlowRate { get; set; }
        public decimal ReservoirPressure { get; set; }
        public decimal TubingSize { get; set; }
        public decimal CasingSize { get; set; }
    }

    /// <summary>DTO for pump sizing result.</summary>
    public class PumpSizingResult : ModelEntityBase
    {
        public string PumpId { get; set; } = string.Empty;
        public decimal RecommendedDisplacement { get; set; }
        public decimal MaximumFlowRate { get; set; }
        public decimal MaximumPressure { get; set; }
        public decimal OptimalSpeed { get; set; }
        public string RecommendedPumpModel { get; set; } = string.Empty;
    }

    /// <summary>DTO for pump sizing request.</summary>
    public class PumpSizingRequest : ModelEntityBase
    {
        public decimal DesiredFlowRate { get; set; }
        public decimal MaxOperatingPressure { get; set; }
        public decimal FluidViscosity { get; set; }
    }

    /// <summary>DTO for pump type selection.</summary>
    public class PumpTypeSelection : ModelEntityBase
    {
        public string WellUWI { get; set; } = string.Empty;
        public List<PumpTypeOption> Options { get; set; } = new();
        public string RecommendedType { get; set; } = string.Empty;
        public string SelectionRationale { get; set; } = string.Empty;
    }

    /// <summary>DTO for pump type option.</summary>
    public class PumpTypeOption : ModelEntityBase
    {
        public string PumpType { get; set; } = string.Empty;
        public decimal Efficiency { get; set; }
        public decimal Cost { get; set; }
        public decimal Reliability { get; set; }
        public List<string> Advantages { get; set; } = new();
        public List<string> Disadvantages { get; set; } = new();
    }

    /// <summary>DTO for pump selection criteria.</summary>
    public class PumpSelectionCriteria : ModelEntityBase
    {
        public List<string> PriorityFactors { get; set; } = new();
    }

    /// <summary>DTO for power requirements.</summary>
    public class PowerRequirements : ModelEntityBase
    {
        public string PumpId { get; set; } = string.Empty;
        public decimal HydraulicPower { get; set; }
        public decimal MechanicalPower { get; set; }
        public decimal TotalPowerRequired { get; set; }
        public decimal PowerEfficiency { get; set; }
        public string PowerUnit { get; set; } = "HP";
    }

    /// <summary>DTO for power calculation request.</summary>
    public class PowerCalculationRequest : ModelEntityBase
    {
        public decimal FlowRate { get; set; }
        public decimal DischargePressure { get; set; }
        public decimal SuctionPressure { get; set; }
    }

    /// <summary>DTO for hydraulic balance.</summary>
    public class HydraulicBalance : ModelEntityBase
    {
        public string PumpId { get; set; } = string.Empty;
        public bool IsBalanced { get; set; }
        public decimal BalanceScore { get; set; }
        public List<BalanceFactor> Factors { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
    }

    /// <summary>DTO for balance request.</summary>
    public class BalanceRequest : ModelEntityBase
    {
        public decimal DischargeFlow { get; set; }
        public decimal DischargePressure { get; set; }
    }

    /// <summary>DTO for balance factor.</summary>
    public class BalanceFactor : ModelEntityBase
    {
        public string FactorName { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>DTO for rod string design.</summary>
    public class RodStringDesign : ModelEntityBase
    {
        public string DesignId { get; set; } = string.Empty;
        public string WellUWI { get; set; } = string.Empty;
        public string RodSize { get; set; } = string.Empty;
        public string RodGrade { get; set; } = string.Empty;
        public int RodSections { get; set; }
        public decimal TotalRodLength { get; set; }
        public decimal SafetyFactor { get; set; }
        public bool IsAdequate { get; set; }
    }

    /// <summary>DTO for rod string request.</summary>
    public class RodStringRequest : ModelEntityBase
    {
        public decimal WellDepth { get; set; }
        public decimal PumpingLoad { get; set; }
    }

    #endregion

    #region Performance Analysis DTOs

    /// <summary>DTO for pump performance analysis.</summary>
    public class PumpPerformanceAnalysis : ModelEntityBase
    {
        public string AnalysisId { get; set; } = string.Empty;
        public string PumpId { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }
        public decimal CurrentFlowRate { get; set; }
        public decimal CurrentEfficiency { get; set; }
        public List<string> Recommendations { get; set; } = new();
    }

    /// <summary>DTO for performance analysis request.</summary>
    public class PerformanceAnalysisRequest : ModelEntityBase
    {
    }

    /// <summary>DTO for pump efficiency.</summary>
    public class PumpEfficiency : ModelEntityBase
    {
        public string PumpId { get; set; } = string.Empty;
        public decimal OverallEfficiency { get; set; }
    }

    /// <summary>DTO for efficiency request.</summary>
    public class EfficiencyRequest : ModelEntityBase
    {
    }

    /// <summary>DTO for cavitation analysis.</summary>
    public class CavitationAnalysis : ModelEntityBase
    {
        public string PumpId { get; set; } = string.Empty;
        public string CavitationRisk { get; set; } = string.Empty;
    }

    /// <summary>DTO for cavitation request.</summary>
    public class CavitationRequest : ModelEntityBase
    {
    }

    /// <summary>DTO for vibration analysis.</summary>
    public class VibrationAnalysis : ModelEntityBase
    {
        public string PumpId { get; set; } = string.Empty;
        public string VibrationLevel { get; set; } = string.Empty;
    }

    /// <summary>DTO for vibration request.</summary>
    public class VibrationRequest : ModelEntityBase
    {
    }

    /// <summary>DTO for pressure dynamics.</summary>
    public class PressureDynamics : ModelEntityBase
    {
        public string PumpId { get; set; } = string.Empty;
        public decimal AveragePressure { get; set; }
    }

    /// <summary>DTO for pressure request.</summary>
    public class PressureRequest : ModelEntityBase
    {
    }

    /// <summary>DTO for flow characteristics.</summary>
    public class FlowCharacteristics : ModelEntityBase
    {
        public string PumpId { get; set; } = string.Empty;
        public decimal FlowRate { get; set; }
    }

    /// <summary>DTO for flow request.</summary>
    public class FlowRequest : ModelEntityBase
    {
    }

    #endregion

    #region Optimization DTOs

    /// <summary>DTO for optimization request.</summary>
    public class OptimizationRequest : ModelEntityBase
    {
    }

    /// <summary>DTO for parameter adjustment.</summary>
    public class ParameterAdjustment : ModelEntityBase
    {
        public string PumpId { get; set; } = string.Empty;
        public List<string> Recommendations { get; set; } = new();
    }

    /// <summary>DTO for efficiency improvement.</summary>
    public class EfficiencyImprovement : ModelEntityBase
    {
        public string PumpId { get; set; } = string.Empty;
        public decimal PotentialImprovement { get; set; }
    }

    /// <summary>DTO for pump comparison.</summary>
    public class PumpComparison : ModelEntityBase
    {
        public string ComparisonId { get; set; } = string.Empty;
        public int PumpCount { get; set; }
    }

    /// <summary>DTO for comparison criteria.</summary>
    public class ComparisonCriteria : ModelEntityBase
    {
    }

    /// <summary>DTO for pump upgrade recommendation.</summary>
    public class PumpUpgradeRecommendation : ModelEntityBase
    {
        public string PumpId { get; set; } = string.Empty;
        public bool UpgradeRecommended { get; set; }
    }

    /// <summary>DTO for upgrade request.</summary>
    public class UpgradeRequest : ModelEntityBase
    {
    }

    #endregion

    #region Monitoring and Diagnostics DTOs

    /// <summary>DTO for pump monitoring data.</summary>
    public class PumpMonitoringData : ModelEntityBase
    {
        public string PumpId { get; set; } = string.Empty;
        public bool MonitoringActive { get; set; }
    }

    /// <summary>DTO for monitoring request.</summary>
    public class MonitoringRequest : ModelEntityBase
    {
    }

    /// <summary>DTO for diagnostics result.</summary>
    public class DiagnosticsResult : ModelEntityBase
    {
        public string PumpId { get; set; } = string.Empty;
        public string DiagnosticsStatus { get; set; } = string.Empty;
    }

    /// <summary>DTO for diagnostics request.</summary>
    public class DiagnosticsRequest : ModelEntityBase
    {
    }

    /// <summary>DTO for condition assessment.</summary>
    public class ConditionAssessment : ModelEntityBase
    {
        public string PumpId { get; set; } = string.Empty;
        public string ConditionRating { get; set; } = string.Empty;
    }

    /// <summary>DTO for anomaly detection.</summary>
    public class AnomalyDetection : ModelEntityBase
    {
        public string PumpId { get; set; } = string.Empty;
        public bool AnomaliesDetected { get; set; }
    }

    /// <summary>DTO for predictive maintenance.</summary>
    public class PredictiveMaintenance : ModelEntityBase
    {
        public string PumpId { get; set; } = string.Empty;
        public DateTime NextMaintenanceDate { get; set; }
    }

    /// <summary>DTO for maintenance request.</summary>
    public class MaintenanceRequest : ModelEntityBase
    {
    }

    #endregion

    #region Reliability DTOs

    /// <summary>DTO for failure mode analysis.</summary>
    public class FailureModeAnalysis : ModelEntityBase
    {
        public string PumpId { get; set; } = string.Empty;
        public string FailureRisk { get; set; } = string.Empty;
    }

    /// <summary>DTO for reliability assessment.</summary>
    public class ReliabilityAssessment : ModelEntityBase
    {
        public string PumpId { get; set; } = string.Empty;
        public decimal ReliabilityScore { get; set; }
    }

    /// <summary>DTO for reliability request.</summary>
    public class ReliabilityRequest : ModelEntityBase
    {
    }

    /// <summary>DTO for MTBF calculation.</summary>
    public class MTBFCalculation : ModelEntityBase
    {
        public string PumpId { get; set; } = string.Empty;
        public decimal MTBF_Hours { get; set; }
    }

    /// <summary>DTO for wear analysis.</summary>
    public class WearAnalysis : ModelEntityBase
    {
        public string PumpId { get; set; } = string.Empty;
        public decimal WearRate { get; set; }
    }

    /// <summary>DTO for wear request.</summary>
    public class WearRequest : ModelEntityBase
    {
    }

    /// <summary>DTO for failure risk assessment.</summary>
    public class FailureRiskAssessment : ModelEntityBase
    {
        public string PumpId { get; set; } = string.Empty;
        public string FailureRisk { get; set; } = string.Empty;
        public decimal RiskScore { get; set; }
    }

    #endregion

    #region Maintenance DTOs

    /// <summary>DTO for maintenance schedule.</summary>
    public class MaintenanceSchedule : ModelEntityBase
    {
        public string PumpId { get; set; } = string.Empty;
        public DateTime NextMaintenanceDate { get; set; }
    }

    /// <summary>DTO for schedule request.</summary>
    public class ScheduleRequest : ModelEntityBase
    {
    }

    /// <summary>DTO for maintenance activity.</summary>
    public class MaintenanceActivity : ModelEntityBase
    {
        public string ActivityId { get; set; } = string.Empty;
        public string PumpId { get; set; } = string.Empty;
        public DateTime ActivityDate { get; set; }
        public string ActivityType { get; set; } = string.Empty;
    }

    /// <summary>DTO for rebuild analysis.</summary>
    public class RebuildAnalysis : ModelEntityBase
    {
        public string PumpId { get; set; } = string.Empty;
        public bool RebuildRequired { get; set; }
    }

    /// <summary>DTO for rebuild request.</summary>
    public class RebuildRequest : ModelEntityBase
    {
    }

    /// <summary>DTO for parts inventory.</summary>
    public class PartsInventory : ModelEntityBase
    {
        public string PumpId { get; set; } = string.Empty;
        public int PartCount { get; set; }
    }

    /// <summary>DTO for parts request.</summary>
    public class PartsRequest : ModelEntityBase
    {
    }

    /// <summary>DTO for maintenance cost estimate.</summary>
    public class MaintenanceCostEstimate : ModelEntityBase
    {
        public string PumpId { get; set; } = string.Empty;
        public decimal EstimatedCost { get; set; }
    }

    /// <summary>DTO for cost estimate request.</summary>
    public class CostEstimateRequest : ModelEntityBase
    {
    }

    #endregion

    #region Fluid Management DTOs

    /// <summary>DTO for hydraulic fluid analysis.</summary>
    public class FluidAnalysis : ModelEntityBase
    {
        public string PumpId { get; set; } = string.Empty;
        public string FluidCondition { get; set; } = string.Empty;
    }

    /// <summary>DTO for fluid analysis request.</summary>
    public class FluidAnalysisRequest : ModelEntityBase
    {
    }

    /// <summary>DTO for fluid change recommendation.</summary>
    public class FluidChangeRecommendation : ModelEntityBase
    {
        public string PumpId { get; set; } = string.Empty;
        public bool ChangeRequired { get; set; }
    }

    /// <summary>DTO for contamination level.</summary>
    public class ContaminationLevel : ModelEntityBase
    {
        public string PumpId { get; set; } = string.Empty;
        public string ContaminationLevel { get; set; } = string.Empty;
    }

    /// <summary>DTO for contamination request.</summary>
    public class ContaminationRequest : ModelEntityBase
    {
    }

    /// <summary>DTO for filtration system.</summary>
    public class FiltrationSystem : ModelEntityBase
    {
        public string PumpId { get; set; } = string.Empty;
        public string FilterStatus { get; set; } = string.Empty;
    }

    /// <summary>DTO for filtration request.</summary>
    public class FiltrationRequest : ModelEntityBase
    {
    }

    #endregion

    #region Integration DTOs

    /// <summary>DTO for SCADA integration.</summary>
    public class SCODAIntegration : ModelEntityBase
    {
        public string PumpId { get; set; } = string.Empty;
        public bool IsIntegrated { get; set; }
        public List<IntegrationParameter> Parameters { get; set; } = new();
        public string IntegrationStatus { get; set; } = string.Empty;
    }

    /// <summary>DTO for SCADA configuration.</summary>
    public class SCADAConfig : ModelEntityBase
    {
    }

    /// <summary>DTO for integration parameter.</summary>
    public class IntegrationParameter : ModelEntityBase
    {
        public string ParameterName { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public string UpdateFrequency { get; set; } = string.Empty;
    }

    /// <summary>DTO for control parameters.</summary>
    public class ControlParameters : ModelEntityBase
    {
        public string PumpId { get; set; } = string.Empty;
        public List<ControlParam> Parameters { get; set; } = new();
        public string ControlMode { get; set; } = string.Empty;
    }

    /// <summary>DTO for control request.</summary>
    public class ControlRequest : ModelEntityBase
    {
    }

    /// <summary>DTO for control parameter.</summary>
    public class ControlParam : ModelEntityBase
    {
        public string ParameterName { get; set; } = string.Empty;
        public decimal CurrentValue { get; set; }
        public decimal MinValue { get; set; }
        public decimal MaxValue { get; set; }
    }

    /// <summary>DTO for wellbore interaction.</summary>
    public class WellboreInteraction : ModelEntityBase
    {
        public string WellUWI { get; set; } = string.Empty;
        public decimal InteractionIntensity { get; set; }
        public List<InteractionFactor> Factors { get; set; } = new();
        public string OverallAssessment { get; set; } = string.Empty;
    }

    /// <summary>DTO for interaction request.</summary>
    public class InteractionRequest : ModelEntityBase
    {
    }

    /// <summary>DTO for interaction factor.</summary>
    public class InteractionFactor : ModelEntityBase
    {
        public string FactorName { get; set; } = string.Empty;
        public decimal Impact { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    #endregion

    #region Data Management DTOs

    /// <summary>DTO for pump history.</summary>
    public class PumpHistory : ModelEntityBase
    {
        public string PumpId { get; set; } = string.Empty;
        public DateTime DateRecord { get; set; }
        public string EventDescription { get; set; } = string.Empty;
    }

    /// <summary>DTO for performance trends.</summary>
    public class PerformanceTrends : ModelEntityBase
    {
        public string PumpId { get; set; } = string.Empty;
        public List<decimal> EfficiencyTrend { get; set; } = new();
        public List<decimal> FlowTrend { get; set; } = new();
    }

    #endregion

    #region Reporting DTOs

    /// <summary>DTO for pump report.</summary>
    public class PumpReport : ModelEntityBase
    {
        public string PumpId { get; set; } = string.Empty;
        public string ReportId { get; set; } = string.Empty;
        public DateTime GeneratedDate { get; set; }
    }

    /// <summary>DTO for report request.</summary>
    public class ReportRequest : ModelEntityBase
    {
    }

    /// <summary>DTO for performance summary report.</summary>
    public class PerformanceSummaryReport : ModelEntityBase
    {
        public string PumpId { get; set; } = string.Empty;
        public decimal AverageEfficiency { get; set; }
        public decimal AverageFlowRate { get; set; }
    }

    /// <summary>DTO for summary report request.</summary>
    public class SummaryReportRequest : ModelEntityBase
    {
    }

    /// <summary>DTO for cost analysis report.</summary>
    public class CostAnalysisReport : ModelEntityBase
    {
        public string PumpId { get; set; } = string.Empty;
        public decimal TotalCost { get; set; }
        public decimal OperatingCost { get; set; }
        public decimal MaintenanceCost { get; set; }
    }

    /// <summary>DTO for cost report request.</summary>
    public class CostReportRequest : ModelEntityBase
    {
    }

    #endregion

    #region Legacy DTOs (kept for backward compatibility)

    /// <summary>DTO for hydraulic pump design (legacy).</summary>
    public class HydraulicPumpDesign : ModelEntityBase
    {
        public string DesignId { get; set; } = string.Empty;
        public string WellUWI { get; set; } = string.Empty;
        public string PumpType { get; set; } = string.Empty;
        public DateTime DesignDate { get; set; }
        public decimal PumpDepth { get; set; }
        public decimal PumpSize { get; set; }
        public decimal OperatingPressure { get; set; }
        public decimal FlowRate { get; set; }
        public decimal Efficiency { get; set; }
        public string? Status { get; set; }
    }

    /// <summary>DTO for pump performance history point (legacy).</summary>
    public class PumpPerformanceHistory : ModelEntityBase
    {
        public DateTime PerformanceDate { get; set; }
        public decimal FlowRate { get; set; }
        public decimal Efficiency { get; set; }
        public decimal PowerConsumption { get; set; }
        public string? Status { get; set; }
    }

    #endregion
}


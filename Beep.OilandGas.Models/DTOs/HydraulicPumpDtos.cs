using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.DTOs
{
    #region Design and Sizing DTOs

    /// <summary>DTO for hydraulic pump design result.</summary>
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
        public string Status { get; set; } = "Designed";
        public List<string> DesignRecommendations { get; set; } = new();
    }

    /// <summary>DTO for pump design request.</summary>
    public class PumpDesignRequestDto
    {
        public string PumpType { get; set; } = string.Empty;
        public decimal WellDepth { get; set; }
        public decimal DesiredFlowRate { get; set; }
        public decimal ReservoirPressure { get; set; }
        public decimal TubingSize { get; set; }
        public decimal CasingSize { get; set; }
    }

    /// <summary>DTO for pump sizing result.</summary>
    public class PumpSizingResultDto
    {
        public string PumpId { get; set; } = string.Empty;
        public decimal RecommendedDisplacement { get; set; }
        public decimal MaximumFlowRate { get; set; }
        public decimal MaximumPressure { get; set; }
        public decimal OptimalSpeed { get; set; }
        public string RecommendedPumpModel { get; set; } = string.Empty;
    }

    /// <summary>DTO for pump sizing request.</summary>
    public class PumpSizingRequestDto
    {
        public decimal DesiredFlowRate { get; set; }
        public decimal MaxOperatingPressure { get; set; }
        public decimal FluidViscosity { get; set; }
    }

    /// <summary>DTO for pump type selection.</summary>
    public class PumpTypeSelectionDto
    {
        public string WellUWI { get; set; } = string.Empty;
        public List<PumpTypeOptionDto> Options { get; set; } = new();
        public string RecommendedType { get; set; } = string.Empty;
        public string SelectionRationale { get; set; } = string.Empty;
    }

    /// <summary>DTO for pump type option.</summary>
    public class PumpTypeOptionDto
    {
        public string PumpType { get; set; } = string.Empty;
        public decimal Efficiency { get; set; }
        public decimal Cost { get; set; }
        public decimal Reliability { get; set; }
        public List<string> Advantages { get; set; } = new();
        public List<string> Disadvantages { get; set; } = new();
    }

    /// <summary>DTO for pump selection criteria.</summary>
    public class PumpSelectionCriteriaDto
    {
        public List<string> PriorityFactors { get; set; } = new();
    }

    /// <summary>DTO for power requirements.</summary>
    public class PowerRequirementsDto
    {
        public string PumpId { get; set; } = string.Empty;
        public decimal HydraulicPower { get; set; }
        public decimal MechanicalPower { get; set; }
        public decimal TotalPowerRequired { get; set; }
        public decimal PowerEfficiency { get; set; }
        public string PowerUnit { get; set; } = "HP";
    }

    /// <summary>DTO for power calculation request.</summary>
    public class PowerCalculationRequestDto
    {
        public decimal FlowRate { get; set; }
        public decimal DischargePressure { get; set; }
        public decimal SuctionPressure { get; set; }
    }

    /// <summary>DTO for hydraulic balance.</summary>
    public class HydraulicBalanceDto
    {
        public string PumpId { get; set; } = string.Empty;
        public bool IsBalanced { get; set; }
        public decimal BalanceScore { get; set; }
        public List<BalanceFactorDto> Factors { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
    }

    /// <summary>DTO for balance request.</summary>
    public class BalanceRequestDto
    {
        public decimal DischargeFlow { get; set; }
        public decimal DischargePressure { get; set; }
    }

    /// <summary>DTO for balance factor.</summary>
    public class BalanceFactorDto
    {
        public string FactorName { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>DTO for rod string design.</summary>
    public class RodStringDesignDto
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
    public class RodStringRequestDto
    {
        public decimal WellDepth { get; set; }
        public decimal PumpingLoad { get; set; }
    }

    #endregion

    #region Performance Analysis DTOs

    /// <summary>DTO for pump performance analysis.</summary>
    public class PumpPerformanceAnalysisDto
    {
        public string AnalysisId { get; set; } = string.Empty;
        public string PumpId { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }
        public decimal CurrentFlowRate { get; set; }
        public decimal CurrentEfficiency { get; set; }
        public List<string> Recommendations { get; set; } = new();
    }

    /// <summary>DTO for performance analysis request.</summary>
    public class PerformanceAnalysisRequestDto
    {
    }

    /// <summary>DTO for pump efficiency.</summary>
    public class PumpEfficiencyDto
    {
        public string PumpId { get; set; } = string.Empty;
        public decimal OverallEfficiency { get; set; }
    }

    /// <summary>DTO for efficiency request.</summary>
    public class EfficiencyRequestDto
    {
    }

    /// <summary>DTO for cavitation analysis.</summary>
    public class CavitationAnalysisDto
    {
        public string PumpId { get; set; } = string.Empty;
        public string CavitationRisk { get; set; } = string.Empty;
    }

    /// <summary>DTO for cavitation request.</summary>
    public class CavitationRequestDto
    {
    }

    /// <summary>DTO for vibration analysis.</summary>
    public class VibrationAnalysisDto
    {
        public string PumpId { get; set; } = string.Empty;
        public string VibrationLevel { get; set; } = string.Empty;
    }

    /// <summary>DTO for vibration request.</summary>
    public class VibrationRequestDto
    {
    }

    /// <summary>DTO for pressure dynamics.</summary>
    public class PressureDynamicsDto
    {
        public string PumpId { get; set; } = string.Empty;
        public decimal AveragePressure { get; set; }
    }

    /// <summary>DTO for pressure request.</summary>
    public class PressureRequestDto
    {
    }

    /// <summary>DTO for flow characteristics.</summary>
    public class FlowCharacteristicsDto
    {
        public string PumpId { get; set; } = string.Empty;
        public decimal FlowRate { get; set; }
    }

    /// <summary>DTO for flow request.</summary>
    public class FlowRequestDto
    {
    }

    #endregion

    #region Optimization DTOs

    /// <summary>DTO for optimization request.</summary>
    public class OptimizationRequestDto
    {
    }

    /// <summary>DTO for parameter adjustment.</summary>
    public class ParameterAdjustmentDto
    {
        public string PumpId { get; set; } = string.Empty;
        public List<string> Recommendations { get; set; } = new();
    }

    /// <summary>DTO for efficiency improvement.</summary>
    public class EfficiencyImprovementDto
    {
        public string PumpId { get; set; } = string.Empty;
        public decimal PotentialImprovement { get; set; }
    }

    /// <summary>DTO for pump comparison.</summary>
    public class PumpComparisonDto
    {
        public string ComparisonId { get; set; } = string.Empty;
        public int PumpCount { get; set; }
    }

    /// <summary>DTO for comparison criteria.</summary>
    public class ComparisonCriteriaDto
    {
    }

    /// <summary>DTO for pump upgrade recommendation.</summary>
    public class PumpUpgradeRecommendationDto
    {
        public string PumpId { get; set; } = string.Empty;
        public bool UpgradeRecommended { get; set; }
    }

    /// <summary>DTO for upgrade request.</summary>
    public class UpgradeRequestDto
    {
    }

    #endregion

    #region Monitoring and Diagnostics DTOs

    /// <summary>DTO for pump monitoring data.</summary>
    public class PumpMonitoringDataDto
    {
        public string PumpId { get; set; } = string.Empty;
        public bool MonitoringActive { get; set; }
    }

    /// <summary>DTO for monitoring request.</summary>
    public class MonitoringRequestDto
    {
    }

    /// <summary>DTO for diagnostics result.</summary>
    public class DiagnosticsResultDto
    {
        public string PumpId { get; set; } = string.Empty;
        public string DiagnosticsStatus { get; set; } = string.Empty;
    }

    /// <summary>DTO for diagnostics request.</summary>
    public class DiagnosticsRequestDto
    {
    }

    /// <summary>DTO for condition assessment.</summary>
    public class ConditionAssessmentDto
    {
        public string PumpId { get; set; } = string.Empty;
        public string ConditionRating { get; set; } = string.Empty;
    }

    /// <summary>DTO for anomaly detection.</summary>
    public class AnomalyDetectionDto
    {
        public string PumpId { get; set; } = string.Empty;
        public bool AnomaliesDetected { get; set; }
    }

    /// <summary>DTO for predictive maintenance.</summary>
    public class PredictiveMaintenanceDto
    {
        public string PumpId { get; set; } = string.Empty;
        public DateTime NextMaintenanceDate { get; set; }
    }

    /// <summary>DTO for maintenance request.</summary>
    public class MaintenanceRequestDto
    {
    }

    #endregion

    #region Reliability DTOs

    /// <summary>DTO for failure mode analysis.</summary>
    public class FailureModeAnalysisDto
    {
        public string PumpId { get; set; } = string.Empty;
        public string FailureRisk { get; set; } = string.Empty;
    }

    /// <summary>DTO for reliability assessment.</summary>
    public class ReliabilityAssessmentDto
    {
        public string PumpId { get; set; } = string.Empty;
        public decimal ReliabilityScore { get; set; }
    }

    /// <summary>DTO for reliability request.</summary>
    public class ReliabilityRequestDto
    {
    }

    /// <summary>DTO for MTBF calculation.</summary>
    public class MTBFCalculationDto
    {
        public string PumpId { get; set; } = string.Empty;
        public decimal MTBF_Hours { get; set; }
    }

    /// <summary>DTO for wear analysis.</summary>
    public class WearAnalysisDto
    {
        public string PumpId { get; set; } = string.Empty;
        public decimal WearRate { get; set; }
    }

    /// <summary>DTO for wear request.</summary>
    public class WearRequestDto
    {
    }

    /// <summary>DTO for failure risk assessment.</summary>
    public class FailureRiskAssessmentDto
    {
        public string PumpId { get; set; } = string.Empty;
        public string FailureRisk { get; set; } = string.Empty;
        public decimal RiskScore { get; set; }
    }

    #endregion

    #region Maintenance DTOs

    /// <summary>DTO for maintenance schedule.</summary>
    public class MaintenanceScheduleDto
    {
        public string PumpId { get; set; } = string.Empty;
        public DateTime NextMaintenanceDate { get; set; }
    }

    /// <summary>DTO for schedule request.</summary>
    public class ScheduleRequestDto
    {
    }

    /// <summary>DTO for maintenance activity.</summary>
    public class MaintenanceActivityDto
    {
        public string ActivityId { get; set; } = string.Empty;
        public string PumpId { get; set; } = string.Empty;
        public DateTime ActivityDate { get; set; }
        public string ActivityType { get; set; } = string.Empty;
    }

    /// <summary>DTO for rebuild analysis.</summary>
    public class RebuildAnalysisDto
    {
        public string PumpId { get; set; } = string.Empty;
        public bool RebuildRequired { get; set; }
    }

    /// <summary>DTO for rebuild request.</summary>
    public class RebuildRequestDto
    {
    }

    /// <summary>DTO for parts inventory.</summary>
    public class PartsInventoryDto
    {
        public string PumpId { get; set; } = string.Empty;
        public int PartCount { get; set; }
    }

    /// <summary>DTO for parts request.</summary>
    public class PartsRequestDto
    {
    }

    /// <summary>DTO for maintenance cost estimate.</summary>
    public class MaintenanceCostEstimateDto
    {
        public string PumpId { get; set; } = string.Empty;
        public decimal EstimatedCost { get; set; }
    }

    /// <summary>DTO for cost estimate request.</summary>
    public class CostEstimateRequestDto
    {
    }

    #endregion

    #region Fluid Management DTOs

    /// <summary>DTO for hydraulic fluid analysis.</summary>
    public class FluidAnalysisDto
    {
        public string PumpId { get; set; } = string.Empty;
        public string FluidCondition { get; set; } = string.Empty;
    }

    /// <summary>DTO for fluid analysis request.</summary>
    public class FluidAnalysisRequestDto
    {
    }

    /// <summary>DTO for fluid change recommendation.</summary>
    public class FluidChangeRecommendationDto
    {
        public string PumpId { get; set; } = string.Empty;
        public bool ChangeRequired { get; set; }
    }

    /// <summary>DTO for contamination level.</summary>
    public class ContaminationLevelDto
    {
        public string PumpId { get; set; } = string.Empty;
        public string ContaminationLevel { get; set; } = string.Empty;
    }

    /// <summary>DTO for contamination request.</summary>
    public class ContaminationRequestDto
    {
    }

    /// <summary>DTO for filtration system.</summary>
    public class FiltrationSystemDto
    {
        public string PumpId { get; set; } = string.Empty;
        public string FilterStatus { get; set; } = string.Empty;
    }

    /// <summary>DTO for filtration request.</summary>
    public class FiltrationRequestDto
    {
    }

    #endregion

    #region Integration DTOs

    /// <summary>DTO for SCADA integration.</summary>
    public class SCODAIntegrationDto
    {
        public string PumpId { get; set; } = string.Empty;
        public bool IsIntegrated { get; set; }
        public List<IntegrationParameterDto> Parameters { get; set; } = new();
        public string IntegrationStatus { get; set; } = string.Empty;
    }

    /// <summary>DTO for SCADA configuration.</summary>
    public class SCADAConfigDto
    {
    }

    /// <summary>DTO for integration parameter.</summary>
    public class IntegrationParameterDto
    {
        public string ParameterName { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public string UpdateFrequency { get; set; } = string.Empty;
    }

    /// <summary>DTO for control parameters.</summary>
    public class ControlParametersDto
    {
        public string PumpId { get; set; } = string.Empty;
        public List<ControlParamDto> Parameters { get; set; } = new();
        public string ControlMode { get; set; } = string.Empty;
    }

    /// <summary>DTO for control request.</summary>
    public class ControlRequestDto
    {
    }

    /// <summary>DTO for control parameter.</summary>
    public class ControlParamDto
    {
        public string ParameterName { get; set; } = string.Empty;
        public decimal CurrentValue { get; set; }
        public decimal MinValue { get; set; }
        public decimal MaxValue { get; set; }
    }

    /// <summary>DTO for wellbore interaction.</summary>
    public class WellboreInteractionDto
    {
        public string WellUWI { get; set; } = string.Empty;
        public decimal InteractionIntensity { get; set; }
        public List<InteractionFactorDto> Factors { get; set; } = new();
        public string OverallAssessment { get; set; } = string.Empty;
    }

    /// <summary>DTO for interaction request.</summary>
    public class InteractionRequestDto
    {
    }

    /// <summary>DTO for interaction factor.</summary>
    public class InteractionFactorDto
    {
        public string FactorName { get; set; } = string.Empty;
        public decimal Impact { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    #endregion

    #region Data Management DTOs

    /// <summary>DTO for pump history.</summary>
    public class PumpHistoryDto
    {
        public string PumpId { get; set; } = string.Empty;
        public DateTime DateRecord { get; set; }
        public string EventDescription { get; set; } = string.Empty;
    }

    /// <summary>DTO for performance trends.</summary>
    public class PerformanceTrendsDto
    {
        public string PumpId { get; set; } = string.Empty;
        public List<decimal> EfficiencyTrend { get; set; } = new();
        public List<decimal> FlowTrend { get; set; } = new();
    }

    #endregion

    #region Reporting DTOs

    /// <summary>DTO for pump report.</summary>
    public class PumpReportDto
    {
        public string PumpId { get; set; } = string.Empty;
        public string ReportId { get; set; } = string.Empty;
        public DateTime GeneratedDate { get; set; }
    }

    /// <summary>DTO for report request.</summary>
    public class ReportRequestDto
    {
    }

    /// <summary>DTO for performance summary report.</summary>
    public class PerformanceSummaryReportDto
    {
        public string PumpId { get; set; } = string.Empty;
        public decimal AverageEfficiency { get; set; }
        public decimal AverageFlowRate { get; set; }
    }

    /// <summary>DTO for summary report request.</summary>
    public class SummaryReportRequestDto
    {
    }

    /// <summary>DTO for cost analysis report.</summary>
    public class CostAnalysisReportDto
    {
        public string PumpId { get; set; } = string.Empty;
        public decimal TotalCost { get; set; }
        public decimal OperatingCost { get; set; }
        public decimal MaintenanceCost { get; set; }
    }

    /// <summary>DTO for cost report request.</summary>
    public class CostReportRequestDto
    {
    }

    #endregion

    #region Legacy DTOs (kept for backward compatibility)

    /// <summary>DTO for hydraulic pump design (legacy).</summary>
    public class HydraulicPumpDesignDto
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
    public class PumpPerformanceHistoryDto
    {
        public DateTime PerformanceDate { get; set; }
        public decimal FlowRate { get; set; }
        public decimal Efficiency { get; set; }
        public decimal PowerConsumption { get; set; }
        public string? Status { get; set; }
    }

    #endregion
}

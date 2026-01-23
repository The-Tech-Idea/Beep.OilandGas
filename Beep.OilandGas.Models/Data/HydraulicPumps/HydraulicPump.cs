using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    #region Design and Sizing DTOs

    /// <summary>DTO for hydraulic pump design result.</summary>
    public class PumpDesignResult : ModelEntityBase
    {
        private string DesignIdValue = string.Empty;

        public string DesignId

        {

            get { return this.DesignIdValue; }

            set { SetProperty(ref DesignIdValue, value); }

        }
        private string WellUWIValue = string.Empty;

        public string WellUWI

        {

            get { return this.WellUWIValue; }

            set { SetProperty(ref WellUWIValue, value); }

        }
        private string PumpTypeValue = string.Empty;

        public string PumpType

        {

            get { return this.PumpTypeValue; }

            set { SetProperty(ref PumpTypeValue, value); }

        }
        private DateTime DesignDateValue;

        public DateTime DesignDate

        {

            get { return this.DesignDateValue; }

            set { SetProperty(ref DesignDateValue, value); }

        }
        private decimal PumpDepthValue;

        public decimal PumpDepth

        {

            get { return this.PumpDepthValue; }

            set { SetProperty(ref PumpDepthValue, value); }

        }
        private decimal TubingSizeValue;

        public decimal TubingSize

        {

            get { return this.TubingSizeValue; }

            set { SetProperty(ref TubingSizeValue, value); }

        }
        private decimal CasingSizeValue;

        public decimal CasingSize

        {

            get { return this.CasingSizeValue; }

            set { SetProperty(ref CasingSizeValue, value); }

        }
        private decimal DesignFlowRateValue;

        public decimal DesignFlowRate

        {

            get { return this.DesignFlowRateValue; }

            set { SetProperty(ref DesignFlowRateValue, value); }

        }
        private decimal DesignPressureValue;

        public decimal DesignPressure

        {

            get { return this.DesignPressureValue; }

            set { SetProperty(ref DesignPressureValue, value); }

        }
        private decimal ExpectedEfficiencyValue;

        public decimal ExpectedEfficiency

        {

            get { return this.ExpectedEfficiencyValue; }

            set { SetProperty(ref ExpectedEfficiencyValue, value); }

        }
        private decimal EstimatedPowerValue;

        public decimal EstimatedPower

        {

            get { return this.EstimatedPowerValue; }

            set { SetProperty(ref EstimatedPowerValue, value); }

        }
        private string StatusValue = "Designed";

        public string Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        private List<string> DesignRecommendationsValue = new();

        public List<string> DesignRecommendations

        {

            get { return this.DesignRecommendationsValue; }

            set { SetProperty(ref DesignRecommendationsValue, value); }

        }
    }

    /// <summary>DTO for pump design request.</summary>
    public class PumpDesignRequest : ModelEntityBase
    {
        private string PumpTypeValue = string.Empty;

        public string PumpType

        {

            get { return this.PumpTypeValue; }

            set { SetProperty(ref PumpTypeValue, value); }

        }
        private decimal WellDepthValue;

        public decimal WellDepth

        {

            get { return this.WellDepthValue; }

            set { SetProperty(ref WellDepthValue, value); }

        }
        private decimal DesiredFlowRateValue;

        public decimal DesiredFlowRate

        {

            get { return this.DesiredFlowRateValue; }

            set { SetProperty(ref DesiredFlowRateValue, value); }

        }
        private decimal ReservoirPressureValue;

        public decimal ReservoirPressure

        {

            get { return this.ReservoirPressureValue; }

            set { SetProperty(ref ReservoirPressureValue, value); }

        }
        private decimal TubingSizeValue;

        public decimal TubingSize

        {

            get { return this.TubingSizeValue; }

            set { SetProperty(ref TubingSizeValue, value); }

        }
        private decimal CasingSizeValue;

        public decimal CasingSize

        {

            get { return this.CasingSizeValue; }

            set { SetProperty(ref CasingSizeValue, value); }

        }
    }

    /// <summary>DTO for pump sizing result.</summary>
    public class PumpSizingResult : ModelEntityBase
    {
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private decimal RecommendedDisplacementValue;

        public decimal RecommendedDisplacement

        {

            get { return this.RecommendedDisplacementValue; }

            set { SetProperty(ref RecommendedDisplacementValue, value); }

        }
        private decimal MaximumFlowRateValue;

        public decimal MaximumFlowRate

        {

            get { return this.MaximumFlowRateValue; }

            set { SetProperty(ref MaximumFlowRateValue, value); }

        }
        private decimal MaximumPressureValue;

        public decimal MaximumPressure

        {

            get { return this.MaximumPressureValue; }

            set { SetProperty(ref MaximumPressureValue, value); }

        }
        private decimal OptimalSpeedValue;

        public decimal OptimalSpeed

        {

            get { return this.OptimalSpeedValue; }

            set { SetProperty(ref OptimalSpeedValue, value); }

        }
        private string RecommendedPumpModelValue = string.Empty;

        public string RecommendedPumpModel

        {

            get { return this.RecommendedPumpModelValue; }

            set { SetProperty(ref RecommendedPumpModelValue, value); }

        }
    }

    /// <summary>DTO for pump sizing request.</summary>
    public class PumpSizingRequest : ModelEntityBase
    {
        private decimal DesiredFlowRateValue;

        public decimal DesiredFlowRate

        {

            get { return this.DesiredFlowRateValue; }

            set { SetProperty(ref DesiredFlowRateValue, value); }

        }
        private decimal MaxOperatingPressureValue;

        public decimal MaxOperatingPressure

        {

            get { return this.MaxOperatingPressureValue; }

            set { SetProperty(ref MaxOperatingPressureValue, value); }

        }
        private decimal FluidViscosityValue;

        public decimal FluidViscosity

        {

            get { return this.FluidViscosityValue; }

            set { SetProperty(ref FluidViscosityValue, value); }

        }
    }

    /// <summary>DTO for pump type selection.</summary>
    public class PumpTypeSelection : ModelEntityBase
    {
        private string WellUWIValue = string.Empty;

        public string WellUWI

        {

            get { return this.WellUWIValue; }

            set { SetProperty(ref WellUWIValue, value); }

        }
        private List<PumpTypeOption> OptionsValue = new();

        public List<PumpTypeOption> Options

        {

            get { return this.OptionsValue; }

            set { SetProperty(ref OptionsValue, value); }

        }
        private string RecommendedTypeValue = string.Empty;

        public string RecommendedType

        {

            get { return this.RecommendedTypeValue; }

            set { SetProperty(ref RecommendedTypeValue, value); }

        }
        private string SelectionRationaleValue = string.Empty;

        public string SelectionRationale

        {

            get { return this.SelectionRationaleValue; }

            set { SetProperty(ref SelectionRationaleValue, value); }

        }
    }

    /// <summary>DTO for pump type option.</summary>
    public class PumpTypeOption : ModelEntityBase
    {
        private string PumpTypeValue = string.Empty;

        public string PumpType

        {

            get { return this.PumpTypeValue; }

            set { SetProperty(ref PumpTypeValue, value); }

        }
        private decimal EfficiencyValue;

        public decimal Efficiency

        {

            get { return this.EfficiencyValue; }

            set { SetProperty(ref EfficiencyValue, value); }

        }
        private decimal CostValue;

        public decimal Cost

        {

            get { return this.CostValue; }

            set { SetProperty(ref CostValue, value); }

        }
        private decimal ReliabilityValue;

        public decimal Reliability

        {

            get { return this.ReliabilityValue; }

            set { SetProperty(ref ReliabilityValue, value); }

        }
        private List<string> AdvantagesValue = new();

        public List<string> Advantages

        {

            get { return this.AdvantagesValue; }

            set { SetProperty(ref AdvantagesValue, value); }

        }
        private List<string> DisadvantagesValue = new();

        public List<string> Disadvantages

        {

            get { return this.DisadvantagesValue; }

            set { SetProperty(ref DisadvantagesValue, value); }

        }
    }

    /// <summary>DTO for pump selection criteria.</summary>
    public class PumpSelectionCriteria : ModelEntityBase
    {
        private List<string> PriorityFactorsValue = new();

        public List<string> PriorityFactors

        {

            get { return this.PriorityFactorsValue; }

            set { SetProperty(ref PriorityFactorsValue, value); }

        }
    }

    /// <summary>DTO for power requirements.</summary>
    public class PowerRequirements : ModelEntityBase
    {
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private decimal HydraulicPowerValue;

        public decimal HydraulicPower

        {

            get { return this.HydraulicPowerValue; }

            set { SetProperty(ref HydraulicPowerValue, value); }

        }
        private decimal MechanicalPowerValue;

        public decimal MechanicalPower

        {

            get { return this.MechanicalPowerValue; }

            set { SetProperty(ref MechanicalPowerValue, value); }

        }
        private decimal TotalPowerRequiredValue;

        public decimal TotalPowerRequired

        {

            get { return this.TotalPowerRequiredValue; }

            set { SetProperty(ref TotalPowerRequiredValue, value); }

        }
        private decimal PowerEfficiencyValue;

        public decimal PowerEfficiency

        {

            get { return this.PowerEfficiencyValue; }

            set { SetProperty(ref PowerEfficiencyValue, value); }

        }
        private string PowerUnitValue = "HP";

        public string PowerUnit

        {

            get { return this.PowerUnitValue; }

            set { SetProperty(ref PowerUnitValue, value); }

        }
    }

    /// <summary>DTO for power calculation request.</summary>
    public class PowerCalculationRequest : ModelEntityBase
    {
        private decimal FlowRateValue;

        public decimal FlowRate

        {

            get { return this.FlowRateValue; }

            set { SetProperty(ref FlowRateValue, value); }

        }
        private decimal DischargePressureValue;

        public decimal DischargePressure

        {

            get { return this.DischargePressureValue; }

            set { SetProperty(ref DischargePressureValue, value); }

        }
        private decimal SuctionPressureValue;

        public decimal SuctionPressure

        {

            get { return this.SuctionPressureValue; }

            set { SetProperty(ref SuctionPressureValue, value); }

        }
    }

    /// <summary>DTO for hydraulic balance.</summary>
    public class HydraulicBalance : ModelEntityBase
    {
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private bool IsBalancedValue;

        public bool IsBalanced

        {

            get { return this.IsBalancedValue; }

            set { SetProperty(ref IsBalancedValue, value); }

        }
        private decimal BalanceScoreValue;

        public decimal BalanceScore

        {

            get { return this.BalanceScoreValue; }

            set { SetProperty(ref BalanceScoreValue, value); }

        }
        private List<BalanceFactor> FactorsValue = new();

        public List<BalanceFactor> Factors

        {

            get { return this.FactorsValue; }

            set { SetProperty(ref FactorsValue, value); }

        }
        private List<string> RecommendationsValue = new();

        public List<string> Recommendations

        {

            get { return this.RecommendationsValue; }

            set { SetProperty(ref RecommendationsValue, value); }

        }
    }

    /// <summary>DTO for balance request.</summary>
    public class BalanceRequest : ModelEntityBase
    {
        private decimal DischargeFlowValue;

        public decimal DischargeFlow

        {

            get { return this.DischargeFlowValue; }

            set { SetProperty(ref DischargeFlowValue, value); }

        }
        private decimal DischargePressureValue;

        public decimal DischargePressure

        {

            get { return this.DischargePressureValue; }

            set { SetProperty(ref DischargePressureValue, value); }

        }
    }

    /// <summary>DTO for balance factor.</summary>
    public class BalanceFactor : ModelEntityBase
    {
        private string FactorNameValue = string.Empty;

        public string FactorName

        {

            get { return this.FactorNameValue; }

            set { SetProperty(ref FactorNameValue, value); }

        }
        private decimal ValueValue;

        public decimal Value

        {

            get { return this.ValueValue; }

            set { SetProperty(ref ValueValue, value); }

        }
        private string StatusValue = string.Empty;

        public string Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
    }

    /// <summary>DTO for rod string design.</summary>
    public class RodStringDesign : ModelEntityBase
    {
        private string DesignIdValue = string.Empty;

        public string DesignId

        {

            get { return this.DesignIdValue; }

            set { SetProperty(ref DesignIdValue, value); }

        }
        private string WellUWIValue = string.Empty;

        public string WellUWI

        {

            get { return this.WellUWIValue; }

            set { SetProperty(ref WellUWIValue, value); }

        }
        private string RodSizeValue = string.Empty;

        public string RodSize

        {

            get { return this.RodSizeValue; }

            set { SetProperty(ref RodSizeValue, value); }

        }
        private string RodGradeValue = string.Empty;

        public string RodGrade

        {

            get { return this.RodGradeValue; }

            set { SetProperty(ref RodGradeValue, value); }

        }
        private int RodSectionsValue;

        public int RodSections

        {

            get { return this.RodSectionsValue; }

            set { SetProperty(ref RodSectionsValue, value); }

        }
        private decimal TotalRodLengthValue;

        public decimal TotalRodLength

        {

            get { return this.TotalRodLengthValue; }

            set { SetProperty(ref TotalRodLengthValue, value); }

        }
        private decimal SafetyFactorValue;

        public decimal SafetyFactor

        {

            get { return this.SafetyFactorValue; }

            set { SetProperty(ref SafetyFactorValue, value); }

        }
        private bool IsAdequateValue;

        public bool IsAdequate

        {

            get { return this.IsAdequateValue; }

            set { SetProperty(ref IsAdequateValue, value); }

        }
    }

    /// <summary>DTO for rod string request.</summary>
    public class RodStringRequest : ModelEntityBase
    {
        private decimal WellDepthValue;

        public decimal WellDepth

        {

            get { return this.WellDepthValue; }

            set { SetProperty(ref WellDepthValue, value); }

        }
        private decimal PumpingLoadValue;

        public decimal PumpingLoad

        {

            get { return this.PumpingLoadValue; }

            set { SetProperty(ref PumpingLoadValue, value); }

        }
    }

    #endregion

    #region Performance Analysis DTOs

    /// <summary>DTO for pump performance analysis.</summary>
    public class PumpPerformanceAnalysis : ModelEntityBase
    {
        private string AnalysisIdValue = string.Empty;

        public string AnalysisId

        {

            get { return this.AnalysisIdValue; }

            set { SetProperty(ref AnalysisIdValue, value); }

        }
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private decimal CurrentFlowRateValue;

        public decimal CurrentFlowRate

        {

            get { return this.CurrentFlowRateValue; }

            set { SetProperty(ref CurrentFlowRateValue, value); }

        }
        private decimal CurrentEfficiencyValue;

        public decimal CurrentEfficiency

        {

            get { return this.CurrentEfficiencyValue; }

            set { SetProperty(ref CurrentEfficiencyValue, value); }

        }
        private List<string> RecommendationsValue = new();

        public List<string> Recommendations

        {

            get { return this.RecommendationsValue; }

            set { SetProperty(ref RecommendationsValue, value); }

        }
    }

    /// <summary>DTO for performance analysis request.</summary>
    public class PerformanceAnalysisRequest : ModelEntityBase
    {
    }

    /// <summary>DTO for pump efficiency.</summary>
    public class PumpEfficiency : ModelEntityBase
    {
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private decimal OverallEfficiencyValue;

        public decimal OverallEfficiency

        {

            get { return this.OverallEfficiencyValue; }

            set { SetProperty(ref OverallEfficiencyValue, value); }

        }
    }

    /// <summary>DTO for efficiency request.</summary>
    public class EfficiencyRequest : ModelEntityBase
    {
    }

    /// <summary>DTO for cavitation analysis.</summary>
    public class CavitationAnalysis : ModelEntityBase
    {
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private string CavitationRiskValue = string.Empty;

        public string CavitationRisk

        {

            get { return this.CavitationRiskValue; }

            set { SetProperty(ref CavitationRiskValue, value); }

        }
    }

    /// <summary>DTO for cavitation request.</summary>
    public class CavitationRequest : ModelEntityBase
    {
    }

    /// <summary>DTO for vibration analysis.</summary>
    public class VibrationAnalysis : ModelEntityBase
    {
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private string VibrationLevelValue = string.Empty;

        public string VibrationLevel

        {

            get { return this.VibrationLevelValue; }

            set { SetProperty(ref VibrationLevelValue, value); }

        }
    }

    /// <summary>DTO for vibration request.</summary>
    public class VibrationRequest : ModelEntityBase
    {
    }

    /// <summary>DTO for pressure dynamics.</summary>
    public class PressureDynamics : ModelEntityBase
    {
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private decimal AveragePressureValue;

        public decimal AveragePressure

        {

            get { return this.AveragePressureValue; }

            set { SetProperty(ref AveragePressureValue, value); }

        }
    }

    /// <summary>DTO for pressure request.</summary>
    public class PressureRequest : ModelEntityBase
    {
    }

    /// <summary>DTO for flow characteristics.</summary>
    public class FlowCharacteristics : ModelEntityBase
    {
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private decimal FlowRateValue;

        public decimal FlowRate

        {

            get { return this.FlowRateValue; }

            set { SetProperty(ref FlowRateValue, value); }

        }
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
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private List<string> RecommendationsValue = new();

        public List<string> Recommendations

        {

            get { return this.RecommendationsValue; }

            set { SetProperty(ref RecommendationsValue, value); }

        }
    }

    /// <summary>DTO for efficiency improvement.</summary>
    public class EfficiencyImprovement : ModelEntityBase
    {
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private decimal PotentialImprovementValue;

        public decimal PotentialImprovement

        {

            get { return this.PotentialImprovementValue; }

            set { SetProperty(ref PotentialImprovementValue, value); }

        }
    }

    /// <summary>DTO for pump comparison.</summary>
    public class PumpComparison : ModelEntityBase
    {
        private string ComparisonIdValue = string.Empty;

        public string ComparisonId

        {

            get { return this.ComparisonIdValue; }

            set { SetProperty(ref ComparisonIdValue, value); }

        }
        private int PumpCountValue;

        public int PumpCount

        {

            get { return this.PumpCountValue; }

            set { SetProperty(ref PumpCountValue, value); }

        }
    }

    /// <summary>DTO for comparison criteria.</summary>
    public class ComparisonCriteria : ModelEntityBase
    {
    }

    /// <summary>DTO for pump upgrade recommendation.</summary>
    public class PumpUpgradeRecommendation : ModelEntityBase
    {
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private bool UpgradeRecommendedValue;

        public bool UpgradeRecommended

        {

            get { return this.UpgradeRecommendedValue; }

            set { SetProperty(ref UpgradeRecommendedValue, value); }

        }
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
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private bool MonitoringActiveValue;

        public bool MonitoringActive

        {

            get { return this.MonitoringActiveValue; }

            set { SetProperty(ref MonitoringActiveValue, value); }

        }
    }

    /// <summary>DTO for monitoring request.</summary>
    public class MonitoringRequest : ModelEntityBase
    {
    }

    /// <summary>DTO for diagnostics result.</summary>
    public class DiagnosticsResult : ModelEntityBase
    {
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private string DiagnosticsStatusValue = string.Empty;

        public string DiagnosticsStatus

        {

            get { return this.DiagnosticsStatusValue; }

            set { SetProperty(ref DiagnosticsStatusValue, value); }

        }
    }

    /// <summary>DTO for diagnostics request.</summary>
    public class DiagnosticsRequest : ModelEntityBase
    {
    }

    /// <summary>DTO for condition assessment.</summary>
    public class ConditionAssessment : ModelEntityBase
    {
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private string ConditionRatingValue = string.Empty;

        public string ConditionRating

        {

            get { return this.ConditionRatingValue; }

            set { SetProperty(ref ConditionRatingValue, value); }

        }
    }

    /// <summary>DTO for anomaly detection.</summary>
    public class AnomalyDetection : ModelEntityBase
    {
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private bool AnomaliesDetectedValue;

        public bool AnomaliesDetected

        {

            get { return this.AnomaliesDetectedValue; }

            set { SetProperty(ref AnomaliesDetectedValue, value); }

        }
    }

    /// <summary>DTO for predictive maintenance.</summary>
    public class PredictiveMaintenance : ModelEntityBase
    {
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private DateTime NextMaintenanceDateValue;

        public DateTime NextMaintenanceDate

        {

            get { return this.NextMaintenanceDateValue; }

            set { SetProperty(ref NextMaintenanceDateValue, value); }

        }
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
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private string FailureRiskValue = string.Empty;

        public string FailureRisk

        {

            get { return this.FailureRiskValue; }

            set { SetProperty(ref FailureRiskValue, value); }

        }
    }

    /// <summary>DTO for reliability assessment.</summary>
    public class ReliabilityAssessment : ModelEntityBase
    {
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private decimal ReliabilityScoreValue;

        public decimal ReliabilityScore

        {

            get { return this.ReliabilityScoreValue; }

            set { SetProperty(ref ReliabilityScoreValue, value); }

        }
    }

    /// <summary>DTO for reliability request.</summary>
    public class ReliabilityRequest : ModelEntityBase
    {
    }

    /// <summary>DTO for MTBF calculation.</summary>
    public class MTBFCalculation : ModelEntityBase
    {
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private decimal MTBF_HoursValue;

        public decimal MTBF_Hours

        {

            get { return this.MTBF_HoursValue; }

            set { SetProperty(ref MTBF_HoursValue, value); }

        }
    }

    /// <summary>DTO for wear analysis.</summary>
    public class WearAnalysis : ModelEntityBase
    {
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private decimal WearRateValue;

        public decimal WearRate

        {

            get { return this.WearRateValue; }

            set { SetProperty(ref WearRateValue, value); }

        }
    }

    /// <summary>DTO for wear request.</summary>
    public class WearRequest : ModelEntityBase
    {
    }

    /// <summary>DTO for failure risk assessment.</summary>
    public class FailureRiskAssessment : ModelEntityBase
    {
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private string FailureRiskValue = string.Empty;

        public string FailureRisk

        {

            get { return this.FailureRiskValue; }

            set { SetProperty(ref FailureRiskValue, value); }

        }
        private decimal RiskScoreValue;

        public decimal RiskScore

        {

            get { return this.RiskScoreValue; }

            set { SetProperty(ref RiskScoreValue, value); }

        }
    }

    #endregion

    #region Maintenance DTOs

    /// <summary>DTO for maintenance schedule.</summary>
    public class MaintenanceSchedule : ModelEntityBase
    {
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private DateTime NextMaintenanceDateValue;

        public DateTime NextMaintenanceDate

        {

            get { return this.NextMaintenanceDateValue; }

            set { SetProperty(ref NextMaintenanceDateValue, value); }

        }
    }

    /// <summary>DTO for schedule request.</summary>
    public class ScheduleRequest : ModelEntityBase
    {
    }

    /// <summary>DTO for maintenance activity.</summary>
    public class MaintenanceActivity : ModelEntityBase
    {
        private string ActivityIdValue = string.Empty;

        public string ActivityId

        {

            get { return this.ActivityIdValue; }

            set { SetProperty(ref ActivityIdValue, value); }

        }
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private DateTime ActivityDateValue;

        public DateTime ActivityDate

        {

            get { return this.ActivityDateValue; }

            set { SetProperty(ref ActivityDateValue, value); }

        }
        private string ActivityTypeValue = string.Empty;

        public string ActivityType

        {

            get { return this.ActivityTypeValue; }

            set { SetProperty(ref ActivityTypeValue, value); }

        }
    }

    /// <summary>DTO for rebuild analysis.</summary>
    public class RebuildAnalysis : ModelEntityBase
    {
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private bool RebuildRequiredValue;

        public bool RebuildRequired

        {

            get { return this.RebuildRequiredValue; }

            set { SetProperty(ref RebuildRequiredValue, value); }

        }
    }

    /// <summary>DTO for rebuild request.</summary>
    public class RebuildRequest : ModelEntityBase
    {
    }

    /// <summary>DTO for parts inventory.</summary>
    public class PartsInventory : ModelEntityBase
    {
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private int PartCountValue;

        public int PartCount

        {

            get { return this.PartCountValue; }

            set { SetProperty(ref PartCountValue, value); }

        }
    }

    /// <summary>DTO for parts request.</summary>
    public class PartsRequest : ModelEntityBase
    {
    }

    /// <summary>DTO for maintenance cost estimate.</summary>
    public class MaintenanceCostEstimate : ModelEntityBase
    {
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private decimal EstimatedCostValue;

        public decimal EstimatedCost

        {

            get { return this.EstimatedCostValue; }

            set { SetProperty(ref EstimatedCostValue, value); }

        }
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
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private string FluidConditionValue = string.Empty;

        public string FluidCondition

        {

            get { return this.FluidConditionValue; }

            set { SetProperty(ref FluidConditionValue, value); }

        }
    }

    /// <summary>DTO for fluid analysis request.</summary>
    public class FluidAnalysisRequest : ModelEntityBase
    {
    }

    /// <summary>DTO for fluid change recommendation.</summary>
    public class FluidChangeRecommendation : ModelEntityBase
    {
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private bool ChangeRequiredValue;

        public bool ChangeRequired

        {

            get { return this.ChangeRequiredValue; }

            set { SetProperty(ref ChangeRequiredValue, value); }

        }
    }

    /// <summary>DTO for contamination level.</summary>
    public class ContaminationLevel : ModelEntityBase
    {
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private string LevelValue = string.Empty;

        public string Level

        {

            get { return this.LevelValue; }

            set { SetProperty(ref LevelValue, value); }

        }
    }

    /// <summary>DTO for contamination request.</summary>
    public class ContaminationRequest : ModelEntityBase
    {
    }

    /// <summary>DTO for filtration system.</summary>
    public class FiltrationSystem : ModelEntityBase
    {
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private string FilterStatusValue = string.Empty;

        public string FilterStatus

        {

            get { return this.FilterStatusValue; }

            set { SetProperty(ref FilterStatusValue, value); }

        }
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
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private bool IsIntegratedValue;

        public bool IsIntegrated

        {

            get { return this.IsIntegratedValue; }

            set { SetProperty(ref IsIntegratedValue, value); }

        }
        private List<IntegrationParameter> ParametersValue = new();

        public List<IntegrationParameter> Parameters

        {

            get { return this.ParametersValue; }

            set { SetProperty(ref ParametersValue, value); }

        }
        private string IntegrationStatusValue = string.Empty;

        public string IntegrationStatus

        {

            get { return this.IntegrationStatusValue; }

            set { SetProperty(ref IntegrationStatusValue, value); }

        }
    }

    /// <summary>DTO for SCADA configuration.</summary>
    public class SCADAConfig : ModelEntityBase
    {
    }

    /// <summary>DTO for integration parameter.</summary>
    public class IntegrationParameter : ModelEntityBase
    {
        private string ParameterNameValue = string.Empty;

        public string ParameterName

        {

            get { return this.ParameterNameValue; }

            set { SetProperty(ref ParameterNameValue, value); }

        }
        private string DataTypeValue = string.Empty;

        public string DataType

        {

            get { return this.DataTypeValue; }

            set { SetProperty(ref DataTypeValue, value); }

        }
        private string UpdateFrequencyValue = string.Empty;

        public string UpdateFrequency

        {

            get { return this.UpdateFrequencyValue; }

            set { SetProperty(ref UpdateFrequencyValue, value); }

        }
    }

    /// <summary>DTO for control parameters.</summary>
    public class ControlParameters : ModelEntityBase
    {
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private List<ControlParam> ParametersValue = new();

        public List<ControlParam> Parameters

        {

            get { return this.ParametersValue; }

            set { SetProperty(ref ParametersValue, value); }

        }
        private string ControlModeValue = string.Empty;

        public string ControlMode

        {

            get { return this.ControlModeValue; }

            set { SetProperty(ref ControlModeValue, value); }

        }
    }

    /// <summary>DTO for control request.</summary>
    public class ControlRequest : ModelEntityBase
    {
    }

    /// <summary>DTO for control parameter.</summary>
    public class ControlParam : ModelEntityBase
    {
        private string ParameterNameValue = string.Empty;

        public string ParameterName

        {

            get { return this.ParameterNameValue; }

            set { SetProperty(ref ParameterNameValue, value); }

        }
        private decimal CurrentValueValue;

        public decimal CurrentValue

        {

            get { return this.CurrentValueValue; }

            set { SetProperty(ref CurrentValueValue, value); }

        }
        private decimal MinValueValue;

        public decimal MinValue

        {

            get { return this.MinValueValue; }

            set { SetProperty(ref MinValueValue, value); }

        }
        private decimal MaxValueValue;

        public decimal MaxValue

        {

            get { return this.MaxValueValue; }

            set { SetProperty(ref MaxValueValue, value); }

        }
    }

    /// <summary>DTO for wellbore interaction.</summary>
    public class WellboreInteraction : ModelEntityBase
    {
        private string WellUWIValue = string.Empty;

        public string WellUWI

        {

            get { return this.WellUWIValue; }

            set { SetProperty(ref WellUWIValue, value); }

        }
        private decimal InteractionIntensityValue;

        public decimal InteractionIntensity

        {

            get { return this.InteractionIntensityValue; }

            set { SetProperty(ref InteractionIntensityValue, value); }

        }
        private List<InteractionFactor> FactorsValue = new();

        public List<InteractionFactor> Factors

        {

            get { return this.FactorsValue; }

            set { SetProperty(ref FactorsValue, value); }

        }
        private string OverallAssessmentValue = string.Empty;

        public string OverallAssessment

        {

            get { return this.OverallAssessmentValue; }

            set { SetProperty(ref OverallAssessmentValue, value); }

        }
    }

    /// <summary>DTO for interaction request.</summary>
    public class InteractionRequest : ModelEntityBase
    {
    }

    /// <summary>DTO for interaction factor.</summary>
    public class InteractionFactor : ModelEntityBase
    {
        private string FactorNameValue = string.Empty;

        public string FactorName

        {

            get { return this.FactorNameValue; }

            set { SetProperty(ref FactorNameValue, value); }

        }
        private decimal ImpactValue;

        public decimal Impact

        {

            get { return this.ImpactValue; }

            set { SetProperty(ref ImpactValue, value); }

        }
        private string DescriptionValue = string.Empty;

        public string Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
    }

    #endregion

    #region Data Management DTOs

    /// <summary>DTO for pump history.</summary>
    public class PumpHistory : ModelEntityBase
    {
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private DateTime DateRecordValue;

        public DateTime DateRecord

        {

            get { return this.DateRecordValue; }

            set { SetProperty(ref DateRecordValue, value); }

        }
        private string EventDescriptionValue = string.Empty;

        public string EventDescription

        {

            get { return this.EventDescriptionValue; }

            set { SetProperty(ref EventDescriptionValue, value); }

        }
    }

    /// <summary>DTO for performance trends.</summary>
    public class PerformanceTrends : ModelEntityBase
    {
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private List<decimal> EfficiencyTrendValue = new();

        public List<decimal> EfficiencyTrend

        {

            get { return this.EfficiencyTrendValue; }

            set { SetProperty(ref EfficiencyTrendValue, value); }

        }
        private List<decimal> FlowTrendValue = new();

        public List<decimal> FlowTrend

        {

            get { return this.FlowTrendValue; }

            set { SetProperty(ref FlowTrendValue, value); }

        }
    }

    #endregion

    #region Reporting DTOs

    /// <summary>DTO for pump report.</summary>
    public class PumpReport : ModelEntityBase
    {
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private string ReportIdValue = string.Empty;

        public string ReportId

        {

            get { return this.ReportIdValue; }

            set { SetProperty(ref ReportIdValue, value); }

        }
        private DateTime GeneratedDateValue;

        public DateTime GeneratedDate

        {

            get { return this.GeneratedDateValue; }

            set { SetProperty(ref GeneratedDateValue, value); }

        }
    }

    /// <summary>DTO for report request.</summary>
    public class ReportRequest : ModelEntityBase
    {
    }

    /// <summary>DTO for performance summary report.</summary>
    public class PerformanceSummaryReport : ModelEntityBase
    {
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private decimal AverageEfficiencyValue;

        public decimal AverageEfficiency

        {

            get { return this.AverageEfficiencyValue; }

            set { SetProperty(ref AverageEfficiencyValue, value); }

        }
        private decimal AverageFlowRateValue;

        public decimal AverageFlowRate

        {

            get { return this.AverageFlowRateValue; }

            set { SetProperty(ref AverageFlowRateValue, value); }

        }
    }

    /// <summary>DTO for summary report request.</summary>
    public class SummaryReportRequest : ModelEntityBase
    {
    }

    /// <summary>DTO for cost analysis report.</summary>
    public class CostAnalysisReport : ModelEntityBase
    {
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private decimal TotalCostValue;

        public decimal TotalCost

        {

            get { return this.TotalCostValue; }

            set { SetProperty(ref TotalCostValue, value); }

        }
        private decimal OperatingCostValue;

        public decimal OperatingCost

        {

            get { return this.OperatingCostValue; }

            set { SetProperty(ref OperatingCostValue, value); }

        }
        private decimal MaintenanceCostValue;

        public decimal MaintenanceCost

        {

            get { return this.MaintenanceCostValue; }

            set { SetProperty(ref MaintenanceCostValue, value); }

        }
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
        private string DesignIdValue = string.Empty;

        public string DesignId

        {

            get { return this.DesignIdValue; }

            set { SetProperty(ref DesignIdValue, value); }

        }
        private string WellUWIValue = string.Empty;

        public string WellUWI

        {

            get { return this.WellUWIValue; }

            set { SetProperty(ref WellUWIValue, value); }

        }
        private string PumpTypeValue = string.Empty;

        public string PumpType

        {

            get { return this.PumpTypeValue; }

            set { SetProperty(ref PumpTypeValue, value); }

        }
        private DateTime DesignDateValue;

        public DateTime DesignDate

        {

            get { return this.DesignDateValue; }

            set { SetProperty(ref DesignDateValue, value); }

        }
        private decimal PumpDepthValue;

        public decimal PumpDepth

        {

            get { return this.PumpDepthValue; }

            set { SetProperty(ref PumpDepthValue, value); }

        }
        private decimal PumpSizeValue;

        public decimal PumpSize

        {

            get { return this.PumpSizeValue; }

            set { SetProperty(ref PumpSizeValue, value); }

        }
        private decimal OperatingPressureValue;

        public decimal OperatingPressure

        {

            get { return this.OperatingPressureValue; }

            set { SetProperty(ref OperatingPressureValue, value); }

        }
        private decimal FlowRateValue;

        public decimal FlowRate

        {

            get { return this.FlowRateValue; }

            set { SetProperty(ref FlowRateValue, value); }

        }
        private decimal EfficiencyValue;

        public decimal Efficiency

        {

            get { return this.EfficiencyValue; }

            set { SetProperty(ref EfficiencyValue, value); }

        }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
    }

    /// <summary>DTO for pump performance history point (legacy).</summary>
    public class PumpPerformanceHistory : ModelEntityBase
    {
        private DateTime PerformanceDateValue;

        public DateTime PerformanceDate

        {

            get { return this.PerformanceDateValue; }

            set { SetProperty(ref PerformanceDateValue, value); }

        }
        private decimal FlowRateValue;

        public decimal FlowRate

        {

            get { return this.FlowRateValue; }

            set { SetProperty(ref FlowRateValue, value); }

        }
        private decimal EfficiencyValue;

        public decimal Efficiency

        {

            get { return this.EfficiencyValue; }

            set { SetProperty(ref EfficiencyValue, value); }

        }
        private decimal PowerConsumptionValue;

        public decimal PowerConsumption

        {

            get { return this.PowerConsumptionValue; }

            set { SetProperty(ref PowerConsumptionValue, value); }

        }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
    }

    #endregion
    #region Hydraulic Pump Analysis DTOs

    /// <summary>
    /// Request for Hydraulic Pump Analysis calculation
    /// </summary>
    public class HydraulicPumpAnalysisRequest : ModelEntityBase
    {
        private string? WellIdValue;

        public string? WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }
        private string? EquipmentIdValue;

        public string? EquipmentId

        {

            get { return this.EquipmentIdValue; }

            set { SetProperty(ref EquipmentIdValue, value); }

        } // WELL_EQUIPMENT ROW_ID
        private string AnalysisTypeValue = "PERFORMANCE";

        public string AnalysisType

        {

            get { return this.AnalysisTypeValue; }

            set { SetProperty(ref AnalysisTypeValue, value); }

        } // PERFORMANCE, DESIGN, EFFICIENCY
        
        // Well properties (optional, will be retrieved from WELL if not provided)
        private decimal? WellDepthValue;

        public decimal? WellDepth

        {

            get { return this.WellDepthValue; }

            set { SetProperty(ref WellDepthValue, value); }

        } // feet
        private decimal? PumpDepthValue;

        public decimal? PumpDepth

        {

            get { return this.PumpDepthValue; }

            set { SetProperty(ref PumpDepthValue, value); }

        } // feet
        private decimal? TubingDiameterValue;

        public decimal? TubingDiameter

        {

            get { return this.TubingDiameterValue; }

            set { SetProperty(ref TubingDiameterValue, value); }

        } // inches
        
        // Pump properties
        private decimal? NozzleSizeValue;

        public decimal? NozzleSize

        {

            get { return this.NozzleSizeValue; }

            set { SetProperty(ref NozzleSizeValue, value); }

        } // inches
        private decimal? ThroatSizeValue;

        public decimal? ThroatSize

        {

            get { return this.ThroatSizeValue; }

            set { SetProperty(ref ThroatSizeValue, value); }

        } // inches
        private decimal? PowerFluidPressureValue;

        public decimal? PowerFluidPressure

        {

            get { return this.PowerFluidPressureValue; }

            set { SetProperty(ref PowerFluidPressureValue, value); }

        } // psia
        private decimal? PowerFluidRateValue;

        public decimal? PowerFluidRate

        {

            get { return this.PowerFluidRateValue; }

            set { SetProperty(ref PowerFluidRateValue, value); }

        } // bbl/day
        private decimal? PowerFluidDensityValue;

        public decimal? PowerFluidDensity

        {

            get { return this.PowerFluidDensityValue; }

            set { SetProperty(ref PowerFluidDensityValue, value); }

        } // lb/ft³
        
        // Production parameters
        private decimal? ProductionRateValue;

        public decimal? ProductionRate

        {

            get { return this.ProductionRateValue; }

            set { SetProperty(ref ProductionRateValue, value); }

        } // bbl/day
        private decimal? DischargePressureValue;

        public decimal? DischargePressure

        {

            get { return this.DischargePressureValue; }

            set { SetProperty(ref DischargePressureValue, value); }

        } // psia
        private decimal? SuctionPressureValue;

        public decimal? SuctionPressure

        {

            get { return this.SuctionPressureValue; }

            set { SetProperty(ref SuctionPressureValue, value); }

        } // psia
        
        // Fluid properties
        private decimal? OilGravityValue;

        public decimal? OilGravity

        {

            get { return this.OilGravityValue; }

            set { SetProperty(ref OilGravityValue, value); }

        } // API
        private decimal? WaterCutValue;

        public decimal? WaterCut

        {

            get { return this.WaterCutValue; }

            set { SetProperty(ref WaterCutValue, value); }

        } // fraction 0-1
        private decimal? GasOilRatioValue;

        public decimal? GasOilRatio

        {

            get { return this.GasOilRatioValue; }

            set { SetProperty(ref GasOilRatioValue, value); }

        } // scf/bbl
        
        // Additional parameters
        public Dictionary<string, object>? AdditionalParameters { get; set; }
        private string? UserIdValue;

        public string? UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
    }

    /// <summary>
    /// Result of Hydraulic Pump Analysis calculation
    /// </summary>
    public class HydraulicPumpAnalysisResult : ModelEntityBase
    {
        private string CalculationIdValue = string.Empty;

        public string CalculationId

        {

            get { return this.CalculationIdValue; }

            set { SetProperty(ref CalculationIdValue, value); }

        }
        private string? WellIdValue;

        public string? WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }
        private string? EquipmentIdValue;

        public string? EquipmentId

        {

            get { return this.EquipmentIdValue; }

            set { SetProperty(ref EquipmentIdValue, value); }

        }
        private string AnalysisTypeValue = string.Empty;

        public string AnalysisType

        {

            get { return this.AnalysisTypeValue; }

            set { SetProperty(ref AnalysisTypeValue, value); }

        }
        private DateTime CalculationDateValue;

        public DateTime CalculationDate

        {

            get { return this.CalculationDateValue; }

            set { SetProperty(ref CalculationDateValue, value); }

        }
        
        // Performance results
        private decimal ProductionRateValue;

        public decimal ProductionRate

        {

            get { return this.ProductionRateValue; }

            set { SetProperty(ref ProductionRateValue, value); }

        } // bbl/day
        private decimal PowerFluidRateValue;

        public decimal PowerFluidRate

        {

            get { return this.PowerFluidRateValue; }

            set { SetProperty(ref PowerFluidRateValue, value); }

        } // bbl/day
        private decimal PowerFluidPressureValue;

        public decimal PowerFluidPressure

        {

            get { return this.PowerFluidPressureValue; }

            set { SetProperty(ref PowerFluidPressureValue, value); }

        } // psia
        private decimal DischargePressureValue;

        public decimal DischargePressure

        {

            get { return this.DischargePressureValue; }

            set { SetProperty(ref DischargePressureValue, value); }

        } // psia
        private decimal SuctionPressureValue;

        public decimal SuctionPressure

        {

            get { return this.SuctionPressureValue; }

            set { SetProperty(ref SuctionPressureValue, value); }

        } // psia
        
        // Efficiency results
        private decimal HydraulicEfficiencyValue;

        public decimal HydraulicEfficiency

        {

            get { return this.HydraulicEfficiencyValue; }

            set { SetProperty(ref HydraulicEfficiencyValue, value); }

        } // fraction 0-1
        private decimal OverallEfficiencyValue;

        public decimal OverallEfficiency

        {

            get { return this.OverallEfficiencyValue; }

            set { SetProperty(ref OverallEfficiencyValue, value); }

        } // fraction 0-1
        private decimal PowerRequiredValue;

        public decimal PowerRequired

        {

            get { return this.PowerRequiredValue; }

            set { SetProperty(ref PowerRequiredValue, value); }

        } // horsepower
        
        // Design results
        private decimal? RecommendedNozzleSizeValue;

        public decimal? RecommendedNozzleSize

        {

            get { return this.RecommendedNozzleSizeValue; }

            set { SetProperty(ref RecommendedNozzleSizeValue, value); }

        } // inches
        private decimal? RecommendedThroatSizeValue;

        public decimal? RecommendedThroatSize

        {

            get { return this.RecommendedThroatSizeValue; }

            set { SetProperty(ref RecommendedThroatSizeValue, value); }

        } // inches
        private decimal? RecommendedPowerFluidRateValue;

        public decimal? RecommendedPowerFluidRate

        {

            get { return this.RecommendedPowerFluidRateValue; }

            set { SetProperty(ref RecommendedPowerFluidRateValue, value); }

        } // bbl/day
        
        // Additional metadata
        public Dictionary<string, object>? AdditionalResults { get; set; }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        } // SUCCESS, FAILED
        private string? ErrorMessageValue;

        public string? ErrorMessage

        {

            get { return this.ErrorMessageValue; }

            set { SetProperty(ref ErrorMessageValue, value); }

        }
        private string? UserIdValue;

        public string? UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
    }

    #endregion
}




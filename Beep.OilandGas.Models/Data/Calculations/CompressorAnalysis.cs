using System;
using System.Collections.Generic;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.Calculations
{
    /// <summary>
    /// DTO for compressor performance analysis
    /// </summary>
    public class CompressorPerformance : ModelEntityBase
    {
        private string AnalysisIdValue = string.Empty;

        public string AnalysisId

        {

            get { return this.AnalysisIdValue; }

            set { SetProperty(ref AnalysisIdValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private string CompressorTypeValue = string.Empty;

        public string CompressorType

        {

            get { return this.CompressorTypeValue; }

            set { SetProperty(ref CompressorTypeValue, value); }

        } // Centrifugal, Reciprocating
        private decimal InletPressureValue;

        public decimal InletPressure

        {

            get { return this.InletPressureValue; }

            set { SetProperty(ref InletPressureValue, value); }

        }
        private decimal DischargePressureValue;

        public decimal DischargePressure

        {

            get { return this.DischargePressureValue; }

            set { SetProperty(ref DischargePressureValue, value); }

        }
        private decimal GasFlowRateValue;

        public decimal GasFlowRate

        {

            get { return this.GasFlowRateValue; }

            set { SetProperty(ref GasFlowRateValue, value); }

        }
        private decimal TemperatureValue;

        public decimal Temperature

        {

            get { return this.TemperatureValue; }

            set { SetProperty(ref TemperatureValue, value); }

        }
        private decimal CompressionRatioValue;

        public decimal CompressionRatio

        {

            get { return this.CompressionRatioValue; }

            set { SetProperty(ref CompressionRatioValue, value); }

        }
        private decimal IsentropicEfficiencyValue;

        public decimal IsentropicEfficiency

        {

            get { return this.IsentropicEfficiencyValue; }

            set { SetProperty(ref IsentropicEfficiencyValue, value); }

        }
        private decimal ActualEfficiencyValue;

        public decimal ActualEfficiency

        {

            get { return this.ActualEfficiencyValue; }

            set { SetProperty(ref ActualEfficiencyValue, value); }

        }
        private decimal PowerRequiredValue;

        public decimal PowerRequired

        {

            get { return this.PowerRequiredValue; }

            set { SetProperty(ref PowerRequiredValue, value); }

        }
        private decimal PolyHeatCapacityRatioValue;

        public decimal PolyHeatCapacityRatio

        {

            get { return this.PolyHeatCapacityRatioValue; }

            set { SetProperty(ref PolyHeatCapacityRatioValue, value); }

        }
    }

    /// <summary>
    /// DTO for compressor design analysis
    /// </summary>
    public class CompressorDesign : ModelEntityBase
    {
        private string DesignIdValue = string.Empty;

        public string DesignId

        {

            get { return this.DesignIdValue; }

            set { SetProperty(ref DesignIdValue, value); }

        }
        private DateTime DesignDateValue;

        public DateTime DesignDate

        {

            get { return this.DesignDateValue; }

            set { SetProperty(ref DesignDateValue, value); }

        }
        private string CompressorTypeValue = string.Empty;

        public string CompressorType

        {

            get { return this.CompressorTypeValue; }

            set { SetProperty(ref CompressorTypeValue, value); }

        }
        private decimal RequiredFlowRateValue;

        public decimal RequiredFlowRate

        {

            get { return this.RequiredFlowRateValue; }

            set { SetProperty(ref RequiredFlowRateValue, value); }

        }
        private decimal RequiredDischargePressureValue;

        public decimal RequiredDischargePressure

        {

            get { return this.RequiredDischargePressureValue; }

            set { SetProperty(ref RequiredDischargePressureValue, value); }

        }
        private decimal RequiredInletPressureValue;

        public decimal RequiredInletPressure

        {

            get { return this.RequiredInletPressureValue; }

            set { SetProperty(ref RequiredInletPressureValue, value); }

        }
        private decimal GasSpecificGravityValue;

        public decimal GasSpecificGravity

        {

            get { return this.GasSpecificGravityValue; }

            set { SetProperty(ref GasSpecificGravityValue, value); }

        }
        private decimal DesignTemperatureValue;

        public decimal DesignTemperature

        {

            get { return this.DesignTemperatureValue; }

            set { SetProperty(ref DesignTemperatureValue, value); }

        }
        private decimal RecommendedStagesValue;

        public decimal RecommendedStages

        {

            get { return this.RecommendedStagesValue; }

            set { SetProperty(ref RecommendedStagesValue, value); }

        }
        private decimal EstimatedEfficiencyValue;

        public decimal EstimatedEfficiency

        {

            get { return this.EstimatedEfficiencyValue; }

            set { SetProperty(ref EstimatedEfficiencyValue, value); }

        }
        private decimal EstimatedPowerValue;

        public decimal EstimatedPower

        {

            get { return this.EstimatedPowerValue; }

            set { SetProperty(ref EstimatedPowerValue, value); }

        }
        private List<CompressorStage> StagesValue = new();

        public List<CompressorStage> Stages

        {

            get { return this.StagesValue; }

            set { SetProperty(ref StagesValue, value); }

        }
    }

    /// <summary>
    /// DTO for individual compressor stage
    /// </summary>
    public class CompressorStage : ModelEntityBase
    {
        private int StageNumberValue;

        public int StageNumber

        {

            get { return this.StageNumberValue; }

            set { SetProperty(ref StageNumberValue, value); }

        }
        private decimal InletPressureValue;

        public decimal InletPressure

        {

            get { return this.InletPressureValue; }

            set { SetProperty(ref InletPressureValue, value); }

        }
        private decimal DischargePressureValue;

        public decimal DischargePressure

        {

            get { return this.DischargePressureValue; }

            set { SetProperty(ref DischargePressureValue, value); }

        }
        private decimal StageCompressionRatioValue;

        public decimal StageCompressionRatio

        {

            get { return this.StageCompressionRatioValue; }

            set { SetProperty(ref StageCompressionRatioValue, value); }

        }
        private decimal StagePowerValue;

        public decimal StagePower

        {

            get { return this.StagePowerValue; }

            set { SetProperty(ref StagePowerValue, value); }

        }
        private decimal StageEfficiencyValue;

        public decimal StageEfficiency

        {

            get { return this.StageEfficiencyValue; }

            set { SetProperty(ref StageEfficiencyValue, value); }

        }
    }

    /// <summary>
    /// DTO for centrifugal compressor analysis
    /// </summary>
    public class CentrifugalCompressorAnalysis : ModelEntityBase
    {
        private string AnalysisIdValue = string.Empty;

        public string AnalysisId

        {

            get { return this.AnalysisIdValue; }

            set { SetProperty(ref AnalysisIdValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private decimal InletFlowRateValue;

        public decimal InletFlowRate

        {

            get { return this.InletFlowRateValue; }

            set { SetProperty(ref InletFlowRateValue, value); }

        }
        private decimal InletPressureValue;

        public decimal InletPressure

        {

            get { return this.InletPressureValue; }

            set { SetProperty(ref InletPressureValue, value); }

        }
        private decimal DischargePressureValue;

        public decimal DischargePressure

        {

            get { return this.DischargePressureValue; }

            set { SetProperty(ref DischargePressureValue, value); }

        }
        private decimal ImpellerDiameterValue;

        public decimal ImpellerDiameter

        {

            get { return this.ImpellerDiameterValue; }

            set { SetProperty(ref ImpellerDiameterValue, value); }

        }
        private decimal ImpellerSpeedValue;

        public decimal ImpellerSpeed

        {

            get { return this.ImpellerSpeedValue; }

            set { SetProperty(ref ImpellerSpeedValue, value); }

        }
        private decimal HeadDevelopedValue;

        public decimal HeadDeveloped

        {

            get { return this.HeadDevelopedValue; }

            set { SetProperty(ref HeadDevelopedValue, value); }

        }
        private decimal SurgeMarginValue;

        public decimal SurgeMargin

        {

            get { return this.SurgeMarginValue; }

            set { SetProperty(ref SurgeMarginValue, value); }

        }
        private decimal PolyIsentropicHeadValue;

        public decimal PolyIsentropicHead

        {

            get { return this.PolyIsentropicHeadValue; }

            set { SetProperty(ref PolyIsentropicHeadValue, value); }

        }
        private decimal ActualHeadValue;

        public decimal ActualHead

        {

            get { return this.ActualHeadValue; }

            set { SetProperty(ref ActualHeadValue, value); }

        }
        private decimal StallMarginValue;

        public decimal StallMargin

        {

            get { return this.StallMarginValue; }

            set { SetProperty(ref StallMarginValue, value); }

        }
        private string OperatingRegionValue = string.Empty;

        public string OperatingRegion

        {

            get { return this.OperatingRegionValue; }

            set { SetProperty(ref OperatingRegionValue, value); }

        } // Normal, Stall, Surge
    }

    /// <summary>
    /// DTO for reciprocating compressor analysis
    /// </summary>
    public class ReciprocationCompressorAnalysis : ModelEntityBase
    {
        private string AnalysisIdValue = string.Empty;

        public string AnalysisId

        {

            get { return this.AnalysisIdValue; }

            set { SetProperty(ref AnalysisIdValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private decimal CylinderCountValue;

        public decimal CylinderCount

        {

            get { return this.CylinderCountValue; }

            set { SetProperty(ref CylinderCountValue, value); }

        }
        private decimal BoreSizeValue;

        public decimal BoreSize

        {

            get { return this.BoreSizeValue; }

            set { SetProperty(ref BoreSizeValue, value); }

        }
        private decimal StrokeLengthValue;

        public decimal StrokeLength

        {

            get { return this.StrokeLengthValue; }

            set { SetProperty(ref StrokeLengthValue, value); }

        }
        private decimal RPMValue;

        public decimal RPM

        {

            get { return this.RPMValue; }

            set { SetProperty(ref RPMValue, value); }

        }
        private decimal VolumetricFlowRateValue;

        public decimal VolumetricFlowRate

        {

            get { return this.VolumetricFlowRateValue; }

            set { SetProperty(ref VolumetricFlowRateValue, value); }

        }
        private decimal DisplacementVolumeValue;

        public decimal DisplacementVolume

        {

            get { return this.DisplacementVolumeValue; }

            set { SetProperty(ref DisplacementVolumeValue, value); }

        }
        private decimal VolumetricEfficiencyValue;

        public decimal VolumetricEfficiency

        {

            get { return this.VolumetricEfficiencyValue; }

            set { SetProperty(ref VolumetricEfficiencyValue, value); }

        }
        private decimal SuctionPressureValue;

        public decimal SuctionPressure

        {

            get { return this.SuctionPressureValue; }

            set { SetProperty(ref SuctionPressureValue, value); }

        }
        private decimal DischargePressureValue;

        public decimal DischargePressure

        {

            get { return this.DischargePressureValue; }

            set { SetProperty(ref DischargePressureValue, value); }

        }
        private decimal RodLoadValue;

        public decimal RodLoad

        {

            get { return this.RodLoadValue; }

            set { SetProperty(ref RodLoadValue, value); }

        }
    }

    /// <summary>
    /// DTO for compressor efficiency analysis
    /// </summary>
    public class CompressorEfficiencyAnalysis : ModelEntityBase
    {
        private string AnalysisIdValue = string.Empty;

        public string AnalysisId

        {

            get { return this.AnalysisIdValue; }

            set { SetProperty(ref AnalysisIdValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private decimal IsentropicEfficiencyValue;

        public decimal IsentropicEfficiency

        {

            get { return this.IsentropicEfficiencyValue; }

            set { SetProperty(ref IsentropicEfficiencyValue, value); }

        }
        private decimal PolyIsentropicEfficiencyValue;

        public decimal PolyIsentropicEfficiency

        {

            get { return this.PolyIsentropicEfficiencyValue; }

            set { SetProperty(ref PolyIsentropicEfficiencyValue, value); }

        }
        private decimal VolumetricEfficiencyValue;

        public decimal VolumetricEfficiency

        {

            get { return this.VolumetricEfficiencyValue; }

            set { SetProperty(ref VolumetricEfficiencyValue, value); }

        }
        private decimal MechanicalEfficiencyValue;

        public decimal MechanicalEfficiency

        {

            get { return this.MechanicalEfficiencyValue; }

            set { SetProperty(ref MechanicalEfficiencyValue, value); }

        }
        private decimal OverallEfficiencyValue;

        public decimal OverallEfficiency

        {

            get { return this.OverallEfficiencyValue; }

            set { SetProperty(ref OverallEfficiencyValue, value); }

        }
        private decimal EfficiencyTrendValue;

        public decimal EfficiencyTrend

        {

            get { return this.EfficiencyTrendValue; }

            set { SetProperty(ref EfficiencyTrendValue, value); }

        } // -1 to +1
        private string EfficiencyStatusValue = string.Empty;

        public string EfficiencyStatus

        {

            get { return this.EfficiencyStatusValue; }

            set { SetProperty(ref EfficiencyStatusValue, value); }

        } // Good, Fair, Poor
    }

    /// <summary>
    /// DTO for compressor maintenance prediction
    /// </summary>
    public class CompressorMaintenancePrediction : ModelEntityBase
    {
        private string PredictionIdValue = string.Empty;

        public string PredictionId

        {

            get { return this.PredictionIdValue; }

            set { SetProperty(ref PredictionIdValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private DateTime NextMaintenanceDateValue;

        public DateTime NextMaintenanceDate

        {

            get { return this.NextMaintenanceDateValue; }

            set { SetProperty(ref NextMaintenanceDateValue, value); }

        }
        private int HoursUntilMaintenanceValue;

        public int HoursUntilMaintenance

        {

            get { return this.HoursUntilMaintenanceValue; }

            set { SetProperty(ref HoursUntilMaintenanceValue, value); }

        }
        private string MaintenanceTypeValue = string.Empty;

        public string MaintenanceType

        {

            get { return this.MaintenanceTypeValue; }

            set { SetProperty(ref MaintenanceTypeValue, value); }

        } // Minor, Major, Overhaul
        private List<string> MaintenanceItemsValue = new();

        public List<string> MaintenanceItems

        {

            get { return this.MaintenanceItemsValue; }

            set { SetProperty(ref MaintenanceItemsValue, value); }

        }
        private decimal MaintenancePriorityValue;

        public decimal MaintenancePriority

        {

            get { return this.MaintenancePriorityValue; }

            set { SetProperty(ref MaintenancePriorityValue, value); }

        } // 0-100 scale
        private string RiskLevelValue = string.Empty;

        public string RiskLevel

        {

            get { return this.RiskLevelValue; }

            set { SetProperty(ref RiskLevelValue, value); }

        } // Low, Medium, High
    }

    /// <summary>
    /// DTO for compressor pressure-flow analysis
    /// </summary>
    public class CompressorPressureFlowAnalysis : ModelEntityBase
    {
        private string AnalysisIdValue = string.Empty;

        public string AnalysisId

        {

            get { return this.AnalysisIdValue; }

            set { SetProperty(ref AnalysisIdValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private List<PressureFlowPoint> PerformancePointsValue = new();

        public List<PressureFlowPoint> PerformancePoints

        {

            get { return this.PerformancePointsValue; }

            set { SetProperty(ref PerformancePointsValue, value); }

        }
        private decimal SurgeLimitValue;

        public decimal SurgeLimit

        {

            get { return this.SurgeLimitValue; }

            set { SetProperty(ref SurgeLimitValue, value); }

        }
        private decimal ChokingLimitValue;

        public decimal ChokingLimit

        {

            get { return this.ChokingLimitValue; }

            set { SetProperty(ref ChokingLimitValue, value); }

        }
        private decimal OptimalFlowRateValue;

        public decimal OptimalFlowRate

        {

            get { return this.OptimalFlowRateValue; }

            set { SetProperty(ref OptimalFlowRateValue, value); }

        }
        private decimal OptimalPressureValue;

        public decimal OptimalPressure

        {

            get { return this.OptimalPressureValue; }

            set { SetProperty(ref OptimalPressureValue, value); }

        }
    }

    /// <summary>
    /// DTO for individual pressure-flow point
    /// </summary>
    public class PressureFlowPoint : ModelEntityBase
    {
        private decimal FlowRateValue;

        public decimal FlowRate

        {

            get { return this.FlowRateValue; }

            set { SetProperty(ref FlowRateValue, value); }

        }
        private decimal PressureValue;

        public decimal Pressure

        {

            get { return this.PressureValue; }

            set { SetProperty(ref PressureValue, value); }

        }
        private decimal PowerValue;

        public decimal Power

        {

            get { return this.PowerValue; }

            set { SetProperty(ref PowerValue, value); }

        }
        private decimal EfficiencyValue;

        public decimal Efficiency

        {

            get { return this.EfficiencyValue; }

            set { SetProperty(ref EfficiencyValue, value); }

        }
    }

    /// <summary>
    /// DTO for compressor power consumption analysis
    /// </summary>
    public class CompressorPowerAnalysis : ModelEntityBase
    {
        private string AnalysisIdValue = string.Empty;

        public string AnalysisId

        {

            get { return this.AnalysisIdValue; }

            set { SetProperty(ref AnalysisIdValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private decimal InletPowerValue;

        public decimal InletPower

        {

            get { return this.InletPowerValue; }

            set { SetProperty(ref InletPowerValue, value); }

        }
        private decimal FrictionLossesValue;

        public decimal FrictionLosses

        {

            get { return this.FrictionLossesValue; }

            set { SetProperty(ref FrictionLossesValue, value); }

        }
        private decimal IsothermicPowerValue;

        public decimal IsothermicPower

        {

            get { return this.IsothermicPowerValue; }

            set { SetProperty(ref IsothermicPowerValue, value); }

        }
        private decimal PolyIsentropicPowerValue;

        public decimal PolyIsentropicPower

        {

            get { return this.PolyIsentropicPowerValue; }

            set { SetProperty(ref PolyIsentropicPowerValue, value); }

        }
        private decimal IsentropicPowerValue;

        public decimal IsentropicPower

        {

            get { return this.IsentropicPowerValue; }

            set { SetProperty(ref IsentropicPowerValue, value); }

        }
        private decimal ActualPowerValue;

        public decimal ActualPower

        {

            get { return this.ActualPowerValue; }

            set { SetProperty(ref ActualPowerValue, value); }

        }
        private decimal PowerSavingsValue;

        public decimal PowerSavings

        {

            get { return this.PowerSavingsValue; }

            set { SetProperty(ref PowerSavingsValue, value); }

        }
        private string OptimizationRecommendationValue = string.Empty;

        public string OptimizationRecommendation

        {

            get { return this.OptimizationRecommendationValue; }

            set { SetProperty(ref OptimizationRecommendationValue, value); }

        }
    }
    /// <summary>
    /// Request for Compressor Analysis calculation
    /// </summary>
    public class CompressorAnalysisRequest : ModelEntityBase
    {
        private string? FacilityIdValue;

        public string? FacilityId

        {

            get { return this.FacilityIdValue; }

            set { SetProperty(ref FacilityIdValue, value); }

        }
        private string? EquipmentIdValue;

        public string? EquipmentId

        {

            get { return this.EquipmentIdValue; }

            set { SetProperty(ref EquipmentIdValue, value); }

        } // FACILITY_EQUIPMENT ROW_ID
        private string CompressorTypeValue = "CENTRIFUGAL";

        public string CompressorType

        {

            get { return this.CompressorTypeValue; }

            set { SetProperty(ref CompressorTypeValue, value); }

        } // CENTRIFUGAL, RECIPROCATING
        private string AnalysisTypeValue = "POWER";

        public string AnalysisType

        {

            get { return this.AnalysisTypeValue; }

            set { SetProperty(ref AnalysisTypeValue, value); }

        } // POWER, PRESSURE, EFFICIENCY
        
        // Compressor properties (optional, will be retrieved from equipment if not provided)
        private decimal? SuctionPressureValue;

        public decimal? SuctionPressure

        {

            get { return this.SuctionPressureValue; }

            set { SetProperty(ref SuctionPressureValue, value); }

        } // psia
        private decimal? DischargePressureValue;

        public decimal? DischargePressure

        {

            get { return this.DischargePressureValue; }

            set { SetProperty(ref DischargePressureValue, value); }

        } // psia
        private decimal? SuctionTemperatureValue;

        public decimal? SuctionTemperature

        {

            get { return this.SuctionTemperatureValue; }

            set { SetProperty(ref SuctionTemperatureValue, value); }

        } // Rankine
        private decimal? FlowRateValue;

        public decimal? FlowRate

        {

            get { return this.FlowRateValue; }

            set { SetProperty(ref FlowRateValue, value); }

        } // Mscf/day or ACFM
        private decimal? GasSpecificGravityValue;

        public decimal? GasSpecificGravity

        {

            get { return this.GasSpecificGravityValue; }

            set { SetProperty(ref GasSpecificGravityValue, value); }

        }
        private decimal? CompressionRatioValue;

        public decimal? CompressionRatio

        {

            get { return this.CompressionRatioValue; }

            set { SetProperty(ref CompressionRatioValue, value); }

        }
        
        // Centrifugal compressor specific
        private decimal? PolytropicEfficiencyValue;

        public decimal? PolytropicEfficiency

        {

            get { return this.PolytropicEfficiencyValue; }

            set { SetProperty(ref PolytropicEfficiencyValue, value); }

        } // fraction 0-1
        private decimal? AdiabaticEfficiencyValue;

        public decimal? AdiabaticEfficiency

        {

            get { return this.AdiabaticEfficiencyValue; }

            set { SetProperty(ref AdiabaticEfficiencyValue, value); }

        } // fraction 0-1
        private int? NumberOfStagesValue;

        public int? NumberOfStages

        {

            get { return this.NumberOfStagesValue; }

            set { SetProperty(ref NumberOfStagesValue, value); }

        }
        
        // Reciprocating compressor specific
        private decimal? CylinderDisplacementValue;

        public decimal? CylinderDisplacement

        {

            get { return this.CylinderDisplacementValue; }

            set { SetProperty(ref CylinderDisplacementValue, value); }

        } // ACFM
        private decimal? VolumetricEfficiencyValue;

        public decimal? VolumetricEfficiency

        {

            get { return this.VolumetricEfficiencyValue; }

            set { SetProperty(ref VolumetricEfficiencyValue, value); }

        } // fraction 0-1
        private int? NumberOfCylindersValue;

        public int? NumberOfCylinders

        {

            get { return this.NumberOfCylindersValue; }

            set { SetProperty(ref NumberOfCylindersValue, value); }

        }
        
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
    /// Result of Compressor Analysis calculation
    /// </summary>
    public class CompressorAnalysisResult : ModelEntityBase
    {
        private string CalculationIdValue = string.Empty;

        public string CalculationId

        {

            get { return this.CalculationIdValue; }

            set { SetProperty(ref CalculationIdValue, value); }

        }
        private string? FacilityIdValue;

        public string? FacilityId

        {

            get { return this.FacilityIdValue; }

            set { SetProperty(ref FacilityIdValue, value); }

        }
        private string? EquipmentIdValue;

        public string? EquipmentId

        {

            get { return this.EquipmentIdValue; }

            set { SetProperty(ref EquipmentIdValue, value); }

        }
        private string CompressorTypeValue = string.Empty;

        public string CompressorType

        {

            get { return this.CompressorTypeValue; }

            set { SetProperty(ref CompressorTypeValue, value); }

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
        
        // Power results
        private decimal PolytropicHeadValue;

        public decimal PolytropicHead

        {

            get { return this.PolytropicHeadValue; }

            set { SetProperty(ref PolytropicHeadValue, value); }

        } // feet
        private decimal AdiabaticHeadValue;

        public decimal AdiabaticHead

        {

            get { return this.AdiabaticHeadValue; }

            set { SetProperty(ref AdiabaticHeadValue, value); }

        } // feet
        private decimal PowerRequiredValue;

        public decimal PowerRequired

        {

            get { return this.PowerRequiredValue; }

            set { SetProperty(ref PowerRequiredValue, value); }

        } // horsepower
        private decimal DischargeTemperatureValue;

        public decimal DischargeTemperature

        {

            get { return this.DischargeTemperatureValue; }

            set { SetProperty(ref DischargeTemperatureValue, value); }

        } // Rankine
        
        // Efficiency results
        private decimal PolytropicEfficiencyValue;

        public decimal PolytropicEfficiency

        {

            get { return this.PolytropicEfficiencyValue; }

            set { SetProperty(ref PolytropicEfficiencyValue, value); }

        } // fraction 0-1
        private decimal AdiabaticEfficiencyValue;

        public decimal AdiabaticEfficiency

        {

            get { return this.AdiabaticEfficiencyValue; }

            set { SetProperty(ref AdiabaticEfficiencyValue, value); }

        } // fraction 0-1
        private decimal OverallEfficiencyValue;

        public decimal OverallEfficiency

        {

            get { return this.OverallEfficiencyValue; }

            set { SetProperty(ref OverallEfficiencyValue, value); }

        } // fraction 0-1
        
        // Pressure and flow results
        private decimal SuctionPressureValue;

        public decimal SuctionPressure

        {

            get { return this.SuctionPressureValue; }

            set { SetProperty(ref SuctionPressureValue, value); }

        } // psia
        private decimal DischargePressureValue;

        public decimal DischargePressure

        {

            get { return this.DischargePressureValue; }

            set { SetProperty(ref DischargePressureValue, value); }

        } // psia
        private decimal CompressionRatioValue;

        public decimal CompressionRatio

        {

            get { return this.CompressionRatioValue; }

            set { SetProperty(ref CompressionRatioValue, value); }

        }
        private decimal FlowRateValue;

        public decimal FlowRate

        {

            get { return this.FlowRateValue; }

            set { SetProperty(ref FlowRateValue, value); }

        } // Mscf/day or ACFM
        
        // Reciprocating compressor specific
        private decimal? CylinderDisplacementValue;

        public decimal? CylinderDisplacement

        {

            get { return this.CylinderDisplacementValue; }

            set { SetProperty(ref CylinderDisplacementValue, value); }

        } // ACFM
        private decimal? VolumetricEfficiencyValue;

        public decimal? VolumetricEfficiency

        {

            get { return this.VolumetricEfficiencyValue; }

            set { SetProperty(ref VolumetricEfficiencyValue, value); }

        } // fraction 0-1
        
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
}





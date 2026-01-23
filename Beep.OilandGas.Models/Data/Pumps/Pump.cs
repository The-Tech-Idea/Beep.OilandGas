using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data.PlungerLift;
using Beep.OilandGas.Models.Data.SuckerRodPumping;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.Pumps
{
    /// <summary>
    /// Request for designing a pump system (generic/hydraulic pump)
    /// </summary>
    public class DesignPumpSystemRequest : ModelEntityBase
    {
        /// <summary>
        /// Well UWI (Unique Well Identifier)
        /// </summary>
        private string WellUWIValue = string.Empty;

        [Required(ErrorMessage = "WellUWI is required")]
        public string WellUWI

        {

            get { return this.WellUWIValue; }

            set { SetProperty(ref WellUWIValue, value); }

        }

        /// <summary>
        /// Pump type (e.g., "ESP", "SuckerRod", "Hydraulic", "PlungerLift")
        /// </summary>
        private string PumpTypeValue = string.Empty;

        [Required(ErrorMessage = "PumpType is required")]
        public string PumpType

        {

            get { return this.PumpTypeValue; }

            set { SetProperty(ref PumpTypeValue, value); }

        }

        /// <summary>
        /// Well depth in feet
        /// </summary>
        private decimal WellDepthValue;

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "WellDepth must be greater than or equal to 0")]
        public decimal WellDepth

        {

            get { return this.WellDepthValue; }

            set { SetProperty(ref WellDepthValue, value); }

        }

        /// <summary>
        /// Desired flow rate (bbl/day)
        /// </summary>
        private decimal DesiredFlowRateValue;

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "DesiredFlowRate must be greater than or equal to 0")]
        public decimal DesiredFlowRate

        {

            get { return this.DesiredFlowRateValue; }

            set { SetProperty(ref DesiredFlowRateValue, value); }

        }
    }

    /// <summary>
    /// Request for designing a plunger lift system
    /// </summary>
    public class DesignPlungerLiftSystemRequest : ModelEntityBase
    {
        /// <summary>
        /// Well UWI (Unique Well Identifier)
        /// </summary>
        private string WellUWIValue = string.Empty;

        [Required(ErrorMessage = "WellUWI is required")]
        public string WellUWI

        {

            get { return this.WellUWIValue; }

            set { SetProperty(ref WellUWIValue, value); }

        }

        /// <summary>
        /// Well properties for plunger lift design
        /// </summary>
        private PlungerLiftWellProperties WellPropertiesValue = null!;

        [Required(ErrorMessage = "WellProperties are required")]
        public PlungerLiftWellProperties WellProperties

        {

            get { return this.WellPropertiesValue; }

            set { SetProperty(ref WellPropertiesValue, value); }

        }
    }

    /// <summary>
    /// Request for designing a sucker rod pump system
    /// </summary>
    public class DesignSuckerRodPumpSystemRequest : ModelEntityBase
    {
        /// <summary>
        /// Well UWI (Unique Well Identifier)
        /// </summary>
        private string WellUWIValue = string.Empty;

        [Required(ErrorMessage = "WellUWI is required")]
        public string WellUWI

        {

            get { return this.WellUWIValue; }

            set { SetProperty(ref WellUWIValue, value); }

        }

        /// <summary>
        /// Well properties for sucker rod pump design
        /// </summary>
        private SuckerRodPumpWellProperties WellPropertiesValue = null!;

        [Required(ErrorMessage = "WellProperties are required")]
        public SuckerRodPumpWellProperties WellProperties

        {

            get { return this.WellPropertiesValue; }

            set { SetProperty(ref WellPropertiesValue, value); }

        }
    }

    /// <summary>
    /// Request for analyzing pump performance
    /// </summary>
    public class AnalyzePerformanceRequest : ModelEntityBase
    {
        /// <summary>
        /// Pump identifier or Well UWI (depending on pump type)
        /// </summary>
        private string PumpIdValue = string.Empty;

        [Required(ErrorMessage = "PumpId or WellUWI is required")]
        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
    }

    /// <summary>
    /// DTO for plunger lift well properties
    /// </summary>
    public class PlungerLiftWellProperties : ModelEntityBase
    {
        /// <summary>
        /// Well depth in feet
        /// </summary>
        private decimal WellDepthValue;

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "WellDepth must be greater than or equal to 0")]
        public decimal WellDepth

        {

            get { return this.WellDepthValue; }

            set { SetProperty(ref WellDepthValue, value); }

        }

        /// <summary>
        /// Tubing size in inches
        /// </summary>
        private decimal TubingSizeValue;

        [Required]
        [Range(0.5, 10, ErrorMessage = "TubingSize must be between 0.5 and 10 inches")]
        public decimal TubingSize

        {

            get { return this.TubingSizeValue; }

            set { SetProperty(ref TubingSizeValue, value); }

        }

        /// <summary>
        /// Casing size in inches
        /// </summary>
        private decimal CasingSizeValue;

        [Required]
        [Range(0.5, 20, ErrorMessage = "CasingSize must be between 0.5 and 20 inches")]
        public decimal CasingSize

        {

            get { return this.CasingSizeValue; }

            set { SetProperty(ref CasingSizeValue, value); }

        }

        /// <summary>
        /// Static fluid level in feet
        /// </summary>
        private decimal StaticFluidLevelValue;

        [Required]
        [Range(0, double.MaxValue)]
        public decimal StaticFluidLevel

        {

            get { return this.StaticFluidLevelValue; }

            set { SetProperty(ref StaticFluidLevelValue, value); }

        }

        /// <summary>
        /// Reservoir pressure in psi
        /// </summary>
        private decimal ReservoirPressureValue;

        [Required]
        [Range(0, double.MaxValue)]
        public decimal ReservoirPressure

        {

            get { return this.ReservoirPressureValue; }

            set { SetProperty(ref ReservoirPressureValue, value); }

        }

        /// <summary>
        /// Desired production rate in bbl/day
        /// </summary>
        private decimal DesiredProductionRateValue;

        [Required]
        [Range(0, double.MaxValue)]
        public decimal DesiredProductionRate

        {

            get { return this.DesiredProductionRateValue; }

            set { SetProperty(ref DesiredProductionRateValue, value); }

        }
    }

    /// <summary>
    /// DTO for sucker rod pump well properties
    /// </summary>
    public class SuckerRodPumpWellProperties : ModelEntityBase
    {
        /// <summary>
        /// Well depth in feet
        /// </summary>
        private decimal WellDepthValue;

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "WellDepth must be greater than or equal to 0")]
        public decimal WellDepth

        {

            get { return this.WellDepthValue; }

            set { SetProperty(ref WellDepthValue, value); }

        }

        /// <summary>
        /// Tubing size in inches
        /// </summary>
        private decimal TubingSizeValue;

        [Required]
        [Range(0.5, 10, ErrorMessage = "TubingSize must be between 0.5 and 10 inches")]
        public decimal TubingSize

        {

            get { return this.TubingSizeValue; }

            set { SetProperty(ref TubingSizeValue, value); }

        }

        /// <summary>
        /// Casing size in inches
        /// </summary>
        private decimal CasingSizeValue;

        [Required]
        [Range(0.5, 20, ErrorMessage = "CasingSize must be between 0.5 and 20 inches")]
        public decimal CasingSize

        {

            get { return this.CasingSizeValue; }

            set { SetProperty(ref CasingSizeValue, value); }

        }

        /// <summary>
        /// Static fluid level in feet
        /// </summary>
        private decimal StaticFluidLevelValue;

        [Required]
        [Range(0, double.MaxValue)]
        public decimal StaticFluidLevel

        {

            get { return this.StaticFluidLevelValue; }

            set { SetProperty(ref StaticFluidLevelValue, value); }

        }

        /// <summary>
        /// Producing fluid level in feet
        /// </summary>
        private decimal ProducingFluidLevelValue;

        [Required]
        [Range(0, double.MaxValue)]
        public decimal ProducingFluidLevel

        {

            get { return this.ProducingFluidLevelValue; }

            set { SetProperty(ref ProducingFluidLevelValue, value); }

        }

        /// <summary>
        /// API gravity of produced fluid
        /// </summary>
        private decimal APIGravityValue;

        [Required]
        [Range(0, 60, ErrorMessage = "APIGravity must be between 0 and 60")]
        public decimal APIGravity

        {

            get { return this.APIGravityValue; }

            set { SetProperty(ref APIGravityValue, value); }

        }

        /// <summary>
        /// Desired production rate in bbl/day
        /// </summary>
        private decimal DesiredProductionRateValue;

        [Required]
        [Range(0, double.MaxValue)]
        public decimal DesiredProductionRate

        {

            get { return this.DesiredProductionRateValue; }

            set { SetProperty(ref DesiredProductionRateValue, value); }

        }

        /// <summary>
        /// Gas-Oil Ratio (GOR) in scf/stb
        /// </summary>
        private decimal GasOilRatioValue;

        [Range(0, double.MaxValue)]
        public decimal GasOilRatio

        {

            get { return this.GasOilRatioValue; }

            set { SetProperty(ref GasOilRatioValue, value); }

        }

        /// <summary>
        /// Fluid density in lb/ft³
        /// </summary>
        private decimal FluidDensityValue;

        [Range(0, double.MaxValue)]
        public decimal FluidDensity

        {

            get { return this.FluidDensityValue; }

            set { SetProperty(ref FluidDensityValue, value); }

        }

        /// <summary>
        /// Pump type/size designation
        /// </summary>
        private string? PumpTypeDesignationValue;

        public string? PumpTypeDesignation

        {

            get { return this.PumpTypeDesignationValue; }

            set { SetProperty(ref PumpTypeDesignationValue, value); }

        }
    }

    /// <summary>
    /// DTO for sucker rod pump design
    /// </summary>
    public class SuckerRodPumpDesign : ModelEntityBase
    {
        /// <summary>
        /// Unique design identifier
        /// </summary>
        private string DesignIdValue = string.Empty;

        public string DesignId

        {

            get { return this.DesignIdValue; }

            set { SetProperty(ref DesignIdValue, value); }

        }

        /// <summary>
        /// Well UWI (Unique Well Identifier)
        /// </summary>
        private string WellUWIValue = string.Empty;

        public string WellUWI

        {

            get { return this.WellUWIValue; }

            set { SetProperty(ref WellUWIValue, value); }

        }

        /// <summary>
        /// Design date
        /// </summary>
        private System.DateTime DesignDateValue;

        public System.DateTime DesignDate

        {

            get { return this.DesignDateValue; }

            set { SetProperty(ref DesignDateValue, value); }

        }

        /// <summary>
        /// Pump depth in feet
        /// </summary>
        private decimal PumpDepthValue;

        [Range(0, double.MaxValue)]
        public decimal PumpDepth

        {

            get { return this.PumpDepthValue; }

            set { SetProperty(ref PumpDepthValue, value); }

        }

        /// <summary>
        /// Pump size in inches
        /// </summary>
        private decimal PumpSizeValue;

        [Range(0, double.MaxValue)]
        public decimal PumpSize

        {

            get { return this.PumpSizeValue; }

            set { SetProperty(ref PumpSizeValue, value); }

        }

        /// <summary>
        /// Stroke length in inches
        /// </summary>
        private decimal StrokeLengthValue;

        [Range(0, double.MaxValue)]
        public decimal StrokeLength

        {

            get { return this.StrokeLengthValue; }

            set { SetProperty(ref StrokeLengthValue, value); }

        }

        /// <summary>
        /// Strokes per minute
        /// </summary>
        private decimal StrokesPerMinuteValue;

        [Range(0, double.MaxValue)]
        public decimal StrokesPerMinute

        {

            get { return this.StrokesPerMinuteValue; }

            set { SetProperty(ref StrokesPerMinuteValue, value); }

        }

        /// <summary>
        /// Rod string load in pounds
        /// </summary>
        private decimal RodStringLoadValue;

        [Range(0, double.MaxValue)]
        public decimal RodStringLoad

        {

            get { return this.RodStringLoadValue; }

            set { SetProperty(ref RodStringLoadValue, value); }

        }

        /// <summary>
        /// Pump type designation
        /// </summary>
        private string? PumpTypeValue;

        public string? PumpType

        {

            get { return this.PumpTypeValue; }

            set { SetProperty(ref PumpTypeValue, value); }

        }

        /// <summary>
        /// Rod grade specification
        /// </summary>
        private string? RodGradeValue;

        public string? RodGrade

        {

            get { return this.RodGradeValue; }

            set { SetProperty(ref RodGradeValue, value); }

        }

        /// <summary>
        /// Estimated capacity in bbl/day
        /// </summary>
        private decimal EstimatedCapacityValue;

        [Range(0, double.MaxValue)]
        public decimal EstimatedCapacity

        {

            get { return this.EstimatedCapacityValue; }

            set { SetProperty(ref EstimatedCapacityValue, value); }

        }

        /// <summary>
        /// Power requirement in horsepower
        /// </summary>
        private decimal PowerRequirementValue;

        [Range(0, double.MaxValue)]
        public decimal PowerRequirement

        {

            get { return this.PowerRequirementValue; }

            set { SetProperty(ref PowerRequirementValue, value); }

        }

        /// <summary>
        /// Design status
        /// </summary>
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
    }

    /// <summary>
    /// DTO for sucker rod pump performance analysis
    /// </summary>
    public class SuckerRodPumpPerformance : ModelEntityBase
    {
        /// <summary>
        /// Pump identifier
        /// </summary>
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }

        /// <summary>
        /// Performance analysis date
        /// </summary>
        private System.DateTime PerformanceDateValue;

        public System.DateTime PerformanceDate

        {

            get { return this.PerformanceDateValue; }

            set { SetProperty(ref PerformanceDateValue, value); }

        }

        /// <summary>
        /// Flow rate in bbl/day
        /// </summary>
        private decimal FlowRateValue;

        [Range(0, double.MaxValue)]
        public decimal FlowRate

        {

            get { return this.FlowRateValue; }

            set { SetProperty(ref FlowRateValue, value); }

        }

        /// <summary>
        /// Pump efficiency (0-1 or percentage)
        /// </summary>
        private decimal EfficiencyValue;

        [Range(0, 1.5)]
        public decimal Efficiency

        {

            get { return this.EfficiencyValue; }

            set { SetProperty(ref EfficiencyValue, value); }

        }

        /// <summary>
        /// Power consumption in horsepower
        /// </summary>
        private decimal PowerConsumptionValue;

        [Range(0, double.MaxValue)]
        public decimal PowerConsumption

        {

            get { return this.PowerConsumptionValue; }

            set { SetProperty(ref PowerConsumptionValue, value); }

        }

        /// <summary>
        /// Rod load percentage (0-100+)
        /// </summary>
        private decimal RodLoadPercentageValue;

        [Range(0, double.MaxValue)]
        public decimal RodLoadPercentage

        {

            get { return this.RodLoadPercentageValue; }

            set { SetProperty(ref RodLoadPercentageValue, value); }

        }

        /// <summary>
        /// Performance status
        /// </summary>
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
    }

    /// <summary>
    /// Result class for sucker rod pump optimization analysis
    /// </summary>
    public class SRPOptimizationResult : ModelEntityBase
    {
        /// <summary>
        /// Optimization analysis date
        /// </summary>
        private System.DateTime OptimizationDateValue;

        public System.DateTime OptimizationDate

        {

            get { return this.OptimizationDateValue; }

            set { SetProperty(ref OptimizationDateValue, value); }

        }

        /// <summary>
        /// User who performed optimization
        /// </summary>
        private string OptimizedByUserValue = string.Empty;

        public string OptimizedByUser

        {

            get { return this.OptimizedByUserValue; }

            set { SetProperty(ref OptimizedByUserValue, value); }

        }

        /// <summary>
        /// Well UWI
        /// </summary>
        private string WellUWIValue = string.Empty;

        public string WellUWI

        {

            get { return this.WellUWIValue; }

            set { SetProperty(ref WellUWIValue, value); }

        }

        /// <summary>
        /// Current production rate in bbl/day
        /// </summary>
        private decimal CurrentProductionValue;

        public decimal CurrentProduction

        {

            get { return this.CurrentProductionValue; }

            set { SetProperty(ref CurrentProductionValue, value); }

        }

        /// <summary>
        /// Target production rate in bbl/day
        /// </summary>
        private decimal TargetProductionValue;

        public decimal TargetProduction

        {

            get { return this.TargetProductionValue; }

            set { SetProperty(ref TargetProductionValue, value); }

        }

        /// <summary>
        /// Production increase percentage
        /// </summary>
        private decimal ProductionIncreasePercentValue;

        public decimal ProductionIncreasePercent

        {

            get { return this.ProductionIncreasePercentValue; }

            set { SetProperty(ref ProductionIncreasePercentValue, value); }

        }

        /// <summary>
        /// Additional daily revenue impact
        /// </summary>
        private decimal AdditionalDailyRevenueValue;

        public decimal AdditionalDailyRevenue

        {

            get { return this.AdditionalDailyRevenueValue; }

            set { SetProperty(ref AdditionalDailyRevenueValue, value); }

        }

        /// <summary>
        /// Annual revenue increase
        /// </summary>
        private decimal AnnualRevenueIncreaseValue;

        public decimal AnnualRevenueIncrease

        {

            get { return this.AnnualRevenueIncreaseValue; }

            set { SetProperty(ref AnnualRevenueIncreaseValue, value); }

        }

        /// <summary>
        /// Flag indicating economic feasibility
        /// </summary>
        private bool IsEconomicallyFeasibleValue;

        public bool IsEconomicallyFeasible

        {

            get { return this.IsEconomicallyFeasibleValue; }

            set { SetProperty(ref IsEconomicallyFeasibleValue, value); }

        }

        /// <summary>
        /// Optimization recommendations
        /// </summary>
        private List<string> RecommendationsValue = new();

        public List<string> Recommendations

        {

            get { return this.RecommendationsValue; }

            set { SetProperty(ref RecommendationsValue, value); }

        }
    }

    /// <summary>
    /// Result class for sucker rod pump diagnostics
    /// </summary>
    public class SRPDiagnosticsResult : ModelEntityBase
    {
        /// <summary>
        /// Diagnosis date
        /// </summary>
        private System.DateTime DiagnosisDateValue;

        public System.DateTime DiagnosisDate

        {

            get { return this.DiagnosisDateValue; }

            set { SetProperty(ref DiagnosisDateValue, value); }

        }

        /// <summary>
        /// User who performed diagnosis
        /// </summary>
        private string DiagnosedByUserValue = string.Empty;

        public string DiagnosedByUser

        {

            get { return this.DiagnosedByUserValue; }

            set { SetProperty(ref DiagnosedByUserValue, value); }

        }

        /// <summary>
        /// Well UWI
        /// </summary>
        private string WellUWIValue = string.Empty;

        public string WellUWI

        {

            get { return this.WellUWIValue; }

            set { SetProperty(ref WellUWIValue, value); }

        }

        /// <summary>
        /// Current load in pounds
        /// </summary>
        private decimal CurrentLoadValue;

        public decimal CurrentLoad

        {

            get { return this.CurrentLoadValue; }

            set { SetProperty(ref CurrentLoadValue, value); }

        }

        /// <summary>
        /// Rated load in pounds
        /// </summary>
        private decimal RatedLoadValue;

        public decimal RatedLoad

        {

            get { return this.RatedLoadValue; }

            set { SetProperty(ref RatedLoadValue, value); }

        }

        /// <summary>
        /// Current motor amperage
        /// </summary>
        private decimal CurrentAmpsValue;

        public decimal CurrentAmps

        {

            get { return this.CurrentAmpsValue; }

            set { SetProperty(ref CurrentAmpsValue, value); }

        }

        /// <summary>
        /// Rated motor amperage
        /// </summary>
        private decimal RatedAmpsValue;

        public decimal RatedAmps

        {

            get { return this.RatedAmpsValue; }

            set { SetProperty(ref RatedAmpsValue, value); }

        }

        /// <summary>
        /// Load percentage (0-100+)
        /// </summary>
        private decimal LoadPercentageValue;

        public decimal LoadPercentage

        {

            get { return this.LoadPercentageValue; }

            set { SetProperty(ref LoadPercentageValue, value); }

        }

        /// <summary>
        /// Amperage percentage (0-100+)
        /// </summary>
        private decimal AmpPercentageValue;

        public decimal AmpPercentage

        {

            get { return this.AmpPercentageValue; }

            set { SetProperty(ref AmpPercentageValue, value); }

        }

        /// <summary>
        /// List of detected issues
        /// </summary>
        private List<string> IssuesDetectedValue = new();

        public List<string> IssuesDetected

        {

            get { return this.IssuesDetectedValue; }

            set { SetProperty(ref IssuesDetectedValue, value); }

        }

        /// <summary>
        /// Diagnosis status (Normal, Warning, Critical)
        /// </summary>
        private string DiagnosisStatusValue = "Normal";

        public string DiagnosisStatus

        {

            get { return this.DiagnosisStatusValue; }

            set { SetProperty(ref DiagnosisStatusValue, value); }

        }

        /// <summary>
        /// Recommended actions
        /// </summary>
        private List<string> RecommendedActionsValue = new();

        public List<string> RecommendedActions

        {

            get { return this.RecommendedActionsValue; }

            set { SetProperty(ref RecommendedActionsValue, value); }

        }
    }
    /// <summary>
    /// Request for Pump Performance Analysis calculation
    /// </summary>
    public class PumpAnalysisRequest : ModelEntityBase
    {
        private string? WellIdValue;

        public string? WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

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

        } // WELL_EQUIPMENT or FACILITY_EQUIPMENT ROW_ID
        private string PumpTypeValue = "ESP";

        public string PumpType

        {

            get { return this.PumpTypeValue; }

            set { SetProperty(ref PumpTypeValue, value); }

        } // ESP, CENTRIFUGAL, POSITIVE_DISPLACEMENT, JET
        private string AnalysisTypeValue = "PERFORMANCE";

        public string AnalysisType

        {

            get { return this.AnalysisTypeValue; }

            set { SetProperty(ref AnalysisTypeValue, value); }

        } // PERFORMANCE, DESIGN, EFFICIENCY, NPSH, SYSTEM_CURVE
        
        // Pump properties (optional, will be retrieved from equipment if not provided)
        private decimal? FlowRateValue;

        public decimal? FlowRate

        {

            get { return this.FlowRateValue; }

            set { SetProperty(ref FlowRateValue, value); }

        } // GPM or bbl/day
        private decimal? HeadValue;

        public decimal? Head

        {

            get { return this.HeadValue; }

            set { SetProperty(ref HeadValue, value); }

        } // feet
        private decimal? PowerValue;

        public decimal? Power

        {

            get { return this.PowerValue; }

            set { SetProperty(ref PowerValue, value); }

        } // horsepower
        private decimal? EfficiencyValue;

        public decimal? Efficiency

        {

            get { return this.EfficiencyValue; }

            set { SetProperty(ref EfficiencyValue, value); }

        } // fraction 0-1
        private decimal? SpeedValue;

        public decimal? Speed

        {

            get { return this.SpeedValue; }

            set { SetProperty(ref SpeedValue, value); }

        } // RPM
        private decimal? ImpellerDiameterValue;

        public decimal? ImpellerDiameter

        {

            get { return this.ImpellerDiameterValue; }

            set { SetProperty(ref ImpellerDiameterValue, value); }

        } // inches
        private int? NumberOfStagesValue;

        public int? NumberOfStages

        {

            get { return this.NumberOfStagesValue; }

            set { SetProperty(ref NumberOfStagesValue, value); }

        } // for ESP
        
        // System properties
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
        private decimal? FluidDensityValue;

        public decimal? FluidDensity

        {

            get { return this.FluidDensityValue; }

            set { SetProperty(ref FluidDensityValue, value); }

        } // lb/ft³
        private decimal? FluidViscosityValue;

        public decimal? FluidViscosity

        {

            get { return this.FluidViscosityValue; }

            set { SetProperty(ref FluidViscosityValue, value); }

        } // cP
        
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
    /// Result of Pump Performance Analysis calculation
    /// </summary>
    public class PumpAnalysisResult : ModelEntityBase
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
        private string PumpTypeValue = string.Empty;

        public string PumpType

        {

            get { return this.PumpTypeValue; }

            set { SetProperty(ref PumpTypeValue, value); }

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
        private decimal? FlowRateValue;

        public decimal? FlowRate

        {

            get { return this.FlowRateValue; }

            set { SetProperty(ref FlowRateValue, value); }

        } // GPM or bbl/day
        private decimal? HeadValue;

        public decimal? Head

        {

            get { return this.HeadValue; }

            set { SetProperty(ref HeadValue, value); }

        } // feet
        private decimal? PowerValue;

        public decimal? Power

        {

            get { return this.PowerValue; }

            set { SetProperty(ref PowerValue, value); }

        } // horsepower
        private decimal? EfficiencyValue;

        public decimal? Efficiency

        {

            get { return this.EfficiencyValue; }

            set { SetProperty(ref EfficiencyValue, value); }

        } // fraction 0-1
        private decimal? BestEfficiencyPointValue;

        public decimal? BestEfficiencyPoint

        {

            get { return this.BestEfficiencyPointValue; }

            set { SetProperty(ref BestEfficiencyPointValue, value); }

        } // flow rate at BEP
        
        // Performance curve points
        private List<PumpPerformancePoint>? PerformanceCurveValue;

        public List<PumpPerformancePoint>? PerformanceCurve

        {

            get { return this.PerformanceCurveValue; }

            set { SetProperty(ref PerformanceCurveValue, value); }

        }
        
        // System analysis
        private decimal? OperatingPointFlowRateValue;

        public decimal? OperatingPointFlowRate

        {

            get { return this.OperatingPointFlowRateValue; }

            set { SetProperty(ref OperatingPointFlowRateValue, value); }

        }
        private decimal? OperatingPointHeadValue;

        public decimal? OperatingPointHead

        {

            get { return this.OperatingPointHeadValue; }

            set { SetProperty(ref OperatingPointHeadValue, value); }

        }
        private decimal? NPSHAvailableValue;

        public decimal? NPSHAvailable

        {

            get { return this.NPSHAvailableValue; }

            set { SetProperty(ref NPSHAvailableValue, value); }

        } // feet
        private decimal? NPSHRequiredValue;

        public decimal? NPSHRequired

        {

            get { return this.NPSHRequiredValue; }

            set { SetProperty(ref NPSHRequiredValue, value); }

        } // feet
        private bool? CavitationRiskValue;

        public bool? CavitationRisk

        {

            get { return this.CavitationRiskValue; }

            set { SetProperty(ref CavitationRiskValue, value); }

        }
        
        // Design results (for ESP)
        private int? RecommendedStagesValue;

        public int? RecommendedStages

        {

            get { return this.RecommendedStagesValue; }

            set { SetProperty(ref RecommendedStagesValue, value); }

        }
        private decimal? RecommendedMotorSizeValue;

        public decimal? RecommendedMotorSize

        {

            get { return this.RecommendedMotorSizeValue; }

            set { SetProperty(ref RecommendedMotorSizeValue, value); }

        } // horsepower
        
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

    /// <summary>
    /// Pump performance point
    /// </summary>
    public class PumpPerformancePoint : ModelEntityBase
    {
        private decimal FlowRateValue;

        public decimal FlowRate

        {

            get { return this.FlowRateValue; }

            set { SetProperty(ref FlowRateValue, value); }

        } // GPM or bbl/day
        private decimal HeadValue;

        public decimal Head

        {

            get { return this.HeadValue; }

            set { SetProperty(ref HeadValue, value); }

        } // feet
        private decimal PowerValue;

        public decimal Power

        {

            get { return this.PowerValue; }

            set { SetProperty(ref PowerValue, value); }

        } // horsepower
        private decimal EfficiencyValue;

        public decimal Efficiency

        {

            get { return this.EfficiencyValue; }

            set { SetProperty(ref EfficiencyValue, value); }

        } // fraction 0-1
    }
}








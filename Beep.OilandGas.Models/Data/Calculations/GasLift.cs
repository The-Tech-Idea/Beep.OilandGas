using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data.GasLift;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.Calculations
{
    /// <summary>
    /// Request for analyzing gas lift potential
    /// </summary>
    public class AnalyzeGasLiftPotentialRequest : ModelEntityBase
    {
        /// <summary>
        /// Well properties for gas lift analysis
        /// </summary>
        private GasLiftWellProperties WellPropertiesValue = null!;

        [Required(ErrorMessage = "WellProperties are required")]
        public GasLiftWellProperties WellProperties

        {

            get { return this.WellPropertiesValue; }

            set { SetProperty(ref WellPropertiesValue, value); }

        }

        /// <summary>
        /// Minimum gas injection rate (Mscf/day)
        /// </summary>
        private decimal MinGasInjectionRateValue;

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "MinGasInjectionRate must be greater than or equal to 0")]
        public decimal MinGasInjectionRate

        {

            get { return this.MinGasInjectionRateValue; }

            set { SetProperty(ref MinGasInjectionRateValue, value); }

        }

        /// <summary>
        /// Maximum gas injection rate (Mscf/day)
        /// </summary>
        private decimal MaxGasInjectionRateValue;

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "MaxGasInjectionRate must be greater than or equal to 0")]
        public decimal MaxGasInjectionRate

        {

            get { return this.MaxGasInjectionRateValue; }

            set { SetProperty(ref MaxGasInjectionRateValue, value); }

        }

        /// <summary>
        /// Number of points for performance curve
        /// </summary>
        private int NumberOfPointsValue = 50;

        [Range(2, 1000, ErrorMessage = "NumberOfPoints must be between 2 and 1000")]
        public int NumberOfPoints

        {

            get { return this.NumberOfPointsValue; }

            set { SetProperty(ref NumberOfPointsValue, value); }

        }
    }

    /// <summary>
    /// Request for designing gas lift valves
    /// </summary>
    public class DesignValvesRequest : ModelEntityBase
    {
        /// <summary>
        /// Well properties for valve design
        /// </summary>
        private GasLiftWellProperties WellPropertiesValue = null!;

        [Required(ErrorMessage = "WellProperties are required")]
        public GasLiftWellProperties WellProperties

        {

            get { return this.WellPropertiesValue; }

            set { SetProperty(ref WellPropertiesValue, value); }

        }

        /// <summary>
        /// Gas injection pressure (psia)
        /// </summary>
        private decimal GasInjectionPressureValue;

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "GasInjectionPressure must be greater than or equal to 0")]
        public decimal GasInjectionPressure

        {

            get { return this.GasInjectionPressureValue; }

            set { SetProperty(ref GasInjectionPressureValue, value); }

        }

        /// <summary>
        /// Number of valves to design
        /// </summary>
        private int NumberOfValvesValue;

        [Range(1, 50, ErrorMessage = "NumberOfValves must be between 1 and 50")]
        public int NumberOfValves

        {

            get { return this.NumberOfValvesValue; }

            set { SetProperty(ref NumberOfValvesValue, value); }

        }

        /// <summary>
        /// Whether to use SI units (false = use field units)
        /// </summary>
        private bool UseSIUnitsValue = false;

        public bool UseSIUnits

        {

            get { return this.UseSIUnitsValue; }

            set { SetProperty(ref UseSIUnitsValue, value); }

        }
    }
    /// <summary>
    /// Request for Gas Lift Analysis calculation
    /// </summary>
    public class GasLiftAnalysisRequest : ModelEntityBase
    {
        private string? WellIdValue;

        public string? WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }
        private string AnalysisTypeValue = "POTENTIAL";

        public string AnalysisType

        {

            get { return this.AnalysisTypeValue; }

            set { SetProperty(ref AnalysisTypeValue, value); }

        } // POTENTIAL, VALVE_DESIGN, VALVE_SPACING
        
        // Well properties (optional, will be retrieved from WELL if not provided)
        private decimal? WellDepthValue;

        public decimal? WellDepth

        {

            get { return this.WellDepthValue; }

            set { SetProperty(ref WellDepthValue, value); }

        } // feet
        private decimal? TubingDiameterValue;

        public decimal? TubingDiameter

        {

            get { return this.TubingDiameterValue; }

            set { SetProperty(ref TubingDiameterValue, value); }

        } // inches
        private decimal? CasingDiameterValue;

        public decimal? CasingDiameter

        {

            get { return this.CasingDiameterValue; }

            set { SetProperty(ref CasingDiameterValue, value); }

        } // inches
        private decimal? WellheadPressureValue;

        public decimal? WellheadPressure

        {

            get { return this.WellheadPressureValue; }

            set { SetProperty(ref WellheadPressureValue, value); }

        } // psia
        private decimal? BottomHolePressureValue;

        public decimal? BottomHolePressure

        {

            get { return this.BottomHolePressureValue; }

            set { SetProperty(ref BottomHolePressureValue, value); }

        } // psia
        private decimal? WellheadTemperatureValue;

        public decimal? WellheadTemperature

        {

            get { return this.WellheadTemperatureValue; }

            set { SetProperty(ref WellheadTemperatureValue, value); }

        } // Rankine
        private decimal? BottomHoleTemperatureValue;

        public decimal? BottomHoleTemperature

        {

            get { return this.BottomHoleTemperatureValue; }

            set { SetProperty(ref BottomHoleTemperatureValue, value); }

        } // Rankine
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
        private decimal? GasSpecificGravityValue;

        public decimal? GasSpecificGravity

        {

            get { return this.GasSpecificGravityValue; }

            set { SetProperty(ref GasSpecificGravityValue, value); }

        }
        private decimal? DesiredProductionRateValue;

        public decimal? DesiredProductionRate

        {

            get { return this.DesiredProductionRateValue; }

            set { SetProperty(ref DesiredProductionRateValue, value); }

        } // bbl/day
        
        // Analysis parameters
        private decimal? MinGasInjectionRateValue;

        public decimal? MinGasInjectionRate

        {

            get { return this.MinGasInjectionRateValue; }

            set { SetProperty(ref MinGasInjectionRateValue, value); }

        } // Mscf/day
        private decimal? MaxGasInjectionRateValue;

        public decimal? MaxGasInjectionRate

        {

            get { return this.MaxGasInjectionRateValue; }

            set { SetProperty(ref MaxGasInjectionRateValue, value); }

        } // Mscf/day
        private int? NumberOfPointsValue;

        public int? NumberOfPoints

        {

            get { return this.NumberOfPointsValue; }

            set { SetProperty(ref NumberOfPointsValue, value); }

        } // for performance curve
        
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
    /// Result of Gas Lift Analysis calculation
    /// </summary>
    public class GasLiftAnalysisResult : ModelEntityBase
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
        
        // Optimal results
        private decimal OptimalGasInjectionRateValue;

        public decimal OptimalGasInjectionRate

        {

            get { return this.OptimalGasInjectionRateValue; }

            set { SetProperty(ref OptimalGasInjectionRateValue, value); }

        } // Mscf/day
        private decimal MaximumProductionRateValue;

        public decimal MaximumProductionRate

        {

            get { return this.MaximumProductionRateValue; }

            set { SetProperty(ref MaximumProductionRateValue, value); }

        } // bbl/day
        private decimal OptimalGasLiquidRatioValue;

        public decimal OptimalGasLiquidRatio

        {

            get { return this.OptimalGasLiquidRatioValue; }

            set { SetProperty(ref OptimalGasLiquidRatioValue, value); }

        }
        
        // Performance curve points
        private List<GasLiftPerformancePoint> PerformancePointsValue = new List<GasLiftPerformancePoint>();

        public List<GasLiftPerformancePoint> PerformancePoints

        {

            get { return this.PerformancePointsValue; }

            set { SetProperty(ref PerformancePointsValue, value); }

        }
        
        // Valve design results (if AnalysisType is VALVE_DESIGN)
        private List<GasLiftValveData>? ValvesValue;

        public List<GasLiftValveData>? Valves

        {

            get { return this.ValvesValue; }

            set { SetProperty(ref ValvesValue, value); }

        }
        private decimal? TotalGasInjectionRateValue;

        public decimal? TotalGasInjectionRate

        {

            get { return this.TotalGasInjectionRateValue; }

            set { SetProperty(ref TotalGasInjectionRateValue, value); }

        }
        private decimal? ExpectedProductionRateValue;

        public decimal? ExpectedProductionRate

        {

            get { return this.ExpectedProductionRateValue; }

            set { SetProperty(ref ExpectedProductionRateValue, value); }

        }
        private decimal? SystemEfficiencyValue;

        public decimal? SystemEfficiency

        {

            get { return this.SystemEfficiencyValue; }

            set { SetProperty(ref SystemEfficiencyValue, value); }

        }
        
        // Valve spacing results (if AnalysisType is VALVE_SPACING)
        private List<decimal>? ValveDepthsValue;

        public List<decimal>? ValveDepths

        {

            get { return this.ValveDepthsValue; }

            set { SetProperty(ref ValveDepthsValue, value); }

        } // feet
        private List<decimal>? OpeningPressuresValue;

        public List<decimal>? OpeningPressures

        {

            get { return this.OpeningPressuresValue; }

            set { SetProperty(ref OpeningPressuresValue, value); }

        } // psia
        private int? NumberOfValvesValue;

        public int? NumberOfValves

        {

            get { return this.NumberOfValvesValue; }

            set { SetProperty(ref NumberOfValvesValue, value); }

        }
        private decimal? TotalDepthCoverageValue;

        public decimal? TotalDepthCoverage

        {

            get { return this.TotalDepthCoverageValue; }

            set { SetProperty(ref TotalDepthCoverageValue, value); }

        } // feet
        
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
    /// Gas lift performance point
    /// </summary>
    public class GasLiftPerformancePoint : ModelEntityBase
    {
        private decimal GasInjectionRateValue;

        public decimal GasInjectionRate

        {

            get { return this.GasInjectionRateValue; }

            set { SetProperty(ref GasInjectionRateValue, value); }

        } // Mscf/day
        private decimal ProductionRateValue;

        public decimal ProductionRate

        {

            get { return this.ProductionRateValue; }

            set { SetProperty(ref ProductionRateValue, value); }

        } // bbl/day
        private decimal GasLiquidRatioValue;

        public decimal GasLiquidRatio

        {

            get { return this.GasLiquidRatioValue; }

            set { SetProperty(ref GasLiquidRatioValue, value); }

        }
        private decimal BottomHolePressureValue;

        public decimal BottomHolePressure

        {

            get { return this.BottomHolePressureValue; }

            set { SetProperty(ref BottomHolePressureValue, value); }

        } // psia
    }

    /// <summary>
    /// Gas lift valve data
    /// </summary>
    public class GasLiftValveData : ModelEntityBase
    {
        private decimal DepthValue;

        public decimal Depth

        {

            get { return this.DepthValue; }

            set { SetProperty(ref DepthValue, value); }

        } // feet
        private decimal PortSizeValue;

        public decimal PortSize

        {

            get { return this.PortSizeValue; }

            set { SetProperty(ref PortSizeValue, value); }

        } // inches
        private decimal OpeningPressureValue;

        public decimal OpeningPressure

        {

            get { return this.OpeningPressureValue; }

            set { SetProperty(ref OpeningPressureValue, value); }

        } // psia
        private decimal ClosingPressureValue;

        public decimal ClosingPressure

        {

            get { return this.ClosingPressureValue; }

            set { SetProperty(ref ClosingPressureValue, value); }

        } // psia
        private string ValveTypeValue = string.Empty;

        public string ValveType

        {

            get { return this.ValveTypeValue; }

            set { SetProperty(ref ValveTypeValue, value); }

        }
        private decimal TemperatureValue;

        public decimal Temperature

        {

            get { return this.TemperatureValue; }

            set { SetProperty(ref TemperatureValue, value); }

        } // Rankine
        private decimal GasInjectionRateValue;

        public decimal GasInjectionRate

        {

            get { return this.GasInjectionRateValue; }

            set { SetProperty(ref GasInjectionRateValue, value); }

        } // Mscf/day
    }
}








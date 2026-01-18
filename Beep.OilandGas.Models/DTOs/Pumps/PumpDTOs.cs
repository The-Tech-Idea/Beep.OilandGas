using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.PlungerLift;
using Beep.OilandGas.Models.SuckerRodPumping;

namespace Beep.OilandGas.Models.DTOs.Pumps
{
    /// <summary>
    /// Request for designing a pump system (generic/hydraulic pump)
    /// </summary>
    public class DesignPumpSystemRequest
    {
        /// <summary>
        /// Well UWI (Unique Well Identifier)
        /// </summary>
        [Required(ErrorMessage = "WellUWI is required")]
        public string WellUWI { get; set; } = string.Empty;

        /// <summary>
        /// Pump type (e.g., "ESP", "SuckerRod", "Hydraulic", "PlungerLift")
        /// </summary>
        [Required(ErrorMessage = "PumpType is required")]
        public string PumpType { get; set; } = string.Empty;

        /// <summary>
        /// Well depth in feet
        /// </summary>
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "WellDepth must be greater than or equal to 0")]
        public decimal WellDepth { get; set; }

        /// <summary>
        /// Desired flow rate (bbl/day)
        /// </summary>
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "DesiredFlowRate must be greater than or equal to 0")]
        public decimal DesiredFlowRate { get; set; }
    }

    /// <summary>
    /// Request for designing a plunger lift system
    /// </summary>
    public class DesignPlungerLiftSystemRequest
    {
        /// <summary>
        /// Well UWI (Unique Well Identifier)
        /// </summary>
        [Required(ErrorMessage = "WellUWI is required")]
        public string WellUWI { get; set; } = string.Empty;

        /// <summary>
        /// Well properties for plunger lift design
        /// </summary>
        [Required(ErrorMessage = "WellProperties are required")]
        public PlungerLiftWellPropertiesDto WellProperties { get; set; } = null!;
    }

    /// <summary>
    /// Request for designing a sucker rod pump system
    /// </summary>
    public class DesignSuckerRodPumpSystemRequest
    {
        /// <summary>
        /// Well UWI (Unique Well Identifier)
        /// </summary>
        [Required(ErrorMessage = "WellUWI is required")]
        public string WellUWI { get; set; } = string.Empty;

        /// <summary>
        /// Well properties for sucker rod pump design
        /// </summary>
        [Required(ErrorMessage = "WellProperties are required")]
        public SuckerRodPumpWellPropertiesDto WellProperties { get; set; } = null!;
    }

    /// <summary>
    /// Request for analyzing pump performance
    /// </summary>
    public class AnalyzePerformanceRequest
    {
        /// <summary>
        /// Pump identifier or Well UWI (depending on pump type)
        /// </summary>
        [Required(ErrorMessage = "PumpId or WellUWI is required")]
        public string PumpId { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for plunger lift well properties
    /// </summary>
    public class PlungerLiftWellPropertiesDto
    {
        /// <summary>
        /// Well depth in feet
        /// </summary>
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "WellDepth must be greater than or equal to 0")]
        public decimal WellDepth { get; set; }

        /// <summary>
        /// Tubing size in inches
        /// </summary>
        [Required]
        [Range(0.5, 10, ErrorMessage = "TubingSize must be between 0.5 and 10 inches")]
        public decimal TubingSize { get; set; }

        /// <summary>
        /// Casing size in inches
        /// </summary>
        [Required]
        [Range(0.5, 20, ErrorMessage = "CasingSize must be between 0.5 and 20 inches")]
        public decimal CasingSize { get; set; }

        /// <summary>
        /// Static fluid level in feet
        /// </summary>
        [Required]
        [Range(0, double.MaxValue)]
        public decimal StaticFluidLevel { get; set; }

        /// <summary>
        /// Reservoir pressure in psi
        /// </summary>
        [Required]
        [Range(0, double.MaxValue)]
        public decimal ReservoirPressure { get; set; }

        /// <summary>
        /// Desired production rate in bbl/day
        /// </summary>
        [Required]
        [Range(0, double.MaxValue)]
        public decimal DesiredProductionRate { get; set; }
    }

    /// <summary>
    /// DTO for sucker rod pump well properties
    /// </summary>
    public class SuckerRodPumpWellPropertiesDto
    {
        /// <summary>
        /// Well depth in feet
        /// </summary>
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "WellDepth must be greater than or equal to 0")]
        public decimal WellDepth { get; set; }

        /// <summary>
        /// Tubing size in inches
        /// </summary>
        [Required]
        [Range(0.5, 10, ErrorMessage = "TubingSize must be between 0.5 and 10 inches")]
        public decimal TubingSize { get; set; }

        /// <summary>
        /// Casing size in inches
        /// </summary>
        [Required]
        [Range(0.5, 20, ErrorMessage = "CasingSize must be between 0.5 and 20 inches")]
        public decimal CasingSize { get; set; }

        /// <summary>
        /// Static fluid level in feet
        /// </summary>
        [Required]
        [Range(0, double.MaxValue)]
        public decimal StaticFluidLevel { get; set; }

        /// <summary>
        /// Producing fluid level in feet
        /// </summary>
        [Required]
        [Range(0, double.MaxValue)]
        public decimal ProducingFluidLevel { get; set; }

        /// <summary>
        /// API gravity of produced fluid
        /// </summary>
        [Required]
        [Range(0, 60, ErrorMessage = "APIGravity must be between 0 and 60")]
        public decimal APIGravity { get; set; }

        /// <summary>
        /// Desired production rate in bbl/day
        /// </summary>
        [Required]
        [Range(0, double.MaxValue)]
        public decimal DesiredProductionRate { get; set; }

        /// <summary>
        /// Gas-Oil Ratio (GOR) in scf/stb
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal GasOilRatio { get; set; }

        /// <summary>
        /// Fluid density in lb/ft³
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal FluidDensity { get; set; }

        /// <summary>
        /// Pump type/size designation
        /// </summary>
        public string? PumpTypeDesignation { get; set; }
    }

    /// <summary>
    /// DTO for sucker rod pump design
    /// </summary>
    public class SuckerRodPumpDesignDto
    {
        /// <summary>
        /// Unique design identifier
        /// </summary>
        public string DesignId { get; set; } = string.Empty;

        /// <summary>
        /// Well UWI (Unique Well Identifier)
        /// </summary>
        public string WellUWI { get; set; } = string.Empty;

        /// <summary>
        /// Design date
        /// </summary>
        public System.DateTime DesignDate { get; set; }

        /// <summary>
        /// Pump depth in feet
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal PumpDepth { get; set; }

        /// <summary>
        /// Pump size in inches
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal PumpSize { get; set; }

        /// <summary>
        /// Stroke length in inches
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal StrokeLength { get; set; }

        /// <summary>
        /// Strokes per minute
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal StrokesPerMinute { get; set; }

        /// <summary>
        /// Rod string load in pounds
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal RodStringLoad { get; set; }

        /// <summary>
        /// Pump type designation
        /// </summary>
        public string? PumpType { get; set; }

        /// <summary>
        /// Rod grade specification
        /// </summary>
        public string? RodGrade { get; set; }

        /// <summary>
        /// Estimated capacity in bbl/day
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal EstimatedCapacity { get; set; }

        /// <summary>
        /// Power requirement in horsepower
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal PowerRequirement { get; set; }

        /// <summary>
        /// Design status
        /// </summary>
        public string? Status { get; set; }
    }

    /// <summary>
    /// DTO for sucker rod pump performance analysis
    /// </summary>
    public class SuckerRodPumpPerformanceDto
    {
        /// <summary>
        /// Pump identifier
        /// </summary>
        public string PumpId { get; set; } = string.Empty;

        /// <summary>
        /// Performance analysis date
        /// </summary>
        public System.DateTime PerformanceDate { get; set; }

        /// <summary>
        /// Flow rate in bbl/day
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal FlowRate { get; set; }

        /// <summary>
        /// Pump efficiency (0-1 or percentage)
        /// </summary>
        [Range(0, 1.5)]
        public decimal Efficiency { get; set; }

        /// <summary>
        /// Power consumption in horsepower
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal PowerConsumption { get; set; }

        /// <summary>
        /// Rod load percentage (0-100+)
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal RodLoadPercentage { get; set; }

        /// <summary>
        /// Performance status
        /// </summary>
        public string? Status { get; set; }
    }

    /// <summary>
    /// Result class for sucker rod pump optimization analysis
    /// </summary>
    public class SRPOptimizationResult
    {
        /// <summary>
        /// Optimization analysis date
        /// </summary>
        public System.DateTime OptimizationDate { get; set; }

        /// <summary>
        /// User who performed optimization
        /// </summary>
        public string OptimizedByUser { get; set; } = string.Empty;

        /// <summary>
        /// Well UWI
        /// </summary>
        public string WellUWI { get; set; } = string.Empty;

        /// <summary>
        /// Current production rate in bbl/day
        /// </summary>
        public decimal CurrentProduction { get; set; }

        /// <summary>
        /// Target production rate in bbl/day
        /// </summary>
        public decimal TargetProduction { get; set; }

        /// <summary>
        /// Production increase percentage
        /// </summary>
        public decimal ProductionIncreasePercent { get; set; }

        /// <summary>
        /// Additional daily revenue impact
        /// </summary>
        public decimal AdditionalDailyRevenue { get; set; }

        /// <summary>
        /// Annual revenue increase
        /// </summary>
        public decimal AnnualRevenueIncrease { get; set; }

        /// <summary>
        /// Flag indicating economic feasibility
        /// </summary>
        public bool IsEconomicallyFeasible { get; set; }

        /// <summary>
        /// Optimization recommendations
        /// </summary>
        public List<string> Recommendations { get; set; } = new();
    }

    /// <summary>
    /// Result class for sucker rod pump diagnostics
    /// </summary>
    public class SRPDiagnosticsResult
    {
        /// <summary>
        /// Diagnosis date
        /// </summary>
        public System.DateTime DiagnosisDate { get; set; }

        /// <summary>
        /// User who performed diagnosis
        /// </summary>
        public string DiagnosedByUser { get; set; } = string.Empty;

        /// <summary>
        /// Well UWI
        /// </summary>
        public string WellUWI { get; set; } = string.Empty;

        /// <summary>
        /// Current load in pounds
        /// </summary>
        public decimal CurrentLoad { get; set; }

        /// <summary>
        /// Rated load in pounds
        /// </summary>
        public decimal RatedLoad { get; set; }

        /// <summary>
        /// Current motor amperage
        /// </summary>
        public decimal CurrentAmps { get; set; }

        /// <summary>
        /// Rated motor amperage
        /// </summary>
        public decimal RatedAmps { get; set; }

        /// <summary>
        /// Load percentage (0-100+)
        /// </summary>
        public decimal LoadPercentage { get; set; }

        /// <summary>
        /// Amperage percentage (0-100+)
        /// </summary>
        public decimal AmpPercentage { get; set; }

        /// <summary>
        /// List of detected issues
        /// </summary>
        public List<string> IssuesDetected { get; set; } = new();

        /// <summary>
        /// Diagnosis status (Normal, Warning, Critical)
        /// </summary>
        public string DiagnosisStatus { get; set; } = "Normal";

        /// <summary>
        /// Recommended actions
        /// </summary>
        public List<string> RecommendedActions { get; set; } = new();
    }
}




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
}

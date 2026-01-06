using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.GasLift;

namespace Beep.OilandGas.Models.DTOs.Calculations
{
    /// <summary>
    /// Request for analyzing gas lift potential
    /// </summary>
    public class AnalyzeGasLiftPotentialRequest
    {
        /// <summary>
        /// Well properties for gas lift analysis
        /// </summary>
        [Required(ErrorMessage = "WellProperties are required")]
        public GasLiftWellProperties WellProperties { get; set; } = null!;

        /// <summary>
        /// Minimum gas injection rate (Mscf/day)
        /// </summary>
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "MinGasInjectionRate must be greater than or equal to 0")]
        public decimal MinGasInjectionRate { get; set; }

        /// <summary>
        /// Maximum gas injection rate (Mscf/day)
        /// </summary>
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "MaxGasInjectionRate must be greater than or equal to 0")]
        public decimal MaxGasInjectionRate { get; set; }

        /// <summary>
        /// Number of points for performance curve
        /// </summary>
        [Range(2, 1000, ErrorMessage = "NumberOfPoints must be between 2 and 1000")]
        public int NumberOfPoints { get; set; } = 50;
    }

    /// <summary>
    /// Request for designing gas lift valves
    /// </summary>
    public class DesignValvesRequest
    {
        /// <summary>
        /// Well properties for valve design
        /// </summary>
        [Required(ErrorMessage = "WellProperties are required")]
        public GasLiftWellProperties WellProperties { get; set; } = null!;

        /// <summary>
        /// Gas injection pressure (psia)
        /// </summary>
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "GasInjectionPressure must be greater than or equal to 0")]
        public decimal GasInjectionPressure { get; set; }

        /// <summary>
        /// Number of valves to design
        /// </summary>
        [Range(1, 50, ErrorMessage = "NumberOfValves must be between 1 and 50")]
        public int NumberOfValves { get; set; }

        /// <summary>
        /// Whether to use SI units (false = use field units)
        /// </summary>
        public bool UseSIUnits { get; set; } = false;
    }
}




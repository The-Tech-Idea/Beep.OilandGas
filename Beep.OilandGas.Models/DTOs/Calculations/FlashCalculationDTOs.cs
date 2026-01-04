using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.FlashCalculations;

namespace Beep.OilandGas.Models.DTOs.Calculations
{
    /// <summary>
    /// Request for multi-stage flash calculation
    /// </summary>
    public class MultiStageFlashRequest
    {
        /// <summary>
        /// Flash conditions
        /// </summary>
        [Required(ErrorMessage = "Conditions are required")]
        public FlashConditions Conditions { get; set; } = null!;

        /// <summary>
        /// Number of flash stages
        /// </summary>
        [Required]
        [Range(1, 100, ErrorMessage = "Stages must be between 1 and 100")]
        public int Stages { get; set; }
    }
}

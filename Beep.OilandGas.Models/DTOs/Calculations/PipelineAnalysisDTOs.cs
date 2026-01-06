using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.DTOs.Calculations
{
    /// <summary>
    /// Request for analyzing pipeline flow
    /// </summary>
    public class AnalyzePipelineFlowRequest
    {
        /// <summary>
        /// Pipeline identifier
        /// </summary>
        [Required(ErrorMessage = "PipelineId is required")]
        public string PipelineId { get; set; } = string.Empty;

        /// <summary>
        /// Flow rate (Mscf/day for gas, bbl/day for liquid)
        /// </summary>
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "FlowRate must be greater than or equal to 0")]
        public decimal FlowRate { get; set; }

        /// <summary>
        /// Inlet pressure (psia)
        /// </summary>
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "InletPressure must be greater than or equal to 0")]
        public decimal InletPressure { get; set; }
    }

    /// <summary>
    /// Request for calculating pressure drop
    /// </summary>
    public class CalculatePressureDropRequest
    {
        /// <summary>
        /// Pipeline identifier
        /// </summary>
        [Required(ErrorMessage = "PipelineId is required")]
        public string PipelineId { get; set; } = string.Empty;

        /// <summary>
        /// Flow rate (Mscf/day for gas, bbl/day for liquid)
        /// </summary>
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "FlowRate must be greater than or equal to 0")]
        public decimal FlowRate { get; set; }
    }
}




using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.DTOs.NodalAnalysis;

namespace Beep.OilandGas.Models.DTOs.Calculations
{
    /// <summary>
    /// Request for performing nodal analysis
    /// </summary>
    public class PerformNodalAnalysisRequest
    {
        /// <summary>
        /// Well UWI (Unique Well Identifier)
        /// </summary>
        [Required(ErrorMessage = "WellUWI is required")]
        public string WellUWI { get; set; } = string.Empty;

        /// <summary>
        /// Nodal analysis parameters
        /// </summary>
        [Required(ErrorMessage = "AnalysisParameters are required")]
        public NodalAnalysisParametersDto AnalysisParameters { get; set; } = null!;
    }

    /// <summary>
    /// Request for optimizing system performance
    /// </summary>
    public class OptimizeSystemRequest
    {
        /// <summary>
        /// Well UWI (Unique Well Identifier)
        /// </summary>
        [Required(ErrorMessage = "WellUWI is required")]
        public string WellUWI { get; set; } = string.Empty;

        /// <summary>
        /// Optimization goals
        /// </summary>
        [Required(ErrorMessage = "OptimizationGoals are required")]
        public OptimizationGoalsDto OptimizationGoals { get; set; } = null!;
    }
}

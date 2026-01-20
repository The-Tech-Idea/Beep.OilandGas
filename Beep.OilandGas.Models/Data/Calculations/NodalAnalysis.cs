using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    /// <summary>
    /// Request for performing nodal analysis
    /// </summary>
    public class PerformNodalAnalysisRequest : ModelEntityBase
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
        public NodalAnalysisParameters AnalysisParameters { get; set; } = null!;
    }

    /// <summary>
    /// Request for optimizing system performance
    /// </summary>
    public class OptimizeSystemRequest : ModelEntityBase
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
        public OptimizationGoals OptimizationGoals { get; set; } = null!;
    }
}





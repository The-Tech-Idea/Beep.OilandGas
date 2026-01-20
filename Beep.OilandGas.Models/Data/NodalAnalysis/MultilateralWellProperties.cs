using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data.NodalAnalysis
{
    /// <summary>
    /// Represents multilateral well properties for deliverability calculations.
    /// </summary>
    public class MultilateralWellProperties : ModelEntityBase
    {
        /// <summary>
        /// Well UWI (Unique Well Identifier).
        /// </summary>
        public string? WellUWI { get; set; }

        /// <summary>
        /// Main wellbore depth in feet.
        /// </summary>
        public double MainWellboreDepth { get; set; }

        /// <summary>
        /// Collection of lateral branches in the well.
        /// </summary>
        public List<LateralBranch> LateralBranches { get; set; } = new List<LateralBranch>();

        /// <summary>
        /// Junction depth where laterals meet in feet.
        /// </summary>
        public double? JunctionDepth { get; set; }

        /// <summary>
        /// Total pressure drop from junction to wellhead in psia.
        /// </summary>
        public double? JunctionToWellheadPressureDrop { get; set; }

        /// <summary>
        /// Wellhead pressure in psia.
        /// </summary>
        public double? WellheadPressure { get; set; }
    }
}

using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data.GasLift
{
    /// <summary>
    /// Represents the result of a gas lift valve spacing calculation.
    /// </summary>
    public class GasLiftValveSpacingResult : ModelEntityBase
    {
        /// <summary>
        /// Number of valves in the design.
        /// </summary>
        public int NumberOfValves { get; set; }

        /// <summary>
        /// Depths of each valve in feet.
        /// </summary>
        public List<decimal> ValveDepths { get; set; } = new List<decimal>();

        /// <summary>
        /// Opening pressures for each valve in psia.
        /// </summary>
        public List<decimal> OpeningPressures { get; set; } = new List<decimal>();

        /// <summary>
        /// Total depth coverage from first to last valve in feet.
        /// </summary>
        public decimal TotalDepthCoverage { get; set; }
    }
}

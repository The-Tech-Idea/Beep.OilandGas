using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data.NodalAnalysis
{
    /// <summary>
    /// Represents the result of a multilateral well deliverability calculation.
    /// </summary>
    public class MultilateralDeliverabilityResult : ModelEntityBase
    {
        /// <summary>
        /// Bottomhole pressure at the junction in psia.
        /// </summary>
        public double JunctionBottomholePressure { get; set; }

        /// <summary>
        /// Total production rate from all branches in bbl/day (oil) or Mscf/day (gas).
        /// </summary>
        public double TotalProductionRate { get; set; }

        /// <summary>
        /// Production rates by branch name in bbl/day or Mscf/day.
        /// </summary>
        public Dictionary<string, double> BranchProductionRates { get; set; } = new Dictionary<string, double>();

        /// <summary>
        /// Average pressure throughout the system in psia.
        /// </summary>
        public double? AveragePressure { get; set; }

        /// <summary>
        /// Maximum pressure drop across any branch in psi.
        /// </summary>
        public double? MaximumPressureDrop { get; set; }

        /// <summary>
        /// Indicates if the system is stable or unstable.
        /// </summary>
        public bool IsStable { get; set; } = true;
    }
}

using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data.PlungerLift
{
    /// <summary>
    /// Represents plunger lift performance analysis
    /// DTO for calculations - Entity class: PLUNGER_LIFT_PERFORMANCE_RESULT
    /// </summary>
    public class PlungerLiftPerformanceResult : ModelEntityBase
    {
        /// <summary>
        /// Cycle analysis results
        /// </summary>
        public PlungerLiftCycleResult CycleResult { get; set; } = new();

        /// <summary>
        /// Gas requirements
        /// </summary>
        public PlungerLiftGasRequirements GasRequirements { get; set; } = new();

        /// <summary>
        /// System efficiency (0-1)
        /// </summary>
        public decimal SystemEfficiency { get; set; }

        /// <summary>
        /// Whether system is feasible
        /// </summary>
        public bool IsFeasible { get; set; }

        /// <summary>
        /// Feasibility reasons
        /// </summary>
        public List<string> FeasibilityReasons { get; set; } = new();
    }
}




using System.Collections.Generic;

using Beep.OilandGas.Models.Data;
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
        private PlungerLiftCycleResult CycleResultValue = new();

        public PlungerLiftCycleResult CycleResult

        {

            get { return this.CycleResultValue; }

            set { SetProperty(ref CycleResultValue, value); }

        }

        /// <summary>
        /// Gas requirements
        /// </summary>
        private PlungerLiftGasRequirements GasRequirementsValue = new();

        public PlungerLiftGasRequirements GasRequirements

        {

            get { return this.GasRequirementsValue; }

            set { SetProperty(ref GasRequirementsValue, value); }

        }

        /// <summary>
        /// System efficiency (0-1)
        /// </summary>
        private decimal SystemEfficiencyValue;

        public decimal SystemEfficiency

        {

            get { return this.SystemEfficiencyValue; }

            set { SetProperty(ref SystemEfficiencyValue, value); }

        }

        /// <summary>
        /// Whether system is feasible
        /// </summary>
        private bool IsFeasibleValue;

        public bool IsFeasible

        {

            get { return this.IsFeasibleValue; }

            set { SetProperty(ref IsFeasibleValue, value); }

        }

        /// <summary>
        /// Feasibility reasons
        /// </summary>
        private List<string> FeasibilityReasonsValue = new();

        public List<string> FeasibilityReasons

        {

            get { return this.FeasibilityReasonsValue; }

            set { SetProperty(ref FeasibilityReasonsValue, value); }

        }
    }
}







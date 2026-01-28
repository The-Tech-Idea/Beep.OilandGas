using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.PlungerLift
{
    public class PlungerLiftPerformanceResult : ModelEntityBase
    {
        /// <summary>
        /// Cycle analysis results
        /// </summary>
        private PLUNGER_LIFT_CYCLE_RESULT CycleResultValue = new();

        public PLUNGER_LIFT_CYCLE_RESULT CycleResult

        {

            get { return this.CycleResultValue; }

            set { SetProperty(ref CycleResultValue, value); }

        }

        /// <summary>
        /// Gas requirements
        /// </summary>
        private PLUNGER_LIFT_GAS_REQUIREMENTS GasRequirementsValue = new();

        public PLUNGER_LIFT_GAS_REQUIREMENTS GasRequirements

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

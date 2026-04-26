using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.EconomicAnalysis;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class OptionValuation : ModelEntityBase
    {
        /// <summary>
        /// Type of option: Expansion, Abandonment, or Switching
        /// </summary>
        private string OptionTypeValue;

        public string OptionType

        {

            get { return this.OptionTypeValue; }

            set { SetProperty(ref OptionTypeValue, value); }

        }

        /// <summary>
        /// Calculated value of the option
        /// </summary>
        private double OptionValueValue;

        public double OptionValue

        {

            get { return this.OptionValueValue; }

            set { SetProperty(ref OptionValueValue, value); }

        }

        /// <summary>
        /// Project value if the option scenario occurs
        /// </summary>
        private double ScenarioValueValue;

        public double ScenarioValue

        {

            get { return this.ScenarioValueValue; }

            set { SetProperty(ref ScenarioValueValue, value); }

        }

        /// <summary>
        /// Cost to exercise the expansion option
        /// </summary>
        private double ExerciseCostValue;

        public double ExerciseCost

        {

            get { return this.ExerciseCostValue; }

            set { SetProperty(ref ExerciseCostValue, value); }

        }

        /// <summary>
        /// Salvage value if abandonment option is exercised
        /// </summary>
        private double SalvageValueValue;

        public double SalvageValue

        {

            get { return this.SalvageValueValue; }

            set { SetProperty(ref SalvageValueValue, value); }

        }

        /// <summary>
        /// NPV of alternative project if switching option is exercised
        /// </summary>
        private double AlternativeNPVValue;

        public double AlternativeNPV

        {

            get { return this.AlternativeNPVValue; }

            set { SetProperty(ref AlternativeNPVValue, value); }

        }

        /// <summary>
        /// Cost to switch to alternative project
        /// </summary>
        private double SwitchingCostValue;

        public double SwitchingCost

        {

            get { return this.SwitchingCostValue; }

            set { SetProperty(ref SwitchingCostValue, value); }

        }
    }
}

using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.EconomicAnalysis;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class DecisionScenario : ModelEntityBase
    {
        /// <summary>
        /// Name of the scenario (e.g., "Success", "Failure")
        /// </summary>
        private string ScenarioNameValue;

        public string ScenarioName

        {

            get { return this.ScenarioNameValue; }

            set { SetProperty(ref ScenarioNameValue, value); }

        }

        /// <summary>
        /// Probability of this scenario occurring
        /// </summary>
        private double ProbabilityValue;

        public double Probability

        {

            get { return this.ProbabilityValue; }

            set { SetProperty(ref ProbabilityValue, value); }

        }

        /// <summary>
        /// Net Present Value of this scenario
        /// </summary>
        private double NPVValue;

        public double NPV

        {

            get { return this.NPVValue; }

            set { SetProperty(ref NPVValue, value); }

        }

        /// <summary>
        /// Internal Rate of Return for this scenario
        /// </summary>
        private double IRRValue;

        public double IRR

        {

            get { return this.IRRValue; }

            set { SetProperty(ref IRRValue, value); }

        }

        /// <summary>
        /// Cumulative cash flow across all periods in this scenario
        /// </summary>
        private double CumulativeCashFlowValue;

        public double CumulativeCashFlow

        {

            get { return this.CumulativeCashFlowValue; }

            set { SetProperty(ref CumulativeCashFlowValue, value); }

        }
    }
}

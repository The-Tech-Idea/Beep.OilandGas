using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.EconomicAnalysis;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class ScenarioAnalysis : ModelEntityBase
    {
        /// <summary>
        /// Unique identifier for the analysis
        /// </summary>
        private string AnalysisIdValue;

        public string AnalysisId

        {

            get { return this.AnalysisIdValue; }

            set { SetProperty(ref AnalysisIdValue, value); }

        }

        /// <summary>
        /// Date the analysis was performed
        /// </summary>
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }

        /// <summary>
        /// NPV in base case scenario
        /// </summary>
        private double BaseNPVValue;

        public double BaseNPV

        {

            get { return this.BaseNPVValue; }

            set { SetProperty(ref BaseNPVValue, value); }

        }

        /// <summary>
        /// NPV in best case scenario
        /// </summary>
        private double BestCaseNPVValue;

        public double BestCaseNPV

        {

            get { return this.BestCaseNPVValue; }

            set { SetProperty(ref BestCaseNPVValue, value); }

        }

        /// <summary>
        /// NPV in worst case scenario
        /// </summary>
        private double WorstCaseNPVValue;

        public double WorstCaseNPV

        {

            get { return this.WorstCaseNPVValue; }

            set { SetProperty(ref WorstCaseNPVValue, value); }

        }

        /// <summary>
        /// Probability-weighted expected NPV
        /// </summary>
        private double ExpectedValueNPVValue;

        public double ExpectedValueNPV

        {

            get { return this.ExpectedValueNPVValue; }

            set { SetProperty(ref ExpectedValueNPVValue, value); }

        }

        /// <summary>
        /// Standard deviation of NPV across scenarios
        /// </summary>
        private double StandardDeviationValue;

        public double StandardDeviation

        {

            get { return this.StandardDeviationValue; }

            set { SetProperty(ref StandardDeviationValue, value); }

        }

        /// <summary>
        /// Coefficient of variation (stdDev / mean)
        /// </summary>
        private double CoefficientOfVariationValue;

        public double CoefficientOfVariation

        {

            get { return this.CoefficientOfVariationValue; }

            set { SetProperty(ref CoefficientOfVariationValue, value); }

        }

        /// <summary>
        /// Detailed results for each scenario
        /// </summary>
        private List<ScenarioResult> ScenariosValue;

        public List<ScenarioResult> Scenarios

        {

            get { return this.ScenariosValue; }

            set { SetProperty(ref ScenariosValue, value); }

        }
    }
}

using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.EconomicAnalysis;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class DecisionTreeAnalysisResult : ModelEntityBase
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
        /// Date and time the analysis was performed
        /// </summary>
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }

        /// <summary>
        /// Initial capital investment required
        /// </summary>
        private double InitialInvestmentValue;

        public double InitialInvestment

        {

            get { return this.InitialInvestmentValue; }

            set { SetProperty(ref InitialInvestmentValue, value); }

        }

        /// <summary>
        /// Probability of success (0.0 to 1.0)
        /// </summary>
        private double SuccessProbabilityValue;

        public double SuccessProbability

        {

            get { return this.SuccessProbabilityValue; }

            set { SetProperty(ref SuccessProbabilityValue, value); }

        }

        /// <summary>
        /// Success scenario with NPV and IRR
        /// </summary>
        private DecisionScenario SuccessScenarioValue;

        public DecisionScenario SuccessScenario

        {

            get { return this.SuccessScenarioValue; }

            set { SetProperty(ref SuccessScenarioValue, value); }

        }

        /// <summary>
        /// Failure scenario with NPV and IRR
        /// </summary>
        private DecisionScenario FailureScenarioValue;

        public DecisionScenario FailureScenario

        {

            get { return this.FailureScenarioValue; }

            set { SetProperty(ref FailureScenarioValue, value); }

        }

        /// <summary>
        /// Expected NPV across all scenarios (probability-weighted)
        /// </summary>
        private double ExpectedNPVValue;

        public double ExpectedNPV

        {

            get { return this.ExpectedNPVValue; }

            set { SetProperty(ref ExpectedNPVValue, value); }

        }

        /// <summary>
        /// Variance of NPV across scenarios (risk measure)
        /// </summary>
        private double VarianceOfNPVValue;

        public double VarianceOfNPV

        {

            get { return this.VarianceOfNPVValue; }

            set { SetProperty(ref VarianceOfNPVValue, value); }

        }

        /// <summary>
        /// Standard deviation of NPV (volatility)
        /// </summary>
        private double StandardDeviationValue;

        public double StandardDeviation

        {

            get { return this.StandardDeviationValue; }

            set { SetProperty(ref StandardDeviationValue, value); }

        }

        /// <summary>
        /// Decision recommendation: "Proceed" or "Do Not Proceed"
        /// </summary>
        private string DecisionValue;

        public string Decision

        {

            get { return this.DecisionValue; }

            set { SetProperty(ref DecisionValue, value); }

        }
    }
}

using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.EconomicAnalysis;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class RiskMetrics : ModelEntityBase
    {
        /// <summary>
        /// Unique identifier for the risk analysis
        /// </summary>
        private string RiskAnalysisIdValue;

        public string RiskAnalysisId

        {

            get { return this.RiskAnalysisIdValue; }

            set { SetProperty(ref RiskAnalysisIdValue, value); }

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
        /// Expected NPV across scenarios
        /// </summary>
        private double ExpectedNPVValue;

        public double ExpectedNPV

        {

            get { return this.ExpectedNPVValue; }

            set { SetProperty(ref ExpectedNPVValue, value); }

        }

        /// <summary>
        /// Standard deviation of NPV distribution
        /// </summary>
        private double StandardDeviationValue;

        public double StandardDeviation

        {

            get { return this.StandardDeviationValue; }

            set { SetProperty(ref StandardDeviationValue, value); }

        }

        /// <summary>
        /// Variance of NPV distribution
        /// </summary>
        private double VarianceValue;

        public double Variance

        {

            get { return this.VarianceValue; }

            set { SetProperty(ref VarianceValue, value); }

        }

        /// <summary>
        /// Coefficient of variation (risk per unit of return)
        /// </summary>
        private double CoefficientOfVariationValue;

        public double CoefficientOfVariation

        {

            get { return this.CoefficientOfVariationValue; }

            set { SetProperty(ref CoefficientOfVariationValue, value); }

        }

        /// <summary>
        /// Value at Risk at 95% confidence level (worst 5% of outcomes)
        /// </summary>
        private double ValueAtRiskValue;

        public double ValueAtRisk

        {

            get { return this.ValueAtRiskValue; }

            set { SetProperty(ref ValueAtRiskValue, value); }

        }

        /// <summary>
        /// Conditional Value at Risk (average of worst 5% outcomes)
        /// </summary>
        private double ConditionalVaRValue;

        public double ConditionalVaR

        {

            get { return this.ConditionalVaRValue; }

            set { SetProperty(ref ConditionalVaRValue, value); }

        }

        /// <summary>
        /// Probability of achieving negative NPV
        /// </summary>
        private double ProbabilityOfLossValue;

        public double ProbabilityOfLoss

        {

            get { return this.ProbabilityOfLossValue; }

            set { SetProperty(ref ProbabilityOfLossValue, value); }

        }
    }
}

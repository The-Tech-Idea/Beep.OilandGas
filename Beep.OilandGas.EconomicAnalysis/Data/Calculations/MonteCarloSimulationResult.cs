using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.EconomicAnalysis;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class MonteCarloSimulationResult : ModelEntityBase
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
        /// Number of simulations performed
        /// </summary>
        private int SimulationCountValue;

        public int SimulationCount

        {

            get { return this.SimulationCountValue; }

            set { SetProperty(ref SimulationCountValue, value); }

        }

        /// <summary>
        /// Distribution of NPV values from all simulations
        /// </summary>
        private List<double> NPVDistributionValue;

        public List<double> NPVDistribution

        {

            get { return this.NPVDistributionValue; }

            set { SetProperty(ref NPVDistributionValue, value); }

        }

        /// <summary>
        /// Distribution of IRR values from all simulations
        /// </summary>
        private List<double> IRRDistributionValue;

        public List<double> IRRDistribution

        {

            get { return this.IRRDistributionValue; }

            set { SetProperty(ref IRRDistributionValue, value); }

        }

        /// <summary>
        /// Mean (average) NPV across all simulations
        /// </summary>
        private double MeanNPVValue;

        public double MeanNPV

        {

            get { return this.MeanNPVValue; }

            set { SetProperty(ref MeanNPVValue, value); }

        }

        /// <summary>
        /// Mean (average) IRR across all simulations
        /// </summary>
        private double MeanIRRValue;

        public double MeanIRR

        {

            get { return this.MeanIRRValue; }

            set { SetProperty(ref MeanIRRValue, value); }

        }

        /// <summary>
        /// Standard deviation of NPV distribution (volatility measure)
        /// </summary>
        private double StdDevNPVValue;

        public double StdDevNPV

        {

            get { return this.StdDevNPVValue; }

            set { SetProperty(ref StdDevNPVValue, value); }

        }

        /// <summary>
        /// Standard deviation of IRR distribution (volatility measure)
        /// </summary>
        private double StdDevIRRValue;

        public double StdDevIRR

        {

            get { return this.StdDevIRRValue; }

            set { SetProperty(ref StdDevIRRValue, value); }

        }

        /// <summary>
        /// 10th percentile NPV (downside scenario)
        /// </summary>
        private double P10NPVValue;

        public double P10NPV

        {

            get { return this.P10NPVValue; }

            set { SetProperty(ref P10NPVValue, value); }

        }

        /// <summary>
        /// 50th percentile NPV (base case)
        /// </summary>
        private double P50NPVValue;

        public double P50NPV

        {

            get { return this.P50NPVValue; }

            set { SetProperty(ref P50NPVValue, value); }

        }

        /// <summary>
        /// 90th percentile NPV (upside scenario)
        /// </summary>
        private double P90NPVValue;

        public double P90NPV

        {

            get { return this.P90NPVValue; }

            set { SetProperty(ref P90NPVValue, value); }

        }

        /// <summary>
        /// Minimum NPV from all simulations
        /// </summary>
        private double MinNPVValue;

        public double MinNPV

        {

            get { return this.MinNPVValue; }

            set { SetProperty(ref MinNPVValue, value); }

        }

        /// <summary>
        /// Maximum NPV from all simulations
        /// </summary>
        private double MaxNPVValue;

        public double MaxNPV

        {

            get { return this.MaxNPVValue; }

            set { SetProperty(ref MaxNPVValue, value); }

        }

        /// <summary>
        /// Probability of achieving negative NPV (risk of loss)
        /// </summary>
        private double ProbabilityOfLossValue;

        public double ProbabilityOfLoss

        {

            get { return this.ProbabilityOfLossValue; }

            set { SetProperty(ref ProbabilityOfLossValue, value); }

        }
    }
}

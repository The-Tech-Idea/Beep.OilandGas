using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class DcaMonteCarloForecastResult : ModelEntityBase
    {
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
        /// Number of months forecasted
        /// </summary>
        private int ForecastMonthsValue;

        public int ForecastMonths

        {

            get { return this.ForecastMonthsValue; }

            set { SetProperty(ref ForecastMonthsValue, value); }

        }

        /// <summary>
        /// Confidence level used (0.0-1.0)
        /// </summary>
        private double ConfidenceLevelValue;

        public double ConfidenceLevel

        {

            get { return this.ConfidenceLevelValue; }

            set { SetProperty(ref ConfidenceLevelValue, value); }

        }

        /// <summary>
        /// Date analysis was performed
        /// </summary>
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }

        /// <summary>
        /// Monthly forecast statistics for each forecast month
        /// </summary>
        private List<DcaMonteCarloMonthlyStats> ForecastMonthlyValue = new();

        public List<DcaMonteCarloMonthlyStats> ForecastMonthly

        {

            get { return this.ForecastMonthlyValue; }

            set { SetProperty(ref ForecastMonthlyValue, value); }

        }

        /// <summary>
        /// Total cumulative production across all months
        /// </summary>
        private double CumulativeProductionValue;

        public double CumulativeProduction

        {

            get { return this.CumulativeProductionValue; }

            set { SetProperty(ref CumulativeProductionValue, value); }

        }

        /// <summary>
        /// Probability well remains above economic limit
        /// </summary>
        private double ProbabilityEconomicViableValue;

        public double ProbabilityEconomicViable

        {

            get { return this.ProbabilityEconomicViableValue; }

            set { SetProperty(ref ProbabilityEconomicViableValue, value); }

        }
    }
}

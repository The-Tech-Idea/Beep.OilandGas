using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class DcaForecastReliabilityResult : ModelEntityBase
    {
        /// <summary>
        /// Date assessment was performed
        /// </summary>
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }

        /// <summary>
        /// Months of historical data available
        /// </summary>
        private int HistoricalMonthsValue;

        public int HistoricalMonths

        {

            get { return this.HistoricalMonthsValue; }

            set { SetProperty(ref HistoricalMonthsValue, value); }

        }

        /// <summary>
        /// R² of the fitted model
        /// </summary>
        private double ModelRSquaredValue;

        public double ModelRSquared

        {

            get { return this.ModelRSquaredValue; }

            set { SetProperty(ref ModelRSquaredValue, value); }

        }

        /// <summary>
        /// Score component from R² (0-10)
        /// </summary>
        private double R2ScoreComponentValue;

        public double R2ScoreComponent

        {

            get { return this.R2ScoreComponentValue; }

            set { SetProperty(ref R2ScoreComponentValue, value); }

        }

        /// <summary>
        /// Score component from historical data span (0-10)
        /// </summary>
        private double HistoryScoreComponentValue;

        public double HistoryScoreComponent

        {

            get { return this.HistoryScoreComponentValue; }

            set { SetProperty(ref HistoryScoreComponentValue, value); }

        }

        /// <summary>
        /// Score component from convergence (0-10)
        /// </summary>
        private double ConvergenceScoreComponentValue;

        public double ConvergenceScoreComponent

        {

            get { return this.ConvergenceScoreComponentValue; }

            set { SetProperty(ref ConvergenceScoreComponentValue, value); }

        }

        /// <summary>
        /// Overall reliability score (0-10)
        /// </summary>
        private double OverallReliabilityScoreValue;

        public double OverallReliabilityScore

        {

            get { return this.OverallReliabilityScoreValue; }

            set { SetProperty(ref OverallReliabilityScoreValue, value); }

        }

        /// <summary>
        /// Reliability assessment (Excellent, Good, Moderate, Poor)
        /// </summary>
        private string ReliabilityAssessmentValue;

        public string ReliabilityAssessment

        {

            get { return this.ReliabilityAssessmentValue; }

            set { SetProperty(ref ReliabilityAssessmentValue, value); }

        }

        /// <summary>
        /// Recommendations for improving forecast
        /// </summary>
        private List<string> RecommendationsValue = new();

        public List<string> Recommendations

        {

            get { return this.RecommendationsValue; }

            set { SetProperty(ref RecommendationsValue, value); }

        }
    }
}

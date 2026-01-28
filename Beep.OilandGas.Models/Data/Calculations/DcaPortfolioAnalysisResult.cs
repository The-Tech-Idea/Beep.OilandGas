using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class DcaPortfolioAnalysisResult : ModelEntityBase
    {
        /// <summary>
        /// Number of wells analyzed
        /// </summary>
        private int WellsAnalyzedValue;

        public int WellsAnalyzed

        {

            get { return this.WellsAnalyzedValue; }

            set { SetProperty(ref WellsAnalyzedValue, value); }

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
        /// Analysis results for each well
        /// </summary>
        public List<DcaPortfolioWellAnalysis> WellAnalyses { get; set; } = new();

        /// <summary>
        /// Average initial production rate (qi) across portfolio
        /// </summary>
        private double AverageQiValue;

        public double AverageQi

        {

            get { return this.AverageQiValue; }

            set { SetProperty(ref AverageQiValue, value); }

        }

        /// <summary>
        /// Average initial decline rate (di) across portfolio
        /// </summary>
        private double AverageDiValue;

        public double AverageDi

        {

            get { return this.AverageDiValue; }

            set { SetProperty(ref AverageDiValue, value); }

        }

        /// <summary>
        /// Average R² across portfolio models
        /// </summary>
        private double AverageRSquaredValue;

        public double AverageRSquared

        {

            get { return this.AverageRSquaredValue; }

            set { SetProperty(ref AverageRSquaredValue, value); }

        }
    }
}

using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.EconomicAnalysis;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class BreakevenAnalysis : ModelEntityBase
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
        /// Price or cost level at which NPV = 0 (breakeven point)
        /// </summary>
        private double BreakevenPriceValue;

        public double BreakevenPrice

        {

            get { return this.BreakevenPriceValue; }

            set { SetProperty(ref BreakevenPriceValue, value); }

        }

        /// <summary>
        /// Margin of safety (distance from base case to breakeven)
        /// </summary>
        private double MarginOfSafetyValue;

        public double MarginOfSafety

        {

            get { return this.MarginOfSafetyValue; }

            set { SetProperty(ref MarginOfSafetyValue, value); }

        }

        /// <summary>
        /// Contribution margin percentage
        /// </summary>
        private double ContributionMarginValue;

        public double ContributionMargin

        {

            get { return this.ContributionMarginValue; }

            set { SetProperty(ref ContributionMarginValue, value); }

        }

        /// <summary>
        /// Detailed breakeven points at various variable levels
        /// </summary>
        private List<BreakevenPoint> BreakevenPointsValue;

        public List<BreakevenPoint> BreakevenPoints

        {

            get { return this.BreakevenPointsValue; }

            set { SetProperty(ref BreakevenPointsValue, value); }

        }
    }
}

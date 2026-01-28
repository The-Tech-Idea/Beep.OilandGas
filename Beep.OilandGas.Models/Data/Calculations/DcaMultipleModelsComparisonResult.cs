using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class DcaMultipleModelsComparisonResult : ModelEntityBase
    {
        /// <summary>
        /// Number of data points analyzed
        /// </summary>
        private int DataPointsAnalyzedValue;

        public int DataPointsAnalyzed

        {

            get { return this.DataPointsAnalyzedValue; }

            set { SetProperty(ref DataPointsAnalyzedValue, value); }

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
        /// Exponential decline model results
        /// </summary>
        private DcaDeclineModel ExponentialModelValue;

        public DcaDeclineModel ExponentialModel

        {

            get { return this.ExponentialModelValue; }

            set { SetProperty(ref ExponentialModelValue, value); }

        }

        /// <summary>
        /// Hyperbolic decline model results
        /// </summary>
        private DcaDeclineModel HyperbolicModelValue;

        public DcaDeclineModel HyperbolicModel

        {

            get { return this.HyperbolicModelValue; }

            set { SetProperty(ref HyperbolicModelValue, value); }

        }

        /// <summary>
        /// Harmonic decline model results
        /// </summary>
        private DcaDeclineModel HarmonicModelValue;

        public DcaDeclineModel HarmonicModel

        {

            get { return this.HarmonicModelValue; }

            set { SetProperty(ref HarmonicModelValue, value); }

        }

        /// <summary>
        /// Name of the best fitting model
        /// </summary>
        private string BestFitModelValue;

        public string BestFitModel

        {

            get { return this.BestFitModelValue; }

            set { SetProperty(ref BestFitModelValue, value); }

        }

        /// <summary>
        /// R² value of the best fitting model
        /// </summary>
        private double BestRSquaredValue;

        public double BestRSquared

        {

            get { return this.BestRSquaredValue; }

            set { SetProperty(ref BestRSquaredValue, value); }

        }
    }
}

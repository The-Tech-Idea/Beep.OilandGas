using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class DcaOptimizedParametersResult : ModelEntityBase
    {
        /// <summary>
        /// Number of data points used
        /// </summary>
        private int DataPointsUsedValue;

        public int DataPointsUsed

        {

            get { return this.DataPointsUsedValue; }

            set { SetProperty(ref DataPointsUsedValue, value); }

        }

        /// <summary>
        /// Date optimization was performed
        /// </summary>
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }

        /// <summary>
        /// Initial production rate (bbl/day)
        /// </summary>
        private double QiValue;

        public double Qi

        {

            get { return this.QiValue; }

            set { SetProperty(ref QiValue, value); }

        }

        /// <summary>
        /// Initial decline rate (1/year)
        /// </summary>
        private double DiValue;

        public double Di

        {

            get { return this.DiValue; }

            set { SetProperty(ref DiValue, value); }

        }

        /// <summary>
        /// Decline exponent (unitless)
        /// </summary>
        private double BValue;

        public double B

        {

            get { return this.BValue; }

            set { SetProperty(ref BValue, value); }

        }

        /// <summary>
        /// Goodness of fit (R²)
        /// </summary>
        private double RSquaredValue;

        public double RSquared

        {

            get { return this.RSquaredValue; }

            set { SetProperty(ref RSquaredValue, value); }

        }

        /// <summary>
        /// Root mean square error
        /// </summary>
        private double RMSEValue;

        public double RMSE

        {

            get { return this.RMSEValue; }

            set { SetProperty(ref RMSEValue, value); }

        }
    }
}

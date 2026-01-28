using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class DcaPortfolioWellAnalysis : ModelEntityBase
    {
        /// <summary>
        /// Well identifier
        /// </summary>
        private string WellIdValue;

        public string WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }

        /// <summary>
        /// Number of data points for this well
        /// </summary>
        private int DataPointsValue;

        public int DataPoints

        {

            get { return this.DataPointsValue; }

            set { SetProperty(ref DataPointsValue, value); }

        }

        /// <summary>
        /// Initial production rate (bbl/day)
        /// </summary>
        private double InitialProductionValue;

        public double InitialProduction

        {

            get { return this.InitialProductionValue; }

            set { SetProperty(ref InitialProductionValue, value); }

        }

        /// <summary>
        /// Final production rate (bbl/day)
        /// </summary>
        private double FinalProductionValue;

        public double FinalProduction

        {

            get { return this.FinalProductionValue; }

            set { SetProperty(ref FinalProductionValue, value); }

        }

        /// <summary>
        /// Fitted qi parameter
        /// </summary>
        private double QiValue;

        public double Qi

        {

            get { return this.QiValue; }

            set { SetProperty(ref QiValue, value); }

        }

        /// <summary>
        /// Fitted di parameter
        /// </summary>
        private double DiValue;

        public double Di

        {

            get { return this.DiValue; }

            set { SetProperty(ref DiValue, value); }

        }

        /// <summary>
        /// R² of model fit
        /// </summary>
        private double RSquaredValue;

        public double RSquared

        {

            get { return this.RSquaredValue; }

            set { SetProperty(ref RSquaredValue, value); }

        }
    }
}

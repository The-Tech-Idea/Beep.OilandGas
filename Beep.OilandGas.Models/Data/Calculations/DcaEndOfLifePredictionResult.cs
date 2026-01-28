using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class DcaEndOfLifePredictionResult : ModelEntityBase
    {
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
        /// Reference date for analysis
        /// </summary>
        private DateTime ReferenceDateValue;

        public DateTime ReferenceDate

        {

            get { return this.ReferenceDateValue; }

            set { SetProperty(ref ReferenceDateValue, value); }

        }

        /// <summary>
        /// Predicted end-of-life date
        /// </summary>
        private DateTime PredictedEOLDateValue;

        public DateTime PredictedEOLDate

        {

            get { return this.PredictedEOLDateValue; }

            set { SetProperty(ref PredictedEOLDateValue, value); }

        }

        /// <summary>
        /// Economic limit threshold (bbl/day)
        /// </summary>
        private double EconomicLimitBblPerDayValue;

        public double EconomicLimitBblPerDay

        {

            get { return this.EconomicLimitBblPerDayValue; }

            set { SetProperty(ref EconomicLimitBblPerDayValue, value); }

        }

        /// <summary>
        /// Remaining well life in months
        /// </summary>
        private int RemainingLifeMonthsValue;

        public int RemainingLifeMonths

        {

            get { return this.RemainingLifeMonthsValue; }

            set { SetProperty(ref RemainingLifeMonthsValue, value); }

        }

        /// <summary>
        /// Remaining well life in years
        /// </summary>
        private double RemainingLifeYearsValue;

        public double RemainingLifeYears

        {

            get { return this.RemainingLifeYearsValue; }

            set { SetProperty(ref RemainingLifeYearsValue, value); }

        }

        /// <summary>
        /// Estimated reserves to end-of-life
        /// </summary>
        private double ReservesToEOLValue;

        public double ReservesToEOL

        {

            get { return this.ReservesToEOLValue; }

            set { SetProperty(ref ReservesToEOLValue, value); }

        }
    }
}

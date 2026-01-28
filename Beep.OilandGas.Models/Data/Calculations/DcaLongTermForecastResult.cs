using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class DcaLongTermForecastResult : ModelEntityBase
    {
        /// <summary>
        /// Number of years in forecast
        /// </summary>
        private int ForecastYearsValue;

        public int ForecastYears

        {

            get { return this.ForecastYearsValue; }

            set { SetProperty(ref ForecastYearsValue, value); }

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
        /// Date analysis was performed
        /// </summary>
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }

        /// <summary>
        /// Monthly production forecast
        /// </summary>
        private List<double> MonthlyProductionValue = new();

        public List<double> MonthlyProduction

        {

            get { return this.MonthlyProductionValue; }

            set { SetProperty(ref MonthlyProductionValue, value); }

        }

        /// <summary>
        /// Total cumulative production
        /// </summary>
        private double TotalCumulativeProductionValue;

        public double TotalCumulativeProduction

        {

            get { return this.TotalCumulativeProductionValue; }

            set { SetProperty(ref TotalCumulativeProductionValue, value); }

        }

        /// <summary>
        /// Average production rate
        /// </summary>
        private double AverageProductionRateValue;

        public double AverageProductionRate

        {

            get { return this.AverageProductionRateValue; }

            set { SetProperty(ref AverageProductionRateValue, value); }

        }

        /// <summary>
        /// Months until economic limit reached
        /// </summary>
        private int MonthsToEconomicLimitValue;

        public int MonthsToEconomicLimit

        {

            get { return this.MonthsToEconomicLimitValue; }

            set { SetProperty(ref MonthsToEconomicLimitValue, value); }

        }

        /// <summary>
        /// Years until economic limit reached
        /// </summary>
        private double YearsToEconomicLimitValue;

        public double YearsToEconomicLimit

        {

            get { return this.YearsToEconomicLimitValue; }

            set { SetProperty(ref YearsToEconomicLimitValue, value); }

        }
    }
}

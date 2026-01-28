using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.ProductionForecasting;
using Beep.OilandGas.Models.Data.ProspectIdentification;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class ProductionForecastPoint : ModelEntityBase
    {
        /// <summary>
        /// Forecast date
        /// </summary>
        private DateTime DateValue;

        public DateTime Date

        {

            get { return this.DateValue; }

            set { SetProperty(ref DateValue, value); }

        }

        /// <summary>
        /// Oil production rate (STB/day)
        /// </summary>
        private decimal? OilRateValue;

        public decimal? OilRate

        {

            get { return this.OilRateValue; }

            set { SetProperty(ref OilRateValue, value); }

        }

        /// <summary>
        /// Gas production rate (MSCF/day)
        /// </summary>
        private decimal? GasRateValue;

        public decimal? GasRate

        {

            get { return this.GasRateValue; }

            set { SetProperty(ref GasRateValue, value); }

        }

        /// <summary>
        /// Water production rate (STB/day)
        /// </summary>
        private decimal? WaterRateValue;

        public decimal? WaterRate

        {

            get { return this.WaterRateValue; }

            set { SetProperty(ref WaterRateValue, value); }

        }

        /// <summary>
        /// Cumulative oil production (STB)
        /// </summary>
        private decimal? CumulativeOilValue;

        public decimal? CumulativeOil

        {

            get { return this.CumulativeOilValue; }

            set { SetProperty(ref CumulativeOilValue, value); }

        }

        /// <summary>
        /// Cumulative gas production (MSCF)
        /// </summary>
        private decimal? CumulativeGasValue;

        public decimal? CumulativeGas

        {

            get { return this.CumulativeGasValue; }

            set { SetProperty(ref CumulativeGasValue, value); }

        }

        /// <summary>
        /// Confidence level (for probabilistic forecasts)
        /// </summary>
        private decimal? ConfidenceLevelValue;

        public decimal? ConfidenceLevel

        {

            get { return this.ConfidenceLevelValue; }

            set { SetProperty(ref ConfidenceLevelValue, value); }

        }

        /// <summary>
        /// P10 rate (low case)
        /// </summary>
        private decimal? P10RateValue;

        public decimal? P10Rate

        {

            get { return this.P10RateValue; }

            set { SetProperty(ref P10RateValue, value); }

        }

        /// <summary>
        /// P50 rate (most likely case)
        /// </summary>
        private decimal? P50RateValue;

        public decimal? P50Rate

        {

            get { return this.P50RateValue; }

            set { SetProperty(ref P50RateValue, value); }

        }

        /// <summary>
        /// P90 rate (high case)
        /// </summary>
        private decimal? P90RateValue;

        public decimal? P90Rate

        {

            get { return this.P90RateValue; }

            set { SetProperty(ref P90RateValue, value); }

        }
    }
}

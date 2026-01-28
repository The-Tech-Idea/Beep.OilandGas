using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.ProductionForecasting;
using Beep.OilandGas.Models.Data.ProspectIdentification;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class ForecastProductionDataPoint : ModelEntityBase
    {
        /// <summary>
        /// Production date
        /// </summary>
        private DateTime DateValue;

        public DateTime Date

        {

            get { return this.DateValue; }

            set { SetProperty(ref DateValue, value); }

        }

        /// <summary>
        /// Time since start (days)
        /// </summary>
        private decimal TimeValue;

        public decimal Time

        {

            get { return this.TimeValue; }

            set { SetProperty(ref TimeValue, value); }

        }

        /// <summary>
        /// Oil rate (STB/day)
        /// </summary>
        private decimal? OilRateValue;

        public decimal? OilRate

        {

            get { return this.OilRateValue; }

            set { SetProperty(ref OilRateValue, value); }

        }

        /// <summary>
        /// Gas rate (MSCF/day)
        /// </summary>
        private decimal? GasRateValue;

        public decimal? GasRate

        {

            get { return this.GasRateValue; }

            set { SetProperty(ref GasRateValue, value); }

        }

        /// <summary>
        /// Water rate (STB/day)
        /// </summary>
        private decimal? WaterRateValue;

        public decimal? WaterRate

        {

            get { return this.WaterRateValue; }

            set { SetProperty(ref WaterRateValue, value); }

        }

        /// <summary>
        /// Cumulative production
        /// </summary>
        private decimal? CumulativeValue;

        public decimal? Cumulative

        {

            get { return this.CumulativeValue; }

            set { SetProperty(ref CumulativeValue, value); }

        }
    }
}

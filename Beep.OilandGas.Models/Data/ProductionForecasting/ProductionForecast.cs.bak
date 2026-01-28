using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionForecasting
{
    public class ProductionForecast : ModelEntityBase
    {
        /// <summary>
        /// Forecast type
        /// </summary>
        private ForecastType ForecastTypeValue;

        public ForecastType ForecastType

        {

            get { return this.ForecastTypeValue; }

            set { SetProperty(ref ForecastTypeValue, value); }

        }

        /// <summary>
        /// Forecast points
        /// </summary>
        private List<ForecastPoint> ForecastPointsValue = new();

        public List<ForecastPoint> ForecastPoints

        {

            get { return this.ForecastPointsValue; }

            set { SetProperty(ref ForecastPointsValue, value); }

        }

        /// <summary>
        /// Total forecast duration in days
        /// </summary>
        private decimal ForecastDurationValue;

        public decimal ForecastDuration

        {

            get { return this.ForecastDurationValue; }

            set { SetProperty(ref ForecastDurationValue, value); }

        }

        /// <summary>
        /// Initial production rate
        /// </summary>
        private decimal InitialProductionRateValue;

        public decimal InitialProductionRate

        {

            get { return this.InitialProductionRateValue; }

            set { SetProperty(ref InitialProductionRateValue, value); }

        }

        /// <summary>
        /// Final production rate
        /// </summary>
        private decimal FinalProductionRateValue;

        public decimal FinalProductionRate

        {

            get { return this.FinalProductionRateValue; }

            set { SetProperty(ref FinalProductionRateValue, value); }

        }

        /// <summary>
        /// Total cumulative production
        /// </summary>
        private decimal TotalCumulativeProductionValue;

        public decimal TotalCumulativeProduction

        {

            get { return this.TotalCumulativeProductionValue; }

            set { SetProperty(ref TotalCumulativeProductionValue, value); }

        }
    }
}

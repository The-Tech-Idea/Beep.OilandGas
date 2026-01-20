using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data.ProductionForecasting
{
    /// <summary>
    /// Represents production forecast results
    /// DTO for calculations - Entity class: PRODUCTION_FORECAST
    /// </summary>
    public class ProductionForecast : ModelEntityBase
    {
        /// <summary>
        /// Forecast type
        /// </summary>
        public ForecastType ForecastType { get; set; }

        /// <summary>
        /// Forecast points
        /// </summary>
        public List<ForecastPoint> ForecastPoints { get; set; } = new();

        /// <summary>
        /// Total forecast duration in days
        /// </summary>
        public decimal ForecastDuration { get; set; }

        /// <summary>
        /// Initial production rate
        /// </summary>
        public decimal InitialProductionRate { get; set; }

        /// <summary>
        /// Final production rate
        /// </summary>
        public decimal FinalProductionRate { get; set; }

        /// <summary>
        /// Total cumulative production
        /// </summary>
        public decimal TotalCumulativeProduction { get; set; }
    }
}




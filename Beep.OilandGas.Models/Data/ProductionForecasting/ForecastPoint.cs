namespace Beep.OilandGas.Models.Data.ProductionForecasting
{
    /// <summary>
    /// Represents a production forecast point
    /// DTO for calculations - Entity class: FORECAST_POINT
    /// </summary>
    public class ForecastPoint : ModelEntityBase
    {
        /// <summary>
        /// Time in days from start
        /// </summary>
        public decimal Time { get; set; }

        /// <summary>
        /// Production rate in bbl/day or Mscf/day
        /// </summary>
        public decimal ProductionRate { get; set; }

        /// <summary>
        /// Cumulative production in bbl or Mscf
        /// </summary>
        public decimal CumulativeProduction { get; set; }

        /// <summary>
        /// Reservoir pressure in psia
        /// </summary>
        public decimal ReservoirPressure { get; set; }

        /// <summary>
        /// Bottom hole pressure in psia
        /// </summary>
        public decimal BottomHolePressure { get; set; }

        /// <summary>
        /// Decline exponent (b) for decline curve analysis
        /// Range 0-1: 0=exponential, 0.5=typical hyperbolic, 1=harmonic
        /// </summary>
        public decimal? DeclineExponent { get; set; }

        /// <summary>
        /// Forecast method used (e.g., "Exponential Decline", "Hyperbolic Decline (b=0.5)")
        /// </summary>
        public string ForecastMethod { get; set; }
    }
}




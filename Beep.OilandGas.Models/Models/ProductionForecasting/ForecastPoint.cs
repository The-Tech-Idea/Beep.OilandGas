namespace Beep.OilandGas.Models.ProductionForecasting
{
    /// <summary>
    /// Represents a production forecast point
    /// DTO for calculations - Entity class: FORECAST_POINT
    /// </summary>
    public class ForecastPoint
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
    }
}

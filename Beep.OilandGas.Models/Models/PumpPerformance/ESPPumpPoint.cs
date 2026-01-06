namespace Beep.OilandGas.Models.PumpPerformance
{
    /// <summary>
    /// Represents a point on ESP pump performance curve
    /// DTO for calculations - Entity class: ESP_PUMP_POINT
    /// </summary>
    public class ESPPumpPoint
    {
        /// <summary>
        /// Flow rate in bbl/day
        /// </summary>
        public decimal FlowRate { get; set; }

        /// <summary>
        /// Head in feet
        /// </summary>
        public decimal Head { get; set; }

        /// <summary>
        /// Efficiency (0-1)
        /// </summary>
        public decimal Efficiency { get; set; }

        /// <summary>
        /// Horsepower
        /// </summary>
        public decimal Horsepower { get; set; }
    }
}




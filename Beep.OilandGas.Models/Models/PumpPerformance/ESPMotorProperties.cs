namespace Beep.OilandGas.Models.PumpPerformance
{
    /// <summary>
    /// Represents ESP motor properties
    /// DTO for calculations - Entity class: ESP_MOTOR_PROPERTIES
    /// </summary>
    public class ESPMotorProperties
    {
        /// <summary>
        /// Motor horsepower
        /// </summary>
        public decimal Horsepower { get; set; }

        /// <summary>
        /// Motor voltage
        /// </summary>
        public decimal Voltage { get; set; }

        /// <summary>
        /// Motor efficiency (0-1)
        /// </summary>
        public decimal Efficiency { get; set; } = 0.9m;

        /// <summary>
        /// Power factor
        /// </summary>
        public decimal PowerFactor { get; set; } = 0.85m;
    }
}

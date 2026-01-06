namespace Beep.OilandGas.Models.GasProperties
{
    /// <summary>
    /// Represents average gas properties over a range
    /// DTO for calculations - Entity class: AVERAGE_GAS_PROPERTIES
    /// </summary>
    public class AverageGasProperties
    {
        /// <summary>
        /// Average pressure in psia
        /// </summary>
        public decimal AveragePressure { get; set; }

        /// <summary>
        /// Average temperature in Rankine
        /// </summary>
        public decimal AverageTemperature { get; set; }

        /// <summary>
        /// Average Z-factor
        /// </summary>
        public decimal AverageZFactor { get; set; }

        /// <summary>
        /// Average viscosity in centipoise
        /// </summary>
        public decimal AverageViscosity { get; set; }
    }
}




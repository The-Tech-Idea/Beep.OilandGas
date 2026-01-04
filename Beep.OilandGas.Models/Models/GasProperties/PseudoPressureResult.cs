namespace Beep.OilandGas.Models.GasProperties
{
    /// <summary>
    /// Represents pseudo-pressure calculation result
    /// DTO for calculations - Entity class: PSEUDO_PRESSURE_RESULT
    /// </summary>
    public class PseudoPressureResult
    {
        /// <summary>
        /// Pressure in psia
        /// </summary>
        public decimal Pressure { get; set; }

        /// <summary>
        /// Pseudo-pressure in psia²/cp
        /// </summary>
        public decimal PseudoPressure { get; set; }

        /// <summary>
        /// Z-factor at this pressure
        /// </summary>
        public decimal ZFactor { get; set; }

        /// <summary>
        /// Viscosity at this pressure in centipoise
        /// </summary>
        public decimal Viscosity { get; set; }
    }
}

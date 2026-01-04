namespace Beep.OilandGas.Models.CompressorAnalysis
{
    /// <summary>
    /// Operating conditions for compressors
    /// DTO for calculations - Entity class: COMPRESSOR_OPERATING_CONDITIONS
    /// </summary>
    public class CompressorOperatingConditions
    {
        /// <summary>
        /// Suction pressure in psia
        /// </summary>
        public decimal SuctionPressure { get; set; }

        /// <summary>
        /// Discharge pressure in psia
        /// </summary>
        public decimal DischargePressure { get; set; }

        /// <summary>
        /// Suction temperature in Rankine
        /// </summary>
        public decimal SuctionTemperature { get; set; }

        /// <summary>
        /// Discharge temperature in Rankine
        /// </summary>
        public decimal DischargeTemperature { get; set; }

        /// <summary>
        /// Gas flow rate in Mscf/day
        /// </summary>
        public decimal GasFlowRate { get; set; }

        /// <summary>
        /// Gas specific gravity (relative to air)
        /// </summary>
        public decimal GasSpecificGravity { get; set; }

        /// <summary>
        /// Compressor efficiency (0-1)
        /// </summary>
        public decimal CompressorEfficiency { get; set; } = 0.75m;

        /// <summary>
        /// Mechanical efficiency (0-1)
        /// </summary>
        public decimal MechanicalEfficiency { get; set; } = 0.95m;
    }
}

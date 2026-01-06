namespace Beep.OilandGas.Models.CompressorAnalysis
{
    /// <summary>
    /// Compressor pressure calculation results
    /// DTO for calculations - Entity class: COMPRESSOR_PRESSURE_RESULT
    /// </summary>
    public class CompressorPressureResult
    {
        /// <summary>
        /// Required discharge pressure in psia
        /// </summary>
        public decimal RequiredDischargePressure { get; set; }

        /// <summary>
        /// Compression ratio
        /// </summary>
        public decimal CompressionRatio { get; set; }

        /// <summary>
        /// Required power in horsepower
        /// </summary>
        public decimal RequiredPower { get; set; }

        /// <summary>
        /// Discharge temperature in Rankine
        /// </summary>
        public decimal DischargeTemperature { get; set; }

        /// <summary>
        /// Whether the operation is feasible
        /// </summary>
        public bool IsFeasible { get; set; }
    }
}




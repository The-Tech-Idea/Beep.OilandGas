namespace Beep.OilandGas.Models.Data.CompressorAnalysis
{
    /// <summary>
    /// Compressor power calculation results
    /// DTO for calculations - Entity class: COMPRESSOR_POWER_RESULT
    /// </summary>
    public class CompressorPowerResult : ModelEntityBase
    {
        /// <summary>
        /// Compression ratio
        /// </summary>
        public decimal CompressionRatio { get; set; }

        /// <summary>
        /// Adiabatic head in feet
        /// </summary>
        public decimal AdiabaticHead { get; set; }

        /// <summary>
        /// Polytropic head in feet
        /// </summary>
        public decimal PolytropicHead { get; set; }

        /// <summary>
        /// Theoretical power in horsepower
        /// </summary>
        public decimal TheoreticalPower { get; set; }

        /// <summary>
        /// Brake horsepower
        /// </summary>
        public decimal BrakeHorsepower { get; set; }

        /// <summary>
        /// Motor horsepower
        /// </summary>
        public decimal MotorHorsepower { get; set; }

        /// <summary>
        /// Power consumption in kW
        /// </summary>
        public decimal PowerConsumptionKW { get; set; }

        /// <summary>
        /// Discharge temperature in Rankine
        /// </summary>
        public decimal DischargeTemperature { get; set; }

        /// <summary>
        /// Overall efficiency (0-1)
        /// </summary>
        public decimal OverallEfficiency { get; set; }
    }
}




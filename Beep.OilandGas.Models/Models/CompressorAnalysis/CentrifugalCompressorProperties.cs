namespace Beep.OilandGas.Models.CompressorAnalysis
{
    /// <summary>
    /// Centrifugal compressor properties
    /// DTO for calculations - Entity class: CENTRIFUGAL_COMPRESSOR_PROPERTIES
    /// </summary>
    public class CentrifugalCompressorProperties
    {
        /// <summary>
        /// Operating conditions
        /// </summary>
        public CompressorOperatingConditions OperatingConditions { get; set; } = new();

        /// <summary>
        /// Specific heat ratio (k = cp/cv)
        /// </summary>
        public decimal SpecificHeatRatio { get; set; } = 1.3m;

        /// <summary>
        /// Polytropic efficiency (0-1)
        /// </summary>
        public decimal PolytropicEfficiency { get; set; } = 0.75m;
    }
}

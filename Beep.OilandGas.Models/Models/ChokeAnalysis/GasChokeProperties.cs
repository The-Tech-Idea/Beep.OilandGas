namespace Beep.OilandGas.Models.ChokeAnalysis
{
    /// <summary>
    /// Represents gas properties for choke calculations
    /// DTO for calculations - Entity class: GAS_CHOKE_PROPERTIES
    /// </summary>
    public class GasChokeProperties
    {
        /// <summary>
        /// Gas specific gravity (relative to air)
        /// </summary>
        public decimal GasSpecificGravity { get; set; }

        /// <summary>
        /// Upstream pressure in psia
        /// </summary>
        public decimal UpstreamPressure { get; set; }

        /// <summary>
        /// Downstream pressure in psia
        /// </summary>
        public decimal DownstreamPressure { get; set; }

        /// <summary>
        /// Temperature in Rankine
        /// </summary>
        public decimal Temperature { get; set; }

        /// <summary>
        /// Z-factor (compressibility factor)
        /// </summary>
        public decimal ZFactor { get; set; }

        /// <summary>
        /// Gas flow rate in Mscf/day
        /// </summary>
        public decimal FlowRate { get; set; }
    }
}




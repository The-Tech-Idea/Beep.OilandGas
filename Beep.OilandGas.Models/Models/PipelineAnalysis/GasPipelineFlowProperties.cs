namespace Beep.OilandGas.Models.PipelineAnalysis
{
    /// <summary>
    /// Represents gas pipeline flow properties
    /// DTO for calculations - Entity class: GAS_PIPELINE_FLOW_PROPERTIES
    /// </summary>
    public class GasPipelineFlowProperties
    {
        /// <summary>
        /// Pipeline properties
        /// </summary>
        public PipelineProperties Pipeline { get; set; } = new();

        /// <summary>
        /// Gas flow rate in Mscf/day
        /// </summary>
        public decimal GasFlowRate { get; set; }

        /// <summary>
        /// Gas specific gravity (relative to air)
        /// </summary>
        public decimal GasSpecificGravity { get; set; }

        /// <summary>
        /// Gas molecular weight
        /// </summary>
        public decimal GasMolecularWeight { get; set; }

        /// <summary>
        /// Base pressure in psia
        /// </summary>
        public decimal BasePressure { get; set; } = 14.7m;

        /// <summary>
        /// Base temperature in Rankine
        /// </summary>
        public decimal BaseTemperature { get; set; } = 520m;

        /// <summary>
        /// Z-factor
        /// </summary>
        public decimal ZFactor { get; set; }
    }
}

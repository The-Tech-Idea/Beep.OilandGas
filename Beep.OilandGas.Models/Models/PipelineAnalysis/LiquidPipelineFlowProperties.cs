namespace Beep.OilandGas.Models.PipelineAnalysis
{
    /// <summary>
    /// Represents liquid pipeline flow properties
    /// DTO for calculations - Entity class: LIQUID_PIPELINE_FLOW_PROPERTIES
    /// </summary>
    public class LiquidPipelineFlowProperties
    {
        /// <summary>
        /// Pipeline properties
        /// </summary>
        public PipelineProperties Pipeline { get; set; } = new();

        /// <summary>
        /// Liquid flow rate in bbl/day
        /// </summary>
        public decimal LiquidFlowRate { get; set; }

        /// <summary>
        /// Liquid specific gravity
        /// </summary>
        public decimal LiquidSpecificGravity { get; set; }

        /// <summary>
        /// Liquid viscosity in cp
        /// </summary>
        public decimal LiquidViscosity { get; set; } = 1.0m;

        /// <summary>
        /// Liquid density
        /// </summary>
        public decimal LiquidDensity { get; set; }
    }
}




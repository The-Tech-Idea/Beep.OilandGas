namespace Beep.OilandGas.Models.PipelineAnalysis
{
    /// <summary>
    /// Represents pipeline capacity calculation results
    /// DTO for calculations - Entity class: PIPELINE_CAPACITY_RESULT
    /// </summary>
    public class PipelineCapacityResult
    {
        /// <summary>
        /// Maximum flow rate in Mscf/day (gas) or bbl/day (liquid)
        /// </summary>
        public decimal MaximumFlowRate { get; set; }

        /// <summary>
        /// Pressure drop in psi
        /// </summary>
        public decimal PressureDrop { get; set; }

        /// <summary>
        /// Flow velocity in ft/s
        /// </summary>
        public decimal FlowVelocity { get; set; }

        /// <summary>
        /// Reynolds number
        /// </summary>
        public decimal ReynoldsNumber { get; set; }

        /// <summary>
        /// Friction factor
        /// </summary>
        public decimal FrictionFactor { get; set; }

        /// <summary>
        /// Pressure gradient in psia/ft
        /// </summary>
        public decimal PressureGradient { get; set; }

        /// <summary>
        /// Outlet pressure in psia
        /// </summary>
        public decimal OutletPressure { get; set; }
    }
}




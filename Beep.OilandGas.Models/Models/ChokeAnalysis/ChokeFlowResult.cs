namespace Beep.OilandGas.Models.ChokeAnalysis
{
    /// <summary>
    /// Represents choke flow calculation results
    /// DTO for calculations - Entity class: CHOKE_FLOW_RESULT
    /// </summary>
    public class ChokeFlowResult
    {
        /// <summary>
        /// Calculated flow rate in Mscf/day
        /// </summary>
        public decimal FlowRate { get; set; }

        /// <summary>
        /// Calculated downstream pressure in psia
        /// </summary>
        public decimal DownstreamPressure { get; set; }

        /// <summary>
        /// Calculated upstream pressure in psia
        /// </summary>
        public decimal UpstreamPressure { get; set; }

        /// <summary>
        /// Pressure ratio (P2/P1)
        /// </summary>
        public decimal PressureRatio { get; set; }

        /// <summary>
        /// Flow regime (subsonic or sonic)
        /// </summary>
        public FlowRegime FlowRegime { get; set; }

        /// <summary>
        /// Critical pressure ratio
        /// </summary>
        public decimal CriticalPressureRatio { get; set; }
    }
}

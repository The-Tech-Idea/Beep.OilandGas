namespace Beep.OilandGas.Models.Data.NodalAnalysis
{
    /// <summary>
    /// Point on IPR (Inflow Performance Relationship) curve
    /// DTO for calculations - Entity class: NODAL_IPR_POINT
    /// </summary>
    public class IPRPoint : ModelEntityBase
    {
        /// <summary>
        /// Flow rate (STB/day or m³/day)
        /// </summary>
        public double FlowRate { get; set; }

        /// <summary>
        /// Flowing bottomhole pressure (psia or kPa)
        /// </summary>
        public double FlowingBottomholePressure { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public IPRPoint()
        {
        }

        /// <summary>
        /// Constructor with flow rate and pressure
        /// </summary>
        public IPRPoint(double flowRate, double flowingBottomholePressure)
        {
            FlowRate = flowRate;
            FlowingBottomholePressure = flowingBottomholePressure;
        }
    }
}




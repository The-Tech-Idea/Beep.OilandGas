namespace Beep.OilandGas.Models.NodalAnalysis
{
    /// <summary>
    /// Point on VLP (Vertical Lift Performance) curve
    /// DTO for calculations - Entity class: NODAL_VLP_POINT
    /// </summary>
    public class VLPPoint
    {
        /// <summary>
        /// Flow rate (STB/day or m³/day)
        /// </summary>
        public double FlowRate { get; set; }

        /// <summary>
        /// Required bottomhole pressure (psia or kPa)
        /// </summary>
        public double BottomholePressure { get; set; }
        public object RequiredBottomholePressure { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public VLPPoint()
        {
        }

        /// <summary>
        /// Constructor with flow rate and pressure
        /// </summary>
        public VLPPoint(double flowRate, double bottomholePressure)
        {
            FlowRate = flowRate;
            BottomholePressure = bottomholePressure;
        }
    }
}




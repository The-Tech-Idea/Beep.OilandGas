namespace Beep.OilandGas.Models.NodalAnalysis
{
    /// <summary>
    /// Point on VLP (Vertical Lift Performance) curve
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

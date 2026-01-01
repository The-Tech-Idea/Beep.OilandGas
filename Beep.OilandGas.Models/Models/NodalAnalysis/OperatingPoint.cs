namespace Beep.OilandGas.Models.NodalAnalysis
{
    /// <summary>
    /// Operating point (intersection of IPR and VLP curves)
    /// </summary>
    public class OperatingPoint
    {
        /// <summary>
        /// Flow rate at operating point (STB/day or m³/day)
        /// </summary>
        public double FlowRate { get; set; }

        /// <summary>
        /// Bottomhole pressure at operating point (psia or kPa)
        /// </summary>
        public double BottomholePressure { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public OperatingPoint()
        {
        }

        /// <summary>
        /// Constructor with flow rate and pressure
        /// </summary>
        public OperatingPoint(double flowRate, double bottomholePressure)
        {
            FlowRate = flowRate;
            BottomholePressure = bottomholePressure;
        }
    }
}

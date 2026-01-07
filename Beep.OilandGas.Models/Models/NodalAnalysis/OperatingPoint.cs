namespace Beep.OilandGas.Models.NodalAnalysis
{
    /// <summary>
    /// Operating point (intersection of IPR and VLP curves)
    /// DTO for calculations - Entity class: NODAL_OPERATING_POINT
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
        public double WellheadPressure { get; set; }

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




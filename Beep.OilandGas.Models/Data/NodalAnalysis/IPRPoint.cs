
namespace Beep.OilandGas.Models.Data.NodalAnalysis
{
    public class IPRPoint : ModelEntityBase
    {
        /// <summary>
        /// Flow rate (STB/day or m³/day)
        /// </summary>
        private double FlowRateValue;

        public double FlowRate

        {

            get { return this.FlowRateValue; }

            set { SetProperty(ref FlowRateValue, value); }

        }

        /// <summary>
        /// Flowing bottomhole pressure (psia or kPa)
        /// </summary>
        private double FlowingBottomholePressureValue;

        public double FlowingBottomholePressure

        {

            get { return this.FlowingBottomholePressureValue; }

            set { SetProperty(ref FlowingBottomholePressureValue, value); }

        }

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

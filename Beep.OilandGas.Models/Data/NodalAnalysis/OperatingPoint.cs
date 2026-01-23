namespace Beep.OilandGas.Models.Data.NodalAnalysis
{
    /// <summary>
    /// Operating point (intersection of IPR and VLP curves)
    /// DTO for calculations - Entity class: NODAL_OPERATING_POINT
    /// </summary>
    public class OperatingPoint : ModelEntityBase
    {
        /// <summary>
        /// Flow rate at operating point (STB/day or m³/day)
        /// </summary>
        private double FlowRateValue;

        public double FlowRate

        {

            get { return this.FlowRateValue; }

            set { SetProperty(ref FlowRateValue, value); }

        }

        /// <summary>
        /// Bottomhole pressure at operating point (psia or kPa)
        /// </summary>
        private double BottomholePressureValue;

        public double BottomholePressure

        {

            get { return this.BottomholePressureValue; }

            set { SetProperty(ref BottomholePressureValue, value); }

        }
        private double WellheadPressureValue;

        public double WellheadPressure

        {

            get { return this.WellheadPressureValue; }

            set { SetProperty(ref WellheadPressureValue, value); }

        }

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





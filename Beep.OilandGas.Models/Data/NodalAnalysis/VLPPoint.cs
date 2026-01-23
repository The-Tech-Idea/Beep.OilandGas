namespace Beep.OilandGas.Models.Data.NodalAnalysis
{
    /// <summary>
    /// Point on VLP (Vertical Lift Performance) curve
    /// DTO for calculations - Entity class: NODAL_VLP_POINT
    /// </summary>
    public class VLPPoint : ModelEntityBase
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
        /// Required bottomhole pressure (psia or kPa)
        /// </summary>
        private double RequiredBottomholePressureValue;

        public double RequiredBottomholePressure

        {

            get { return this.RequiredBottomholePressureValue; }

            set { SetProperty(ref RequiredBottomholePressureValue, value); }

        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public VLPPoint()
        {
        }

        /// <summary>
        /// Constructor with flow rate and pressure
        /// </summary>
        public VLPPoint(double flowRate, double requiredBottomholePressure)
        {
            FlowRate = flowRate;
            RequiredBottomholePressure = requiredBottomholePressure;
        }
    }
}


namespace Beep.OilandGas.Models.Data.WellTestAnalysis
{
    /// <summary>
    /// Represents a point on a pressure-time curve
    /// DTO for calculations - Entity class: PRESSURE_TIME_POINT
    /// </summary>
    public class PressureTimePoint : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the time in hours
        /// </summary>
        private double TimeValue;

        public double Time

        {

            get { return this.TimeValue; }

            set { SetProperty(ref TimeValue, value); }

        }

        /// <summary>
        /// Gets or sets the pressure in psi
        /// </summary>
        private double PressureValue;

        public double Pressure

        {

            get { return this.PressureValue; }

            set { SetProperty(ref PressureValue, value); }

        }

        /// <summary>
        /// Gets or sets the pressure derivative (for diagnostic plots)
        /// </summary>
        private double? PressureDerivativeValue;

        public double? PressureDerivative

        {

            get { return this.PressureDerivativeValue; }

            set { SetProperty(ref PressureDerivativeValue, value); }

        }

        public PressureTimePoint() { }

        public PressureTimePoint(double time, double pressure)
        {
            Time = time;
            Pressure = pressure;
        }
    }
}





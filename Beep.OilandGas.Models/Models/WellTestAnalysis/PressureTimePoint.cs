namespace Beep.OilandGas.Models.WellTestAnalysis
{
    /// <summary>
    /// Represents a point on a pressure-time curve
    /// DTO for calculations - Entity class: PRESSURE_TIME_POINT
    /// </summary>
    public class PressureTimePoint
    {
        /// <summary>
        /// Gets or sets the time in hours
        /// </summary>
        public double Time { get; set; }

        /// <summary>
        /// Gets or sets the pressure in psi
        /// </summary>
        public double Pressure { get; set; }

        /// <summary>
        /// Gets or sets the pressure derivative (for diagnostic plots)
        /// </summary>
        public double? PressureDerivative { get; set; }

        public PressureTimePoint() { }

        public PressureTimePoint(double time, double pressure)
        {
            Time = time;
            Pressure = pressure;
        }
    }
}





namespace Beep.OilandGas.Models.Data.PipelineAnalysis
{
    public class PipelineProperties : ModelEntityBase
    {
        /// <summary>
        /// Pipeline diameter in inches
        /// </summary>
        private decimal DiameterValue;

        public decimal Diameter

        {

            get { return this.DiameterValue; }

            set { SetProperty(ref DiameterValue, value); }

        }

        /// <summary>
        /// Pipeline length in feet
        /// </summary>
        private decimal LengthValue;

        public decimal Length

        {

            get { return this.LengthValue; }

            set { SetProperty(ref LengthValue, value); }

        }

        /// <summary>
        /// Pipeline roughness in feet
        /// </summary>
        private decimal RoughnessValue = 0.00015m;

        public decimal Roughness

        {

            get { return this.RoughnessValue; }

            set { SetProperty(ref RoughnessValue, value); }

        } // Typical for steel

        /// <summary>
        /// Elevation change in feet (positive = uphill)
        /// </summary>
        private decimal ElevationChangeValue = 0m;

        public decimal ElevationChange

        {

            get { return this.ElevationChangeValue; }

            set { SetProperty(ref ElevationChangeValue, value); }

        }

        /// <summary>
        /// Inlet pressure in psia
        /// </summary>
        private decimal InletPressureValue;

        public decimal InletPressure

        {

            get { return this.InletPressureValue; }

            set { SetProperty(ref InletPressureValue, value); }

        }

        /// <summary>
        /// Outlet pressure in psia
        /// </summary>
        private decimal OutletPressureValue;

        public decimal OutletPressure

        {

            get { return this.OutletPressureValue; }

            set { SetProperty(ref OutletPressureValue, value); }

        }

        /// <summary>
        /// Average temperature in Rankine
        /// </summary>
        private decimal AverageTemperatureValue;

        public decimal AverageTemperature

        {

            get { return this.AverageTemperatureValue; }

            set { SetProperty(ref AverageTemperatureValue, value); }

        }
    }
}

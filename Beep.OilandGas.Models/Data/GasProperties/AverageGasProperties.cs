
namespace Beep.OilandGas.Models.Data.GasProperties
{
    public class AverageGasProperties : ModelEntityBase
    {
        /// <summary>
        /// Average pressure in psia
        /// </summary>
        private decimal AveragePressureValue;

        public decimal AveragePressure

        {

            get { return this.AveragePressureValue; }

            set { SetProperty(ref AveragePressureValue, value); }

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

        /// <summary>
        /// Average Z-factor
        /// </summary>
        private decimal AverageZFactorValue;

        public decimal AverageZFactor

        {

            get { return this.AverageZFactorValue; }

            set { SetProperty(ref AverageZFactorValue, value); }

        }

        /// <summary>
        /// Average viscosity in centipoise
        /// </summary>
        private decimal AverageViscosityValue;

        public decimal AverageViscosity

        {

            get { return this.AverageViscosityValue; }

            set { SetProperty(ref AverageViscosityValue, value); }

        }
    }
}


namespace Beep.OilandGas.Models.Data.OilProperties
{
    public class OilPropertyConditions : ModelEntityBase
    {
        /// <summary>
        /// Pressure in psia
        /// </summary>
        private decimal PressureValue;

        public decimal Pressure

        {

            get { return this.PressureValue; }

            set { SetProperty(ref PressureValue, value); }

        }

        /// <summary>
        /// Temperature in Rankine
        /// </summary>
        private decimal TemperatureValue;

        public decimal Temperature

        {

            get { return this.TemperatureValue; }

            set { SetProperty(ref TemperatureValue, value); }

        }

        /// <summary>
        /// Oil API gravity at standard conditions
        /// </summary>
        private decimal ApiGravityValue;

        public decimal ApiGravity

        {

            get { return this.ApiGravityValue; }

            set { SetProperty(ref ApiGravityValue, value); }

        }

        /// <summary>
        /// Gas specific gravity (relative to air)
        /// </summary>
        private decimal GasSpecificGravityValue = 0.65m;

        public decimal GasSpecificGravity

        {

            get { return this.GasSpecificGravityValue; }

            set { SetProperty(ref GasSpecificGravityValue, value); }

        }

        /// <summary>
        /// Solution gas-oil ratio in scf/STB (if known)
        /// </summary>
        private decimal? SolutionGasOilRatioValue;

        public decimal? SolutionGasOilRatio

        {

            get { return this.SolutionGasOilRatioValue; }

            set { SetProperty(ref SolutionGasOilRatioValue, value); }

        }

        /// <summary>
        /// Bubble point pressure in psia (if known)
        /// </summary>
        private decimal? BubblePointPressureValue;

        public decimal? BubblePointPressure

        {

            get { return this.BubblePointPressureValue; }

            set { SetProperty(ref BubblePointPressureValue, value); }

        }
    }
}

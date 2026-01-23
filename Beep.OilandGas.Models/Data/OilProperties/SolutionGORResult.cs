namespace Beep.OilandGas.Models.Data.OilProperties
{
    /// <summary>
    /// Represents solution GOR calculation result
    /// DTO for calculations - Entity class: SOLUTION_GOR_RESULT
    /// </summary>
    public class SolutionGORResult : ModelEntityBase
    {
        /// <summary>
        /// Solution gas-oil ratio in scf/STB
        /// </summary>
        private decimal SolutionGasOilRatioValue;

        public decimal SolutionGasOilRatio

        {

            get { return this.SolutionGasOilRatioValue; }

            set { SetProperty(ref SolutionGasOilRatioValue, value); }

        }

        /// <summary>
        /// Bubble point pressure in psia
        /// </summary>
        private decimal BubblePointPressureValue;

        public decimal BubblePointPressure

        {

            get { return this.BubblePointPressureValue; }

            set { SetProperty(ref BubblePointPressureValue, value); }

        }
    }
}





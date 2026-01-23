namespace Beep.OilandGas.Models.Data.OilProperties
{
    /// <summary>
    /// Represents bubble point pressure calculation result
    /// DTO for calculations - Entity class: BUBBLE_POINT_RESULT
    /// </summary>
    public class BubblePointResult : ModelEntityBase
    {
        /// <summary>
        /// Bubble point pressure in psia
        /// </summary>
        private decimal BubblePointPressureValue;

        public decimal BubblePointPressure

        {

            get { return this.BubblePointPressureValue; }

            set { SetProperty(ref BubblePointPressureValue, value); }

        }

        /// <summary>
        /// Solution gas-oil ratio at bubble point in scf/STB
        /// </summary>
        private decimal SolutionGasOilRatioValue;

        public decimal SolutionGasOilRatio

        {

            get { return this.SolutionGasOilRatioValue; }

            set { SetProperty(ref SolutionGasOilRatioValue, value); }

        }
    }
}





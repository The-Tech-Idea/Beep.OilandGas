namespace Beep.OilandGas.Models.OilProperties
{
    /// <summary>
    /// Represents bubble point pressure calculation result
    /// DTO for calculations - Entity class: BUBBLE_POINT_RESULT
    /// </summary>
    public class BubblePointResult
    {
        /// <summary>
        /// Bubble point pressure in psia
        /// </summary>
        public decimal BubblePointPressure { get; set; }

        /// <summary>
        /// Solution gas-oil ratio at bubble point in scf/STB
        /// </summary>
        public decimal SolutionGasOilRatio { get; set; }
    }
}




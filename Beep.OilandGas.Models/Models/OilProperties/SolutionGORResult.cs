namespace Beep.OilandGas.Models.OilProperties
{
    /// <summary>
    /// Represents solution GOR calculation result
    /// DTO for calculations - Entity class: SOLUTION_GOR_RESULT
    /// </summary>
    public class SolutionGORResult
    {
        /// <summary>
        /// Solution gas-oil ratio in scf/STB
        /// </summary>
        public decimal SolutionGasOilRatio { get; set; }

        /// <summary>
        /// Bubble point pressure in psia
        /// </summary>
        public decimal BubblePointPressure { get; set; }
    }
}

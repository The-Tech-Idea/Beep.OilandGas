namespace Beep.OilandGas.Models.OilProperties
{
    /// <summary>
    /// Represents oil property calculation conditions
    /// DTO for calculations - Entity class: OIL_PROPERTY_CONDITIONS
    /// </summary>
    public class OilPropertyConditions
    {
        /// <summary>
        /// Pressure in psia
        /// </summary>
        public decimal Pressure { get; set; }

        /// <summary>
        /// Temperature in Rankine
        /// </summary>
        public decimal Temperature { get; set; }

        /// <summary>
        /// Oil API gravity at standard conditions
        /// </summary>
        public decimal ApiGravity { get; set; }

        /// <summary>
        /// Gas specific gravity (relative to air)
        /// </summary>
        public decimal GasSpecificGravity { get; set; } = 0.65m;

        /// <summary>
        /// Solution gas-oil ratio in scf/STB (if known)
        /// </summary>
        public decimal? SolutionGasOilRatio { get; set; }

        /// <summary>
        /// Bubble point pressure in psia (if known)
        /// </summary>
        public decimal? BubblePointPressure { get; set; }
    }
}

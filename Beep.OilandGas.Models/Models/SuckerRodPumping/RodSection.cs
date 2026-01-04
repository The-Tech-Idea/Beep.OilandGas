namespace Beep.OilandGas.Models.SuckerRodPumping
{
    /// <summary>
    /// Represents a section of sucker rod string
    /// DTO for calculations - Entity class: ROD_SECTION
    /// </summary>
    public class RodSection
    {
        /// <summary>
        /// Rod diameter in inches
        /// </summary>
        public decimal Diameter { get; set; }

        /// <summary>
        /// Section length in feet
        /// </summary>
        public decimal Length { get; set; }

        /// <summary>
        /// Rod material density in lb/ft³
        /// </summary>
        public decimal Density { get; set; } = 490m; // Steel

        /// <summary>
        /// Section weight in pounds
        /// </summary>
        public decimal Weight { get; set; }
    }
}

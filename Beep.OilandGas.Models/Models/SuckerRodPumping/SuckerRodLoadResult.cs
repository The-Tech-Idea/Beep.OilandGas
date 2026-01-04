namespace Beep.OilandGas.Models.SuckerRodPumping
{
    /// <summary>
    /// Represents sucker rod load analysis results
    /// DTO for calculations - Entity class: SUCKER_ROD_LOAD_RESULT
    /// </summary>
    public class SuckerRodLoadResult
    {
        /// <summary>
        /// Peak load in pounds
        /// </summary>
        public decimal PeakLoad { get; set; }

        /// <summary>
        /// Minimum load in pounds
        /// </summary>
        public decimal MinimumLoad { get; set; }

        /// <summary>
        /// Polished rod load in pounds
        /// </summary>
        public decimal PolishedRodLoad { get; set; }

        /// <summary>
        /// Rod string weight in pounds
        /// </summary>
        public decimal RodStringWeight { get; set; }

        /// <summary>
        /// Fluid load in pounds
        /// </summary>
        public decimal FluidLoad { get; set; }

        /// <summary>
        /// Dynamic load in pounds
        /// </summary>
        public decimal DynamicLoad { get; set; }

        /// <summary>
        /// Load range in pounds
        /// </summary>
        public decimal LoadRange { get; set; }

        /// <summary>
        /// Stress range in psi
        /// </summary>
        public decimal StressRange { get; set; }

        /// <summary>
        /// Maximum stress in psi
        /// </summary>
        public decimal MaximumStress { get; set; }

        /// <summary>
        /// Load factor (safety factor)
        /// </summary>
        public decimal LoadFactor { get; set; }
    }
}

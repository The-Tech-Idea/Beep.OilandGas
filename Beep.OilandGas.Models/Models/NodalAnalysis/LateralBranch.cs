namespace Beep.OilandGas.Models.NodalAnalysis
{
    /// <summary>
    /// Represents lateral branch properties for multilateral well calculations.
    /// </summary>
    public class LateralBranch
    {
        /// <summary>
        /// Branch name or identifier.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Reservoir pressure at the branch in psia.
        /// </summary>
        public double ReservoirPressure { get; set; }

        /// <summary>
        /// Productivity index for the branch in bbl/day/psi.
        /// </summary>
        public double ProductivityIndex { get; set; }

        /// <summary>
        /// Permeability of the formation in md.
        /// </summary>
        public double Permeability { get; set; }

        /// <summary>
        /// Formation thickness in feet.
        /// </summary>
        public double FormationThickness { get; set; }

        /// <summary>
        /// Drainage radius in feet.
        /// </summary>
        public double DrainageRadius { get; set; }

        /// <summary>
        /// Wellbore radius in feet.
        /// </summary>
        public double WellboreRadius { get; set; }

        /// <summary>
        /// Skin factor (dimensionless).
        /// </summary>
        public double SkinFactor { get; set; }
    }
}

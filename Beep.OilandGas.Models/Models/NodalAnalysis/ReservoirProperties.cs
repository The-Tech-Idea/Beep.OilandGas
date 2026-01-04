namespace Beep.OilandGas.Models.NodalAnalysis
{
    /// <summary>
    /// Reservoir properties for nodal analysis
    /// DTO for calculations - Entity class: NODAL_RESERVOIR_PROPERTIES
    /// </summary>
    public class ReservoirProperties
    {
        /// <summary>
        /// Reservoir pressure (psia or kPa)
        /// </summary>
        public double ReservoirPressure { get; set; }

        /// <summary>
        /// Bubble point pressure (psia or kPa)
        /// </summary>
        public double BubblePointPressure { get; set; }

        /// <summary>
        /// Productivity index (STB/day/psi or m³/day/kPa)
        /// </summary>
        public double ProductivityIndex { get; set; }

        /// <summary>
        /// Water cut (fraction, 0 to 1)
        /// </summary>
        public double WaterCut { get; set; }

        /// <summary>
        /// Gas-oil ratio (scf/STB or m³/m³)
        /// </summary>
        public double GasOilRatio { get; set; }

        /// <summary>
        /// Oil gravity (API degrees)
        /// </summary>
        public double OilGravity { get; set; }

        /// <summary>
        /// Formation volume factor (rbbl/STB or m³/m³)
        /// </summary>
        public double FormationVolumeFactor { get; set; }

        /// <summary>
        /// Oil viscosity (cp)
        /// </summary>
        public double OilViscosity { get; set; }
    }
}

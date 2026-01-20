namespace Beep.OilandGas.Models.Data.NodalAnalysis
{
    /// <summary>
    /// Wellbore properties for nodal analysis
    /// DTO for calculations - Entity class: NODAL_WELLBORE_PROPERTIES
    /// </summary>
    public class WellboreProperties : ModelEntityBase
    {
        /// <summary>
        /// Tubing diameter (inches or mm)
        /// </summary>
        public double TubingDiameter { get; set; }

        /// <summary>
        /// Tubing length (feet or meters)
        /// </summary>
        public double TubingLength { get; set; }

        /// <summary>
        /// Wellhead pressure (psia or kPa)
        /// </summary>
        public double WellheadPressure { get; set; }

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
        /// Gas specific gravity (relative to air)
        /// </summary>
        public double GasSpecificGravity { get; set; }

        /// <summary>
        /// Wellhead temperature (°F or °C)
        /// </summary>
        public double WellheadTemperature { get; set; }

        /// <summary>
        /// Bottomhole temperature (°F or °C)
        /// </summary>
        public double BottomholeTemperature { get; set; }

        /// <summary>
        /// Tubing roughness (inches or mm)
        /// </summary>
        public double TubingRoughness { get; set; }
    }
}




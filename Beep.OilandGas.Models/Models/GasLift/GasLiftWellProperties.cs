namespace Beep.OilandGas.Models.GasLift
{
    /// <summary>
    /// Well properties for gas lift analysis and design
    /// DTO for calculations - Entity class: GAS_LIFT_WELL_PROPERTIES
    /// </summary>
    public class GasLiftWellProperties
    {
        /// <summary>
        /// Well depth (in feet or meters depending on units)
        /// </summary>
        public decimal WellDepth { get; set; }

        /// <summary>
        /// Wellhead pressure (in psia or kPa)
        /// </summary>
        public decimal WellheadPressure { get; set; }

        /// <summary>
        /// Bottom hole pressure (in psia or kPa)
        /// </summary>
        public decimal BottomHolePressure { get; set; }

        /// <summary>
        /// Gas-oil ratio (scf/STB or m³/m³)
        /// </summary>
        public decimal GasOilRatio { get; set; }

        /// <summary>
        /// Desired production rate (STB/day or m³/day)
        /// </summary>
        public decimal DesiredProductionRate { get; set; }

        /// <summary>
        /// Oil gravity (API degrees)
        /// </summary>
        public decimal OilGravity { get; set; }

        /// <summary>
        /// Water cut (fraction, 0 to 1)
        /// </summary>
        public decimal WaterCut { get; set; }

        /// <summary>
        /// Wellhead temperature (in °F or °C)
        /// </summary>
        public decimal WellheadTemperature { get; set; }

        /// <summary>
        /// Bottom hole temperature (in °F or °C)
        /// </summary>
        public decimal BottomHoleTemperature { get; set; }

        /// <summary>
        /// Gas specific gravity (relative to air)
        /// </summary>
        public decimal GasSpecificGravity { get; set; }
    }
}




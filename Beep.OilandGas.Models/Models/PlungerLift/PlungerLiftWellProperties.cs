namespace Beep.OilandGas.Models.PlungerLift
{
    /// <summary>
    /// Represents plunger lift well properties
    /// DTO for calculations - Entity class: PLUNGER_LIFT_WELL_PROPERTIES
    /// </summary>
    public class PlungerLiftWellProperties
    {
        /// <summary>
        /// Well depth in feet
        /// </summary>
        public decimal WellDepth { get; set; }

        /// <summary>
        /// Tubing diameter in inches
        /// </summary>
        public decimal TubingDiameter { get; set; }

        /// <summary>
        /// Plunger diameter in inches
        /// </summary>
        public decimal PlungerDiameter { get; set; }

        /// <summary>
        /// Wellhead pressure in psia
        /// </summary>
        public decimal WellheadPressure { get; set; }

        /// <summary>
        /// Casing pressure in psia
        /// </summary>
        public decimal CasingPressure { get; set; }

        /// <summary>
        /// Bottom hole pressure in psia
        /// </summary>
        public decimal BottomHolePressure { get; set; }

        /// <summary>
        /// Wellhead temperature in Rankine
        /// </summary>
        public decimal WellheadTemperature { get; set; }

        /// <summary>
        /// Bottom hole temperature in Rankine
        /// </summary>
        public decimal BottomHoleTemperature { get; set; }

        /// <summary>
        /// Oil gravity in API
        /// </summary>
        public decimal OilGravity { get; set; }

        /// <summary>
        /// Water cut (fraction, 0-1)
        /// </summary>
        public decimal WaterCut { get; set; }

        /// <summary>
        /// Gas-oil ratio in scf/bbl
        /// </summary>
        public decimal GasOilRatio { get; set; }

        /// <summary>
        /// Gas specific gravity (relative to air)
        /// </summary>
        public decimal GasSpecificGravity { get; set; }

        /// <summary>
        /// Liquid production rate in bbl/day
        /// </summary>
        public decimal LiquidProductionRate { get; set; }
    }
}




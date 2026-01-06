namespace Beep.OilandGas.Models.SuckerRodPumping
{
    /// <summary>
    /// Represents sucker rod pumping system properties
    /// DTO for calculations - Entity class: SUCKER_ROD_SYSTEM_PROPERTIES
    /// </summary>
    public class SuckerRodSystemProperties
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
        /// Rod diameter in inches
        /// </summary>
        public decimal RodDiameter { get; set; }

        /// <summary>
        /// Pump diameter in inches
        /// </summary>
        public decimal PumpDiameter { get; set; }

        /// <summary>
        /// Stroke length in inches
        /// </summary>
        public decimal StrokeLength { get; set; }

        /// <summary>
        /// Strokes per minute (SPM)
        /// </summary>
        public decimal StrokesPerMinute { get; set; }

        /// <summary>
        /// Wellhead pressure in psia
        /// </summary>
        public decimal WellheadPressure { get; set; }

        /// <summary>
        /// Bottom hole pressure in psia
        /// </summary>
        public decimal BottomHolePressure { get; set; }

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
        /// Rod material density in lb/ft³
        /// </summary>
        public decimal RodDensity { get; set; } = 490m; // Steel

        /// <summary>
        /// Pump efficiency (0-1)
        /// </summary>
        public decimal PumpEfficiency { get; set; } = 0.85m;

        /// <summary>
        /// Fluid level
        /// </summary>
        public decimal FluidLevel { get; set; }

        /// <summary>
        /// Fluid density
        /// </summary>
        public decimal FluidDensity { get; set; }
    }
}




namespace Beep.OilandGas.Models.HydraulicPumps
{
    /// <summary>
    /// Represents well properties for hydraulic pump calculations.
    /// </summary>
    public class HydraulicPumpWellProperties
    {
        /// <summary>
        /// Well depth in feet.
        /// </summary>
        public decimal WellDepth { get; set; }

        /// <summary>
        /// Tubing diameter in inches.
        /// </summary>
        public decimal TubingDiameter { get; set; }

        /// <summary>
        /// API gravity of the oil (degrees API).
        /// </summary>
        public decimal OilGravity { get; set; }

        /// <summary>
        /// Water cut as a fraction (0-1).
        /// </summary>
        public decimal WaterCut { get; set; }

        /// <summary>
        /// Bottom hole pressure in psia.
        /// </summary>
        public decimal BottomHolePressure { get; set; }

        /// <summary>
        /// Wellhead pressure in psia.
        /// </summary>
        public decimal WellheadPressure { get; set; }

        /// <summary>
        /// Average reservoir temperature in degrees Rankine.
        /// </summary>
        public decimal? ReservoirTemperature { get; set; }

        /// <summary>
        /// Well UWI (Unique Well Identifier).
        /// </summary>
        public string? WellUWI { get; set; }
        public int GasOilRatio { get; set; }
        public int GasSpecificGravity { get; set; }
        public int DesiredProductionRate { get; set; }
    }
}

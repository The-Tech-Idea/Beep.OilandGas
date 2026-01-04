namespace Beep.OilandGas.Models.PumpPerformance
{
    /// <summary>
    /// Represents ESP (Electric Submersible Pump) design properties
    /// DTO for calculations - Entity class: ESP_DESIGN_PROPERTIES
    /// </summary>
    public class ESPDesignProperties
    {
        /// <summary>
        /// Desired flow rate in bbl/day
        /// </summary>
        public decimal DesiredFlowRate { get; set; }

        /// <summary>
        /// Total dynamic head in feet
        /// </summary>
        public decimal TotalDynamicHead { get; set; }

        /// <summary>
        /// Well depth in feet
        /// </summary>
        public decimal WellDepth { get; set; }

        /// <summary>
        /// Casing diameter in inches
        /// </summary>
        public decimal CasingDiameter { get; set; }

        /// <summary>
        /// Tubing diameter in inches
        /// </summary>
        public decimal TubingDiameter { get; set; }

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
        /// Wellhead pressure in psia
        /// </summary>
        public decimal WellheadPressure { get; set; }

        /// <summary>
        /// Bottom hole temperature in Rankine
        /// </summary>
        public decimal BottomHoleTemperature { get; set; }

        /// <summary>
        /// Gas specific gravity (relative to air)
        /// </summary>
        public decimal GasSpecificGravity { get; set; }

        /// <summary>
        /// Pump setting depth in feet
        /// </summary>
        public decimal PumpSettingDepth { get; set; }
    }
}

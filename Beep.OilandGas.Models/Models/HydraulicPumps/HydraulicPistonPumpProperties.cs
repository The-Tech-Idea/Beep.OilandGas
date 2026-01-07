namespace Beep.OilandGas.Models.HydraulicPumps
{
    /// <summary>
    /// Represents piston pump properties for hydraulic pump calculations.
    /// </summary>
    public class HydraulicPistonPumpProperties
    {
        /// <summary>
        /// Piston diameter in inches.
        /// </summary>
        public decimal PistonDiameter { get; set; }

        /// <summary>
        /// Rod diameter in inches.
        /// </summary>
        public decimal RodDiameter { get; set; }

        /// <summary>
        /// Stroke length in inches.
        /// </summary>
        public decimal StrokeLength { get; set; }

        /// <summary>
        /// Strokes per minute (pump speed).
        /// </summary>
        public decimal StrokesPerMinute { get; set; }

        /// <summary>
        /// Power fluid pressure in psia.
        /// </summary>
        public decimal PowerFluidPressure { get; set; }

        /// <summary>
        /// Power fluid rate in bbl/day.
        /// </summary>
        public decimal PowerFluidRate { get; set; }

        /// <summary>
        /// Power fluid specific gravity (relative to water).
        /// </summary>
        public decimal PowerFluidSpecificGravity { get; set; }

        /// <summary>
        /// Pump displacement in cubic inches per stroke.
        /// </summary>
        public decimal? Displacement { get; set; }

        /// <summary>
        /// Pump volumetric efficiency (fraction 0-1).
        /// </summary>
        public decimal VolumetricEfficiency { get; set; }

        /// <summary>
        /// Pump mechanical efficiency (fraction 0-1).
        /// </summary>
        public decimal MechanicalEfficiency { get; set; }

        /// <summary>
        /// Pump manufacturer.
        /// </summary>
        public string? Manufacturer { get; set; }

        /// <summary>
        /// Pump model/ID.
        /// </summary>
        public string? PumpID { get; set; }
    }
}

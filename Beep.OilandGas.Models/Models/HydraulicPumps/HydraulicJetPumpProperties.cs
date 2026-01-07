namespace Beep.OilandGas.Models.HydraulicPumps
{
    /// <summary>
    /// Represents jet pump properties for hydraulic pump calculations.
    /// </summary>
    public class HydraulicJetPumpProperties
    {
        /// <summary>
        /// Nozzle diameter in inches.
        /// </summary>
        public decimal NozzleDiameter { get; set; }

        /// <summary>
        /// Throat diameter in inches.
        /// </summary>
        public decimal ThroatDiameter { get; set; }

        /// <summary>
        /// Diffuser diameter in inches.
        /// </summary>
        public decimal? DiffuserDiameter { get; set; }

        /// <summary>
        /// Power fluid flow rate in bbl/day.
        /// </summary>
        public decimal PowerFluidRate { get; set; }

        /// <summary>
        /// Power fluid pressure in psia.
        /// </summary>
        public decimal PowerFluidPressure { get; set; }

        /// <summary>
        /// Power fluid specific gravity (relative to water).
        /// </summary>
        public decimal PowerFluidSpecificGravity { get; set; }

        /// <summary>
        /// Production fluid type (Oil, Water, Gas).
        /// </summary>
        public string? ProductionFluidType { get; set; }

        /// <summary>
        /// Pump depth in feet.
        /// </summary>
        public decimal? PumpDepth { get; set; }

        /// <summary>
        /// Pump ID/serial number.
        /// </summary>
        public string? PumpID { get; set; }

        /// <summary>
        /// Pump manufacturer.
        /// </summary>
        public string? Manufacturer { get; set; }
    }
}

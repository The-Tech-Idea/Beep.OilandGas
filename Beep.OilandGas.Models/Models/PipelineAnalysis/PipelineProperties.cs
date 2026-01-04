namespace Beep.OilandGas.Models.PipelineAnalysis
{
    /// <summary>
    /// Represents pipeline properties
    /// DTO for calculations - Entity class: PIPELINE_PROPERTIES
    /// </summary>
    public class PipelineProperties
    {
        /// <summary>
        /// Pipeline diameter in inches
        /// </summary>
        public decimal Diameter { get; set; }

        /// <summary>
        /// Pipeline length in feet
        /// </summary>
        public decimal Length { get; set; }

        /// <summary>
        /// Pipeline roughness in feet
        /// </summary>
        public decimal Roughness { get; set; } = 0.00015m; // Typical for steel

        /// <summary>
        /// Elevation change in feet (positive = uphill)
        /// </summary>
        public decimal ElevationChange { get; set; } = 0m;

        /// <summary>
        /// Inlet pressure in psia
        /// </summary>
        public decimal InletPressure { get; set; }

        /// <summary>
        /// Outlet pressure in psia
        /// </summary>
        public decimal OutletPressure { get; set; }

        /// <summary>
        /// Average temperature in Rankine
        /// </summary>
        public decimal AverageTemperature { get; set; }
    }
}

namespace Beep.OilandGas.Models.CompressorAnalysis
{
    /// <summary>
    /// Reciprocating compressor properties
    /// DTO for calculations - Entity class: RECIPROCATING_COMPRESSOR_PROPERTIES
    /// </summary>
    public class ReciprocatingCompressorProperties
    {
        /// <summary>
        /// Operating conditions
        /// </summary>
        public CompressorOperatingConditions OperatingConditions { get; set; } = new();

        /// <summary>
        /// Cylinder diameter in inches
        /// </summary>
        public decimal CylinderDiameter { get; set; }

        /// <summary>
        /// Stroke length in inches
        /// </summary>
        public decimal StrokeLength { get; set; }

        /// <summary>
        /// Rotational speed in RPM
        /// </summary>
        public decimal RotationalSpeed { get; set; }

        /// <summary>
        /// Number of cylinders
        /// </summary>
        public int NumberOfCylinders { get; set; } = 1;

        /// <summary>
        /// Volumetric efficiency (0-1)
        /// </summary>
        public decimal VolumetricEfficiency { get; set; } = 0.85m;
        public int ClearanceFactor { get; set; }
    }
}




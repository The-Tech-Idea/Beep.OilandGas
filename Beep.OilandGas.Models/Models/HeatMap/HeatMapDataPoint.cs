namespace Beep.OilandGas.Models.HeatMap
{
    /// <summary>
    /// Data point for heat map visualization
    /// </summary>
    public class HeatMapDataPoint
    {
        /// <summary>
        /// X coordinate (normalized or screen coordinates)
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Y coordinate (normalized or screen coordinates)
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// Original X coordinate (data space)
        /// </summary>
        public double OriginalX { get; set; }

        /// <summary>
        /// Original Y coordinate (data space)
        /// </summary>
        public double OriginalY { get; set; }

        /// <summary>
        /// Value at this point
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// Optional label for this point
        /// </summary>
        public string? Label { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public HeatMapDataPoint()
        {
        }

        /// <summary>
        /// Constructor with coordinates and value
        /// </summary>
        public HeatMapDataPoint(double originalX, double originalY, double value, string? label = null)
        {
            OriginalX = originalX;
            OriginalY = originalY;
            X = originalX;
            Y = originalY;
            Value = value;
            Label = label;
        }
    }
}

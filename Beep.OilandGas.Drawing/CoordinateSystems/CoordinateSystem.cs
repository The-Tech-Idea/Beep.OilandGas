using System;

namespace Beep.OilandGas.Drawing.CoordinateSystems
{
    /// <summary>
    /// Represents a coordinate system for oil and gas visualizations.
    /// </summary>
    public enum CoordinateSystemType
    {
        /// <summary>
        /// Depth-based coordinate system (feet or meters).
        /// </summary>
        Depth,

        /// <summary>
        /// Time-based coordinate system (days, months, years).
        /// </summary>
        Time,

        /// <summary>
        /// Geographic coordinate system (latitude/longitude).
        /// </summary>
        Geographic,

        /// <summary>
        /// Custom coordinate system.
        /// </summary>
        Custom
    }

    /// <summary>
    /// Provides coordinate system transformations and utilities.
    /// </summary>
    public class CoordinateSystem
    {
        /// <summary>
        /// Gets or sets the coordinate system type.
        /// </summary>
        public CoordinateSystemType Type { get; set; }

        /// <summary>
        /// Gets or sets the unit of measurement.
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// Gets or sets the minimum value in the coordinate system.
        /// </summary>
        public double MinValue { get; set; }

        /// <summary>
        /// Gets or sets the maximum value in the coordinate system.
        /// </summary>
        public double MaxValue { get; set; }

        /// <summary>
        /// Gets or sets whether the coordinate system is inverted (e.g., depth increases downward).
        /// </summary>
        public bool IsInverted { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CoordinateSystem"/> class.
        /// </summary>
        /// <param name="type">The coordinate system type.</param>
        /// <param name="unit">The unit of measurement.</param>
        /// <param name="minValue">Minimum value.</param>
        /// <param name="maxValue">Maximum value.</param>
        /// <param name="isInverted">Whether the system is inverted.</param>
        public CoordinateSystem(
            CoordinateSystemType type,
            string unit,
            double minValue,
            double maxValue,
            bool isInverted = false)
        {
            Type = type;
            Unit = unit;
            MinValue = minValue;
            MaxValue = maxValue;
            IsInverted = isInverted;
        }

        /// <summary>
        /// Creates a depth-based coordinate system.
        /// </summary>
        /// <param name="minDepth">Minimum depth.</param>
        /// <param name="maxDepth">Maximum depth.</param>
        /// <param name="unit">Unit (feet, meters).</param>
        /// <returns>A depth coordinate system.</returns>
        public static CoordinateSystem CreateDepthSystem(double minDepth, double maxDepth, string unit = "feet")
        {
            return new CoordinateSystem(CoordinateSystemType.Depth, unit, minDepth, maxDepth, isInverted: true);
        }

        /// <summary>
        /// Creates a time-based coordinate system.
        /// </summary>
        /// <param name="minTime">Minimum time.</param>
        /// <param name="maxTime">Maximum time.</param>
        /// <param name="unit">Unit (days, months, years).</param>
        /// <returns>A time coordinate system.</returns>
        public static CoordinateSystem CreateTimeSystem(double minTime, double maxTime, string unit = "days")
        {
            return new CoordinateSystem(CoordinateSystemType.Time, unit, minTime, maxTime, isInverted: false);
        }

        /// <summary>
        /// Normalizes a value to the range [0, 1] within the coordinate system.
        /// </summary>
        /// <param name="value">The value to normalize.</param>
        /// <returns>Normalized value between 0 and 1.</returns>
        public double Normalize(double value)
        {
            if (MaxValue <= MinValue)
                return 0;

            double normalized = (value - MinValue) / (MaxValue - MinValue);
            
            if (IsInverted)
                normalized = 1.0 - normalized;

            return Math.Max(0, Math.Min(1, normalized));
        }

        /// <summary>
        /// Denormalizes a value from [0, 1] to the coordinate system range.
        /// </summary>
        /// <param name="normalized">The normalized value (0 to 1).</param>
        /// <returns>The denormalized value.</returns>
        public double Denormalize(double normalized)
        {
            normalized = Math.Max(0, Math.Min(1, normalized));

            if (IsInverted)
                normalized = 1.0 - normalized;

            return MinValue + normalized * (MaxValue - MinValue);
        }

        /// <summary>
        /// Converts a value from this coordinate system to screen coordinates.
        /// </summary>
        /// <param name="value">The value in the coordinate system.</param>
        /// <param name="screenHeight">The screen height in pixels.</param>
        /// <returns>The screen Y coordinate.</returns>
        public float ToScreenY(double value, float screenHeight)
        {
            double normalized = Normalize(value);
            return (float)(normalized * screenHeight);
        }

        /// <summary>
        /// Converts a screen Y coordinate to a value in this coordinate system.
        /// </summary>
        /// <param name="screenY">The screen Y coordinate.</param>
        /// <param name="screenHeight">The screen height in pixels.</param>
        /// <returns>The value in the coordinate system.</returns>
        public double FromScreenY(float screenY, float screenHeight)
        {
            double normalized = screenY / screenHeight;
            return Denormalize(normalized);
        }
    }
}


using System;

namespace Beep.OilandGas.Drawing.CoordinateSystems
{
    /// <summary>
    /// Represents the semantic role of a coordinate axis.
    /// </summary>
    public enum CoordinateAxisKind
    {
        Easting,
        Northing,
        Longitude,
        Latitude,
        Depth,
        MeasuredDepth,
        SectionDistance,
        Time,
        Custom
    }

    /// <summary>
    /// Describes one axis in a coordinate reference definition.
    /// </summary>
    public sealed class CoordinateAxisDefinition
    {
        /// <summary>
        /// Gets the semantic axis kind.
        /// </summary>
        public CoordinateAxisKind Kind { get; }

        /// <summary>
        /// Gets the human-readable axis name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the unit of measure.
        /// </summary>
        public MeasurementUnit Unit { get; }

        /// <summary>
        /// Gets whether the axis increases in the visually inverted direction.
        /// </summary>
        public bool IsInverted { get; }

        public CoordinateAxisDefinition(CoordinateAxisKind kind, string name, MeasurementUnit unit, bool isInverted = false)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Axis name is required.", nameof(name));

            Kind = kind;
            Name = name.Trim();
            Unit = unit ?? throw new ArgumentNullException(nameof(unit));
            IsInverted = isInverted;
        }
    }
}
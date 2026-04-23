using System;

namespace Beep.OilandGas.Drawing.CoordinateSystems
{
    /// <summary>
    /// Represents the broad measurement family used by a coordinate axis.
    /// </summary>
    public enum MeasurementDimension
    {
        Unknown,
        Length,
        Angle,
        Time,
        Scalar
    }

    /// <summary>
    /// Represents a normalized unit of measure for scene and CRS contracts.
    /// </summary>
    public sealed class MeasurementUnit : IEquatable<MeasurementUnit>
    {
        private readonly string normalizedCode;

        /// <summary>
        /// Gets the stable unit code.
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// Gets the display name.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Gets the measurement family.
        /// </summary>
        public MeasurementDimension Dimension { get; }

        public static MeasurementUnit Unknown { get; } = new("unknown", "Unknown", MeasurementDimension.Unknown);
        public static MeasurementUnit Feet { get; } = new("ft", "Feet", MeasurementDimension.Length);
        public static MeasurementUnit Meters { get; } = new("m", "Meters", MeasurementDimension.Length);
        public static MeasurementUnit DecimalDegrees { get; } = new("deg", "Decimal Degrees", MeasurementDimension.Angle);
        public static MeasurementUnit Milliseconds { get; } = new("ms", "Milliseconds", MeasurementDimension.Time);
        public static MeasurementUnit Days { get; } = new("d", "Days", MeasurementDimension.Time);

        /// <summary>
        /// Initializes a new instance of the <see cref="MeasurementUnit"/> class.
        /// </summary>
        public MeasurementUnit(string code, string displayName, MeasurementDimension dimension)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("Unit code is required.", nameof(code));
            if (string.IsNullOrWhiteSpace(displayName))
                throw new ArgumentException("Unit display name is required.", nameof(displayName));

            Code = code.Trim();
            DisplayName = displayName.Trim();
            Dimension = dimension;
            normalizedCode = Code.ToLowerInvariant();
        }

        /// <summary>
        /// Creates a normalized unit instance from a common code or symbol.
        /// </summary>
        public static MeasurementUnit FromCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return Unknown;

            var normalized = code.Trim().ToLowerInvariant();
            return normalized switch
            {
                "ft" or "feet" or "foot" => Feet,
                "m" or "meter" or "meters" or "metre" or "metres" => Meters,
                "deg" or "degree" or "degrees" or "dd" => DecimalDegrees,
                "ms" or "millisecond" or "milliseconds" => Milliseconds,
                "d" or "day" or "days" => Days,
                _ => new MeasurementUnit(code, code, MeasurementDimension.Unknown)
            };
        }

        public bool Equals(MeasurementUnit other)
        {
            if (ReferenceEquals(null, other))
                return false;

            return string.Equals(normalizedCode, other.normalizedCode, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is MeasurementUnit other && Equals(other);
        }

        public override int GetHashCode()
        {
            return normalizedCode.GetHashCode(StringComparison.Ordinal);
        }

        public override string ToString()
        {
            return Code;
        }
    }
}
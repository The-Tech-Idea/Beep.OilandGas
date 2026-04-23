using System.Globalization;

namespace Beep.OilandGas.Drawing.Measurements
{
    /// <summary>
    /// Represents a typed measurement result produced from a drawing scene.
    /// </summary>
    public sealed class SceneMeasurementResult
    {
        /// <summary>
        /// Gets the measurement kind.
        /// </summary>
        public SceneMeasurementKind Kind { get; }

        /// <summary>
        /// Gets the source geometry kind.
        /// </summary>
        public SceneMeasurementGeometryKind GeometryKind { get; }

        /// <summary>
        /// Gets the numeric measurement value.
        /// </summary>
        public double Value { get; }

        /// <summary>
        /// Gets the normalized unit code.
        /// </summary>
        public string UnitCode { get; }

        /// <summary>
        /// Gets the display label for the unit.
        /// </summary>
        public string UnitLabel { get; }

        /// <summary>
        /// Gets the measurement method label.
        /// </summary>
        public string Method { get; }

        /// <summary>
        /// Gets whether the measurement is approximate.
        /// </summary>
        public bool IsApproximate { get; }

        /// <summary>
        /// Gets the number of vertices used in the measurement.
        /// </summary>
        public int VertexCount { get; }

        /// <summary>
        /// Gets a formatted measurement label.
        /// </summary>
        public string DisplayText => string.IsNullOrWhiteSpace(UnitLabel)
            ? Value.ToString("0.###", CultureInfo.InvariantCulture)
            : $"{Value.ToString("0.###", CultureInfo.InvariantCulture)} {UnitLabel}";

        public SceneMeasurementResult(
            SceneMeasurementKind kind,
            SceneMeasurementGeometryKind geometryKind,
            double value,
            string unitCode,
            string unitLabel,
            string method,
            bool isApproximate,
            int vertexCount)
        {
            Kind = kind;
            GeometryKind = geometryKind;
            Value = value;
            UnitCode = unitCode ?? string.Empty;
            UnitLabel = unitLabel ?? string.Empty;
            Method = method ?? string.Empty;
            IsApproximate = isApproximate;
            VertexCount = vertexCount;
        }
    }
}
using System.Collections.Generic;
using Beep.OilandGas.Drawing.Measurements;

namespace Beep.OilandGas.Drawing.Interaction
{
    /// <summary>
    /// Represents a persisted measurement annotation on a scene.
    /// </summary>
    public sealed class SceneMeasurementAnnotation
    {
        /// <summary>
        /// Gets or sets the annotation identifier.
        /// </summary>
        public string AnnotationId { get; set; }

        /// <summary>
        /// Gets or sets the display label.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the measurement kind.
        /// </summary>
        public SceneMeasurementKind Kind { get; set; }

        /// <summary>
        /// Gets or sets the source geometry kind.
        /// </summary>
        public SceneMeasurementGeometryKind GeometryKind { get; set; }

        /// <summary>
        /// Gets or sets the numeric value.
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// Gets or sets the unit code.
        /// </summary>
        public string UnitCode { get; set; }

        /// <summary>
        /// Gets or sets the formatted display value.
        /// </summary>
        public string DisplayText { get; set; }

        /// <summary>
        /// Gets or sets the calculation method label.
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// Gets or sets whether the measurement is approximate.
        /// </summary>
        public bool IsApproximate { get; set; }

        /// <summary>
        /// Gets the persisted world-space vertices.
        /// </summary>
        public List<SceneWorldPoint> Vertices { get; } = new List<SceneWorldPoint>();
    }
}
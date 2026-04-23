using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Drawing.Measurements;
using SkiaSharp;

namespace Beep.OilandGas.Drawing.Interaction
{
    /// <summary>
    /// Stores persisted interaction artifacts for a scene.
    /// </summary>
    public sealed class SceneInteractionState
    {
        /// <summary>
        /// Gets persisted feature selections.
        /// </summary>
        public List<SceneSelectionAnnotation> Selections { get; } = new List<SceneSelectionAnnotation>();

        /// <summary>
        /// Gets persisted measurement annotations.
        /// </summary>
        public List<SceneMeasurementAnnotation> Measurements { get; } = new List<SceneMeasurementAnnotation>();

        /// <summary>
        /// Gets persisted interaction rendering style options.
        /// </summary>
        public SceneInteractionRenderStyle RenderStyle { get; } = new SceneInteractionRenderStyle();

        /// <summary>
        /// Clears all selection annotations.
        /// </summary>
        public void ClearSelections()
        {
            Selections.Clear();
        }

        /// <summary>
        /// Clears all measurement annotations.
        /// </summary>
        public void ClearMeasurements()
        {
            Measurements.Clear();
        }

        /// <summary>
        /// Removes a persisted selection annotation by annotation identifier.
        /// </summary>
        public bool RemoveSelection(string annotationId)
        {
            if (string.IsNullOrWhiteSpace(annotationId))
                return false;

            var selection = Selections.FirstOrDefault(candidate => string.Equals(candidate.AnnotationId, annotationId, StringComparison.OrdinalIgnoreCase));
            if (selection == null)
                return false;

            return Selections.Remove(selection);
        }

        /// <summary>
        /// Removes a persisted measurement annotation by annotation identifier.
        /// </summary>
        public bool RemoveMeasurement(string annotationId)
        {
            if (string.IsNullOrWhiteSpace(annotationId))
                return false;

            var measurement = Measurements.FirstOrDefault(candidate => string.Equals(candidate.AnnotationId, annotationId, StringComparison.OrdinalIgnoreCase));
            if (measurement == null)
                return false;

            return Measurements.Remove(measurement);
        }

        /// <summary>
        /// Adds a persisted selection annotation.
        /// </summary>
        public SceneSelectionAnnotation AddSelection(LayerHitResult hit, bool replaceExisting = false)
        {
            if (hit == null)
                throw new ArgumentNullException(nameof(hit));

            if (replaceExisting)
                Selections.Clear();

            var annotation = new SceneSelectionAnnotation
            {
                AnnotationId = Guid.NewGuid().ToString("N"),
                LayerName = hit.LayerName,
                FeatureId = hit.FeatureId,
                FeatureLabel = hit.FeatureLabel,
                FeatureKind = hit.FeatureKind,
                Anchor = SceneWorldPoint.From(hit.WorldAnchor),
                Metadata = hit.Metadata?.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase)
                    ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            };

            Selections.Add(annotation);
            return annotation;
        }

        /// <summary>
        /// Adds a persisted measurement annotation.
        /// </summary>
        public SceneMeasurementAnnotation AddMeasurement(SceneMeasurementResult measurement, IEnumerable<SKPoint> worldVertices, string label = null)
        {
            if (measurement == null)
                throw new ArgumentNullException(nameof(measurement));
            if (worldVertices == null)
                throw new ArgumentNullException(nameof(worldVertices));

            var annotation = new SceneMeasurementAnnotation
            {
                AnnotationId = Guid.NewGuid().ToString("N"),
                Label = string.IsNullOrWhiteSpace(label) ? measurement.Kind.ToString() : label.Trim(),
                Kind = measurement.Kind,
                GeometryKind = measurement.GeometryKind,
                Value = measurement.Value,
                UnitCode = measurement.UnitCode,
                DisplayText = measurement.DisplayText,
                Method = measurement.Method,
                IsApproximate = measurement.IsApproximate
            };

            annotation.Vertices.AddRange(worldVertices.Select(SceneWorldPoint.From));
            Measurements.Add(annotation);
            return annotation;
        }
    }
}
using System.Collections.Generic;
using SkiaSharp;

namespace Beep.OilandGas.Drawing.Interaction
{
    /// <summary>
    /// Describes a feature resolved by a layer hit test.
    /// </summary>
    public sealed class LayerHitResult
    {
        /// <summary>
        /// Gets the resolved layer name.
        /// </summary>
        public string LayerName { get; }

        /// <summary>
        /// Gets the feature identifier.
        /// </summary>
        public string FeatureId { get; }

        /// <summary>
        /// Gets the feature label.
        /// </summary>
        public string FeatureLabel { get; }

        /// <summary>
        /// Gets the feature kind label.
        /// </summary>
        public string FeatureKind { get; }

        /// <summary>
        /// Gets the world-space anchor point for the feature.
        /// </summary>
        public SKPoint WorldAnchor { get; }

        /// <summary>
        /// Gets the query distance in world units.
        /// </summary>
        public float Distance { get; }

        /// <summary>
        /// Gets feature metadata.
        /// </summary>
        public IReadOnlyDictionary<string, string> Metadata { get; }

        public LayerHitResult(
            string layerName,
            string featureId,
            string featureLabel,
            string featureKind,
            SKPoint worldAnchor,
            float distance,
            IReadOnlyDictionary<string, string> metadata)
        {
            LayerName = layerName ?? string.Empty;
            FeatureId = featureId ?? string.Empty;
            FeatureLabel = featureLabel ?? string.Empty;
            FeatureKind = featureKind ?? string.Empty;
            WorldAnchor = worldAnchor;
            Distance = distance;
            Metadata = metadata ?? new Dictionary<string, string>();
        }
    }
}
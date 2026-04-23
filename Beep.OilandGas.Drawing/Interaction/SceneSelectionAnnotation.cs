using System.Collections.Generic;

namespace Beep.OilandGas.Drawing.Interaction
{
    /// <summary>
    /// Represents a persisted feature selection on a scene.
    /// </summary>
    public sealed class SceneSelectionAnnotation
    {
        /// <summary>
        /// Gets or sets the selection identifier.
        /// </summary>
        public string AnnotationId { get; set; }

        /// <summary>
        /// Gets or sets the layer name.
        /// </summary>
        public string LayerName { get; set; }

        /// <summary>
        /// Gets or sets the feature identifier.
        /// </summary>
        public string FeatureId { get; set; }

        /// <summary>
        /// Gets or sets the feature label.
        /// </summary>
        public string FeatureLabel { get; set; }

        /// <summary>
        /// Gets or sets the feature kind.
        /// </summary>
        public string FeatureKind { get; set; }

        /// <summary>
        /// Gets or sets the selection anchor.
        /// </summary>
        public SceneWorldPoint Anchor { get; set; }

        /// <summary>
        /// Gets or sets copied feature metadata.
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    }
}
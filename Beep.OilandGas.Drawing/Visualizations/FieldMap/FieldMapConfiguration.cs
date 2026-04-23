using Beep.OilandGas.Drawing.Primitives;
using SkiaSharp;

namespace Beep.OilandGas.Drawing.Visualizations.FieldMap
{
    /// <summary>
    /// Controls styling and overlays for field map rendering.
    /// </summary>
    public sealed class FieldMapConfiguration
    {
        /// <summary>
        /// Gets or sets the map background color.
        /// </summary>
        public SKColor BackgroundColor { get; set; } = new SKColor(250, 251, 247);

        /// <summary>
        /// Gets or sets whether polygon and boundary assets are rendered.
        /// </summary>
        public bool ShowAreas { get; set; } = true;

        /// <summary>
        /// Gets or sets whether area labels are rendered.
        /// </summary>
        public bool ShowAreaLabels { get; set; } = true;

        /// <summary>
        /// Gets or sets whether point assets are rendered.
        /// </summary>
        public bool ShowPoints { get; set; } = true;

        /// <summary>
        /// Gets or sets whether connection assets are rendered.
        /// </summary>
        public bool ShowConnections { get; set; } = true;

        /// <summary>
        /// Gets or sets whether point asset labels are rendered.
        /// </summary>
        public bool ShowPointLabels { get; set; } = true;

        /// <summary>
        /// Gets or sets whether connection labels are rendered.
        /// </summary>
        public bool ShowConnectionLabels { get; set; } = true;

        /// <summary>
        /// Gets or sets whether asset paths are rendered when present.
        /// </summary>
        public bool ShowPaths { get; set; } = true;

        /// <summary>
        /// Gets or sets whether the legend is rendered.
        /// </summary>
        public bool ShowLegend { get; set; } = true;

        /// <summary>
        /// Gets or sets the legend title.
        /// </summary>
        public string LegendTitle { get; set; } = "Field Map Assets";

        /// <summary>
        /// Gets or sets the legend anchor.
        /// </summary>
        public OverlayAnchor LegendAnchor { get; set; } = OverlayAnchor.TopRight;

        /// <summary>
        /// Gets or sets the polygon fill opacity.
        /// </summary>
        public byte AreaFillAlpha { get; set; } = 72;

        /// <summary>
        /// Gets or sets the boundary stroke width.
        /// </summary>
        public float AreaStrokeWidth { get; set; } = 2.0f;

        /// <summary>
        /// Gets or sets the asset path stroke width.
        /// </summary>
        public float PathStrokeWidth { get; set; } = 1.6f;

        /// <summary>
        /// Gets or sets the explicit connection stroke width.
        /// </summary>
        public float ConnectionStrokeWidth { get; set; } = 2.0f;

        /// <summary>
        /// Gets or sets the marker radius.
        /// </summary>
        public float MarkerRadius { get; set; } = 6f;

        /// <summary>
        /// Gets or sets the label font size.
        /// </summary>
        public float LabelFontSize { get; set; } = 10f;
    }
}
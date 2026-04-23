using SkiaSharp;

namespace Beep.OilandGas.Drawing.Interaction
{
    /// <summary>
    /// Controls how persisted scene interaction artifacts render through the core pipeline.
    /// </summary>
    public sealed class SceneInteractionRenderStyle
    {
        /// <summary>
        /// Gets or sets whether persisted annotations should be rendered.
        /// </summary>
        public bool IsVisible { get; set; } = true;

        /// <summary>
        /// Gets or sets the selection marker color.
        /// </summary>
        public SKColor SelectionColor { get; set; } = new(126, 87, 194);

        /// <summary>
        /// Gets or sets the selection ring stroke width.
        /// </summary>
        public float SelectionStrokeWidth { get; set; } = 3f;

        /// <summary>
        /// Gets or sets the radius of the selection ring.
        /// </summary>
        public float SelectionRingRadius { get; set; } = 11f;

        /// <summary>
        /// Gets or sets the radius of the selection core marker.
        /// </summary>
        public float SelectionCoreRadius { get; set; } = 5f;

        /// <summary>
        /// Gets or sets the dash pattern used for selection rings.
        /// </summary>
        public float[] SelectionDashPattern { get; set; } = new[] { 5f, 4f };

        /// <summary>
        /// Gets or sets whether selection labels should be rendered.
        /// </summary>
        public bool ShowSelectionLabels { get; set; } = true;

        /// <summary>
        /// Gets or sets the X offset for selection labels.
        /// </summary>
        public float SelectionLabelOffsetX { get; set; } = 14f;

        /// <summary>
        /// Gets or sets the Y offset for selection labels.
        /// </summary>
        public float SelectionLabelOffsetY { get; set; } = -12f;

        /// <summary>
        /// Gets or sets the color used for line-based measurements.
        /// </summary>
        public SKColor MeasurementLineColor { get; set; } = new(2, 136, 209);

        /// <summary>
        /// Gets or sets the color used for polygon measurement outlines and fills.
        /// </summary>
        public SKColor MeasurementAreaColor { get; set; } = new(251, 140, 0);

        /// <summary>
        /// Gets or sets the alpha used for polygon measurement fills.
        /// </summary>
        public byte MeasurementAreaFillAlpha { get; set; } = 52;

        /// <summary>
        /// Gets or sets the measurement stroke width.
        /// </summary>
        public float MeasurementStrokeWidth { get; set; } = 3f;

        /// <summary>
        /// Gets or sets the radius of measurement vertices.
        /// </summary>
        public float MeasurementVertexRadius { get; set; } = 4f;

        /// <summary>
        /// Gets or sets the halo color used around measurement vertices.
        /// </summary>
        public SKColor MeasurementVertexHaloColor { get; set; } = new(255, 255, 255, 220);

        /// <summary>
        /// Gets or sets the halo stroke width used around measurement vertices.
        /// </summary>
        public float MeasurementVertexHaloStrokeWidth { get; set; } = 2f;

        /// <summary>
        /// Gets or sets whether measurement labels should be rendered.
        /// </summary>
        public bool ShowMeasurementLabels { get; set; } = true;

        /// <summary>
        /// Gets or sets the X offset for measurement labels.
        /// </summary>
        public float MeasurementLabelOffsetX { get; set; } = 8f;

        /// <summary>
        /// Gets or sets the Y offset for measurement labels.
        /// </summary>
        public float MeasurementLabelOffsetY { get; set; } = -8f;

        /// <summary>
        /// Gets or sets the label text fill color.
        /// </summary>
        public SKColor LabelFillColor { get; set; } = new(33, 33, 33);

        /// <summary>
        /// Gets or sets the label halo color.
        /// </summary>
        public SKColor LabelStrokeColor { get; set; } = new(255, 255, 255, 230);

        /// <summary>
        /// Gets or sets the label font size.
        /// </summary>
        public float LabelTextSize { get; set; } = 13f;

        /// <summary>
        /// Gets or sets the label halo stroke width.
        /// </summary>
        public float LabelStrokeWidth { get; set; } = 4f;
    }
}
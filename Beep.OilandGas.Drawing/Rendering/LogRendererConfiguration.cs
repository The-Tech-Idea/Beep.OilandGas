using SkiaSharp;
using System.Collections.Generic;

namespace Beep.OilandGas.Drawing.Rendering
{
    /// <summary>
    /// Configuration for log rendering along wellbore paths.
    /// </summary>
    public class LogRendererConfiguration
    {
        /// <summary>
        /// Gets or sets the background color.
        /// </summary>
        public SKColor BackgroundColor { get; set; } = SKColors.White;

        /// <summary>
        /// Gets or sets the log track width in pixels.
        /// </summary>
        public float TrackWidth { get; set; } = 100f;

        /// <summary>
        /// Gets or sets the spacing between log tracks.
        /// </summary>
        public float TrackSpacing { get; set; } = 10f;

        /// <summary>
        /// Gets or sets the log curve line width.
        /// </summary>
        public float CurveLineWidth { get; set; } = 1.5f;

        /// <summary>
        /// Gets or sets whether to fill log curves.
        /// </summary>
        public bool FillCurves { get; set; } = false;

        /// <summary>
        /// Gets or sets the fill color for filled curves.
        /// </summary>
        public SKColor FillColor { get; set; } = new SKColor(255, 200, 0, 128);

        /// <summary>
        /// Gets or sets whether to show grid lines.
        /// </summary>
        public bool ShowGrid { get; set; } = true;

        /// <summary>
        /// Gets or sets the grid color.
        /// </summary>
        public SKColor GridColor { get; set; } = new SKColor(200, 200, 200);

        /// <summary>
        /// Gets or sets the grid line width.
        /// </summary>
        public float GridLineWidth { get; set; } = 0.5f;

        /// <summary>
        /// Gets or sets whether to show depth scale.
        /// </summary>
        public bool ShowDepthScale { get; set; } = true;

        /// <summary>
        /// Gets or sets the depth scale color.
        /// </summary>
        public SKColor DepthScaleColor { get; set; } = SKColors.Black;

        /// <summary>
        /// Gets or sets the depth scale font size.
        /// </summary>
        public float DepthScaleFontSize { get; set; } = 10f;

        /// <summary>
        /// Gets or sets whether to show curve names.
        /// </summary>
        public bool ShowCurveNames { get; set; } = true;

        /// <summary>
        /// Gets or sets the curve name font size.
        /// </summary>
        public float CurveNameFontSize { get; set; } = 11f;

        /// <summary>
        /// Gets or sets the curve name color.
        /// </summary>
        public SKColor CurveNameColor { get; set; } = SKColors.Black;

        /// <summary>
        /// Gets or sets the default curve colors (curve name to color mapping).
        /// </summary>
        public Dictionary<string, SKColor> DefaultCurveColors { get; set; } = new Dictionary<string, SKColor>
        {
            ["GR"] = SKColors.Green,
            ["RHOB"] = SKColors.Red,
            ["NPHI"] = SKColors.Blue,
            ["DT"] = SKColors.Orange,
            ["RES"] = SKColors.Purple
        };

        /// <summary>
        /// Gets or sets the depth interval for depth markers.
        /// </summary>
        public double DepthInterval { get; set; } = 100.0;

        /// <summary>
        /// Gets or sets whether to use logarithmic scale for resistivity curves.
        /// </summary>
        public bool UseLogScaleForResistivity { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to normalize curves to track width.
        /// </summary>
        public bool NormalizeCurves { get; set; } = true;

        /// <summary>
        /// Gets or sets the minimum value for normalization (if null, uses curve min).
        /// </summary>
        public Dictionary<string, double?> MinValues { get; set; } = new Dictionary<string, double?>();

        /// <summary>
        /// Gets or sets the maximum value for normalization (if null, uses curve max).
        /// </summary>
        public Dictionary<string, double?> MaxValues { get; set; } = new Dictionary<string, double?>();

        /// <summary>
        /// Gets or sets whether to show wellbore path.
        /// </summary>
        public bool ShowWellborePath { get; set; } = true;

        /// <summary>
        /// Gets or sets the wellbore path color.
        /// </summary>
        public SKColor WellborePathColor { get; set; } = SKColors.Black;

        /// <summary>
        /// Gets or sets the wellbore path width.
        /// </summary>
        public float WellborePathWidth { get; set; } = 2f;

        /// <summary>
        /// Gets or sets whether to show depth markers along wellbore.
        /// </summary>
        public bool ShowDepthMarkers { get; set; } = true;

        /// <summary>
        /// Gets or sets the depth marker color.
        /// </summary>
        public SKColor DepthMarkerColor { get; set; } = SKColors.Gray;

        /// <summary>
        /// Gets or sets the depth marker line width.
        /// </summary>
        public float DepthMarkerLineWidth { get; set; } = 1f;

        /// <summary>
        /// Gets or sets the horizontal stretch factor for horizontal well sections.
        /// </summary>
        public float HorizontalStretchFactor { get; set; } = 10f;
    }
}


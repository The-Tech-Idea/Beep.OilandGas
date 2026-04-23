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
        /// Gets or sets the default width of the dedicated depth track.
        /// </summary>
        public float DepthTrackWidth { get; set; } = 72f;

        /// <summary>
        /// Gets or sets the height of the track header band.
        /// </summary>
        public float TrackHeaderHeight { get; set; } = 34f;

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
        /// Gets or sets whether the depth scale should render as a dedicated track when possible.
        /// </summary>
        public bool RenderDepthScaleAsTrack { get; set; } = true;

        /// <summary>
        /// Gets or sets the depth scale color.
        /// </summary>
        public SKColor DepthScaleColor { get; set; } = SKColors.Black;

        /// <summary>
        /// Gets or sets the depth scale font size.
        /// </summary>
        public float DepthScaleFontSize { get; set; } = 10f;

        /// <summary>
        /// Gets or sets the major tick length for the depth track.
        /// </summary>
        public float DepthTrackMajorTickLength { get; set; } = 12f;

        /// <summary>
        /// Gets or sets the minor tick length for the depth track.
        /// </summary>
        public float DepthTrackMinorTickLength { get; set; } = 6f;

        /// <summary>
        /// Gets or sets whether to show curve names.
        /// </summary>
        public bool ShowCurveNames { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to show track headers.
        /// </summary>
        public bool ShowTrackHeaders { get; set; } = true;

        /// <summary>
        /// Gets or sets the curve name font size.
        /// </summary>
        public float CurveNameFontSize { get; set; } = 11f;

        /// <summary>
        /// Gets or sets the header title font size.
        /// </summary>
        public float TrackHeaderFontSize { get; set; } = 11f;

        /// <summary>
        /// Gets or sets the header detail font size.
        /// </summary>
        public float TrackHeaderDetailFontSize { get; set; } = 9f;

        /// <summary>
        /// Gets or sets the curve name color.
        /// </summary>
        public SKColor CurveNameColor { get; set; } = SKColors.Black;

        /// <summary>
        /// Gets or sets the track header background color.
        /// </summary>
        public SKColor TrackHeaderBackgroundColor { get; set; } = new SKColor(245, 245, 245);

        /// <summary>
        /// Gets or sets the track header border color.
        /// </summary>
        public SKColor TrackHeaderBorderColor { get; set; } = new SKColor(180, 180, 180);

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

        /// <summary>
        /// Gets or sets the explicit track layout used for multi-track rendering.
        /// </summary>
        public List<LogTrackDefinition> Tracks { get; set; } = new List<LogTrackDefinition>();

        /// <summary>
        /// Gets or sets whether standard petrophysical track templates should be used when no explicit layout is provided.
        /// </summary>
        public bool UseStandardTrackTemplates { get; set; } = true;

        /// <summary>
        /// Gets or sets whether track scale annotations should be rendered inside curve tracks.
        /// </summary>
        public bool ShowTrackScaleAnnotations { get; set; } = true;

        /// <summary>
        /// Gets or sets the font size for per-track scale annotations.
        /// </summary>
        public float TrackScaleAnnotationFontSize { get; set; } = 8.5f;

        /// <summary>
        /// Gets or sets the row height for per-track scale annotations.
        /// </summary>
        public float TrackScaleAnnotationRowHeight { get; set; } = 11f;

        /// <summary>
        /// Gets or sets the padding around the track scale annotation band.
        /// </summary>
        public float TrackScaleAnnotationPadding { get; set; } = 3f;

        /// <summary>
        /// Gets or sets the separator color for the scale annotation band.
        /// </summary>
        public SKColor TrackScaleAnnotationSeparatorColor { get; set; } = new SKColor(210, 210, 210);

        /// <summary>
        /// Gets or sets whether density-neutron crossover shading should be rendered when both curves are present.
        /// </summary>
        public bool ShowDensityNeutronCrossoverShading { get; set; } = true;

        /// <summary>
        /// Gets or sets the fill color used for density-neutron crossover shading.
        /// </summary>
        public SKColor DensityNeutronCrossoverFillColor { get; set; } = new SKColor(255, 215, 0, 72);

        /// <summary>
        /// Gets or sets whether logarithmic tracks should render decade-based vertical grid treatment.
        /// </summary>
        public bool ShowLogDecadeGridLines { get; set; } = true;

        /// <summary>
        /// Gets or sets whether labels should be rendered inside interval tracks when space allows.
        /// </summary>
        public bool ShowIntervalLabels { get; set; } = true;

        /// <summary>
        /// Gets or sets the font size used for interval labels.
        /// </summary>
        public float IntervalLabelFontSize { get; set; } = 9.5f;

        /// <summary>
        /// Gets or sets the minimum interval height required before drawing a label.
        /// </summary>
        public float MinimumIntervalLabelHeight { get; set; } = 18f;

        /// <summary>
        /// Gets or sets the text color used for interval labels.
        /// </summary>
        public SKColor IntervalLabelColor { get; set; } = SKColors.Black;

        /// <summary>
        /// Gets or sets the border color for interval tracks.
        /// </summary>
        public SKColor IntervalBorderColor { get; set; } = new SKColor(90, 90, 90);

        /// <summary>
        /// Gets or sets the border width for interval blocks.
        /// </summary>
        public float IntervalBorderWidth { get; set; } = 0.8f;

        /// <summary>
        /// Gets or sets the pattern size used for lithology interval fills.
        /// </summary>
        public float IntervalPatternSize { get; set; } = 5f;

        /// <summary>
        /// Gets or sets whether non-pay intervals should be visually dimmed.
        /// </summary>
        public bool DimNonPayIntervals { get; set; } = true;
    }
}


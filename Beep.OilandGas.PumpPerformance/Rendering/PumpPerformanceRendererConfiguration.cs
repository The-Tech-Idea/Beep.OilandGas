using SkiaSharp;

namespace Beep.OilandGas.PumpPerformance.Rendering
{
    /// <summary>
    /// Configuration for pump performance curve rendering with SkiaSharp.
    /// </summary>
    public class PumpPerformanceRendererConfiguration
    {
        /// <summary>
        /// Gets or sets the background color.
        /// </summary>
        public SKColor BackgroundColor { get; set; } = SKColors.White;

        /// <summary>
        /// Gets or sets the plot area background color.
        /// </summary>
        public SKColor PlotAreaBackgroundColor { get; set; } = SKColors.White;

        /// <summary>
        /// Gets or sets whether to show grid lines.
        /// </summary>
        public bool ShowGrid { get; set; } = true;

        /// <summary>
        /// Gets or sets the grid color.
        /// </summary>
        public SKColor GridColor { get; set; } = new SKColor(224, 224, 224);

        /// <summary>
        /// Gets or sets the grid line width.
        /// </summary>
        public float GridLineWidth { get; set; } = 1f;

        /// <summary>
        /// Gets or sets whether to show major grid lines only.
        /// </summary>
        public bool ShowMajorGridOnly { get; set; } = false;

        // H-Q Curve Colors
        /// <summary>
        /// Gets or sets the H-Q curve color.
        /// </summary>
        public SKColor HQCurveColor { get; set; } = new SKColor(33, 150, 243); // Blue

        /// <summary>
        /// Gets or sets the H-Q curve line width.
        /// </summary>
        public float HQCurveLineWidth { get; set; } = 2.5f;

        /// <summary>
        /// Gets or sets whether to show H-Q curve points.
        /// </summary>
        public bool ShowHQPoints { get; set; } = true;

        // P-Q (Power-Quantity) Curve Colors
        /// <summary>
        /// Gets or sets the P-Q curve color.
        /// </summary>
        public SKColor PQCurveColor { get; set; } = new SKColor(255, 87, 34); // Orange

        /// <summary>
        /// Gets or sets the P-Q curve line width.
        /// </summary>
        public float PQCurveLineWidth { get; set; } = 2.5f;

        /// <summary>
        /// Gets or sets whether to show P-Q curve points.
        /// </summary>
        public bool ShowPQPoints { get; set; } = false;

        // Efficiency Curve Colors
        /// <summary>
        /// Gets or sets the efficiency curve color.
        /// </summary>
        public SKColor EfficiencyCurveColor { get; set; } = new SKColor(76, 175, 80); // Green

        /// <summary>
        /// Gets or sets the efficiency curve line width.
        /// </summary>
        public float EfficiencyCurveLineWidth { get; set; } = 2.5f;

        /// <summary>
        /// Gets or sets whether to show efficiency curve points.
        /// </summary>
        public bool ShowEfficiencyPoints { get; set; } = false;

        // System Curve Colors
        /// <summary>
        /// Gets or sets the system curve color.
        /// </summary>
        public SKColor SystemCurveColor { get; set; } = new SKColor(156, 39, 176); // Purple

        /// <summary>
        /// Gets or sets the system curve line width.
        /// </summary>
        public float SystemCurveLineWidth { get; set; } = 2.5f;

        /// <summary>
        /// Gets or sets the system curve line style (dashed).
        /// </summary>
        public bool SystemCurveDashed { get; set; } = true;

        // Operating Point
        /// <summary>
        /// Gets or sets the operating point color.
        /// </summary>
        public SKColor OperatingPointColor { get; set; } = new SKColor(244, 67, 54); // Red

        /// <summary>
        /// Gets or sets the operating point size.
        /// </summary>
        public float OperatingPointSize { get; set; } = 8f;

        /// <summary>
        /// Gets or sets whether to show operating point label.
        /// </summary>
        public bool ShowOperatingPointLabel { get; set; } = true;

        // Best Efficiency Point (BEP)
        /// <summary>
        /// Gets or sets the BEP point color.
        /// </summary>
        public SKColor BEPPointColor { get; set; } = new SKColor(255, 193, 7); // Amber

        /// <summary>
        /// Gets or sets the BEP point size.
        /// </summary>
        public float BEPPointSize { get; set; } = 8f;

        /// <summary>
        /// Gets or sets whether to show BEP label.
        /// </summary>
        public bool ShowBEPLabel { get; set; } = true;

        // Multi-Pump Configuration
        /// <summary>
        /// Gets or sets colors for multiple pump curves.
        /// </summary>
        public SKColor[] MultiPumpColors { get; set; } = new SKColor[]
        {
            new SKColor(33, 150, 243),   // Blue
            new SKColor(255, 87, 34),    // Orange
            new SKColor(76, 175, 80),     // Green
            new SKColor(156, 39, 176),    // Purple
            new SKColor(244, 67, 54),     // Red
            new SKColor(255, 193, 7),     // Amber
            new SKColor(0, 188, 212),     // Cyan
            new SKColor(233, 30, 99)      // Pink
        };

        /// <summary>
        /// Gets or sets whether to show pump labels.
        /// </summary>
        public bool ShowPumpLabels { get; set; } = true;

        // Affinity Laws (Speed Variation)
        /// <summary>
        /// Gets or sets the affinity law curve color.
        /// </summary>
        public SKColor AffinityLawCurveColor { get; set; } = new SKColor(158, 158, 158); // Gray

        /// <summary>
        /// Gets or sets the affinity law curve line width.
        /// </summary>
        public float AffinityLawCurveLineWidth { get; set; } = 2f;

        /// <summary>
        /// Gets or sets whether affinity law curves are dashed.
        /// </summary>
        public bool AffinityLawCurveDashed { get; set; } = true;

        // NPSH Curves
        /// <summary>
        /// Gets or sets the NPSH required curve color.
        /// </summary>
        public SKColor NPSHRequiredColor { get; set; } = new SKColor(255, 152, 0); // Orange

        /// <summary>
        /// Gets or sets the NPSH available curve color.
        /// </summary>
        public SKColor NPSHAvailableColor { get; set; } = new SKColor(139, 195, 74); // Light Green

        /// <summary>
        /// Gets or sets the NPSH margin fill color.
        /// </summary>
        public SKColor NPSHMarginColor { get; set; } = new SKColor(139, 195, 74, 64); // Light Green with transparency

        // Typography
        /// <summary>
        /// Gets or sets the font size for labels.
        /// </summary>
        public float FontSize { get; set; } = 12f;

        /// <summary>
        /// Gets or sets the title font size.
        /// </summary>
        public float TitleFontSize { get; set; } = 18f;

        /// <summary>
        /// Gets or sets the axis label font size.
        /// </summary>
        public float AxisLabelFontSize { get; set; } = 14f;

        /// <summary>
        /// Gets or sets the tick label font size.
        /// </summary>
        public float TickLabelFontSize { get; set; } = 10f;

        /// <summary>
        /// Gets or sets the text color.
        /// </summary>
        public SKColor TextColor { get; set; } = SKColors.Black;

        /// <summary>
        /// Gets or sets the title color.
        /// </summary>
        public SKColor TitleColor { get; set; } = SKColors.Black;

        // Margins and Spacing
        /// <summary>
        /// Gets or sets the left margin.
        /// </summary>
        public float LeftMargin { get; set; } = 80f;

        /// <summary>
        /// Gets or sets the right margin.
        /// </summary>
        public float RightMargin { get; set; } = 40f;

        /// <summary>
        /// Gets or sets the top margin.
        /// </summary>
        public float TopMargin { get; set; } = 60f;

        /// <summary>
        /// Gets or sets the bottom margin.
        /// </summary>
        public float BottomMargin { get; set; } = 80f;

        // Axis Configuration
        /// <summary>
        /// Gets or sets the number of X-axis ticks.
        /// </summary>
        public int XAxisTickCount { get; set; } = 10;

        /// <summary>
        /// Gets or sets the number of Y-axis ticks.
        /// </summary>
        public int YAxisTickCount { get; set; } = 10;

        /// <summary>
        /// Gets or sets the X-axis label.
        /// </summary>
        public string XAxisLabel { get; set; } = "Flow Rate (GPM)";

        /// <summary>
        /// Gets or sets the Y-axis label.
        /// </summary>
        public string YAxisLabel { get; set; } = "Head (ft)";

        /// <summary>
        /// Gets or sets the plot title.
        /// </summary>
        public string Title { get; set; } = "Pump Performance Curves";

        /// <summary>
        /// Gets or sets whether to show legend.
        /// </summary>
        public bool ShowLegend { get; set; } = true;

        /// <summary>
        /// Gets or sets the legend position.
        /// </summary>
        public LegendPosition LegendPosition { get; set; } = LegendPosition.TopRight;

        /// <summary>
        /// Gets or sets the legend background color.
        /// </summary>
        public SKColor LegendBackgroundColor { get; set; } = new SKColor(255, 255, 255, 230);

        /// <summary>
        /// Gets or sets the legend border color.
        /// </summary>
        public SKColor LegendBorderColor { get; set; } = new SKColor(200, 200, 200);

        // Zoom and Pan
        /// <summary>
        /// Gets or sets the minimum zoom level.
        /// </summary>
        public double MinZoom { get; set; } = 0.1;

        /// <summary>
        /// Gets or sets the maximum zoom level.
        /// </summary>
        public double MaxZoom { get; set; } = 10.0;

        /// <summary>
        /// Gets or sets the default zoom level.
        /// </summary>
        public double DefaultZoom { get; set; } = 1.0;

        // Point Size
        /// <summary>
        /// Gets or sets the default point size.
        /// </summary>
        public float PointSize { get; set; } = 4f;

        // Curve Smoothing
        /// <summary>
        /// Gets or sets whether to use curve smoothing (spline interpolation).
        /// </summary>
        public bool UseCurveSmoothing { get; set; } = true;

        /// <summary>
        /// Gets or sets the smoothing factor (0-1, higher = smoother).
        /// </summary>
        public float SmoothingFactor { get; set; } = 0.5f;

        // Export Settings
        /// <summary>
        /// Gets or sets the export DPI.
        /// </summary>
        public float ExportDPI { get; set; } = 300f;

        /// <summary>
        /// Gets or sets the export background color.
        /// </summary>
        public SKColor ExportBackgroundColor { get; set; } = SKColors.White;
    }

    /// <summary>
    /// Legend position options.
    /// </summary>
    public enum LegendPosition
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }
}


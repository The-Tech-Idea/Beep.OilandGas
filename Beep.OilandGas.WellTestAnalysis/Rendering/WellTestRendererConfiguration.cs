using SkiaSharp;

namespace Beep.OilandGas.WellTestAnalysis.Rendering
{
    /// <summary>
    /// Configuration for well test plot rendering with SkiaSharp.
    /// </summary>
    public class WellTestRendererConfiguration
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

        // Pressure curve
        /// <summary>
        /// Gets or sets the pressure curve color.
        /// </summary>
        public SKColor PressureCurveColor { get; set; } = new SKColor(33, 150, 243); // Blue

        /// <summary>
        /// Gets or sets the pressure curve line width.
        /// </summary>
        public float PressureCurveLineWidth { get; set; } = 2.5f;

        /// <summary>
        /// Gets or sets whether to show pressure points.
        /// </summary>
        public bool ShowPressurePoints { get; set; } = true;

        // Derivative curve
        /// <summary>
        /// Gets or sets the derivative curve color.
        /// </summary>
        public SKColor DerivativeCurveColor { get; set; } = new SKColor(255, 87, 34); // Orange

        /// <summary>
        /// Gets or sets the derivative curve line width.
        /// </summary>
        public float DerivativeCurveLineWidth { get; set; } = 2.5f;

        /// <summary>
        /// Gets or sets whether to show derivative curve.
        /// </summary>
        public bool ShowDerivative { get; set; } = true;

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

        // Margins
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

        // Axis
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
        public string XAxisLabel { get; set; } = "Time (hours)";

        /// <summary>
        /// Gets or sets the Y-axis label.
        /// </summary>
        public string YAxisLabel { get; set; } = "Pressure (psi)";

        /// <summary>
        /// Gets or sets the plot title.
        /// </summary>
        public string Title { get; set; } = "Well Test Analysis";

        /// <summary>
        /// Gets or sets whether to use log scale for X-axis.
        /// </summary>
        public bool UseLogScaleX { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to use log scale for Y-axis.
        /// </summary>
        public bool UseLogScaleY { get; set; } = false;

        /// <summary>
        /// Gets or sets the plot type.
        /// </summary>
        public WellTestPlotType PlotType { get; set; } = WellTestPlotType.LogLog;

        /// <summary>
        /// Gets or sets the point size.
        /// </summary>
        public float PointSize { get; set; } = 4f;

        /// <summary>
        /// Gets or sets whether to show legend.
        /// </summary>
        public bool ShowLegend { get; set; } = true;

        /// <summary>
        /// Gets or sets the zoom limits.
        /// </summary>
        public double MinZoom { get; set; } = 0.1;

        /// <summary>
        /// Gets or sets the maximum zoom level.
        /// </summary>
        public double MaxZoom { get; set; } = 10.0;

        /// <summary>
        /// Gets or sets the export DPI.
        /// </summary>
        public float ExportDPI { get; set; } = 300f;
    }

    /// <summary>
    /// Well test plot type enumeration.
    /// </summary>
    public enum WellTestPlotType
    {
        /// <summary>
        /// Log-log plot (log time vs log pressure/derivative).
        /// </summary>
        LogLog,

        /// <summary>
        /// Semi-log plot (log time vs linear pressure).
        /// </summary>
        SemiLog,

        /// <summary>
        /// Linear plot (linear time vs linear pressure).
        /// </summary>
        Linear
    }
}


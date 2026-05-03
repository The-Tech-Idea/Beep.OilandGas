using SkiaSharp;

namespace Beep.OilandGas.DCA.Rendering
{
    /// <summary>
    /// Configuration for DCA plot rendering with SkiaSharp.
    /// </summary>
    public class DCARendererConfiguration
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

        /// <summary>
        /// Gets or sets the observed data point color.
        /// </summary>
        public SKColor ObservedColor { get; set; } = new SKColor(33, 150, 243); // Blue

        /// <summary>
        /// Gets or sets the predicted curve color.
        /// </summary>
        public SKColor PredictedColor { get; set; } = new SKColor(255, 87, 34); // Red

        /// <summary>
        /// Gets or sets the forecast curve color.
        /// </summary>
        public SKColor ForecastColor { get; set; } = new SKColor(255, 152, 0); // Orange

        /// <summary>
        /// Gets or sets the confidence interval fill color.
        /// </summary>
        public SKColor ConfidenceIntervalColor { get; set; } = new SKColor(255, 87, 34, 32); // Red with transparency

        /// <summary>
        /// Gets or sets the confidence interval border color.
        /// </summary>
        public SKColor ConfidenceIntervalBorderColor { get; set; } = new SKColor(255, 87, 34, 128);

        /// <summary>
        /// Gets or sets the point size for scatter plots.
        /// </summary>
        public float PointSize { get; set; } = 4f;

        /// <summary>
        /// Gets or sets the line width for curves.
        /// </summary>
        public float LineWidth { get; set; } = 2f;

        /// <summary>
        /// Gets or sets the font size for labels.
        /// </summary>
        public float FontSize { get; set; } = 12f;

        /// <summary>
        /// Gets or sets the title font size.
        /// </summary>
        public float TitleFontSize { get; set; } = 16f;

        /// <summary>
        /// Gets or sets the axis label font size.
        /// </summary>
        public float AxisLabelFontSize { get; set; } = 12f;

        /// <summary>
        /// Gets or sets the tick label font size.
        /// </summary>
        public float TickLabelFontSize { get; set; } = 10f;

        /// <summary>
        /// Gets or sets the text color.
        /// </summary>
        public SKColor TextColor { get; set; } = SKColors.Black;

        /// <summary>
        /// Gets or sets the top margin.
        /// </summary>
        public float TopMargin { get; set; } = 50f;

        /// <summary>
        /// Gets or sets the bottom margin.
        /// </summary>
        public float BottomMargin { get; set; } = 50f;

        /// <summary>
        /// Gets or sets the left margin.
        /// </summary>
        public float LeftMargin { get; set; } = 60f;

        /// <summary>
        /// Gets or sets the right margin.
        /// </summary>
        public float RightMargin { get; set; } = 40f;

        /// <summary>
        /// Gets or sets whether to show legend.
        /// </summary>
        public bool ShowLegend { get; set; } = true;

        /// <summary>
        /// Gets or sets the legend position.
        /// </summary>
        public LegendPosition LegendPosition { get; set; } = LegendPosition.TopRight;

        /// <summary>
        /// Gets or sets whether to use logarithmic scale for Y-axis.
        /// </summary>
        public bool UseLogScale { get; set; } = false;

        /// <summary>
        /// Gets or sets whether to show prediction intervals.
        /// </summary>
        public bool ShowPredictionIntervals { get; set; } = false;

        /// <summary>
        /// Gets or sets whether to show observed points.
        /// </summary>
        public bool ShowObservedPoints { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to show predicted curve.
        /// </summary>
        public bool ShowPredictedCurve { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to show forecast curve.
        /// </summary>
        public bool ShowForecastCurve { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to show axis labels.
        /// </summary>
        public bool ShowAxisLabels { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to show tick marks.
        /// </summary>
        public bool ShowTicks { get; set; } = true;

        /// <summary>
        /// Gets or sets the number of X-axis ticks.
        /// </summary>
        public int XAxisTickCount { get; set; } = 10;

        /// <summary>
        /// Gets or sets the number of Y-axis ticks.
        /// </summary>
        public int YAxisTickCount { get; set; } = 10;

        /// <summary>
        /// Gets or sets whether to enable zoom.
        /// </summary>
        public bool EnableZoom { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to enable pan.
        /// </summary>
        public bool EnablePan { get; set; } = true;

        /// <summary>
        /// Gets or sets the minimum zoom level.
        /// </summary>
        public double MinZoom { get; set; } = 0.1;

        /// <summary>
        /// Gets or sets the maximum zoom level.
        /// </summary>
        public double MaxZoom { get; set; } = 10.0;

        /// <summary>
        /// Gets or sets the zoom sensitivity.
        /// </summary>
        public double ZoomSensitivity { get; set; } = 0.1;

        /// <summary>
        /// Gets or sets whether to show tooltips on hover.
        /// </summary>
        public bool ShowTooltips { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to show crosshair.
        /// </summary>
        public bool ShowCrosshair { get; set; } = false;

        /// <summary>
        /// Gets or sets the crosshair color.
        /// </summary>
        public SKColor CrosshairColor { get; set; } = SKColors.Red;

        /// <summary>
        /// Gets or sets the crosshair line width.
        /// </summary>
        public float CrosshairLineWidth { get; set; } = 1f;
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


using System;

namespace Beep.OilandGas.DCA.Visualization
{
    /// <summary>
    /// Configuration options for plot styling and appearance.
    /// </summary>
    public class PlotConfiguration
    {
        /// <summary>
        /// Gets or sets the plot width in pixels.
        /// </summary>
        public int Width { get; set; } = 800;

        /// <summary>
        /// Gets or sets the plot height in pixels.
        /// </summary>
        public int Height { get; set; } = 600;

        /// <summary>
        /// Gets or sets whether to use logarithmic scale for Y-axis.
        /// </summary>
        public bool UseLogScale { get; set; } = false;

        /// <summary>
        /// Gets or sets whether to show grid lines.
        /// </summary>
        public bool ShowGrid { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to show legend.
        /// </summary>
        public bool ShowLegend { get; set; } = true;

        /// <summary>
        /// Gets or sets the background color (as hex string, e.g., "#FFFFFF").
        /// </summary>
        public string BackgroundColor { get; set; } = "#FFFFFF";

        /// <summary>
        /// Gets or sets the grid color (as hex string).
        /// </summary>
        public string GridColor { get; set; } = "#E0E0E0";

        /// <summary>
        /// Gets or sets the observed data point color (as hex string).
        /// </summary>
        public string ObservedColor { get; set; } = "#2196F3"; // Blue

        /// <summary>
        /// Gets or sets the predicted curve color (as hex string).
        /// </summary>
        public string PredictedColor { get; set; } = "#FF5722"; // Red

        /// <summary>
        /// Gets or sets the forecast curve color (as hex string).
        /// </summary>
        public string ForecastColor { get; set; } = "#FF9800"; // Orange

        /// <summary>
        /// Gets or sets the confidence interval fill color (as hex string with alpha).
        /// </summary>
        public string ConfidenceIntervalColor { get; set; } = "#FF572220"; // Red with transparency

        /// <summary>
        /// Gets or sets the point size for scatter plots.
        /// </summary>
        public double PointSize { get; set; } = 4.0;

        /// <summary>
        /// Gets or sets the line width for curves.
        /// </summary>
        public double LineWidth { get; set; } = 2.0;

        /// <summary>
        /// Gets or sets the font size for labels.
        /// </summary>
        public double FontSize { get; set; } = 12.0;

        /// <summary>
        /// Gets or sets the font family.
        /// </summary>
        public string FontFamily { get; set; } = "Arial";

        /// <summary>
        /// Gets or sets the margin around the plot (in pixels).
        /// </summary>
        public PlotMargins Margins { get; set; } = new PlotMargins();

        /// <summary>
        /// Gets or sets whether to show prediction intervals.
        /// </summary>
        public bool ShowPredictionIntervals { get; set; } = false;

        /// <summary>
        /// Gets or sets the confidence level for prediction intervals (0-1).
        /// </summary>
        public double PredictionIntervalConfidence { get; set; } = 0.95;
    }

    /// <summary>
    /// Represents margins around a plot.
    /// </summary>
    public class PlotMargins
    {
        /// <summary>
        /// Gets or sets the top margin in pixels.
        /// </summary>
        public int Top { get; set; } = 50;

        /// <summary>
        /// Gets or sets the bottom margin in pixels.
        /// </summary>
        public int Bottom { get; set; } = 50;

        /// <summary>
        /// Gets or sets the left margin in pixels.
        /// </summary>
        public int Left { get; set; } = 60;

        /// <summary>
        /// Gets or sets the right margin in pixels.
        /// </summary>
        public int Right { get; set; } = 40;
    }
}


using System;
using System.Collections.Generic;
using SkiaSharp;

namespace Beep.OilandGas.HeatMap.Annotations
{
    /// <summary>
    /// Configuration for axis tick marks and labels.
    /// </summary>
    public class AxisTicksConfiguration
    {
        /// <summary>
        /// Gets or sets whether to show tick marks.
        /// </summary>
        public bool ShowTicks { get; set; } = true;

        /// <summary>
        /// Gets or sets the tick mark length.
        /// </summary>
        public float TickLength { get; set; } = 5f;

        /// <summary>
        /// Gets or sets the tick mark color.
        /// </summary>
        public SKColor TickColor { get; set; } = SKColors.Black;

        /// <summary>
        /// Gets or sets the tick mark line width.
        /// </summary>
        public float TickLineWidth { get; set; } = 1f;

        /// <summary>
        /// Gets or sets whether to show tick labels.
        /// </summary>
        public bool ShowTickLabels { get; set; } = true;

        /// <summary>
        /// Gets or sets the number of tick intervals (0 = auto-calculate).
        /// </summary>
        public int TickCount { get; set; } = 0; // 0 = auto

        /// <summary>
        /// Gets or sets the tick interval (0 = auto-calculate).
        /// </summary>
        public double TickInterval { get; set; } = 0; // 0 = auto

        /// <summary>
        /// Gets or sets the tick label font size.
        /// </summary>
        public float LabelFontSize { get; set; } = 10f;

        /// <summary>
        /// Gets or sets the tick label color.
        /// </summary>
        public SKColor LabelColor { get; set; } = SKColors.Black;

        /// <summary>
        /// Gets or sets the tick label format string.
        /// </summary>
        public string LabelFormat { get; set; } = "F1";

        /// <summary>
        /// Gets or sets the margin from axis (in pixels).
        /// </summary>
        public float Margin { get; set; } = 5f;

        /// <summary>
        /// Gets or sets whether to use logarithmic scale.
        /// </summary>
        public bool UseLogScale { get; set; } = false;
    }

    /// <summary>
    /// Renders axis tick marks and labels for heat maps.
    /// </summary>
    public static class AxisTicks
    {
        /// <summary>
        /// Renders X-axis ticks and labels.
        /// </summary>
        public static void RenderXAxisTicks(
            SKCanvas canvas,
            float width,
            float height,
            double minX,
            double maxX,
            AxisTicksConfiguration config)
        {
            if (!config.ShowTicks && !config.ShowTickLabels)
                return;

            var tickPaint = new SKPaint
            {
                Color = config.TickColor,
                StrokeWidth = config.TickLineWidth,
                Style = SKPaintStyle.Stroke,
                IsAntialias = true
            };

            var labelPaint = new SKPaint
            {
                Color = config.LabelColor,
                TextSize = config.LabelFontSize,
                TextAlign = SKTextAlign.Center,
                IsAntialias = true
            };

            // Calculate tick positions
            var tickPositions = CalculateTickPositions(minX, maxX, config);

            float y = height - config.Margin;

            foreach (var tickValue in tickPositions)
            {
                // Convert data coordinate to screen coordinate
                float x = (float)((tickValue - minX) / (maxX - minX) * width);

                // Draw tick mark
                if (config.ShowTicks)
                {
                    canvas.DrawLine(x, y, x, y + config.TickLength, tickPaint);
                }

                // Draw label
                if (config.ShowTickLabels)
                {
                    string label = tickValue.ToString(config.LabelFormat);
                    canvas.DrawText(label, x, y + config.TickLength + config.LabelFontSize + 2, labelPaint);
                }
            }
        }

        /// <summary>
        /// Renders Y-axis ticks and labels.
        /// </summary>
        public static void RenderYAxisTicks(
            SKCanvas canvas,
            float width,
            float height,
            double minY,
            double maxY,
            AxisTicksConfiguration config)
        {
            if (!config.ShowTicks && !config.ShowTickLabels)
                return;

            var tickPaint = new SKPaint
            {
                Color = config.TickColor,
                StrokeWidth = config.TickLineWidth,
                Style = SKPaintStyle.Stroke,
                IsAntialias = true
            };

            var labelPaint = new SKPaint
            {
                Color = config.LabelColor,
                TextSize = config.LabelFontSize,
                TextAlign = SKTextAlign.Right,
                IsAntialias = true
            };

            // Calculate tick positions
            var tickPositions = CalculateTickPositions(minY, maxY, config);

            float x = config.Margin;

            foreach (var tickValue in tickPositions)
            {
                // Convert data coordinate to screen coordinate (inverted Y)
                float y = height - (float)((tickValue - minY) / (maxY - minY) * height);

                // Draw tick mark
                if (config.ShowTicks)
                {
                    canvas.DrawLine(x, y, x + config.TickLength, y, tickPaint);
                }

                // Draw label
                if (config.ShowTickLabels)
                {
                    canvas.Save();
                    canvas.Translate(x - 5, y);
                    string label = tickValue.ToString(config.LabelFormat);
                    canvas.DrawText(label, 0, config.LabelFontSize / 3, labelPaint);
                    canvas.Restore();
                }
            }
        }

        /// <summary>
        /// Calculates tick positions based on configuration.
        /// </summary>
        private static List<double> CalculateTickPositions(double min, double max, AxisTicksConfiguration config)
        {
            var positions = new List<double>();

            if (config.UseLogScale)
            {
                // Logarithmic scale
                double logMin = Math.Log10(Math.Max(min, 1e-10));
                double logMax = Math.Log10(Math.Max(max, 1e-10));
                int tickCount = config.TickCount > 0 ? config.TickCount : 5;

                for (int i = 0; i <= tickCount; i++)
                {
                    double logValue = logMin + (logMax - logMin) * i / tickCount;
                    positions.Add(Math.Pow(10, logValue));
                }
            }
            else
            {
                // Linear scale
                if (config.TickInterval > 0)
                {
                    // Use specified interval
                    double start = Math.Ceiling(min / config.TickInterval) * config.TickInterval;
                    for (double value = start; value <= max; value += config.TickInterval)
                    {
                        positions.Add(value);
                    }
                }
                else
                {
                    // Auto-calculate
                    int tickCount = config.TickCount > 0 ? config.TickCount : CalculateOptimalTickCount(min, max);
                    double interval = (max - min) / tickCount;

                    // Round interval to nice number
                    interval = RoundToNiceNumber(interval);

                    double start = Math.Ceiling(min / interval) * interval;
                    for (double value = start; value <= max; value += interval)
                    {
                        positions.Add(value);
                    }
                }
            }

            return positions;
        }

        /// <summary>
        /// Calculates optimal number of ticks.
        /// </summary>
        private static int CalculateOptimalTickCount(double min, double max)
        {
            double range = max - min;
            if (range <= 0) return 5;

            // Aim for 5-10 ticks
            double roughInterval = range / 7;
            double niceInterval = RoundToNiceNumber(roughInterval);
            int count = (int)Math.Ceiling(range / niceInterval);

            return Math.Max(3, Math.Min(10, count));
        }

        /// <summary>
        /// Rounds a number to a "nice" interval (1, 2, 5, 10, 20, 50, 100, etc.).
        /// </summary>
        private static double RoundToNiceNumber(double value)
        {
            if (value <= 0) return 1;

            double magnitude = Math.Pow(10, Math.Floor(Math.Log10(value)));
            double normalized = value / magnitude;

            double nice;
            if (normalized <= 1)
                nice = 1;
            else if (normalized <= 2)
                nice = 2;
            else if (normalized <= 5)
                nice = 5;
            else
                nice = 10;

            return nice * magnitude;
        }
    }
}


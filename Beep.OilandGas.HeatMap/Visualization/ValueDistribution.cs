using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;

namespace Beep.OilandGas.HeatMap.Visualization
{
    /// <summary>
    /// Represents a histogram bin.
    /// </summary>
    public class HistogramBin
    {
        /// <summary>
        /// Gets or sets the bin start value.
        /// </summary>
        public double Start { get; set; }

        /// <summary>
        /// Gets or sets the bin end value.
        /// </summary>
        public double End { get; set; }

        /// <summary>
        /// Gets or sets the count of values in this bin.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Gets or sets the normalized count (0-1).
        /// </summary>
        public double NormalizedCount { get; set; }
    }

    /// <summary>
    /// Provides value distribution visualization (histogram, distribution curve).
    /// </summary>
    public static class ValueDistribution
    {
        /// <summary>
        /// Creates a histogram from data point values.
        /// </summary>
        /// <param name="dataPoints">The data points.</param>
        /// <param name="numBins">Number of bins (default: auto-calculated using Sturges' rule).</param>
        /// <param name="minValue">Minimum value (optional, auto-calculated if not provided).</param>
        /// <param name="maxValue">Maximum value (optional, auto-calculated if not provided).</param>
        /// <returns>List of histogram bins.</returns>
        public static List<HistogramBin> CreateHistogram(
            List<HEAT_MAP_DATA_POINT> dataPoints,
            int? numBins = null,
            double? minValue = null,
            double? maxValue = null)
        {
            if (dataPoints == null || dataPoints.Count == 0)
                return new List<HistogramBin>();

            // Calculate bounds if not provided
            if (!minValue.HasValue || !maxValue.HasValue)
            {
                minValue = dataPoints.Min(p => p.Value);
                maxValue = dataPoints.Max(p => p.Value);
            }

            // Calculate number of bins using Sturges' rule if not provided
            if (!numBins.HasValue)
            {
                numBins = (int)Math.Ceiling(Math.Log2(dataPoints.Count) + 1);
                numBins = Math.Max(5, Math.Min(50, numBins.Value)); // Clamp between 5 and 50
            }

            double binWidth = (maxValue.Value - minValue.Value) / numBins.Value;
            var bins = new List<HistogramBin>();

            // Initialize bins
            for (int i = 0; i < numBins.Value; i++)
            {
                bins.Add(new HistogramBin
                {
                    Start = minValue.Value + i * binWidth,
                    End = minValue.Value + (i + 1) * binWidth,
                    Count = 0
                });
            }

            // Count values in each bin
            foreach (var point in dataPoints)
            {
                int binIndex = (int)Math.Floor((point.Value - minValue.Value) / binWidth);
                binIndex = Math.Max(0, Math.Min(numBins.Value - 1, binIndex));
                bins[binIndex].Count++;
            }

            // Normalize counts
            int maxCount = bins.Max(b => b.Count);
            if (maxCount > 0)
            {
                foreach (var bin in bins)
                {
                    bin.NormalizedCount = (double)bin.Count / maxCount;
                }
            }

            return bins;
        }

        /// <summary>
        /// Calculates a smooth distribution curve using kernel density estimation.
        /// </summary>
        /// <param name="dataPoints">The data points.</param>
        /// <param name="numPoints">Number of points in the curve.</param>
        /// <param name="bandwidth">Bandwidth for KDE (optional, auto-calculated if not provided).</param>
        /// <returns>List of (value, density) points.</returns>
        public static List<(double value, double density)> CreateDistributionCurve(
            List<HEAT_MAP_DATA_POINT> dataPoints,
            int numPoints = 100,
            double? bandwidth = null)
        {
            if (dataPoints == null || dataPoints.Count == 0)
                return new List<(double, double)>();

            var values = dataPoints.Select(p => p.Value).ToList();
            double minValue = values.Min();
            double maxValue = values.Max();
            double range = maxValue - minValue;

            // Auto-calculate bandwidth if not provided
            if (!bandwidth.HasValue)
            {
                double stdDev = CalculateStandardDeviation(values);
                bandwidth = 1.06 * stdDev * Math.Pow(values.Count, -0.2); // Silverman's rule
            }

            var curve = new List<(double, double)>();
            double step = range / (numPoints - 1);

            for (int i = 0; i < numPoints; i++)
            {
                double value = minValue + i * step;
                double density = 0;

                foreach (var v in values)
                {
                    double u = (value - v) / bandwidth.Value;
                    density += Math.Exp(-0.5 * u * u) / (bandwidth.Value * Math.Sqrt(2 * Math.PI));
                }

                density /= values.Count;
                curve.Add((value, density));
            }

            return curve;
        }

        /// <summary>
        /// Calculates standard deviation of values.
        /// </summary>
        private static double CalculateStandardDeviation(List<double> values)
        {
            if (values.Count == 0)
                return 0;

            double mean = values.Average();
            double variance = values.Sum(v => Math.Pow(v - mean, 2)) / values.Count;
            return Math.Sqrt(variance);
        }

        /// <summary>
        /// Renders a histogram on the canvas.
        /// </summary>
        public static void RenderHistogram(
            SKCanvas canvas,
            List<HistogramBin> bins,
            float x, float y,
            float width, float height,
            SKColor barColor,
            SKColor borderColor,
            bool showLabels = true)
        {
            if (bins == null || bins.Count == 0)
                return;

            float barWidth = width / bins.Count;
            float maxHeight = height * 0.9f; // Leave 10% for labels

            using (var barPaint = new SKPaint
            {
                Color = barColor,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            })
            using (var borderPaint = new SKPaint
            {
                Color = borderColor,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1f,
                IsAntialias = true
            })
            {
                for (int i = 0; i < bins.Count; i++)
                {
                    var bin = bins[i];
                    float barHeight = (float)(bin.NormalizedCount * maxHeight);
                    float barX = x + i * barWidth;
                    float barY = y + maxHeight - barHeight;

                    var rect = new SKRect(barX, barY, barX + barWidth, y + maxHeight);
                    canvas.DrawRect(rect, barPaint);
                    canvas.DrawRect(rect, borderPaint);

                    // Draw label
                    if (showLabels && i % Math.Max(1, bins.Count / 10) == 0)
                    {
                        using (var textPaint = new SKPaint
                        {
                            Color = SKColors.Black,
                            TextSize = 8f,
                            TextAlign = SKTextAlign.Center,
                            IsAntialias = true
                        })
                        {
                            string label = $"{bin.Start:F1}";
                            canvas.DrawText(label, barX + barWidth / 2, y + maxHeight + 12, textPaint);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Renders a distribution curve on the canvas.
        /// </summary>
        public static void RenderDistributionCurve(
            SKCanvas canvas,
            List<(double value, double density)> curve,
            float x, float y,
            float width, float height,
            SKColor curveColor,
            float lineWidth = 2f,
            bool fillArea = false)
        {
            if (curve == null || curve.Count < 2)
                return;

            double minValue = curve.Min(p => p.value);
            double maxValue = curve.Max(p => p.value);
            double maxDensity = curve.Max(p => p.density);

            if (maxDensity <= 0)
                return;

            using (var path = new SKPath())
            {
                float scaleX = width / (float)(maxValue - minValue);
                float scaleY = height * 0.9f / (float)maxDensity; // Leave 10% margin

                // Start path
                float firstX = x + (float)((curve[0].value - minValue) * scaleX);
                float firstY = y + height - (float)(curve[0].density * scaleY);
                path.MoveTo(firstX, firstY);

                // Add curve points
                for (int i = 1; i < curve.Count; i++)
                {
                    float px = x + (float)((curve[i].value - minValue) * scaleX);
                    float py = y + height - (float)(curve[i].density * scaleY);
                    path.LineTo(px, py);
                }

                // Close path for fill
                if (fillArea)
                {
                    path.LineTo(x + width, y + height);
                    path.LineTo(x, y + height);
                    path.Close();
                }

                // Draw
                if (fillArea)
                {
                    using (var fillPaint = new SKPaint
                    {
                        Color = new SKColor(curveColor.Red, curveColor.Green, curveColor.Blue, 100),
                        Style = SKPaintStyle.Fill,
                        IsAntialias = true
                    })
                    {
                        canvas.DrawPath(path, fillPaint);
                    }
                }

                using (var strokePaint = new SKPaint
                {
                    Color = curveColor,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = lineWidth,
                    IsAntialias = true
                })
                {
                    canvas.DrawPath(path, strokePaint);
                }
            }
        }
    }
}


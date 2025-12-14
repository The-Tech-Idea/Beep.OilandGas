using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;

namespace Beep.OilandGas.HeatMap.Statistics
{
    /// <summary>
    /// Provides statistical analysis and overlay visualization for heat maps.
    /// </summary>
    public static class StatisticalOverlays
    {
        /// <summary>
        /// Calculates statistical contours based on value distribution.
        /// </summary>
        /// <param name="dataPoints">List of data points.</param>
        /// <param name="contourLevels">Number of contour levels to generate.</param>
        /// <returns>Dictionary mapping contour level to list of points at that level.</returns>
        public static Dictionary<double, List<HeatMapDataPoint>> CalculateStatisticalContours(
            List<HeatMapDataPoint> dataPoints,
            int contourLevels = 5)
        {
            if (dataPoints == null || dataPoints.Count == 0)
                return new Dictionary<double, List<HeatMapDataPoint>>();

            double minValue = dataPoints.Min(p => p.Value);
            double maxValue = dataPoints.Max(p => p.Value);
            double range = maxValue - minValue;

            if (range <= 0)
                return new Dictionary<double, List<HeatMapDataPoint>>();

            var contours = new Dictionary<double, List<HeatMapDataPoint>>();

            for (int i = 0; i <= contourLevels; i++)
            {
                double level = minValue + (range * i / contourLevels);
                var pointsAtLevel = dataPoints.Where(p =>
                    Math.Abs(p.Value - level) < (range / contourLevels / 2.0)).ToList();

                if (pointsAtLevel.Count > 0)
                {
                    contours[level] = pointsAtLevel;
                }
            }

            return contours;
        }

        /// <summary>
        /// Identifies outliers using the IQR (Interquartile Range) method.
        /// </summary>
        /// <param name="dataPoints">List of data points.</param>
        /// <param name="outlierFactor">Factor for outlier detection (default: 1.5).</param>
        /// <returns>List of outlier points.</returns>
        public static List<HeatMapDataPoint> IdentifyOutliers(
            List<HeatMapDataPoint> dataPoints,
            double outlierFactor = 1.5)
        {
            if (dataPoints == null || dataPoints.Count < 4)
                return new List<HeatMapDataPoint>();

            var values = dataPoints.Select(p => p.Value).OrderBy(v => v).ToList();
            int n = values.Count;

            // Calculate quartiles
            double q1 = CalculatePercentile(values, 25);
            double q3 = CalculatePercentile(values, 75);
            double iqr = q3 - q1;

            // Calculate outlier bounds
            double lowerBound = q1 - outlierFactor * iqr;
            double upperBound = q3 + outlierFactor * iqr;

            // Find outliers
            return dataPoints.Where(p =>
                p.Value < lowerBound || p.Value > upperBound).ToList();
        }

        /// <summary>
        /// Calculates confidence intervals for point values.
        /// </summary>
        /// <param name="dataPoints">List of data points.</param>
        /// <param name="confidenceLevel">Confidence level (0.0 to 1.0, default: 0.95).</param>
        /// <returns>Tuple containing (mean, lowerBound, upperBound).</returns>
        public static (double mean, double lowerBound, double upperBound) CalculateConfidenceInterval(
            List<HeatMapDataPoint> dataPoints,
            double confidenceLevel = 0.95)
        {
            if (dataPoints == null || dataPoints.Count == 0)
                return (0, 0, 0);

            var values = dataPoints.Select(p => p.Value).ToList();
            double mean = values.Average();
            double stdDev = CalculateStandardDeviation(values, mean);

            int n = values.Count;
            if (n < 2)
                return (mean, mean, mean);

            // Use t-distribution for small samples, normal for large
            double tValue = GetTValue(confidenceLevel, n - 1);
            double margin = tValue * stdDev / Math.Sqrt(n);

            return (mean, mean - margin, mean + margin);
        }

        /// <summary>
        /// Generates a histogram of value distribution.
        /// </summary>
        /// <param name="dataPoints">List of data points.</param>
        /// <param name="bins">Number of bins in the histogram.</param>
        /// <returns>Dictionary mapping bin center to count.</returns>
        public static Dictionary<double, int> GenerateHistogram(
            List<HeatMapDataPoint> dataPoints,
            int bins = 20)
        {
            if (dataPoints == null || dataPoints.Count == 0)
                return new Dictionary<double, int>();

            double minValue = dataPoints.Min(p => p.Value);
            double maxValue = dataPoints.Max(p => p.Value);
            double binWidth = (maxValue - minValue) / bins;

            if (binWidth <= 0)
                return new Dictionary<double, int>();

            var histogram = new Dictionary<double, int>();

            for (int i = 0; i < bins; i++)
            {
                double binCenter = minValue + (i + 0.5) * binWidth;
                double binMin = minValue + i * binWidth;
                double binMax = minValue + (i + 1) * binWidth;

                int count = dataPoints.Count(p =>
                    p.Value >= binMin && (i == bins - 1 ? p.Value <= binMax : p.Value < binMax));

                histogram[binCenter] = count;
            }

            return histogram;
        }

        /// <summary>
        /// Calculates the percentile of a sorted list.
        /// </summary>
        private static double CalculatePercentile(List<double> sortedValues, double percentile)
        {
            if (sortedValues == null || sortedValues.Count == 0)
                return 0;

            double index = (percentile / 100.0) * (sortedValues.Count - 1);
            int lowerIndex = (int)Math.Floor(index);
            int upperIndex = (int)Math.Ceiling(index);

            if (lowerIndex == upperIndex)
                return sortedValues[lowerIndex];

            double weight = index - lowerIndex;
            return sortedValues[lowerIndex] * (1 - weight) + sortedValues[upperIndex] * weight;
        }

        /// <summary>
        /// Calculates the standard deviation.
        /// </summary>
        private static double CalculateStandardDeviation(List<double> values, double mean)
        {
            if (values == null || values.Count < 2)
                return 0;

            double sumSquaredDifferences = values.Sum(v => Math.Pow(v - mean, 2));
            return Math.Sqrt(sumSquaredDifferences / (values.Count - 1));
        }

        /// <summary>
        /// Gets t-value for confidence interval calculation (simplified).
        /// </summary>
        private static double GetTValue(double confidenceLevel, int degreesOfFreedom)
        {
            // Simplified t-value lookup (for common confidence levels)
            // For production, use a proper t-distribution table or library
            if (degreesOfFreedom >= 30)
            {
                // Use normal distribution approximation
                return confidenceLevel switch
                {
                    0.90 => 1.645,
                    0.95 => 1.960,
                    0.99 => 2.576,
                    _ => 1.960
                };
            }
            else
            {
                // Simplified t-values for small samples
                return confidenceLevel switch
                {
                    0.90 => 1.645 + (30 - degreesOfFreedom) * 0.1,
                    0.95 => 1.960 + (30 - degreesOfFreedom) * 0.15,
                    0.99 => 2.576 + (30 - degreesOfFreedom) * 0.2,
                    _ => 1.960 + (30 - degreesOfFreedom) * 0.15
                };
            }
        }
    }

    /// <summary>
    /// Provides rendering methods for statistical overlays.
    /// </summary>
    public static class StatisticalOverlayRenderer
    {
        /// <summary>
        /// Draws outlier points highlighted on the canvas.
        /// </summary>
        /// <param name="canvas">The canvas to draw on.</param>
        /// <param name="outliers">List of outlier points.</param>
        /// <param name="zoom">Current zoom level.</param>
        /// <param name="panOffset">Current pan offset.</param>
        /// <param name="highlightColor">Color for highlighting outliers.</param>
        public static void DrawOutliers(
            SKCanvas canvas,
            List<HeatMapDataPoint> outliers,
            double zoom,
            SKPoint panOffset,
            SKColor highlightColor = default)
        {
            if (outliers == null || outliers.Count == 0)
                return;

            if (highlightColor == default)
                highlightColor = SKColors.Red;

            var paint = new SKPaint
            {
                Color = highlightColor,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 3f,
                IsAntialias = true
            };

            foreach (var outlier in outliers)
            {
                float x = (float)(outlier.X * zoom + panOffset.X);
                float y = (float)(outlier.Y * zoom + panOffset.Y);

                // Draw a circle around the outlier
                canvas.DrawCircle(x, y, 15f, paint);
            }
        }

        /// <summary>
        /// Draws confidence interval bands on the canvas.
        /// </summary>
        /// <param name="canvas">The canvas to draw on.</param>
        /// <param name="dataPoints">List of data points.</param>
        /// <param name="confidenceInterval">Confidence interval tuple (mean, lower, upper).</param>
        /// <param name="zoom">Current zoom level.</param>
        /// <param name="panOffset">Current pan offset.</param>
        public static void DrawConfidenceInterval(
            SKCanvas canvas,
            List<HeatMapDataPoint> dataPoints,
            (double mean, double lowerBound, double upperBound) confidenceInterval,
            double zoom,
            SKPoint panOffset)
        {
            if (dataPoints == null || dataPoints.Count == 0)
                return;

            // Draw points within confidence interval in one color
            // Draw points outside in another color
            var withinInterval = dataPoints.Where(p =>
                p.Value >= confidenceInterval.lowerBound &&
                p.Value <= confidenceInterval.upperBound).ToList();

            var outsideInterval = dataPoints.Where(p =>
                p.Value < confidenceInterval.lowerBound ||
                p.Value > confidenceInterval.upperBound).ToList();

            // Draw points within interval (normal color)
            var normalPaint = new SKPaint { Color = SKColors.Blue, IsAntialias = true };
            foreach (var point in withinInterval)
            {
                float x = (float)(point.X * zoom + panOffset.X);
                float y = (float)(point.Y * zoom + panOffset.Y);
                canvas.DrawCircle(x, y, 5f, normalPaint);
            }

            // Draw points outside interval (highlighted)
            var highlightPaint = new SKPaint { Color = SKColors.Orange, IsAntialias = true };
            foreach (var point in outsideInterval)
            {
                float x = (float)(point.X * zoom + panOffset.X);
                float y = (float)(point.Y * zoom + panOffset.Y);
                canvas.DrawCircle(x, y, 5f, highlightPaint);
            }
        }
    }
}


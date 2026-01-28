using System;
using System.Collections.Generic;
using System.Linq;

namespace Beep.OilandGas.HeatMap.Analysis
{
    /// <summary>
    /// Provides statistical analysis and summary for heat map data.
    /// </summary>
    public static class DataAnalysis
    {
        /// <summary>
        /// Calculates comprehensive statistical summary for data points.
        /// </summary>
        /// <param name="dataPoints">List of data points to analyze.</param>
        /// <returns>Statistical summary object.</returns>
        public static StatisticalSummary CalculateSummary(List<HEAT_MAP_DATA_POINT> dataPoints)
        {
            if (dataPoints == null || dataPoints.Count == 0)
                return new StatisticalSummary();

            var values = dataPoints.Select(p => p.Value).ToList();
            var xValues = dataPoints.Select(p => p.X).ToList();
            var yValues = dataPoints.Select(p => p.Y).ToList();

            return new StatisticalSummary
            {
                Count = dataPoints.Count,
                MinValue = values.Min(),
                MaxValue = values.Max(),
                MeanValue = values.Average(),
                MedianValue = CalculateMedian(values),
                StandardDeviation = CalculateStandardDeviation(values),
                Variance = CalculateVariance(values),
                MinX = xValues.Min(),
                MaxX = xValues.Max(),
                MeanX = xValues.Average(),
                MinY = yValues.Min(),
                MaxY = yValues.Max(),
                MeanY = yValues.Average(),
                RangeX = xValues.Max() - xValues.Min(),
                RangeY = yValues.Max() - yValues.Min()
            };
        }

        /// <summary>
        /// Calculates correlation between two sets of data points.
        /// </summary>
        /// <param name="points1">First set of data points.</param>
        /// <param name="points2">Second set of data points.</param>
        /// <param name="matchRadius">Radius for matching points between sets.</param>
        /// <returns>Correlation coefficient (-1 to 1), or null if insufficient matches.</returns>
        public static double? CalculateCorrelation(
            List<HEAT_MAP_DATA_POINT> points1,
            List<HEAT_MAP_DATA_POINT> points2,
            double matchRadius = 10.0)
        {
            if (points1 == null || points2 == null || points1.Count == 0 || points2.Count == 0)
                return null;

            var matchedPairs = new List<(double value1, double value2)>();

            foreach (var p1 in points1)
            {
                var nearest = points2
                    .OrderBy(p2 => Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2)))
                    .FirstOrDefault();

                if (nearest != null)
                {
                    double distance = Math.Sqrt(
                        Math.Pow(nearest.X - p1.X, 2) +
                        Math.Pow(nearest.Y - p1.Y, 2));

                    if (distance <= matchRadius)
                    {
                        matchedPairs.Add((p1.Value, nearest.Value));
                    }
                }
            }

            if (matchedPairs.Count < 2)
                return null;

            return CalculatePearsonCorrelation(
                matchedPairs.Select(p => p.value1).ToList(),
                matchedPairs.Select(p => p.value2).ToList());
        }

        /// <summary>
        /// Calculates Pearson correlation coefficient.
        /// </summary>
        private static double CalculatePearsonCorrelation(List<double> x, List<double> y)
        {
            if (x.Count != y.Count || x.Count < 2)
                return 0;

            double meanX = x.Average();
            double meanY = y.Average();

            double numerator = 0;
            double sumSqX = 0;
            double sumSqY = 0;

            for (int i = 0; i < x.Count; i++)
            {
                double dx = x[i] - meanX;
                double dy = y[i] - meanY;
                numerator += dx * dy;
                sumSqX += dx * dx;
                sumSqY += dy * dy;
            }

            double denominator = Math.Sqrt(sumSqX * sumSqY);
            if (Math.Abs(denominator) < 1e-10)
                return 0;

            return numerator / denominator;
        }

        /// <summary>
        /// Calculates the median of a list of values.
        /// </summary>
        private static double CalculateMedian(List<double> values)
        {
            if (values == null || values.Count == 0)
                return 0;

            var sorted = values.OrderBy(v => v).ToList();
            int mid = sorted.Count / 2;

            if (sorted.Count % 2 == 0)
            {
                return (sorted[mid - 1] + sorted[mid]) / 2.0;
            }
            else
            {
                return sorted[mid];
            }
        }

        /// <summary>
        /// Calculates the standard deviation.
        /// </summary>
        private static double CalculateStandardDeviation(List<double> values)
        {
            if (values == null || values.Count < 2)
                return 0;

            double mean = values.Average();
            double sumSquaredDifferences = values.Sum(v => Math.Pow(v - mean, 2));
            return Math.Sqrt(sumSquaredDifferences / (values.Count - 1));
        }

        /// <summary>
        /// Calculates the variance.
        /// </summary>
        private static double CalculateVariance(List<double> values)
        {
            if (values == null || values.Count < 2)
                return 0;

            double mean = values.Average();
            return values.Sum(v => Math.Pow(v - mean, 2)) / (values.Count - 1);
        }
    }

    /// <summary>
    /// Represents a statistical summary of heat map data.
    /// </summary>
    public class StatisticalSummary
    {
        /// <summary>
        /// Gets or sets the number of data points.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Gets or sets the minimum value.
        /// </summary>
        public double MinValue { get; set; }

        /// <summary>
        /// Gets or sets the maximum value.
        /// </summary>
        public double MaxValue { get; set; }

        /// <summary>
        /// Gets or sets the mean (average) value.
        /// </summary>
        public double MeanValue { get; set; }

        /// <summary>
        /// Gets or sets the median value.
        /// </summary>
        public double MedianValue { get; set; }

        /// <summary>
        /// Gets or sets the standard deviation.
        /// </summary>
        public double StandardDeviation { get; set; }

        /// <summary>
        /// Gets or sets the variance.
        /// </summary>
        public double Variance { get; set; }

        /// <summary>
        /// Gets or sets the minimum X coordinate.
        /// </summary>
        public double MinX { get; set; }

        /// <summary>
        /// Gets or sets the maximum X coordinate.
        /// </summary>
        public double MaxX { get; set; }

        /// <summary>
        /// Gets or sets the mean X coordinate.
        /// </summary>
        public double MeanX { get; set; }

        /// <summary>
        /// Gets or sets the minimum Y coordinate.
        /// </summary>
        public double MinY { get; set; }

        /// <summary>
        /// Gets or sets the maximum Y coordinate.
        /// </summary>
        public double MaxY { get; set; }

        /// <summary>
        /// Gets or sets the mean Y coordinate.
        /// </summary>
        public double MeanY { get; set; }

        /// <summary>
        /// Gets or sets the X coordinate range.
        /// </summary>
        public double RangeX { get; set; }

        /// <summary>
        /// Gets or sets the Y coordinate range.
        /// </summary>
        public double RangeY { get; set; }

        /// <summary>
        /// Gets a formatted string representation of the summary.
        /// </summary>
        public string GetFormattedSummary()
        {
            return $"Statistical Summary:\n" +
                   $"  Count: {Count}\n" +
                   $"  Value Range: {MinValue:F2} to {MaxValue:F2}\n" +
                   $"  Mean: {MeanValue:F2}\n" +
                   $"  Median: {MedianValue:F2}\n" +
                   $"  Std Dev: {StandardDeviation:F2}\n" +
                   $"  Variance: {Variance:F2}\n" +
                   $"  X Range: {MinX:F2} to {MaxX:F2} (Range: {RangeX:F2})\n" +
                   $"  Y Range: {MinY:F2} to {MaxY:F2} (Range: {RangeY:F2})";
        }
    }
}


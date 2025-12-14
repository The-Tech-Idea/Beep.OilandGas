using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Beep.OilandGas.DCA.Constants;
using Beep.OilandGas.DCA.Exceptions;
using Beep.OilandGas.DCA.Results;

namespace Beep.OilandGas.DCA.Visualization
{
    /// <summary>
    /// Provides methods for generating various types of plots and exporting them.
    /// </summary>
    public static class PlotGenerator
    {
        /// <summary>
        /// Generates a comprehensive decline curve plot with all features.
        /// </summary>
        /// <param name="result">The DCA fit result.</param>
        /// <param name="timeData">Time data.</param>
        /// <param name="forecastDays">Number of days to forecast.</param>
        /// <param name="showIntervals">Whether to include prediction intervals.</param>
        /// <param name="configuration">Plot configuration options.</param>
        /// <returns>A DeclineCurvePlot or PlotWithIntervals object.</returns>
        public static object GenerateDeclineCurvePlot(
            DCAFitResult result,
            List<DateTime> timeData,
            int forecastDays = 365,
            bool showIntervals = false,
            PlotConfiguration configuration = null)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));
            if (timeData == null)
                throw new ArgumentNullException(nameof(timeData));

            if (showIntervals)
            {
                return PlotWithIntervals.FromFitResult(result, timeData, forecastDays, 0.95);
            }
            else
            {
                return DeclineCurvePlot.FromFitResult(result, timeData, forecastDays);
            }
        }

        /// <summary>
        /// Generates a residual plot for model diagnostics.
        /// </summary>
        /// <param name="result">The DCA fit result.</param>
        /// <param name="timeData">Time data.</param>
        /// <returns>A ResidualPlot object.</returns>
        public static ResidualPlot GenerateResidualPlot(
            DCAFitResult result,
            List<DateTime> timeData)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));
            if (timeData == null)
                throw new ArgumentNullException(nameof(timeData));

            return ResidualPlot.FromFitResult(result, timeData);
        }

        /// <summary>
        /// Generates a comparison plot for multiple DCA results.
        /// </summary>
        /// <param name="results">Dictionary mapping series names to (result, timeData) tuples.</param>
        /// <param name="includeForecast">Whether to include forecast for each series.</param>
        /// <param name="forecastDays">Number of days to forecast.</param>
        /// <returns>A ComparisonPlot object.</returns>
        public static ComparisonPlot GenerateComparisonPlot(
            Dictionary<string, (DCAFitResult result, List<DateTime> timeData)> results,
            bool includeForecast = true,
            int forecastDays = 365)
        {
            if (results == null)
                throw new ArgumentNullException(nameof(results));

            if (results.Count == 0)
            {
                throw new Exceptions.InvalidDataException("At least one result is required for comparison.");
            }

            var plot = new ComparisonPlot
            {
                Title = "Decline Curve Comparison"
            };

            foreach (var kvp in results)
            {
                plot.AddSeriesFromResult(
                    kvp.Value.result,
                    kvp.Value.timeData,
                    kvp.Key,
                    includeForecast,
                    forecastDays);
            }

            return plot;
        }

        /// <summary>
        /// Exports multiple plots to a single CSV file with separate sheets (as separate sections).
        /// </summary>
        /// <param name="plots">Dictionary mapping plot names to plot objects.</param>
        /// <param name="filePath">Path to the output CSV file.</param>
        public static void ExportMultiplePlotsToCsv(
            Dictionary<string, object> plots,
            string filePath)
        {
            if (plots == null)
                throw new ArgumentNullException(nameof(plots));
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                foreach (var kvp in plots)
                {
                    writer.WriteLine($"=== {kvp.Key} ===");

                    switch (kvp.Value)
                    {
                        case DeclineCurvePlot declinePlot:
                            writer.WriteLine("Type,Time,ProductionRate,Label");
                            WritePlotPoints(writer, declinePlot.ObservedPoints, "Observed");
                            WritePlotPoints(writer, declinePlot.PredictedPoints, "Predicted");
                            WritePlotPoints(writer, declinePlot.ForecastPoints, "Forecast");
                            break;

                        case PlotWithIntervals intervalPlot:
                            writer.WriteLine("Type,Time,ProductionRate,LowerBound,UpperBound,Label");
                            WritePlotPoints(writer, intervalPlot.ObservedPoints, "Observed");
                            WriteIntervalPoints(writer, intervalPlot.PredictedPointsWithIntervals, "Predicted");
                            WriteIntervalPoints(writer, intervalPlot.ForecastPointsWithIntervals, "Forecast");
                            break;

                        case ResidualPlot residualPlot:
                            writer.WriteLine("PlotType,XValue,YValue,Label");
                            WritePlotPoints(writer, residualPlot.ResidualPoints, "ResidualVsPredicted");
                            WritePlotPoints(writer, residualPlot.ResidualVsTimePoints, "ResidualVsTime");
                            WritePlotPoints(writer, residualPlot.QQPlotPoints, "QQPlot");
                            break;

                        case ComparisonPlot comparisonPlot:
                            writer.WriteLine("Series,Time,ProductionRate,Label");
                            foreach (var series in comparisonPlot.Series.Where(s => s.IsVisible))
                            {
                                foreach (var point in series.Points)
                                {
                                    writer.WriteLine($"{series.Name},{point.Time}," +
                                                   $"{point.ProductionRate.ToString(CultureInfo.InvariantCulture)}," +
                                                   $"{point.Label}");
                                }
                            }
                            break;
                    }

                    writer.WriteLine();
                }
            }
        }

        /// <summary>
        /// Generates plot data in JSON format for web-based visualization.
        /// </summary>
        /// <param name="plot">The plot object to export.</param>
        /// <returns>JSON string representation of the plot.</returns>
        public static string ExportToJson(object plot)
        {
            if (plot == null)
                throw new ArgumentNullException(nameof(plot));

            var json = new StringBuilder();
            json.AppendLine("{");

            switch (plot)
            {
                case DeclineCurvePlot declinePlot:
                    json.AppendLine($"  \"title\": \"{EscapeJson(declinePlot.Title)}\",");
                    json.AppendLine($"  \"xAxisLabel\": \"{EscapeJson(declinePlot.XAxisLabel)}\",");
                    json.AppendLine($"  \"yAxisLabel\": \"{EscapeJson(declinePlot.YAxisLabel)}\",");
                    json.AppendLine("  \"observed\": [");
                    json.AppendLine(string.Join(",\n", declinePlot.ObservedPoints.Select(p =>
                        $"    {{\"time\": {p.Time.ToString(CultureInfo.InvariantCulture)}, \"rate\": {p.ProductionRate.ToString(CultureInfo.InvariantCulture)}}}")));
                    json.AppendLine("  ],");
                    json.AppendLine("  \"predicted\": [");
                    json.AppendLine(string.Join(",\n", declinePlot.PredictedPoints.Select(p =>
                        $"    {{\"time\": {p.Time.ToString(CultureInfo.InvariantCulture)}, \"rate\": {p.ProductionRate.ToString(CultureInfo.InvariantCulture)}}}")));
                    json.AppendLine("  ],");
                    json.AppendLine("  \"forecast\": [");
                    json.AppendLine(string.Join(",\n", declinePlot.ForecastPoints.Select(p =>
                        $"    {{\"time\": {p.Time.ToString(CultureInfo.InvariantCulture)}, \"rate\": {p.ProductionRate.ToString(CultureInfo.InvariantCulture)}}}")));
                    json.AppendLine("  ]");
                    break;

                case ComparisonPlot comparisonPlot:
                    json.AppendLine($"  \"title\": \"{EscapeJson(comparisonPlot.Title)}\",");
                    json.AppendLine("  \"series\": [");
                    var seriesJson = comparisonPlot.Series.Where(s => s.IsVisible).Select(series =>
                    {
                        var pointsJson = string.Join(",", series.Points.Select(p =>
                            $"{{\"time\": {p.Time.ToString(CultureInfo.InvariantCulture)}, \"rate\": {p.ProductionRate.ToString(CultureInfo.InvariantCulture)}}}"));
                        return $"    {{\"name\": \"{EscapeJson(series.Name)}\", \"color\": \"{series.Color}\", \"points\": [{pointsJson}]}}";
                    });
                    json.AppendLine(string.Join(",\n", seriesJson));
                    json.AppendLine("  ]");
                    break;

                default:
                    throw new Exceptions.InvalidDataException($"Unsupported plot type: {plot.GetType().Name}");
            }

            json.AppendLine("}");
            return json.ToString();
        }

        /// <summary>
        /// Writes plot points to a stream writer.
        /// </summary>
        private static void WritePlotPoints(StreamWriter writer, List<PlotPoint> points, string type)
        {
            foreach (var point in points)
            {
                writer.WriteLine($"{type},{point.Time.ToString(CultureInfo.InvariantCulture)}," +
                               $"{point.ProductionRate.ToString(CultureInfo.InvariantCulture)},{point.Label}");
            }
        }

        /// <summary>
        /// Writes interval plot points to a stream writer.
        /// </summary>
        private static void WriteIntervalPoints(StreamWriter writer, List<IntervalPlotPoint> points, string type)
        {
            foreach (var point in points)
            {
                writer.WriteLine($"{type},{point.Time.ToString(CultureInfo.InvariantCulture)}," +
                               $"{point.ProductionRate.ToString(CultureInfo.InvariantCulture)}," +
                               $"{point.LowerBound.ToString(CultureInfo.InvariantCulture)}," +
                               $"{point.UpperBound.ToString(CultureInfo.InvariantCulture)},{point.Label}");
            }
        }

        /// <summary>
        /// Escapes special characters in JSON strings.
        /// </summary>
        private static string EscapeJson(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            return value.Replace("\\", "\\\\")
                       .Replace("\"", "\\\"")
                       .Replace("\n", "\\n")
                       .Replace("\r", "\\r")
                       .Replace("\t", "\\t");
        }
    }
}


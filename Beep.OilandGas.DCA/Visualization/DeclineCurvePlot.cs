using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.DCA.Constants;
using Beep.OilandGas.DCA.Results;
using static Beep.OilandGas.DCA.Constants.DCAConstants;

namespace Beep.OilandGas.DCA.Visualization
{
    /// <summary>
    /// Represents a data point for plotting decline curves.
    /// </summary>
    public class PlotPoint
    {
        /// <summary>
        /// Gets or sets the time value (x-axis).
        /// </summary>
        public double Time { get; set; }

        /// <summary>
        /// Gets or sets the production rate value (y-axis).
        /// </summary>
        public double ProductionRate { get; set; }

        /// <summary>
        /// Gets or sets the point label.
        /// </summary>
        public string Label { get; set; }
    }

    /// <summary>
    /// Represents a decline curve plot with data points and forecast.
    /// </summary>
    public class DeclineCurvePlot
    {
        /// <summary>
        /// Gets the observed data points.
        /// </summary>
        public List<PlotPoint> ObservedPoints { get; }

        /// <summary>
        /// Gets the predicted/fitted data points.
        /// </summary>
        public List<PlotPoint> PredictedPoints { get; }

        /// <summary>
        /// Gets the forecast data points (beyond observed data).
        /// </summary>
        public List<PlotPoint> ForecastPoints { get; }

        /// <summary>
        /// Gets the plot title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets the x-axis label.
        /// </summary>
        public string XAxisLabel { get; set; }

        /// <summary>
        /// Gets the y-axis label.
        /// </summary>
        public string YAxisLabel { get; set; }

        /// <summary>
        /// Gets or sets the plot configuration.
        /// </summary>
        public PlotConfiguration Configuration { get; set; }

        /// <summary>
        /// Gets the minimum time value in the plot.
        /// </summary>
        public double MinTime => GetAllPoints().Min(p => p.Time);

        /// <summary>
        /// Gets the maximum time value in the plot.
        /// </summary>
        public double MaxTime => GetAllPoints().Max(p => p.Time);

        /// <summary>
        /// Gets the minimum production rate value in the plot.
        /// </summary>
        public double MinProductionRate => GetAllPoints().Min(p => p.ProductionRate);

        /// <summary>
        /// Gets the maximum production rate value in the plot.
        /// </summary>
        public double MaxProductionRate => GetAllPoints().Max(p => p.ProductionRate);

        /// <summary>
        /// Initializes a new instance of the <see cref="DeclineCurvePlot"/> class.
        /// </summary>
        public DeclineCurvePlot()
        {
            ObservedPoints = new List<PlotPoint>();
            PredictedPoints = new List<PlotPoint>();
            ForecastPoints = new List<PlotPoint>();
            Title = "Decline Curve Analysis";
            XAxisLabel = "Time (days)";
            YAxisLabel = "Production Rate";
            Configuration = new PlotConfiguration();
        }

        /// <summary>
        /// Creates a plot from a DCA fit result.
        /// </summary>
        /// <param name="result">The DCA fit result.</param>
        /// <param name="timeData">Time data corresponding to observed values.</param>
        /// <param name="forecastDays">Number of days to forecast beyond the observed data.</param>
        /// <returns>A DeclineCurvePlot object.</returns>
        public static DeclineCurvePlot FromFitResult(
            DCAFitResult result,
            List<DateTime> timeData,
            int forecastDays = 365)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));
            if (timeData == null)
                throw new ArgumentNullException(nameof(timeData));

            var plot = new DeclineCurvePlot();

            DateTime startTime = timeData[0];

            // Add observed and predicted points
            for (int i = 0; i < result.ObservedValues.Length; i++)
            {
                double time = (timeData[i] - startTime).TotalDays;

                plot.ObservedPoints.Add(new PlotPoint
                {
                    Time = time,
                    ProductionRate = result.ObservedValues[i],
                    Label = $"Observed {i}"
                });

                plot.PredictedPoints.Add(new PlotPoint
                {
                    Time = time,
                    ProductionRate = result.PredictedValues[i],
                    Label = $"Predicted {i}"
                });
            }

            // Generate forecast points
            if (forecastDays > 0 && result.Parameters.Length >= 2)
            {
                double qi = result.Parameters[0];
                double b = result.Parameters.Length > 1 ? result.Parameters[1] : Constants.DCAConstants.DefaultDeclineExponent;
                double di = Constants.DCAConstants.DefaultInitialDeclineRate;
                double lastTime = (timeData.Last() - startTime).TotalDays;

                int forecastSteps = Math.Min(forecastDays, 365);
                double timeStep = forecastDays / (double)forecastSteps;

                for (int i = 1; i <= forecastSteps; i++)
                {
                    double forecastTime = lastTime + (i * timeStep);
                    double forecastRate = DCAGenerator.HyperbolicDecline(qi, di, forecastTime, b);

                    plot.ForecastPoints.Add(new PlotPoint
                    {
                        Time = forecastTime,
                        ProductionRate = forecastRate,
                        Label = $"Forecast {i}"
                    });
                }
            }

            return plot;
        }

        /// <summary>
        /// Gets all points from all series.
        /// </summary>
        private IEnumerable<PlotPoint> GetAllPoints()
        {
            return ObservedPoints.Concat(PredictedPoints).Concat(ForecastPoints);
        }

        /// <summary>
        /// Adds a custom data series to the plot.
        /// </summary>
        /// <param name="points">Data points to add.</param>
        /// <param name="seriesType">Type of series (Observed, Predicted, Forecast, or Custom).</param>
        public void AddSeries(List<PlotPoint> points, string seriesType = "Custom")
        {
            if (points == null)
                throw new ArgumentNullException(nameof(points));

            switch (seriesType.ToLower())
            {
                case "observed":
                    ObservedPoints.AddRange(points);
                    break;
                case "predicted":
                    PredictedPoints.AddRange(points);
                    break;
                case "forecast":
                    ForecastPoints.AddRange(points);
                    break;
                default:
                    // For custom series, add to predicted points
                    PredictedPoints.AddRange(points);
                    break;
            }
        }

        /// <summary>
        /// Creates a logarithmic version of the plot (log scale for production rate).
        /// </summary>
        /// <returns>A new DeclineCurvePlot with logarithmic Y-axis values.</returns>
        public DeclineCurvePlot ToLogScale()
        {
            var logPlot = new DeclineCurvePlot
            {
                Title = Title + " (Log Scale)",
                XAxisLabel = XAxisLabel,
                YAxisLabel = "Log(" + YAxisLabel + ")",
                Configuration = Configuration
            };
            logPlot.Configuration.UseLogScale = true;

            foreach (var point in ObservedPoints)
            {
                logPlot.ObservedPoints.Add(new PlotPoint
                {
                    Time = point.Time,
                    ProductionRate = Math.Log10(Math.Max(point.ProductionRate, 0.001)), // Avoid log(0)
                    Label = point.Label
                });
            }

            foreach (var point in PredictedPoints)
            {
                logPlot.PredictedPoints.Add(new PlotPoint
                {
                    Time = point.Time,
                    ProductionRate = Math.Log10(Math.Max(point.ProductionRate, 0.001)),
                    Label = point.Label
                });
            }

            foreach (var point in ForecastPoints)
            {
                logPlot.ForecastPoints.Add(new PlotPoint
                {
                    Time = point.Time,
                    ProductionRate = Math.Log10(Math.Max(point.ProductionRate, 0.001)),
                    Label = point.Label
                });
            }

            return logPlot;
        }

        /// <summary>
        /// Exports plot data to CSV format for use in external plotting tools.
        /// </summary>
        /// <param name="filePath">Path to the output CSV file.</param>
        /// <param name="includeMetadata">Whether to include plot metadata in the CSV.</param>
        public void ExportToCsv(string filePath, bool includeMetadata = false)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            using (var writer = new System.IO.StreamWriter(filePath))
            {
                if (includeMetadata)
                {
                    writer.WriteLine($"# Title: {Title}");
                    writer.WriteLine($"# X-Axis: {XAxisLabel}");
                    writer.WriteLine($"# Y-Axis: {YAxisLabel}");
                    writer.WriteLine($"# Min Time: {MinTime}");
                    writer.WriteLine($"# Max Time: {MaxTime}");
                    writer.WriteLine($"# Min Rate: {MinProductionRate}");
                    writer.WriteLine($"# Max Rate: {MaxProductionRate}");
                    writer.WriteLine();
                }

                writer.WriteLine("Type,Time,ProductionRate,Label");

                foreach (var point in ObservedPoints)
                {
                    writer.WriteLine($"Observed,{point.Time.ToString(System.Globalization.CultureInfo.InvariantCulture)}," +
                                   $"{point.ProductionRate.ToString(System.Globalization.CultureInfo.InvariantCulture)},{point.Label}");
                }

                foreach (var point in PredictedPoints)
                {
                    writer.WriteLine($"Predicted,{point.Time.ToString(System.Globalization.CultureInfo.InvariantCulture)}," +
                                   $"{point.ProductionRate.ToString(System.Globalization.CultureInfo.InvariantCulture)},{point.Label}");
                }

                foreach (var point in ForecastPoints)
                {
                    writer.WriteLine($"Forecast,{point.Time.ToString(System.Globalization.CultureInfo.InvariantCulture)}," +
                                   $"{point.ProductionRate.ToString(System.Globalization.CultureInfo.InvariantCulture)},{point.Label}");
                }
            }
        }

        /// <summary>
        /// Gets plot statistics summary.
        /// </summary>
        /// <returns>Dictionary containing plot statistics.</returns>
        public Dictionary<string, object> GetStatistics()
        {
            var allRates = GetAllPoints().Select(p => p.ProductionRate).ToList();
            
            return new Dictionary<string, object>
            {
                ["MinTime"] = MinTime,
                ["MaxTime"] = MaxTime,
                ["TimeRange"] = MaxTime - MinTime,
                ["MinProductionRate"] = MinProductionRate,
                ["MaxProductionRate"] = MaxProductionRate,
                ["AverageProductionRate"] = allRates.Average(),
                ["TotalPoints"] = GetAllPoints().Count(),
                ["ObservedPoints"] = ObservedPoints.Count,
                ["PredictedPoints"] = PredictedPoints.Count,
                ["ForecastPoints"] = ForecastPoints.Count
            };
        }
    }
}


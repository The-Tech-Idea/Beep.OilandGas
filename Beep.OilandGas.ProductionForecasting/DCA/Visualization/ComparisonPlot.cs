using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.DCA.Constants;
using Beep.OilandGas.DCA.Results;
using static Beep.OilandGas.DCA.Constants.DCAConstants;

namespace Beep.OilandGas.DCA.Visualization
{
    /// <summary>
    /// Represents a data series for comparison plots.
    /// </summary>
    public class PlotSeries
    {
        /// <summary>
        /// Gets or sets the series name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the data points for this series.
        /// </summary>
        public List<PlotPoint> Points { get; set; }

        /// <summary>
        /// Gets or sets the series color (as hex string).
        /// </summary>
        public string Color { get; set; }

        /// <summary>
        /// Gets or sets whether this series is visible.
        /// </summary>
        public bool IsVisible { get; set; } = true;

        /// <summary>
        /// Gets or sets the line style (e.g., "solid", "dashed", "dotted").
        /// </summary>
        public string LineStyle { get; set; } = "solid";

        /// <summary>
        /// Gets or sets the marker style (e.g., "circle", "square", "none").
        /// </summary>
        public string MarkerStyle { get; set; } = "none";

        /// <summary>
        /// Initializes a new instance of the <see cref="PlotSeries"/> class.
        /// </summary>
        public PlotSeries()
        {
            Points = new List<PlotPoint>();
        }
    }

    /// <summary>
    /// Represents a comparison plot for comparing multiple decline curves or wells.
    /// </summary>
    public class ComparisonPlot
    {
        /// <summary>
        /// Gets the data series in this plot.
        /// </summary>
        public List<PlotSeries> Series { get; }

        /// <summary>
        /// Gets or sets the plot title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the x-axis label.
        /// </summary>
        public string XAxisLabel { get; set; }

        /// <summary>
        /// Gets or sets the y-axis label.
        /// </summary>
        public string YAxisLabel { get; set; }

        /// <summary>
        /// Gets or sets the plot configuration.
        /// </summary>
        public PlotConfiguration Configuration { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComparisonPlot"/> class.
        /// </summary>
        public ComparisonPlot()
        {
            Series = new List<PlotSeries>();
            Title = "Decline Curve Comparison";
            XAxisLabel = "Time (days)";
            YAxisLabel = "Production Rate";
            Configuration = new PlotConfiguration();
        }

        /// <summary>
        /// Adds a series from a DCA fit result.
        /// </summary>
        /// <param name="result">The DCA fit result.</param>
        /// <param name="timeData">Time data.</param>
        /// <param name="seriesName">Name for this series.</param>
        /// <param name="includeForecast">Whether to include forecast points.</param>
        /// <param name="forecastDays">Number of days to forecast.</param>
        /// <returns>The created plot series.</returns>
        public PlotSeries AddSeriesFromResult(
            DCAFitResult result,
            List<DateTime> timeData,
            string seriesName,
            bool includeForecast = false,
            int forecastDays = 365)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));
            if (timeData == null)
                throw new ArgumentNullException(nameof(timeData));

            var series = new PlotSeries
            {
                Name = seriesName,
                Color = GetNextColor(Series.Count)
            };

            DateTime startTime = timeData[0];

            // Add observed/predicted points
            for (int i = 0; i < result.PredictedValues.Length; i++)
            {
                double time = (timeData[i] - startTime).TotalDays;
                series.Points.Add(new PlotPoint
                {
                    Time = time,
                    ProductionRate = result.PredictedValues[i],
                    Label = $"{seriesName} - {i}"
                });
            }

            // Add forecast if requested
            if (includeForecast && result.Parameters.Length >= 2)
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

                    series.Points.Add(new PlotPoint
                    {
                        Time = forecastTime,
                        ProductionRate = forecastRate,
                        Label = $"{seriesName} - Forecast {i}"
                    });
                }
            }

            Series.Add(series);
            return series;
        }

        /// <summary>
        /// Adds a custom series.
        /// </summary>
        /// <param name="name">Series name.</param>
        /// <param name="points">Data points.</param>
        /// <param name="color">Series color (hex string).</param>
        /// <returns>The created plot series.</returns>
        public PlotSeries AddSeries(string name, List<PlotPoint> points, string color = null)
        {
            var series = new PlotSeries
            {
                Name = name,
                Points = points ?? new List<PlotPoint>(),
                Color = color ?? GetNextColor(Series.Count)
            };

            Series.Add(series);
            return series;
        }

        /// <summary>
        /// Removes a series by name.
        /// </summary>
        /// <param name="name">Name of the series to remove.</param>
        /// <returns>True if the series was found and removed.</returns>
        public bool RemoveSeries(string name)
        {
            var series = Series.FirstOrDefault(s => s.Name == name);
            if (series != null)
            {
                Series.Remove(series);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Toggles visibility of a series.
        /// </summary>
        /// <param name="name">Name of the series.</param>
        /// <returns>True if the series was found.</returns>
        public bool ToggleSeriesVisibility(string name)
        {
            var series = Series.FirstOrDefault(s => s.Name == name);
            if (series != null)
            {
                series.IsVisible = !series.IsVisible;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Exports comparison plot data to CSV format.
        /// </summary>
        /// <param name="filePath">Path to the output CSV file.</param>
        public void ExportToCsv(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            using (var writer = new System.IO.StreamWriter(filePath))
            {
                writer.WriteLine("Series,Time,ProductionRate,Label");

                foreach (var series in Series.Where(s => s.IsVisible))
                {
                    foreach (var point in series.Points)
                    {
                        writer.WriteLine($"{series.Name},{point.Time},{point.ProductionRate},{point.Label}");
                    }
                }
            }
        }

        /// <summary>
        /// Gets a color for a series based on its index.
        /// </summary>
        private static string GetNextColor(int index)
        {
            string[] colors = {
                "#2196F3", // Blue
                "#FF5722", // Red
                "#4CAF50", // Green
                "#FF9800", // Orange
                "#9C27B0", // Purple
                "#00BCD4", // Cyan
                "#F44336", // Red
                "#8BC34A", // Light Green
                "#FFC107", // Amber
                "#795548"  // Brown
            };

            return colors[index % colors.Length];
        }
    }
}


using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.DCA.Constants;
using Beep.OilandGas.DCA.Results;
using Beep.OilandGas.DCA.Statistics;
using static Beep.OilandGas.DCA.Constants.DCAConstants;

namespace Beep.OilandGas.DCA.Visualization
{
    /// <summary>
    /// Represents a plot point with confidence/prediction intervals.
    /// </summary>
    public class IntervalPlotPoint : PlotPoint
    {
        /// <summary>
        /// Gets or sets the lower bound of the interval.
        /// </summary>
        public double LowerBound { get; set; }

        /// <summary>
        /// Gets or sets the upper bound of the interval.
        /// </summary>
        public double UpperBound { get; set; }
    }

    /// <summary>
    /// Represents a decline curve plot with prediction intervals.
    /// </summary>
    public class PlotWithIntervals
    {
        /// <summary>
        /// Gets the observed data points.
        /// </summary>
        public List<PlotPoint> ObservedPoints { get; }

        /// <summary>
        /// Gets the predicted data points with intervals.
        /// </summary>
        public List<IntervalPlotPoint> PredictedPointsWithIntervals { get; }

        /// <summary>
        /// Gets the forecast data points with intervals.
        /// </summary>
        public List<IntervalPlotPoint> ForecastPointsWithIntervals { get; }

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
        /// Gets or sets the confidence level used for intervals (0-1).
        /// </summary>
        public double ConfidenceLevel { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlotWithIntervals"/> class.
        /// </summary>
        public PlotWithIntervals()
        {
            ObservedPoints = new List<PlotPoint>();
            PredictedPointsWithIntervals = new List<IntervalPlotPoint>();
            ForecastPointsWithIntervals = new List<IntervalPlotPoint>();
            Title = "Decline Curve with Prediction Intervals";
            XAxisLabel = "Time (days)";
            YAxisLabel = "Production Rate";
            ConfidenceLevel = 0.95;
        }

        /// <summary>
        /// Creates a plot with intervals from a DCA fit result.
        /// </summary>
        /// <param name="result">The DCA fit result.</param>
        /// <param name="timeData">Time data corresponding to observed values.</param>
        /// <param name="forecastDays">Number of days to forecast.</param>
        /// <param name="confidenceLevel">Confidence level for intervals (default 0.95).</param>
        /// <returns>A PlotWithIntervals object.</returns>
        public static PlotWithIntervals FromFitResult(
            DCAFitResult result,
            List<DateTime> timeData,
            int forecastDays = 365,
            double confidenceLevel = 0.95)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));
            if (timeData == null)
                throw new ArgumentNullException(nameof(timeData));

            var plot = new PlotWithIntervals
            {
                ConfidenceLevel = confidenceLevel
            };

            DateTime startTime = timeData[0];
            double residualStdError = Statistics.StatisticalAnalysis.CalculateRMSE(result.Residuals);
            int degreesOfFreedom = result.ObservedValues.Length - result.Parameters.Length;

            // Add observed points
            for (int i = 0; i < result.ObservedValues.Length; i++)
            {
                double time = (timeData[i] - startTime).TotalDays;
                plot.ObservedPoints.Add(new PlotPoint
                {
                    Time = time,
                    ProductionRate = result.ObservedValues[i],
                    Label = $"Observed {i}"
                });
            }

            // Add predicted points with intervals
            for (int i = 0; i < result.PredictedValues.Length; i++)
            {
                double time = (timeData[i] - startTime).TotalDays;
                var interval = Statistics.StatisticalAnalysis.CalculatePredictionInterval(
                    result.PredictedValues[i],
                    residualStdError,
                    confidenceLevel,
                    degreesOfFreedom);

                plot.PredictedPointsWithIntervals.Add(new IntervalPlotPoint
                {
                    Time = time,
                    ProductionRate = result.PredictedValues[i],
                    LowerBound = interval.lowerBound,
                    UpperBound = interval.upperBound,
                    Label = $"Predicted {i}"
                });
            }

            // Generate forecast points with intervals
            if (forecastDays > 0 && result.Parameters.Length >= 2)
            {
                double qi = result.Parameters[0];
                double b = result.Parameters.Length > 1 ? result.Parameters[1] : DefaultDeclineExponent;
                double di = DefaultInitialDeclineRate;
                double lastTime = (timeData.Last() - startTime).TotalDays;

                int forecastSteps = Math.Min(forecastDays, 365);
                double timeStep = forecastDays / (double)forecastSteps;

                for (int i = 1; i <= forecastSteps; i++)
                {
                    double forecastTime = lastTime + (i * timeStep);
                    double forecastRate = DCAGenerator.HyperbolicDecline(qi, di, forecastTime, b);

                    var interval = Statistics.StatisticalAnalysis.CalculatePredictionInterval(
                        forecastRate,
                        residualStdError,
                        confidenceLevel,
                        degreesOfFreedom);

                    plot.ForecastPointsWithIntervals.Add(new IntervalPlotPoint
                    {
                        Time = forecastTime,
                        ProductionRate = forecastRate,
                        LowerBound = interval.lowerBound,
                        UpperBound = interval.upperBound,
                        Label = $"Forecast {i}"
                    });
                }
            }

            return plot;
        }

        /// <summary>
        /// Exports plot data with intervals to CSV format.
        /// </summary>
        /// <param name="filePath">Path to the output CSV file.</param>
        public void ExportToCsv(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            using (var writer = new System.IO.StreamWriter(filePath))
            {
                writer.WriteLine("Type,Time,ProductionRate,LowerBound,UpperBound,Label");

                foreach (var point in ObservedPoints)
                {
                    writer.WriteLine($"Observed,{point.Time},{point.ProductionRate},,{point.Label}");
                }

                foreach (var point in PredictedPointsWithIntervals)
                {
                    writer.WriteLine($"Predicted,{point.Time},{point.ProductionRate}," +
                                   $"{point.LowerBound},{point.UpperBound},{point.Label}");
                }

                foreach (var point in ForecastPointsWithIntervals)
                {
                    writer.WriteLine($"Forecast,{point.Time},{point.ProductionRate}," +
                                   $"{point.LowerBound},{point.UpperBound},{point.Label}");
                }
            }
        }
    }
}


using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.DCA.Results;

namespace Beep.OilandGas.DCA.Visualization
{
    /// <summary>
    /// Represents a residual plot for model diagnostics.
    /// </summary>
    public class ResidualPlot
    {
        /// <summary>
        /// Gets the residual data points (residual vs predicted).
        /// </summary>
        public List<PlotPoint> ResidualPoints { get; }

        /// <summary>
        /// Gets the residual vs time data points.
        /// </summary>
        public List<PlotPoint> ResidualVsTimePoints { get; }

        /// <summary>
        /// Gets the Q-Q plot points (quantile-quantile plot).
        /// </summary>
        public List<PlotPoint> QQPlotPoints { get; }

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
        /// Initializes a new instance of the <see cref="ResidualPlot"/> class.
        /// </summary>
        public ResidualPlot()
        {
            ResidualPoints = new List<PlotPoint>();
            ResidualVsTimePoints = new List<PlotPoint>();
            QQPlotPoints = new List<PlotPoint>();
            Title = "Residual Analysis";
            XAxisLabel = "Predicted Values";
            YAxisLabel = "Residuals";
        }

        /// <summary>
        /// Creates a residual plot from a DCA fit result.
        /// </summary>
        /// <param name="result">The DCA fit result.</param>
        /// <param name="timeData">Time data corresponding to observed values.</param>
        /// <returns>A ResidualPlot object.</returns>
        public static ResidualPlot FromFitResult(DCAFitResult result, List<DateTime> timeData)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));
            if (timeData == null)
                throw new ArgumentNullException(nameof(timeData));

            var plot = new ResidualPlot();

            DateTime startTime = timeData[0];

            // Create residual vs predicted plot
            for (int i = 0; i < result.Residuals.Length; i++)
            {
                plot.ResidualPoints.Add(new PlotPoint
                {
                    Time = result.PredictedValues[i],
                    ProductionRate = result.Residuals[i],
                    Label = $"Residual {i}"
                });
            }

            // Create residual vs time plot
            for (int i = 0; i < result.Residuals.Length; i++)
            {
                double time = (timeData[i] - startTime).TotalDays;
                plot.ResidualVsTimePoints.Add(new PlotPoint
                {
                    Time = time,
                    ProductionRate = result.Residuals[i],
                    Label = $"Residual {i}"
                });
            }

            // Create Q-Q plot (quantile-quantile plot for normality check)
            var sortedResiduals = result.Residuals.OrderBy(r => r).ToArray();
            int n = sortedResiduals.Length;

            for (int i = 0; i < n; i++)
            {
                double theoreticalQuantile = CalculateTheoreticalQuantile((i + 0.5) / n);
                plot.QQPlotPoints.Add(new PlotPoint
                {
                    Time = theoreticalQuantile,
                    ProductionRate = sortedResiduals[i],
                    Label = $"Q-Q {i}"
                });
            }

            return plot;
        }

        /// <summary>
        /// Calculates theoretical quantile for normal distribution (approximation).
        /// </summary>
        private static double CalculateTheoreticalQuantile(double probability)
        {
            // Approximation of inverse normal CDF
            // Using Beasley-Springer-Moro algorithm approximation
            if (probability < 0.5)
            {
                double t = Math.Sqrt(-2.0 * Math.Log(probability));
                return -(t - (2.515517 + 0.802853 * t + 0.010328 * t * t) /
                    (1.0 + 1.432788 * t + 0.189269 * t * t + 0.001308 * t * t * t));
            }
            else
            {
                double t = Math.Sqrt(-2.0 * Math.Log(1.0 - probability));
                return (t - (2.515517 + 0.802853 * t + 0.010328 * t * t) /
                    (1.0 + 1.432788 * t + 0.189269 * t * t + 0.001308 * t * t * t));
            }
        }

        /// <summary>
        /// Exports residual plot data to CSV format.
        /// </summary>
        /// <param name="filePath">Path to the output CSV file.</param>
        public void ExportToCsv(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            using (var writer = new System.IO.StreamWriter(filePath))
            {
                writer.WriteLine("PlotType,XValue,YValue,Label");

                foreach (var point in ResidualPoints)
                {
                    writer.WriteLine($"ResidualVsPredicted,{point.Time},{point.ProductionRate},{point.Label}");
                }

                foreach (var point in ResidualVsTimePoints)
                {
                    writer.WriteLine($"ResidualVsTime,{point.Time},{point.ProductionRate},{point.Label}");
                }

                foreach (var point in QQPlotPoints)
                {
                    writer.WriteLine($"QQPlot,{point.Time},{point.ProductionRate},{point.Label}");
                }
            }
        }
    }
}


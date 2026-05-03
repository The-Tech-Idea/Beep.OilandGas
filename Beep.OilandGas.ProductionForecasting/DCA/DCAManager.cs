using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.DCA.Constants;
using Beep.OilandGas.DCA.Exceptions;
using Beep.OilandGas.DCA.Performance;
using Beep.OilandGas.DCA.Results;
using Beep.OilandGas.DCA.Statistics;
using Beep.OilandGas.DCA.Validation;
using static Beep.OilandGas.DCA.Constants.DCAConstants;

namespace Beep.OilandGas.DCA
{
    /// <summary>
    /// Manager class for Decline Curve Analysis operations.
    /// Provides high-level methods for performing DCA calculations with statistical analysis.
    /// </summary>
    public class DCAManager
    {
        /// <summary>
        /// Gets or sets the initial production rate (qi).
        /// </summary>
        public double InitialProductionRate { get; set; }

        /// <summary>
        /// Gets or sets the decline exponent (b) for hyperbolic decline.
        /// </summary>
        public double DeclineExponent { get; set; }

        /// <summary>
        /// Gets or sets the initial decline rate (di).
        /// </summary>
        public double InitialDeclineRate { get; set; }

        /// <summary>
        /// Gets or sets the time since start of production.
        /// </summary>
        public DateTime ProductionTime { get; set; }

        /// <summary>
        /// Gets or sets the production rate at time t.
        /// </summary>
        public double ProductionRate { get; set; }

        /// <summary>
        /// Gets the last fit result, if available.
        /// </summary>
        public DCAFitResult LastFitResult { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DCAManager"/> class.
        /// </summary>
        public DCAManager()
        {
        }

        /// <summary>
        /// Generates decline curve analysis parameters by fitting a curve to production data.
        /// </summary>
        /// <param name="productionData">List of production rate values over time.</param>
        /// <param name="timeData">List of DateTime values corresponding to production data points.</param>
        /// <param name="qi">Initial production rate estimate. Defaults to 1000.</param>
        /// <param name="di">Initial decline rate estimate. Defaults to 0.1.</param>
        /// <returns>Array of fitted parameters [qi, b] where qi is initial production rate and b is decline exponent.</returns>
        /// <exception cref="Exceptions.InvalidDataException">Thrown when input data is invalid.</exception>
        /// <exception cref="Exceptions.ConvergenceException">Thrown when curve fitting fails to converge.</exception>
        public static double[] GenerateDCA(
            List<double> productionData, 
            List<DateTime> timeData, 
            double qi = DefaultInitialProductionRate, 
            double di = DefaultInitialDeclineRate)
        {
            // Validate input data
            DataValidator.ValidateProductionData(productionData, nameof(productionData));
            DataValidator.ValidateTimeData(timeData, productionData.Count, nameof(timeData));
            DataValidator.ValidateInitialProductionRate(qi, nameof(qi));
            DataValidator.ValidateInitialDeclineRate(di, nameof(di));

            // Perform curve fitting
            double[] parameters = DCAGenerator.FitCurve(productionData, timeData, qi, di);
            return parameters;
        }

        /// <summary>
        /// Performs comprehensive decline curve analysis with statistical metrics.
        /// </summary>
        /// <param name="productionData">List of production rate values over time.</param>
        /// <param name="timeData">List of DateTime values corresponding to production data points.</param>
        /// <param name="qi">Initial production rate estimate.</param>
        /// <param name="di">Initial decline rate estimate.</param>
        /// <param name="confidenceLevel">Confidence level for intervals (default 0.95).</param>
        /// <returns>DCAFitResult containing fitted parameters and statistical metrics.</returns>
        /// <exception cref="Exceptions.InvalidDataException">Thrown when input data is invalid.</exception>
        /// <exception cref="Exceptions.ConvergenceException">Thrown when curve fitting fails to converge.</exception>
        public DCAFitResult AnalyzeWithStatistics(
            List<double> productionData,
            List<DateTime> timeData,
            double qi = DefaultInitialProductionRate,
            double di = DefaultInitialDeclineRate,
            double confidenceLevel = 0.95)
        {
            // Validate input data
            DataValidator.ValidateProductionData(productionData, nameof(productionData));
            DataValidator.ValidateTimeData(timeData, productionData.Count, nameof(timeData));
            DataValidator.ValidateInitialProductionRate(qi, nameof(qi));
            DataValidator.ValidateInitialDeclineRate(di, nameof(di));

            // Perform curve fitting
            double[] parameters = DCAGenerator.FitCurve(productionData, timeData, qi, di);

            // Calculate predicted values
            DateTime startTime = timeData[0];
            double[] predictedValues = new double[productionData.Count];
            double b = parameters.Length > 1 ? parameters[1] : DefaultDeclineExponent;

            for (int i = 0; i < productionData.Count; i++)
            {
                double t = (timeData[i] - startTime).TotalDays;
                predictedValues[i] = DCAGenerator.HyperbolicDecline(parameters[0], di, t, b);
            }

            // Create fit result with statistics
            var result = new DCAFitResult(
                parameters,
                productionData.ToArray(),
                predictedValues,
                DefaultMaxIterations,
                true,
                confidenceLevel);

            LastFitResult = result;
            return result;
        }

        /// <summary>
        /// Performs decline curve analysis asynchronously.
        /// </summary>
        /// <param name="productionData">List of production rate values over time.</param>
        /// <param name="timeData">List of DateTime values corresponding to production data points.</param>
        /// <param name="qi">Initial production rate estimate.</param>
        /// <param name="di">Initial decline rate estimate.</param>
        /// <returns>Task that represents the asynchronous operation. The result contains the fit results.</returns>
        public async Task<DCAFitResult> AnalyzeAsync(
            List<double> productionData,
            List<DateTime> timeData,
            double qi = DefaultInitialProductionRate,
            double di = DefaultInitialDeclineRate)
        {
            var result = await AsyncDCACalculator.FitCurveAsync(productionData, timeData, qi, di);
            LastFitResult = result;
            return result;
        }
    }
}

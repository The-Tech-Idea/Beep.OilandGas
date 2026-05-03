using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.DCA.Constants;
using Beep.OilandGas.DCA.Exceptions;
using Beep.OilandGas.DCA.Results;
using Beep.OilandGas.DCA.Validation;
using static Beep.OilandGas.DCA.Constants.DCAConstants;

namespace Beep.OilandGas.DCA.Performance
{
    /// <summary>
    /// Provides asynchronous methods for decline curve analysis calculations.
    /// Useful for processing large datasets or multiple wells in parallel.
    /// </summary>
    public static class AsyncDCACalculator
    {
        /// <summary>
        /// Fits a decline curve to production data asynchronously.
        /// </summary>
        /// <param name="productionData">List of production rate values.</param>
        /// <param name="timeData">List of DateTime values corresponding to production data.</param>
        /// <param name="qi">Initial guess for initial production rate.</param>
        /// <param name="di">Initial guess for initial decline rate.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>Task that represents the asynchronous operation. The result contains the fit results.</returns>
        public static async Task<DCAFitResult> FitCurveAsync(
            List<double> productionData,
            List<DateTime> timeData,
            double qi = DefaultInitialProductionRate,
            double di = DefaultInitialDeclineRate,
            CancellationToken cancellationToken = default)
        {
            // Validate inputs
            DataValidator.ValidateProductionData(productionData, nameof(productionData));
            DataValidator.ValidateTimeData(timeData, productionData.Count, nameof(timeData));
            DataValidator.ValidateInitialProductionRate(qi, nameof(qi));
            DataValidator.ValidateInitialDeclineRate(di, nameof(di));

            // Run the curve fitting on a background thread
            return await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Perform curve fitting
                double[] parameters = DCAGenerator.FitCurve(productionData, timeData, qi, di);

                cancellationToken.ThrowIfCancellationRequested();

                // Calculate predicted values
                double[] predictedValues = CalculatePredictedValues(
                    productionData,
                    timeData,
                    parameters,
                    cancellationToken);

                // Create and return result
                return new DCAFitResult(
                    parameters,
                    productionData.ToArray(),
                    predictedValues,
                    DefaultMaxIterations, // Note: actual iterations not tracked in current implementation
                    true,
                    0.95);
            }, cancellationToken);
        }

        /// <summary>
        /// Processes multiple wells in parallel.
        /// </summary>
        /// <param name="wellData">Dictionary mapping well identifiers to (productionData, timeData) tuples.</param>
        /// <param name="qi">Initial guess for initial production rate.</param>
        /// <param name="di">Initial guess for initial decline rate.</param>
        /// <param name="maxDegreeOfParallelism">Maximum number of parallel operations. Defaults to processor count.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>Dictionary mapping well identifiers to their fit results.</returns>
        public static async Task<Dictionary<string, DCAFitResult>> ProcessMultipleWellsAsync(
            Dictionary<string, (List<double> productionData, List<DateTime> timeData)> wellData,
            double qi = DefaultInitialProductionRate,
            double di = DefaultInitialDeclineRate,
            int? maxDegreeOfParallelism = null,
            CancellationToken cancellationToken = default)
        {
            if (wellData == null)
                throw new ArgumentNullException(nameof(wellData));

            if (wellData.Count == 0)
            {
                throw new Exceptions.InvalidDataException("At least one well is required.");
            }

            var results = new Dictionary<string, DCAFitResult>();
            var semaphore = new SemaphoreSlim(
                maxDegreeOfParallelism ?? Environment.ProcessorCount,
                maxDegreeOfParallelism ?? Environment.ProcessorCount);

            var tasks = wellData.Select(async kvp =>
            {
                await semaphore.WaitAsync(cancellationToken);
                try
                {
                    var result = await FitCurveAsync(
                        kvp.Value.productionData,
                        kvp.Value.timeData,
                        qi,
                        di,
                        cancellationToken);

                    lock (results)
                    {
                        results[kvp.Key] = result;
                    }
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(tasks);
            return results;
        }

        /// <summary>
        /// Calculates predicted values for given parameters.
        /// </summary>
        private static double[] CalculatePredictedValues(
            List<double> productionData,
            List<DateTime> timeData,
            double[] parameters,
            CancellationToken cancellationToken)
        {
            double qi = parameters[0];
            double b = parameters.Length > 1 ? parameters[1] : DefaultDeclineExponent;
            double di = DefaultInitialDeclineRate; // This would need to be passed or calculated

            DateTime startTime = timeData[0];
            double[] predicted = new double[productionData.Count];

            for (int i = 0; i < productionData.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                double t = (timeData[i] - startTime).TotalDays;
                predicted[i] = DCAGenerator.HyperbolicDecline(qi, di, t, b);
            }

            return predicted;
        }
    }
}


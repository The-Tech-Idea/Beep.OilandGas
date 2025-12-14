using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.DCA.Constants;
using Beep.OilandGas.DCA.Exceptions;
using static Beep.OilandGas.DCA.Constants.DCAConstants;

namespace Beep.OilandGas.DCA.Statistics
{
    /// <summary>
    /// Provides statistical analysis methods for decline curve analysis results.
    /// </summary>
    public static class StatisticalAnalysis
    {
        /// <summary>
        /// Calculates the coefficient of determination (R²) for a fitted decline curve.
        /// R² measures how well the model fits the data (0 to 1, where 1 is perfect fit).
        /// </summary>
        /// <param name="observedValues">Observed production values.</param>
        /// <param name="predictedValues">Predicted production values from the model.</param>
        /// <returns>R² value between 0 and 1.</returns>
        /// <exception cref="ArgumentNullException">Thrown when input arrays are null.</exception>
        /// <exception cref="Exceptions.InvalidDataException">Thrown when arrays have mismatched lengths.</exception>
        public static double CalculateRSquared(IEnumerable<double> observedValues, IEnumerable<double> predictedValues)
        {
            if (observedValues == null)
                throw new ArgumentNullException(nameof(observedValues));
            if (predictedValues == null)
                throw new ArgumentNullException(nameof(predictedValues));

            var observed = observedValues.ToArray();
            var predicted = predictedValues.ToArray();

            if (observed.Length != predicted.Length)
            {
                throw new Exceptions.InvalidDataException(
                    $"Observed values length ({observed.Length}) must match predicted values length ({predicted.Length}).");
            }

            if (observed.Length < MinDataPoints)
            {
                throw new Exceptions.InvalidDataException(
                    $"At least {MinDataPoints} data points are required. Provided: {observed.Length}.");
            }

            double meanObserved = observed.Average();
            double totalSumSquares = observed.Sum(x => Math.Pow(x - meanObserved, 2));
            double residualSumSquares = observed.Zip(predicted, (obs, pred) => Math.Pow(obs - pred, 2)).Sum();

            if (Math.Abs(totalSumSquares) < Epsilon)
            {
                // All observed values are the same
                return double.IsNaN(residualSumSquares) || residualSumSquares > Epsilon ? 0.0 : 1.0;
            }

            double rSquared = 1.0 - (residualSumSquares / totalSumSquares);
            return Math.Max(0.0, Math.Min(1.0, rSquared)); // Clamp between 0 and 1
        }

        /// <summary>
        /// Calculates the adjusted R² which accounts for the number of parameters in the model.
        /// </summary>
        /// <param name="rSquared">The R² value.</param>
        /// <param name="n">Number of data points.</param>
        /// <param name="p">Number of parameters in the model.</param>
        /// <returns>Adjusted R² value.</returns>
        public static double CalculateAdjustedRSquared(double rSquared, int n, int p)
        {
            if (n <= p)
            {
                throw new Exceptions.InvalidDataException(
                    $"Number of data points ({n}) must be greater than number of parameters ({p}).");
            }

            if (n < MinDataPoints)
            {
                throw new Exceptions.InvalidDataException(
                    $"At least {MinDataPoints} data points are required. Provided: {n}.");
            }

            double adjustedRSquared = 1.0 - ((1.0 - rSquared) * (n - 1) / (n - p - 1));
            return Math.Max(0.0, Math.Min(1.0, adjustedRSquared));
        }

        /// <summary>
        /// Calculates residuals (observed - predicted) for each data point.
        /// </summary>
        /// <param name="observedValues">Observed production values.</param>
        /// <param name="predictedValues">Predicted production values.</param>
        /// <returns>Array of residuals.</returns>
        public static double[] CalculateResiduals(IEnumerable<double> observedValues, IEnumerable<double> predictedValues)
        {
            if (observedValues == null)
                throw new ArgumentNullException(nameof(observedValues));
            if (predictedValues == null)
                throw new ArgumentNullException(nameof(predictedValues));

            var observed = observedValues.ToArray();
            var predicted = predictedValues.ToArray();

            if (observed.Length != predicted.Length)
            {
                throw new Exceptions.InvalidDataException(
                    $"Observed values length ({observed.Length}) must match predicted values length ({predicted.Length}).");
            }

            return observed.Zip(predicted, (obs, pred) => obs - pred).ToArray();
        }

        /// <summary>
        /// Calculates the root mean square error (RMSE) of the residuals.
        /// </summary>
        /// <param name="residuals">Array of residuals.</param>
        /// <returns>RMSE value.</returns>
        public static double CalculateRMSE(IEnumerable<double> residuals)
        {
            if (residuals == null)
                throw new ArgumentNullException(nameof(residuals));

            var residualArray = residuals.ToArray();
            if (residualArray.Length == 0)
            {
                throw new Exceptions.InvalidDataException("Residuals array cannot be empty.");
            }

            double meanSquaredError = residualArray.Average(r => r * r);
            return Math.Sqrt(meanSquaredError);
        }

        /// <summary>
        /// Calculates the mean absolute error (MAE) of the residuals.
        /// </summary>
        /// <param name="residuals">Array of residuals.</param>
        /// <returns>MAE value.</returns>
        public static double CalculateMAE(IEnumerable<double> residuals)
        {
            if (residuals == null)
                throw new ArgumentNullException(nameof(residuals));

            var residualArray = residuals.ToArray();
            if (residualArray.Length == 0)
            {
                throw new Exceptions.InvalidDataException("Residuals array cannot be empty.");
            }

            return residualArray.Average(r => Math.Abs(r));
        }

        /// <summary>
        /// Calculates the Akaike Information Criterion (AIC) for model comparison.
        /// Lower AIC values indicate better models.
        /// </summary>
        /// <param name="n">Number of data points.</param>
        /// <param name="residualSumSquares">Sum of squared residuals.</param>
        /// <param name="k">Number of parameters in the model.</param>
        /// <returns>AIC value.</returns>
        public static double CalculateAIC(int n, double residualSumSquares, int k)
        {
            if (n <= k)
            {
                throw new Exceptions.InvalidDataException(
                    $"Number of data points ({n}) must be greater than number of parameters ({k}).");
            }

            if (residualSumSquares < 0)
            {
                throw new Exceptions.InvalidDataException("Residual sum of squares cannot be negative.");
            }

            // AIC = n * ln(RSS/n) + 2*k
            double rssOverN = residualSumSquares / n;
            if (rssOverN <= 0)
            {
                return double.PositiveInfinity;
            }

            return n * Math.Log(rssOverN) + 2.0 * k;
        }

        /// <summary>
        /// Calculates the Bayesian Information Criterion (BIC) for model comparison.
        /// Lower BIC values indicate better models.
        /// </summary>
        /// <param name="n">Number of data points.</param>
        /// <param name="residualSumSquares">Sum of squared residuals.</param>
        /// <param name="k">Number of parameters in the model.</param>
        /// <returns>BIC value.</returns>
        public static double CalculateBIC(int n, double residualSumSquares, int k)
        {
            if (n <= k)
            {
                throw new Exceptions.InvalidDataException(
                    $"Number of data points ({n}) must be greater than number of parameters ({k}).");
            }

            if (residualSumSquares < 0)
            {
                throw new Exceptions.InvalidDataException("Residual sum of squares cannot be negative.");
            }

            // BIC = n * ln(RSS/n) + k * ln(n)
            double rssOverN = residualSumSquares / n;
            if (rssOverN <= 0)
            {
                return double.PositiveInfinity;
            }

            return n * Math.Log(rssOverN) + k * Math.Log(n);
        }

        /// <summary>
        /// Calculates confidence intervals for fitted parameters using standard errors.
        /// </summary>
        /// <param name="parameters">Fitted parameter values.</param>
        /// <param name="residuals">Residuals from the fit.</param>
        /// <param name="confidenceLevel">Confidence level (e.g., 0.95 for 95% confidence).</param>
        /// <returns>Array of tuples containing (lowerBound, upperBound) for each parameter.</returns>
        public static (double lowerBound, double upperBound)[] CalculateConfidenceIntervals(
            double[] parameters,
            double[] residuals,
            double confidenceLevel = 0.95)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));
            if (residuals == null)
                throw new ArgumentNullException(nameof(residuals));

            if (confidenceLevel <= 0 || confidenceLevel >= 1)
            {
                throw new Exceptions.InvalidDataException(
                    $"Confidence level must be between 0 and 1. Provided: {confidenceLevel}.");
            }

            int n = residuals.Length;
            int k = parameters.Length;

            if (n <= k)
            {
                throw new Exceptions.InvalidDataException(
                    $"Number of residuals ({n}) must be greater than number of parameters ({k}).");
            }

            // Calculate standard error of residuals
            double residualVariance = residuals.Sum(r => r * r) / (n - k);
            double residualStdError = Math.Sqrt(residualVariance);

            // Calculate t-statistic for confidence level
            // Using approximation: t ≈ 1.96 for 95% confidence (normal approximation)
            double alpha = 1.0 - confidenceLevel;
            double tValue = GetTValue(alpha / 2, n - k);

            // Approximate standard errors for parameters
            // This is a simplified approach; full implementation would use covariance matrix
            double[] standardErrors = new double[k];
            for (int i = 0; i < k; i++)
            {
                // Simplified: use a fraction of parameter value as standard error estimate
                standardErrors[i] = Math.Abs(parameters[i]) * 0.1; // 10% of parameter value
            }

            // Calculate confidence intervals
            var intervals = new (double lowerBound, double upperBound)[k];
            for (int i = 0; i < k; i++)
            {
                double margin = tValue * standardErrors[i];
                intervals[i] = (parameters[i] - margin, parameters[i] + margin);
            }

            return intervals;
        }

        /// <summary>
        /// Calculates prediction intervals for forecasted values.
        /// </summary>
        /// <param name="predictedValue">The predicted production value.</param>
        /// <param name="residualStdError">Standard error of residuals.</param>
        /// <param name="confidenceLevel">Confidence level (e.g., 0.95 for 95% confidence).</param>
        /// <param name="degreesOfFreedom">Degrees of freedom (n - k).</param>
        /// <returns>Tuple containing (lowerBound, upperBound) prediction interval.</returns>
        public static (double lowerBound, double upperBound) CalculatePredictionInterval(
            double predictedValue,
            double residualStdError,
            double confidenceLevel = 0.95,
            int degreesOfFreedom = 100)
        {
            if (predictedValue < 0)
            {
                throw new Exceptions.InvalidDataException("Predicted value cannot be negative.");
            }

            if (residualStdError < 0)
            {
                throw new Exceptions.InvalidDataException("Residual standard error cannot be negative.");
            }

            if (confidenceLevel <= 0 || confidenceLevel >= 1)
            {
                throw new Exceptions.InvalidDataException(
                    $"Confidence level must be between 0 and 1. Provided: {confidenceLevel}.");
            }

            double alpha = 1.0 - confidenceLevel;
            double tValue = GetTValue(alpha / 2, degreesOfFreedom);

            // Prediction interval includes both model uncertainty and residual uncertainty
            double margin = tValue * residualStdError * Math.Sqrt(1 + 1.0 / degreesOfFreedom);

            double lowerBound = Math.Max(0, predictedValue - margin);
            double upperBound = predictedValue + margin;

            return (lowerBound, upperBound);
        }

        /// <summary>
        /// Gets the t-value for a given alpha and degrees of freedom.
        /// Uses approximation for common confidence levels.
        /// </summary>
        private static double GetTValue(double alpha, int degreesOfFreedom)
        {
            // Simplified t-value lookup for common cases
            // For production use, consider using a proper statistical library
            if (degreesOfFreedom >= 30)
            {
                // Normal approximation for large samples
                if (Math.Abs(alpha - 0.025) < 0.001) return 1.96; // 95% confidence
                if (Math.Abs(alpha - 0.05) < 0.001) return 1.645; // 90% confidence
                if (Math.Abs(alpha - 0.005) < 0.001) return 2.576; // 99% confidence
            }

            // For smaller samples, use conservative estimate
            // In production, use proper t-distribution lookup table or library
            if (degreesOfFreedom >= 20)
            {
                if (Math.Abs(alpha - 0.025) < 0.001) return 2.086;
                if (Math.Abs(alpha - 0.05) < 0.001) return 1.725;
            }

            // Default: use normal approximation
            return 1.96;
        }
    }
}


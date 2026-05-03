using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.DCA.Statistics;

namespace Beep.OilandGas.DCA.Results
{
    /// <summary>
    /// Represents the results of a decline curve analysis fit, including statistical metrics.
    /// </summary>
    public class DCAFitResult
    {
        /// <summary>
        /// Gets the fitted parameters (e.g., [qi, b] for hyperbolic decline).
        /// </summary>
        public double[] Parameters { get; }

        /// <summary>
        /// Gets the observed production values used in the fit.
        /// </summary>
        public double[] ObservedValues { get; }

        /// <summary>
        /// Gets the predicted production values from the fitted model.
        /// </summary>
        public double[] PredictedValues { get; }

        /// <summary>
        /// Gets the residuals (observed - predicted).
        /// </summary>
        public double[] Residuals { get; }

        /// <summary>
        /// Gets the R² (coefficient of determination) value.
        /// </summary>
        public double RSquared { get; }

        /// <summary>
        /// Gets the adjusted R² value.
        /// </summary>
        public double AdjustedRSquared { get; }

        /// <summary>
        /// Gets the root mean square error (RMSE).
        /// </summary>
        public double RMSE { get; }

        /// <summary>
        /// Gets the mean absolute error (MAE).
        /// </summary>
        public double MAE { get; }

        /// <summary>
        /// Gets the Akaike Information Criterion (AIC).
        /// </summary>
        public double AIC { get; }

        /// <summary>
        /// Gets the Bayesian Information Criterion (BIC).
        /// </summary>
        public double BIC { get; }

        /// <summary>
        /// Gets the confidence intervals for each parameter.
        /// </summary>
        public (double lowerBound, double upperBound)[] ConfidenceIntervals { get; }

        /// <summary>
        /// Gets the number of iterations performed during fitting.
        /// </summary>
        public int Iterations { get; }

        /// <summary>
        /// Gets whether the fit converged successfully.
        /// </summary>
        public bool Converged { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DCAFitResult"/> class.
        /// </summary>
        /// <param name="parameters">Fitted parameters.</param>
        /// <param name="observedValues">Observed production values.</param>
        /// <param name="predictedValues">Predicted production values.</param>
        /// <param name="iterations">Number of iterations performed.</param>
        /// <param name="converged">Whether the fit converged.</param>
        /// <param name="confidenceLevel">Confidence level for intervals (default 0.95).</param>
        public DCAFitResult(
            double[] parameters,
            double[] observedValues,
            double[] predictedValues,
            int iterations,
            bool converged,
            double confidenceLevel = 0.95)
        {
            Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
            ObservedValues = observedValues ?? throw new ArgumentNullException(nameof(observedValues));
            PredictedValues = predictedValues ?? throw new ArgumentNullException(nameof(predictedValues));

            if (observedValues.Length != predictedValues.Length)
            {
                throw new ArgumentException(
                    "Observed and predicted values must have the same length.",
                    nameof(predictedValues));
            }

            Iterations = iterations;
            Converged = converged;

            // Calculate residuals
            Residuals = StatisticalAnalysis.CalculateResiduals(observedValues, predictedValues);

            // Calculate statistical metrics
            RSquared = StatisticalAnalysis.CalculateRSquared(observedValues, predictedValues);
            AdjustedRSquared = StatisticalAnalysis.CalculateAdjustedRSquared(
                RSquared,
                observedValues.Length,
                parameters.Length);

            RMSE = StatisticalAnalysis.CalculateRMSE(Residuals);
            MAE = StatisticalAnalysis.CalculateMAE(Residuals);

            double residualSumSquares = Residuals.Sum(r => r * r);
            AIC = StatisticalAnalysis.CalculateAIC(observedValues.Length, residualSumSquares, parameters.Length);
            BIC = StatisticalAnalysis.CalculateBIC(observedValues.Length, residualSumSquares, parameters.Length);

            // Calculate confidence intervals
            ConfidenceIntervals = StatisticalAnalysis.CalculateConfidenceIntervals(
                parameters,
                Residuals,
                confidenceLevel);
        }

        /// <summary>
        /// Gets a summary string of the fit results.
        /// </summary>
        /// <returns>Formatted summary string.</returns>
        public string GetSummary()
        {
            return $"Fit Results:\n" +
                   $"  Converged: {Converged}\n" +
                   $"  Iterations: {Iterations}\n" +
                   $"  R²: {RSquared:F4}\n" +
                   $"  Adjusted R²: {AdjustedRSquared:F4}\n" +
                   $"  RMSE: {RMSE:F2}\n" +
                   $"  MAE: {MAE:F2}\n" +
                   $"  AIC: {AIC:F2}\n" +
                   $"  BIC: {BIC:F2}\n" +
                   $"  Parameters: [{string.Join(", ", Parameters.Select(p => p.ToString("F4")))}]";
        }
    }
}


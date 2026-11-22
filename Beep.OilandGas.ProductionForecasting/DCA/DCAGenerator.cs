using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.DCA.Constants;
using Beep.OilandGas.DCA.Exceptions;
using Beep.OilandGas.DCA.Validation;
using static Beep.OilandGas.DCA.Constants.DCAConstants;

namespace Beep.OilandGas.DCA
{
    /// <summary>
    /// Provides methods for generating decline curves using various decline models.
    /// Supports exponential, harmonic, and hyperbolic decline methods.
    /// </summary>
    public static class DCAGenerator
    {
        /// <summary>
        /// Calculates production rate using exponential decline model.
        /// Formula: q(t) = qi * exp(-Di * t)
        /// </summary>
        /// <param name="qi">Initial production rate.</param>
        /// <param name="di">Initial decline rate (1/time).</param>
        /// <param name="t">Time since start of production.</param>
        /// <returns>Production rate at time t.</returns>
        /// <exception cref="Exceptions.InvalidDataException">Thrown when input parameters are invalid.</exception>
        public static double ExponentialDecline(double qi, double di, double t)
        {
            DataValidator.ValidateInitialProductionRate(qi, nameof(qi));
            DataValidator.ValidateInitialDeclineRate(di, nameof(di));

            if (t < 0)
            {
                throw new Exceptions.InvalidDataException($"Time parameter {nameof(t)} must be non-negative. Provided: {t}.");
            }

            return qi * Math.Exp(-di * t);
        }

        /// <summary>
        /// Calculates production rate using harmonic decline model.
        /// Formula: q(t) = qi / (1 + Di * t)
        /// </summary>
        /// <param name="qi">Initial production rate.</param>
        /// <param name="di">Initial decline rate (1/time).</param>
        /// <param name="t">Time since start of production.</param>
        /// <returns>Production rate at time t.</returns>
        /// <exception cref="Exceptions.InvalidDataException">Thrown when input parameters are invalid.</exception>
        public static double HarmonicDecline(double qi, double di, double t)
        {
            DataValidator.ValidateInitialProductionRate(qi, nameof(qi));
            DataValidator.ValidateInitialDeclineRate(di, nameof(di));

            if (t < 0)
            {
                throw new Exceptions.InvalidDataException($"Time parameter {nameof(t)} must be non-negative. Provided: {t}.");
            }

            if (Math.Abs(1 + di * t) < Epsilon)
            {
                throw new Exceptions.InvalidDataException($"Division by zero: 1 + di * t = 0. di={di}, t={t}.");
            }

            return qi / (1 + di * t);
        }

        /// <summary>
        /// Calculates production rate using hyperbolic decline model.
        /// Formula: q(t) = qi / (1 + b * Di * t)^(1/b)
        /// </summary>
        /// <param name="qi">Initial production rate.</param>
        /// <param name="di">Initial decline rate (1/time).</param>
        /// <param name="t">Time since start of production.</param>
        /// <param name="b">Decline exponent (0 &lt; b &lt; 1).</param>
        /// <returns>Production rate at time t.</returns>
        /// <exception cref="Exceptions.InvalidDataException">Thrown when input parameters are invalid.</exception>
        public static double HyperbolicDecline(double qi, double di, double t, double b)
        {
            DataValidator.ValidateInitialProductionRate(qi, nameof(qi));
            DataValidator.ValidateInitialDeclineRate(di, nameof(di));
            DataValidator.ValidateDeclineExponent(b, nameof(b));

            if (t < 0)
            {
                throw new Exceptions.InvalidDataException($"Time parameter {nameof(t)} must be non-negative. Provided: {t}.");
            }

            double denominator = 1 + b * di * t;
            if (Math.Abs(denominator) < Epsilon)
            {
                throw new Exceptions.InvalidDataException($"Division by zero: 1 + b * di * t = 0. b={b}, di={di}, t={t}.");
            }

            if (Math.Abs(b) < Epsilon)
            {
                throw new Exceptions.InvalidDataException($"Decline exponent b cannot be zero for hyperbolic decline.");
            }

            return qi / Math.Pow(denominator, 1.0 / b);
        }
        /// <summary>
        /// Estimates initial production rate (qi), cumulative production, and decline exponent (b) 
        /// from production data using the specified decline exponent B.
        /// </summary>
        /// <param name="b">Decline exponent to use in calculations.</param>
        /// <param name="productionData">Array of production rate values.</param>
        /// <param name="timeData">Array of DateTime values corresponding to production data.</param>
        /// <returns>Array containing [initialProductionRate, cumulativeProduction, estimatedB].</returns>
        /// <exception cref="ArgumentNullException">Thrown when input arrays are null.</exception>
        /// <exception cref="InvalidDataException">Thrown when input data is invalid.</exception>
        public static double[] EstimateQiDiB(double b, double[] productionData, DateTime[] timeData)
        {
            if (productionData == null)
                throw new ArgumentNullException(nameof(productionData));
            if (timeData == null)
                throw new ArgumentNullException(nameof(timeData));

            DataValidator.ValidateProductionData(productionData, nameof(productionData));
            DataValidator.ValidateTimeData(timeData, productionData.Length, nameof(timeData));
            DataValidator.ValidateDeclineExponent(b, nameof(b));

            if (productionData.Length != timeData.Length)
            {
                throw new Exceptions.InvalidDataException(
                    $"Production data length ({productionData.Length}) must match time data length ({timeData.Length}).");
            }

            double initialProductionRate = productionData[0];
            double cumulativeProduction = 0.0;
            double timeSinceFirstProduction = 0.0;
            double[] qOverQt = new double[productionData.Length];

            DateTime startTime = timeData[0];

            for (int i = 0; i < productionData.Length; i++)
            {
                double qi = initialProductionRate;
                double qt = productionData[i];
                double t = (timeData[i] - startTime).TotalDays;

                if (Math.Abs(qi) < Epsilon)
                {
                    throw new Exceptions.InvalidDataException($"Initial production rate cannot be zero.");
                }

                qOverQt[i] = qt / qi;

                if (Math.Abs(b) > Epsilon && Math.Abs(qt) > Epsilon)
                {
                    double ratio = qi / qt;
                    double exponent = (1.0 - b) / b;
                    cumulativeProduction += (qi - qt) / b * Math.Pow(ratio, exponent) * (Math.Exp(-b * t) - 1.0);
                }

                timeSinceFirstProduction = t;
            }

            if (timeSinceFirstProduction < Epsilon)
            {
                throw new Exceptions.InvalidDataException("Time span is too small for reliable estimation.");
            }

            if (qOverQt[0] < Epsilon || qOverQt[qOverQt.Length - 1] < Epsilon)
            {
                throw new Exceptions.InvalidDataException("Production rate ratio cannot be zero.");
            }

            double estimatedB = (Math.Log(qOverQt[qOverQt.Length - 1]) - Math.Log(qOverQt[0])) / 
                                 (timeSinceFirstProduction / DaysPerYear);

            return new double[] { initialProductionRate, cumulativeProduction, estimatedB };
        }
        /// <summary>
        /// Calculates production rate for an oil well using hyperbolic decline.
        /// </summary>
        /// <param name="t">Time point for calculation.</param>
        /// <param name="qi">Initial production rate.</param>
        /// <param name="b">Decline exponent.</param>
        /// <param name="di">Initial decline rate.</param>
        /// <returns>Production rate at time t.</returns>
        private static double CalculateProductionRate(DateTime t, double qi, double b, double di)
        {
            double days = (t - DCAConstants.ReferenceDate).TotalDays;
            double denominator = 1 + b * di * days;
            
            if (Math.Abs(denominator) < Epsilon || Math.Abs(b) < Epsilon)
            {
                throw new Exceptions.InvalidDataException("Invalid parameters for production rate calculation.");
            }

            double q = qi / Math.Pow(denominator, 1.0 / b);
            return q;
        }

        /// <summary>
        /// Calculates production rate for a gas well using hyperbolic decline.
        /// </summary>
        /// <param name="t">Time point for calculation.</param>
        /// <param name="qi">Initial production rate.</param>
        /// <param name="b">Decline exponent.</param>
        /// <param name="di">Initial decline rate.</param>
        /// <returns>Production rate at time t.</returns>
        private static double CalculateGasWellProductionRate(DateTime t, double qi, double b, double di)
        {
            double days = (t - DCAConstants.ReferenceDate).TotalDays;
            double denominator = 1 + b * di * days;
            
            if (Math.Abs(denominator) < Epsilon || Math.Abs(b) < Epsilon)
            {
                throw new Exceptions.InvalidDataException("Invalid parameters for gas well production rate calculation.");
            }

            double q = qi / Math.Pow(denominator, (1.0 - b) / b);
            return q;
        }
        /// <summary>
        /// Fits a decline curve to production data using nonlinear regression.
        /// </summary>
        /// <param name="productionData">List of production rate values.</param>
        /// <param name="timeData">List of DateTime values corresponding to production data.</param>
        /// <param name="qi">Initial guess for initial production rate.</param>
        /// <param name="di">Initial guess for initial decline rate.</param>
        /// <returns>Array of fitted parameters [qi, b] where qi is initial production rate and b is decline exponent.</returns>
        /// <exception cref="ArgumentNullException">Thrown when input lists are null.</exception>
        /// <exception cref="InvalidDataException">Thrown when input data is invalid.</exception>
        /// <exception cref="ConvergenceException">Thrown when curve fitting fails to converge.</exception>
        public static double[] FitCurve(List<double> productionData, List<DateTime> timeData, double qi, double di)
        {
            if (productionData == null)
                throw new ArgumentNullException(nameof(productionData));
            if (timeData == null)
                throw new ArgumentNullException(nameof(timeData));

            DataValidator.ValidateProductionData(productionData, nameof(productionData));
            DataValidator.ValidateTimeData(timeData, productionData.Count, nameof(timeData));
            DataValidator.ValidateInitialProductionRate(qi, nameof(qi));
            DataValidator.ValidateInitialDeclineRate(di, nameof(di));

            double[] p = new double[] { qi, DefaultDeclineExponent };
            
            // Basis functions for the least-squares regression
            Func<DateTime, double>[] basis = new Func<DateTime, double>[] {
                t => 1.0,
                t => Math.Pow((1 + p[1] * di * (t - ReferenceDate).TotalDays), (-1.0 / p[1]))
            };

            // Perform the least-squares regression to estimate the parameters
            double[] parameters = NonlinearRegression.Solve(
                basis, 
                productionData.ToArray(), 
                timeData.ToArray(), 
                p, 
                DefaultMaxIterations, 
                DefaultTolerance);

            return parameters;
        }
        /// <summary>
        /// The outcome of <see cref="TryFitDecline"/>: the Arps parameters, the fit quality and the predicted rates.
        /// </summary>
        /// <param name="Qi">Initial rate.</param>
        /// <param name="Di">Initial nominal decline rate, per day.</param>
        /// <param name="B">The decline exponent the fit settled on.</param>
        /// <param name="RSquared">Coefficient of determination in ln(rate) space.</param>
        /// <param name="DiPerDay">True when <paramref name="Di"/> is per day.</param>
        /// <param name="Predicted">The model's rate at each observation.</param>
        /// <param name="PointsUsed">How many observations went into the fit.</param>
        /// <param name="PinnedAtMinimum">The best b the data allows is at or below <c>minimumB</c>.</param>
        /// <param name="PinnedAtMaximum">The best b the data allows is at or above <c>maximumB</c> - the fit is unbounded.</param>
        public readonly record struct ArpsFit(
            double Qi,
            double Di,
            double B,
            double RSquared,
            bool DiPerDay,
            double[] Predicted,
            int PointsUsed,
            bool PinnedAtMinimum,
            bool PinnedAtMaximum);

        /// <summary>
        /// Fits an Arps decline (qi, Di, b) to rate-time observations, holding b inside the bounds the caller states and
        /// reporting whether the data wanted to leave them.
        /// </summary>
        /// <remarks>
        /// For a fixed b the model is linear: <c>q^-b = qi^-b (1 + b Di t)</c>, so qi and Di come from an exact ordinary
        /// least-squares line of <c>q^-b</c> on <c>t</c> (<c>b = 0</c> is the exponential case, linear in <c>ln q</c>).
        /// b itself is then found by a golden-section search on the sum of squared <c>ln(rate)</c> residuals, restricted
        /// to <c>[minimumB, maximumB]</c>. A result pinned at <c>maximumB</c> means the data wanted a b the caller does
        /// not allow, which is what lets a screen refuse an unbounded fit instead of reporting the bound as an answer.
        /// <para>
        /// <c>FitCurve</c> is unchanged and keeps its signature; this method is the bounded, self-checking form a caller
        /// that must show a refusal should use.
        /// </para>
        /// </remarks>
        public static bool TryFitDecline(
            IReadOnlyList<double> productionData,
            IReadOnlyList<DateTime> timeData,
            decimal minimumB,
            decimal maximumB,
            out ArpsFit fit)
        {
            if (productionData == null)
                throw new ArgumentNullException(nameof(productionData));

            if (timeData == null)
                throw new ArgumentNullException(nameof(timeData));

            if (productionData.Count != timeData.Count)
            {
                throw new Exceptions.InvalidDataException(
                    $"Production data length ({productionData.Count}) must match time data length ({timeData.Count}).");
            }

            if (minimumB < 0m || maximumB > 1m || maximumB <= minimumB)
            {
                throw new Exceptions.InvalidDataException(
                    $"The decline exponent bounds must satisfy 0 <= minimumB < maximumB <= 1; got {minimumB} and {maximumB}.");
            }

            fit = default;

            if (productionData.Count < 2)
            {
                return false;
            }

            // Work in days from the first observation, so qi is the rate at that observation.
            var origin = timeData[0];
            var times = timeData.Select(date => (date - origin).TotalDays).ToArray();

            if (productionData.Any(rate => rate <= 0.0) || times.Any(time => time < 0.0))
            {
                return false;
            }

            var low = (double)minimumB;
            var high = (double)maximumB;

            // At b -> 0 the q^-b transform loses all precision, so a b this small is the exponential case exactly.
            const double exponentialThreshold = 1e-6;

            double Sse(double b)
            {
                var candidate = b <= exponentialThreshold ? 0.0 : b;

                if (!TryLinearFit(productionData, times, candidate, out var qi, out var di) || qi <= 0.0 || di < 0.0)
                {
                    return double.PositiveInfinity;
                }

                var total = 0.0;

                for (var index = 0; index < productionData.Count; index++)
                {
                    var predicted = Rate(qi, di, times[index], candidate);

                    if (predicted <= 0.0 || double.IsNaN(predicted) || double.IsInfinity(predicted))
                    {
                        return double.PositiveInfinity;
                    }

                    var residual = Math.Log(productionData[index]) - Math.Log(predicted);
                    total += residual * residual;
                }

                return total;
            }

            // Golden-section search for b inside the caller's bounds.
            const double ratio = 0.6180339887498949;
            var left = low;
            var right = high;
            var x1 = right - ratio * (right - left);
            var x2 = left + ratio * (right - left);
            var f1 = Sse(x1);
            var f2 = Sse(x2);

            for (var iteration = 0; iteration < 80 && right - left > 1e-9; iteration++)
            {
                if (f1 < f2)
                {
                    right = x2;
                    x2 = x1;
                    f2 = f1;
                    x1 = right - ratio * (right - left);
                    f1 = Sse(x1);
                }
                else
                {
                    left = x1;
                    x1 = x2;
                    f1 = f2;
                    x2 = left + ratio * (right - left);
                    f2 = Sse(x2);
                }
            }

            // The optimum is whichever of the search result and the two bounds has the smallest residual: a search
            // restricted to an interval reports a bound when the data wants to leave it, and that has to be exact.
            var bestB = (left + right) / 2.0;
            var bestSse = Sse(bestB);

            foreach (var boundary in new[] { low, high })
            {
                var boundarySse = Sse(boundary);

                if (boundarySse < bestSse)
                {
                    bestSse = boundarySse;
                    bestB = boundary;
                }
            }

            if (bestB <= exponentialThreshold)
            {
                bestB = 0.0;
            }

            if (!TryLinearFit(productionData, times, bestB, out var bestQi, out var bestDi))
            {
                return false;
            }

            if (bestQi <= 0.0 || bestDi < 0.0)
            {
                return false;
            }

            var predictedValues = new double[productionData.Count];

            for (var index = 0; index < productionData.Count; index++)
            {
                predictedValues[index] = Rate(bestQi, bestDi, times[index], bestB);
            }

            var mean = productionData.Average(rate => Math.Log(rate));
            var ssTotal = productionData.Sum(rate => Math.Pow(Math.Log(rate) - mean, 2));
            var ssResidual = productionData
                .Select((rate, index) => Math.Pow(Math.Log(rate) - Math.Log(predictedValues[index]), 2))
                .Sum();

            // A b within 1e-6 of a bound is the data asking for a value outside it, not a value at it.
            var pinTolerance = 1e-6;

            fit = new ArpsFit(
                bestQi,
                bestDi,
                bestB,
                ssTotal > 1e-12 ? 1.0 - ssResidual / ssTotal : 1.0,
                true,
                predictedValues,
                productionData.Count,
                bestB <= low + pinTolerance,
                bestB >= high - pinTolerance);

            return true;
        }

        /// <summary>
        /// Fits qi and Di with b held at the caller's value - the fixed-b form of <see cref="TryFitDecline"/>, for a
        /// screen that offers exponential (b = 0), harmonic (b = 1) or a hyperbolic fit at a b the engineer chose.
        /// </summary>
        public static bool TryFitDeclineAtB(
            IReadOnlyList<double> productionData,
            IReadOnlyList<DateTime> timeData,
            decimal b,
            out ArpsFit fit)
        {
            if (productionData == null)
                throw new ArgumentNullException(nameof(productionData));

            if (timeData == null)
                throw new ArgumentNullException(nameof(timeData));

            if (productionData.Count != timeData.Count)
            {
                throw new Exceptions.InvalidDataException(
                    $"Production data length ({productionData.Count}) must match time data length ({timeData.Count}).");
            }

            if (b < 0m || b > 1m)
            {
                throw new Exceptions.InvalidDataException(
                    $"The decline exponent must be between 0 and 1 inclusive; got {b}.");
            }

            fit = default;

            if (productionData.Count < 2 ||
                productionData.Any(rate => rate <= 0.0))
            {
                return false;
            }

            var origin = timeData[0];
            var times = timeData.Select(date => (date - origin).TotalDays).ToArray();

            if (times.Any(time => time < 0.0))
            {
                return false;
            }

            var exponent = (double)b;

            if (!TryLinearFit(productionData, times, exponent, out var qi, out var di) || qi <= 0.0 || di < 0.0)
            {
                return false;
            }

            var predicted = new double[productionData.Count];

            for (var index = 0; index < productionData.Count; index++)
            {
                predicted[index] = Rate(qi, di, times[index], exponent);
            }

            var mean = productionData.Average(rate => Math.Log(rate));
            var ssTotal = productionData.Sum(rate => Math.Pow(Math.Log(rate) - mean, 2));
            var ssResidual = productionData
                .Select((rate, index) => Math.Pow(Math.Log(rate) - Math.Log(predicted[index]), 2))
                .Sum();

            fit = new ArpsFit(
                qi,
                di,
                exponent,
                ssTotal > 1e-12 ? 1.0 - ssResidual / ssTotal : 1.0,
                true,
                predicted,
                productionData.Count,
                false,
                false);

            return true;
        }

        /// <summary>The exact least-squares qi and Di for one candidate b (see <see cref="TryFitDecline"/>).</summary>
        private static bool TryLinearFit(
            IReadOnlyList<double> productionData,
            IReadOnlyList<double> times,
            double b,
            out double qi,
            out double di)
        {
            qi = 0.0;
            di = 0.0;

            var transformed = new double[productionData.Count];

            for (var index = 0; index < productionData.Count; index++)
            {
                transformed[index] = b == 0.0
                    ? Math.Log(productionData[index])
                    : Math.Pow(productionData[index], -b);
            }

            var meanTime = times.Average();
            var meanValue = transformed.Average();

            var sxx = 0.0;
            var sxy = 0.0;

            for (var index = 0; index < times.Count; index++)
            {
                var dx = times[index] - meanTime;
                sxx += dx * dx;
                sxy += dx * (transformed[index] - meanValue);
            }

            if (sxx <= 1e-15)
            {
                return false;
            }

            var slope = sxy / sxx;
            var intercept = meanValue - slope * meanTime;

            if (b == 0.0)
            {
                qi = Math.Exp(intercept);
                di = -slope;
            }
            else
            {
                if (intercept <= 0.0)
                {
                    return false;
                }

                qi = Math.Pow(intercept, -1.0 / b);
                di = slope / (b * intercept);
            }

            return !double.IsNaN(qi) && !double.IsNaN(di) && !double.IsInfinity(qi) && !double.IsInfinity(di);
        }

        /// <summary>The Arps rate at one time; b = 0 is exponential and b = 1 harmonic, as the same relation.</summary>
        private static double Rate(double qi, double di, double time, double b) =>
            b == 0.0 ? qi * Math.Exp(-di * time) : qi / Math.Pow(1.0 + b * di * time, 1.0 / b);

        /// <summary>
        /// Fits a decline curve to production data using custom basis functions and initial parameter guess.
        /// </summary>
        /// <param name="productionData">List of production rate values.</param>
        /// <param name="timeData">List of DateTime values corresponding to production data.</param>
        /// <param name="initialGuess">Initial guess for parameters.</param>
        /// <param name="basis">Basis functions for the regression model.</param>
        /// <returns>Array of fitted parameters.</returns>
        /// <exception cref="ArgumentNullException">Thrown when input parameters are null.</exception>
        /// <exception cref="InvalidDataException">Thrown when input data is invalid.</exception>
        /// <exception cref="ConvergenceException">Thrown when curve fitting fails to converge.</exception>
        public static double[] FitCurve(
            List<double> productionData, 
            List<DateTime> timeData, 
            double[] initialGuess, 
            Func<DateTime, double>[] basis)
        {
            if (productionData == null)
                throw new ArgumentNullException(nameof(productionData));
            if (timeData == null)
                throw new ArgumentNullException(nameof(timeData));
            if (initialGuess == null)
                throw new ArgumentNullException(nameof(initialGuess));
            if (basis == null)
                throw new ArgumentNullException(nameof(basis));

            DataValidator.ValidateProductionData(productionData, nameof(productionData));
            DataValidator.ValidateTimeData(timeData, productionData.Count, nameof(timeData));

            if (initialGuess.Length == 0)
            {
                throw new Exceptions.InvalidDataException("Initial guess array cannot be empty.");
            }

            if (basis.Length == 0)
            {
                throw new Exceptions.InvalidDataException("Basis functions array cannot be empty.");
            }

            if (initialGuess.Length != basis.Length)
            {
                throw new Exceptions.InvalidDataException(
                    $"Initial guess length ({initialGuess.Length}) must match basis functions length ({basis.Length}).");
            }

            // Perform the least-squares regression to estimate the parameters
            double[] parameters = NonlinearRegression.Solve(
                basis, 
                productionData.ToArray(), 
                timeData.ToArray(), 
                initialGuess, 
                DefaultMaxIterations, 
                DefaultTolerance);

            return parameters;
        }

        /// <summary>
        /// Estimates initial production rate (qi) and initial decline rate (di) using Material Balance Method (MBM).
        /// The b parameter is used to calculate the pressure decline in the reservoir.
        /// </summary>
        /// <param name="productionData">List of production rate values.</param>
        /// <param name="timeData">List of DateTime values corresponding to production data.</param>
        /// <param name="b">Decline exponent.</param>
        /// <param name="di">Initial guess for initial decline rate.</param>
        /// <returns>Array containing [estimatedQi, estimatedDi].</returns>
        /// <exception cref="ArgumentNullException">Thrown when input lists are null.</exception>
        /// <exception cref="InvalidDataException">Thrown when input data is invalid.</exception>
        /// <exception cref="ConvergenceException">Thrown when estimation fails to converge.</exception>
        public static double[] EstimateQiDi(List<double> productionData, List<DateTime> timeData, double b, double di)
        {
            if (productionData == null)
                throw new ArgumentNullException(nameof(productionData));
            if (timeData == null)
                throw new ArgumentNullException(nameof(timeData));

            DataValidator.ValidateProductionData(productionData, nameof(productionData));
            DataValidator.ValidateTimeData(timeData, productionData.Count, nameof(timeData));
            DataValidator.ValidateDeclineExponent(b, nameof(b));
            DataValidator.ValidateInitialDeclineRate(di, nameof(di));

            double[] p = new double[] { DefaultInitialProductionRate, DefaultInitialDeclineRate };
            double[] residuals = new double[productionData.Count];

            DateTime startTime = timeData.First();
            double[] qLog = productionData.Select(Math.Log10).ToArray();
            double[] tLog = timeData.Select(t => Math.Log10((t - startTime).TotalDays)).ToArray();

            for (int i = 0; i < DefaultMaxIterations; i++)
            {
                // Calculate the expected decline curve for the current qi and di
                for (int j = 0; j < productionData.Count; j++)
                {
                    var subsetProd = productionData.Take(j + 1).ToList();
                    var subsetTime = timeData.Take(j + 1).ToList();
                    var fittedParams = FitCurve(subsetProd, subsetTime, p[0], p[1]);
                    residuals[j] = qLog[j] - Math.Log10(fittedParams[0]);
                }

                // Calculate the Jacobian matrix
                double[,] jacobian = new double[productionData.Count, 2];

                for (int j = 0; j < productionData.Count; j++)
                {
                    double timeDays = (timeData[j] - startTime).TotalDays;
                    double denominator = 1 + p[1] * di * timeDays;
                    
                    if (Math.Abs(denominator) < Epsilon || Math.Abs(p[1]) < Epsilon)
                    {
                        throw new Exceptions.InvalidDataException("Invalid parameters in Jacobian calculation.");
                    }

                    double power = (1.0 - b) / b;
                    jacobian[j, 0] = 1.0 / (p[1] * Math.Log(10) * Math.Pow(denominator, power));
                    
                    if (timeDays > Epsilon)
                    {
                        double logTime = Math.Log10(timeDays);
                        double logDenom = Math.Log10(denominator);
                        jacobian[j, 1] = -(logTime / b) * (logTime + logDenom);
                    }
                    else
                    {
                        jacobian[j, 1] = 0.0;
                    }
                }

                // Solve the linear system to obtain the parameter updates
                double[] dp = SolveLinearSystem(jacobian, residuals);

                // Update the parameters
                p[0] = Math.Pow(10.0, Math.Log10(p[0]) + dp[0]);
                p[1] += dp[1];

                // Validate updated parameters
                if (p[0] <= 0 || p[1] <= 0)
                {
                    throw new ConvergenceException(
                        "Parameter estimation resulted in invalid values.",
                        i + 1,
                        Math.Max(Math.Abs(dp[0]), Math.Abs(dp[1])));
                }

                // Check for convergence
                if (Math.Abs(dp[0]) < DefaultTolerance && Math.Abs(dp[1]) < DefaultTolerance)
                {
                    return new double[] { p[0], p[1] };
                }
            }

            throw new ConvergenceException(
                "Failed to estimate qi and di within maximum iterations.",
                DefaultMaxIterations,
                Math.Max(Math.Abs(p[0] - DefaultInitialProductionRate), Math.Abs(p[1] - DefaultInitialDeclineRate)));
        }
        /// <summary>
        /// Estimates initial production rate for a gas well using empirical correlations.
        /// </summary>
        /// <param name="reservoirPressure">Reservoir pressure in psia.</param>
        /// <param name="depth">Well depth in feet.</param>
        /// <param name="gasGravity">Gas specific gravity (air = 1.0).</param>
        /// <param name="temperature">Reservoir temperature in Fahrenheit.</param>
        /// <param name="permeability">Formation permeability in millidarcies.</param>
        /// <returns>Estimated initial production rate in MMSCFD (Million Standard Cubic Feet per Day).</returns>
        /// <exception cref="InvalidDataException">Thrown when input parameters are invalid.</exception>
        public static double EstimateQiForGasWell(
            double reservoirPressure, 
            double depth, 
            double gasGravity, 
            double temperature, 
            double permeability)
        {
            if (reservoirPressure <= 0)
                throw new Exceptions.InvalidDataException($"Reservoir pressure must be positive. Provided: {reservoirPressure}.");
            if (depth <= 0)
                throw new Exceptions.InvalidDataException($"Depth must be positive. Provided: {depth}.");
            if (gasGravity <= 0)
                throw new Exceptions.InvalidDataException($"Gas gravity must be positive. Provided: {gasGravity}.");
            if (permeability <= 0)
                throw new Exceptions.InvalidDataException($"Permeability must be positive. Provided: {permeability}.");

            // Convert temperature from Fahrenheit to Celsius
            double tempCelsius = (temperature - 32) / FahrenheitToCelsius;
            
            // Convert pressure from psia to standard
            double pressureStandard = reservoirPressure / PsiaToStandard;

            // Calculate reservoir area (acres) using empirical correlation
            double area = Math.Exp(-3.9 + 0.07 * depth + 1.95 * Math.Log10(gasGravity) - 0.0014 * tempCelsius - 0.35 * Math.Log10(permeability));
            
            // Calculate reservoir thickness (ft) using empirical correlation
            double thickness = Math.Exp(-7.67 + 0.076 * depth + 2.6 * Math.Log10(gasGravity) - 0.00045 * tempCelsius - 0.28 * Math.Log10(permeability));

            // Calculate initial production rate (MMSCFD)
            double qi = 0.484 * area * thickness * Math.Sqrt(pressureStandard) * gasGravity;

            if (double.IsNaN(qi) || double.IsInfinity(qi) || qi <= 0)
            {
                throw new Exceptions.InvalidDataException($"Calculated initial production rate is invalid: {qi}.");
            }

            return qi;
        }
        /// <summary>
        /// Solves a system of linear equations using Gaussian elimination with partial pivoting.
        /// </summary>
        /// <param name="A">Coefficient matrix (n x n).</param>
        /// <param name="b">Right-hand side vector (n).</param>
        /// <returns>Solution vector x such that A*x = b.</returns>
        /// <exception cref="ArgumentNullException">Thrown when input arrays are null.</exception>
        /// <exception cref="InvalidDataException">Thrown when matrix is singular or invalid.</exception>
        private static double[] SolveLinearSystem(double[,] A, double[] b)
        {
            if (A == null)
                throw new ArgumentNullException(nameof(A));
            if (b == null)
                throw new ArgumentNullException(nameof(b));

            int n = b.Length;
            
            if (A.GetLength(0) != n || A.GetLength(1) != n)
            {
                throw new Exceptions.InvalidDataException(
                    $"Matrix dimensions must match vector length. Matrix: {A.GetLength(0)}x{A.GetLength(1)}, Vector: {n}.");
            }

            // Create augmented matrix [A|b]
            double[,] Ab = new double[n, n + 1];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    Ab[i, j] = A[i, j];
                }
                Ab[i, n] = b[i];
            }

            // Gaussian elimination with partial pivoting
            for (int i = 0; i < n; i++)
            {
                // Find pivot row and swap
                int maxIndex = i;
                double maxValue = Math.Abs(Ab[i, i]);

                for (int j = i + 1; j < n; j++)
                {
                    if (Math.Abs(Ab[j, i]) > maxValue)
                    {
                        maxIndex = j;
                        maxValue = Math.Abs(Ab[j, i]);
                    }
                }

                // Swap rows
                if (maxIndex != i)
                {
                    for (int j = 0; j <= n; j++)
                    {
                        double temp = Ab[i, j];
                        Ab[i, j] = Ab[maxIndex, j];
                        Ab[maxIndex, j] = temp;
                    }
                }

                // Check for singular matrix
                if (Math.Abs(Ab[i, i]) < Epsilon)
                {
                    throw new Exceptions.InvalidDataException("Matrix is singular or nearly singular. Cannot solve linear system.");
                }

                // Eliminate below pivot
                for (int j = i + 1; j < n; j++)
                {
                    double factor = Ab[j, i] / Ab[i, i];

                    for (int k = i + 1; k <= n; k++)
                    {
                        Ab[j, k] -= factor * Ab[i, k];
                    }
                }
            }

            // Back substitution
            double[] x = new double[n];

            for (int i = n - 1; i >= 0; i--)
            {
                double sum = 0.0;

                for (int j = i + 1; j < n; j++)
                {
                    sum += Ab[i, j] * x[j];
                }

                x[i] = (Ab[i, n] - sum) / Ab[i, i];
            }

            return x;
        }
        /// <summary>
        /// Estimates initial production rate (qi) and initial decline rate (di) using Material Balance Method (MBM).
        /// Note: B is the formation volume factor, and Di is the nominal decline rate, both of which are separate parameters.
        /// </summary>
        /// <param name="productionData">Array of production rate values.</param>
        /// <param name="timeData">Array of DateTime values corresponding to production data.</param>
        /// <param name="porosity">Formation porosity (fraction).</param>
        /// <param name="thickness">Formation thickness in feet.</param>
        /// <param name="area">Reservoir area in acres.</param>
        /// <param name="reservoirPressure">Initial reservoir pressure in psia.</param>
        /// <param name="gasGravity">Gas specific gravity.</param>
        /// <param name="oilGravity">Oil specific gravity (API degrees).</param>
        /// <param name="waterSaturation">Water saturation (fraction).</param>
        /// <param name="formationVolumeFactor">Formation volume factor.</param>
        /// <param name="b">Formation volume factor parameter.</param>
        /// <param name="di">Nominal decline rate.</param>
        /// <returns>Array containing [initialProductionRate, initialDeclineRate].</returns>
        /// <exception cref="ArgumentNullException">Thrown when input arrays are null.</exception>
        /// <exception cref="Exceptions.InvalidDataException">Thrown when input data is invalid.</exception>
        public static double[] EstimateQiDiMBM(double[] productionData, DateTime[] timeData, double porosity, double thickness, double area, double reservoirPressure, double gasGravity, double oilGravity, double waterSaturation, double formationVolumeFactor, double b, double di)
        {
            if (productionData == null)
                throw new ArgumentNullException(nameof(productionData));
            if (timeData == null)
                throw new ArgumentNullException(nameof(timeData));

            DataValidator.ValidateProductionData(productionData, nameof(productionData));
            DataValidator.ValidateTimeData(timeData, productionData.Length, nameof(timeData));

            // Validate reservoir parameters
            if (porosity <= 0 || porosity >= 1)
                throw new Exceptions.InvalidDataException($"Porosity must be between 0 and 1. Provided: {porosity}.");
            if (thickness <= 0)
                throw new Exceptions.InvalidDataException($"Thickness must be positive. Provided: {thickness}.");
            if (area <= 0)
                throw new Exceptions.InvalidDataException($"Area must be positive. Provided: {area}.");
            if (reservoirPressure <= 0)
                throw new Exceptions.InvalidDataException($"Reservoir pressure must be positive. Provided: {reservoirPressure}.");
            if (waterSaturation < 0 || waterSaturation >= 1)
                throw new Exceptions.InvalidDataException($"Water saturation must be between 0 and 1. Provided: {waterSaturation}.");

            DateTime startTime = timeData[0];
            double totalTimeDays = (timeData[timeData.Length - 1] - startTime).TotalDays;

            if (totalTimeDays < Epsilon)
            {
                throw new Exceptions.InvalidDataException("Time span is too small for MBM estimation.");
            }

            // Calculate the average reservoir pressure
            double averagePressure = 0.0;
            for (int i = 0; i < productionData.Length; i++)
            {
                averagePressure += reservoirPressure * (1.0 - waterSaturation);
            }
            averagePressure /= productionData.Length;

            // Calculate the oil formation volume factor
            double pressureDiff = averagePressure - PsiaToStandard;
            double oilFormationVolumeFactor = formationVolumeFactor / (1.0 + 0.000025 * pressureDiff);

            if (oilFormationVolumeFactor <= 0)
            {
                throw new Exceptions.InvalidDataException("Calculated oil formation volume factor is invalid.");
            }

            // Calculate the total produced oil and gas volumes
            double totalOilVolume = 0.0;
            double totalGasVolume = 0.0;
            const double standardTemperature = 520.0 + 460.0; // Rankine

            for (int i = 0; i < productionData.Length; i++)
            {
                double timeDays = (timeData[i] - startTime).TotalDays;
                double p = reservoirPressure * (1.0 - b * di * timeDays);
                if (p <= 0)
                {
                    throw new Exceptions.InvalidDataException($"Calculated pressure is invalid at time index {i}.");
                }

                double gasFormationVolumeFactor = 0.002303 * gasGravity * standardTemperature / (p * oilFormationVolumeFactor);
                double solutionGasOilRatio = 0.00006 * gasGravity * standardTemperature * 
                    Math.Exp(0.0125 * oilGravity / gasGravity * p / standardTemperature);
                double gasOilRatio = solutionGasOilRatio * gasFormationVolumeFactor / oilFormationVolumeFactor;
                
                double oilVolume = productionData[i] / (1.0 - waterSaturation) * oilFormationVolumeFactor;
                double gasVolume = oilVolume * gasOilRatio;
                
                totalOilVolume += oilVolume;
                totalGasVolume += gasVolume;
            }

            if (totalOilVolume <= 0 || totalGasVolume <= 0)
            {
                throw new Exceptions.InvalidDataException("Calculated total volumes are invalid.");
            }

            // Calculate the initial hydrocarbon in place
            double initialHydrocarbonInPlace = (BarrelsPerAcreFoot * area * thickness * porosity * pressureDiff * totalOilVolume) / 
                (oilFormationVolumeFactor * totalGasVolume);

            if (initialHydrocarbonInPlace <= 0)
            {
                throw new Exceptions.InvalidDataException("Calculated initial hydrocarbon in place is invalid.");
            }

            // Calculate the initial production rate
            double initialProductionRate = totalOilVolume / totalTimeDays;

            // Calculate the initial decline rate
            double denominator = initialProductionRate - totalOilVolume / initialHydrocarbonInPlace;
            
            if (denominator <= 0)
            {
                throw new Exceptions.InvalidDataException("Cannot calculate decline rate: invalid denominator in decline rate formula.");
            }

            double initialDeclineRate = Math.Log(initialProductionRate / denominator) / totalTimeDays;

            if (double.IsNaN(initialDeclineRate) || double.IsInfinity(initialDeclineRate) || initialDeclineRate <= 0)
            {
                throw new Exceptions.InvalidDataException($"Calculated initial decline rate is invalid: {initialDeclineRate}.");
            }

            return new double[] { initialProductionRate, initialDeclineRate };
        }
        /// <summary>
        /// Estimates initial decline rate using linear regression on logarithmic production data.
        /// </summary>
        /// <param name="time">Array of time values (e.g., months since start).</param>
        /// <param name="production">Array of production rate values.</param>
        /// <returns>Estimated initial decline rate.</returns>
        /// <exception cref="ArgumentNullException">Thrown when input arrays are null.</exception>
        /// <exception cref="InvalidDataException">Thrown when input data is invalid.</exception>
        public static double EstimateInitialDeclineRate(double[] time, double[] production)
        {
            if (time == null)
                throw new ArgumentNullException(nameof(time));
            if (production == null)
                throw new ArgumentNullException(nameof(production));

            if (time.Length != production.Length)
            {
                throw new Exceptions.InvalidDataException(
                    $"Time array length ({time.Length}) must match production array length ({production.Length}).");
            }

            if (time.Length < MinDataPoints)
            {
                throw new Exceptions.InvalidDataException(
                    $"At least {MinDataPoints} data points are required. Provided: {time.Length}.");
            }

            if (production.Any(p => p <= 0))
            {
                throw new Exceptions.InvalidDataException("Production rates must be positive for logarithmic regression.");
            }

            int n = time.Length;

            // Convert production rates to logarithms
            double[] logProduction = production.Select(x => Math.Log(x)).ToArray();

            double sumTime = time.Sum();
            double sumLogProduction = logProduction.Sum();
            double sumTimeLogProduction = 0;
            double sumTimeSquared = 0;

            for (int i = 0; i < n; i++)
            {
                sumTimeLogProduction += time[i] * logProduction[i];
                sumTimeSquared += Math.Pow(time[i], 2);
            }

            double denominator = n * sumTimeSquared - Math.Pow(sumTime, 2);
            
            if (Math.Abs(denominator) < Epsilon)
            {
                throw new Exceptions.InvalidDataException("Cannot estimate decline rate: insufficient variation in time data.");
            }

            double slope = (n * sumTimeLogProduction - sumTime * sumLogProduction) / denominator;

            return -slope;
        }

        /// <summary>
        /// Estimates initial decline rate using linear regression on logarithmic production data with DateTime values.
        /// </summary>
        /// <param name="time">Array of DateTime values.</param>
        /// <param name="production">Array of production rate values.</param>
        /// <returns>Estimated initial decline rate (1/days).</returns>
        /// <exception cref="ArgumentNullException">Thrown when input arrays are null.</exception>
        /// <exception cref="InvalidDataException">Thrown when input data is invalid.</exception>
        public static double EstimateInitialDeclineRate(DateTime[] time, double[] production)
        {
            if (time == null)
                throw new ArgumentNullException(nameof(time));
            if (production == null)
                throw new ArgumentNullException(nameof(production));

            DataValidator.ValidateTimeData(time, production.Length, nameof(time));
            DataValidator.ValidateProductionData(production, nameof(production));

            // Convert DateTime array to a double array representing the time difference in days
            DateTime startTime = time[0];
            double[] timeDifference = time.Select(x => (x - startTime).TotalDays).ToArray();

            return EstimateInitialDeclineRate(timeDifference, production);
        }
        
    }

}




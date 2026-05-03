using System;
using Beep.OilandGas.DCA.Constants;
using Beep.OilandGas.DCA.Exceptions;
using Beep.OilandGas.DCA.Validation;

namespace Beep.OilandGas.DCA.AdvancedDeclineMethods
{
    /// <summary>
    /// Provides Power-Law Exponential Decline (PLE) method for decline curve analysis.
    /// Formula: q(t) = qi * exp(-Di * t^n)
    /// where n is the power-law exponent.
    /// </summary>
    public static class PowerLawExponentialDecline
    {
        /// <summary>
        /// Calculates production rate using Power-Law Exponential decline model.
        /// Formula: q(t) = qi * exp(-Di * t^n)
        /// </summary>
        /// <param name="qi">Initial production rate.</param>
        /// <param name="di">Initial decline rate (1/time^n).</param>
        /// <param name="t">Time since start of production.</param>
        /// <param name="n">Power-law exponent (typically 0 &lt; n &lt; 1).</param>
        /// <returns>Production rate at time t.</returns>
        /// <exception cref="Exceptions.InvalidDataException">Thrown when input parameters are invalid.</exception>
        public static double CalculateProductionRate(double qi, double di, double t, double n)
        {
            DataValidator.ValidateInitialProductionRate(qi, nameof(qi));
            DataValidator.ValidateInitialDeclineRate(di, nameof(di));

            if (t < 0)
            {
                throw new Exceptions.InvalidDataException($"Time parameter {nameof(t)} must be non-negative. Provided: {t}.");
            }

            if (n <= 0 || n > 2)
            {
                throw new Exceptions.InvalidDataException(
                    $"Power-law exponent n must be between 0 and 2. Provided: {n}.");
            }

            if (double.IsNaN(n) || double.IsInfinity(n))
            {
                throw new Exceptions.InvalidDataException($"Power-law exponent n must be a finite number. Provided: {n}.");
            }

            double exponent = -di * Math.Pow(t, n);
            return qi * Math.Exp(exponent);
        }

        /// <summary>
        /// Estimates cumulative production using Power-Law Exponential decline.
        /// </summary>
        /// <param name="qi">Initial production rate.</param>
        /// <param name="di">Initial decline rate.</param>
        /// <param name="t">Time since start of production.</param>
        /// <param name="n">Power-law exponent.</param>
        /// <returns>Cumulative production up to time t.</returns>
        public static double CalculateCumulativeProduction(double qi, double di, double t, double n)
        {
            // For PLE decline, cumulative production requires numerical integration
            // Using trapezoidal rule approximation
            const int steps = 1000;
            double dt = t / steps;
            double cumulative = 0.0;

            for (int i = 0; i < steps; i++)
            {
                double t1 = i * dt;
                double t2 = (i + 1) * dt;
                double q1 = CalculateProductionRate(qi, di, t1, n);
                double q2 = CalculateProductionRate(qi, di, t2, n);
                cumulative += (q1 + q2) * dt / 2.0;
            }

            return cumulative;
        }
    }
}


using System;
using Beep.OilandGas.DCA.Constants;
using Beep.OilandGas.DCA.Exceptions;
using Beep.OilandGas.DCA.Validation;

namespace Beep.OilandGas.DCA.AdvancedDeclineMethods
{
    /// <summary>
    /// Provides Stretched Exponential Decline (SE) method for decline curve analysis.
    /// Also known as Kohlrausch function or Weibull distribution.
    /// Formula: q(t) = qi * exp(-(Di * t)^β)
    /// where β is the stretching exponent.
    /// </summary>
    public static class StretchedExponentialDecline
    {
        /// <summary>
        /// Calculates production rate using Stretched Exponential decline model.
        /// Formula: q(t) = qi * exp(-(Di * t)^β)
        /// </summary>
        /// <param name="qi">Initial production rate.</param>
        /// <param name="di">Initial decline rate (1/time).</param>
        /// <param name="t">Time since start of production.</param>
        /// <param name="beta">Stretching exponent (typically 0 &lt; β &lt; 1).</param>
        /// <returns>Production rate at time t.</returns>
        /// <exception cref="Exceptions.InvalidDataException">Thrown when input parameters are invalid.</exception>
        public static double CalculateProductionRate(double qi, double di, double t, double beta)
        {
            DataValidator.ValidateInitialProductionRate(qi, nameof(qi));
            DataValidator.ValidateInitialDeclineRate(di, nameof(di));

            if (t < 0)
            {
                throw new Exceptions.InvalidDataException($"Time parameter {nameof(t)} must be non-negative. Provided: {t}.");
            }

            if (beta <= 0 || beta > 2)
            {
                throw new Exceptions.InvalidDataException(
                    $"Stretching exponent β must be between 0 and 2. Provided: {beta}.");
            }

            if (double.IsNaN(beta) || double.IsInfinity(beta))
            {
                throw new Exceptions.InvalidDataException($"Stretching exponent β must be a finite number. Provided: {beta}.");
            }

            double exponent = -Math.Pow(di * t, beta);
            return qi * Math.Exp(exponent);
        }

        /// <summary>
        /// Estimates cumulative production using Stretched Exponential decline.
        /// </summary>
        /// <param name="qi">Initial production rate.</param>
        /// <param name="di">Initial decline rate.</param>
        /// <param name="t">Time since start of production.</param>
        /// <param name="beta">Stretching exponent.</param>
        /// <returns>Cumulative production up to time t.</returns>
        public static double CalculateCumulativeProduction(double qi, double di, double t, double beta)
        {
            // For SE decline, cumulative production requires numerical integration
            // Using trapezoidal rule approximation
            const int steps = 1000;
            double dt = t / steps;
            double cumulative = 0.0;

            for (int i = 0; i < steps; i++)
            {
                double t1 = i * dt;
                double t2 = (i + 1) * dt;
                double q1 = CalculateProductionRate(qi, di, t1, beta);
                double q2 = CalculateProductionRate(qi, di, t2, beta);
                cumulative += (q1 + q2) * dt / 2.0;
            }

            return cumulative;
        }
    }
}


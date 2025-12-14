using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.GasProperties.Models;
using Beep.OilandGas.GasProperties.Constants;

namespace Beep.OilandGas.GasProperties.Calculations
{
    /// <summary>
    /// Provides pseudo-pressure calculation methods.
    /// </summary>
    public static class PseudoPressureCalculator
    {
        /// <summary>
        /// Calculates pseudo-pressure using numerical integration.
        /// </summary>
        /// <param name="pressure">Pressure in psia.</param>
        /// <param name="temperature">Temperature in Rankine.</param>
        /// <param name="specificGravity">Gas specific gravity (relative to air).</param>
        /// <param name="zFactorCalculator">Function to calculate Z-factor.</param>
        /// <param name="viscosityCalculator">Function to calculate viscosity.</param>
        /// <returns>Pseudo-pressure in psia²/cp.</returns>
        public static decimal CalculatePseudoPressure(
            decimal pressure,
            decimal temperature,
            decimal specificGravity,
            Func<decimal, decimal, decimal, decimal> zFactorCalculator,
            Func<decimal, decimal, decimal, decimal, decimal> viscosityCalculator)
        {
            if (pressure <= 0)
                throw new ArgumentException("Pressure must be greater than zero.", nameof(pressure));

            if (temperature <= 0)
                throw new ArgumentException("Temperature must be greater than zero.", nameof(temperature));

            if (specificGravity <= 0)
                throw new ArgumentException("Specific gravity must be greater than zero.", nameof(specificGravity));

            // Use Simpson's rule for numerical integration
            // Pseudo-pressure = 2 * ∫(P / (μ * Z)) dP from 0 to P
            int n = 100; // Number of integration points
            decimal h = pressure / n;
            decimal sum = 0m;

            for (int i = 0; i <= n; i++)
            {
                decimal p = i * h;
                if (p <= 0)
                    continue;

                decimal z = zFactorCalculator(p, temperature, specificGravity);
                decimal mu = viscosityCalculator(p, temperature, specificGravity, z);

                if (mu <= 0 || z <= 0)
                    continue;

                decimal integrand = p / (mu * z);

                if (i == 0 || i == n)
                    sum += integrand;
                else if (i % 2 == 1)
                    sum += 4m * integrand;
                else
                    sum += 2m * integrand;
            }

            decimal pseudoPressure = 2m * h * sum / 3m;

            return Math.Max(0m, pseudoPressure);
        }

        /// <summary>
        /// Calculates pseudo-pressure using trapezoidal integration.
        /// </summary>
        public static decimal CalculatePseudoPressureTrapezoidal(
            decimal pressure,
            decimal temperature,
            decimal specificGravity,
            Func<decimal, decimal, decimal, decimal> zFactorCalculator,
            Func<decimal, decimal, decimal, decimal, decimal> viscosityCalculator)
        {
            if (pressure <= 0)
                throw new ArgumentException("Pressure must be greater than zero.", nameof(pressure));

            int n = 100; // Number of integration points
            decimal h = pressure / n;
            decimal sum = 0m;

            for (int i = 0; i <= n; i++)
            {
                decimal p = i * h;
                if (p <= 0)
                    continue;

                decimal z = zFactorCalculator(p, temperature, specificGravity);
                decimal mu = viscosityCalculator(p, temperature, specificGravity, z);

                if (mu <= 0 || z <= 0)
                    continue;

                decimal integrand = p / (mu * z);

                if (i == 0 || i == n)
                    sum += integrand;
                else
                    sum += 2m * integrand;
            }

            decimal pseudoPressure = h * sum / 2m;

            return Math.Max(0m, pseudoPressure);
        }

        /// <summary>
        /// Generates pseudo-pressure curve over a pressure range.
        /// </summary>
        public static List<PseudoPressureResult> GeneratePseudoPressureCurve(
            decimal minPressure,
            decimal maxPressure,
            int numberOfPoints,
            decimal temperature,
            decimal specificGravity,
            Func<decimal, decimal, decimal, decimal> zFactorCalculator,
            Func<decimal, decimal, decimal, decimal, decimal> viscosityCalculator)
        {
            if (minPressure <= 0)
                throw new ArgumentException("Minimum pressure must be greater than zero.", nameof(minPressure));

            if (maxPressure <= minPressure)
                throw new ArgumentException("Maximum pressure must be greater than minimum pressure.", nameof(maxPressure));

            if (numberOfPoints < 2)
                throw new ArgumentException("Number of points must be at least 2.", nameof(numberOfPoints));

            var results = new List<PseudoPressureResult>();
            decimal pressureStep = (maxPressure - minPressure) / (numberOfPoints - 1);

            for (int i = 0; i < numberOfPoints; i++)
            {
                decimal pressure = minPressure + i * pressureStep;
                decimal zFactor = zFactorCalculator(pressure, temperature, specificGravity);
                decimal viscosity = viscosityCalculator(pressure, temperature, specificGravity, zFactor);
                decimal pseudoPressure = CalculatePseudoPressure(
                    pressure, temperature, specificGravity, zFactorCalculator, viscosityCalculator);

                results.Add(new PseudoPressureResult
                {
                    Pressure = pressure,
                    ZFactor = zFactor,
                    Viscosity = viscosity,
                    PseudoPressure = pseudoPressure
                });
            }

            return results;
        }
    }
}


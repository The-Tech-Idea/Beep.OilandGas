using System;
using Beep.OilandGas.GasProperties.Models;
using Beep.OilandGas.GasProperties.Constants;

namespace Beep.OilandGas.GasProperties.Calculations
{
    /// <summary>
    /// Provides Z-factor (compressibility factor) calculation methods.
    /// </summary>
    public static class ZFactorCalculator
    {
        /// <summary>
        /// Calculates Z-factor using Brill-Beggs correlation.
        /// </summary>
        /// <param name="pressure">Pressure in psia.</param>
        /// <param name="temperature">Temperature in Rankine.</param>
        /// <param name="specificGravity">Gas specific gravity (relative to air).</param>
        /// <returns>Z-factor (dimensionless).</returns>
        public static decimal CalculateBrillBeggs(
            decimal pressure,
            decimal temperature,
            decimal specificGravity)
        {
            if (pressure <= 0)
                throw new ArgumentException("Pressure must be greater than zero.", nameof(pressure));

            if (temperature <= 0)
                throw new ArgumentException("Temperature must be greater than zero.", nameof(temperature));

            if (specificGravity <= 0)
                throw new ArgumentException("Specific gravity must be greater than zero.", nameof(specificGravity));

            // Calculate pseudo-critical properties
            decimal pseudoCriticalPressure = 756.8m - 131.0m * specificGravity - 3.6m * specificGravity * specificGravity;
            decimal pseudoCriticalTemperature = 169.2m + 349.5m * specificGravity - 74.0m * specificGravity * specificGravity;

            // Calculate pseudo-reduced properties
            decimal pseudoReducedPressure = pressure / pseudoCriticalPressure;
            decimal pseudoReducedTemperature = temperature / pseudoCriticalTemperature;

            // Brill-Beggs correlation
            decimal A = 1.39m * (decimal)Math.Pow((double)(pseudoReducedTemperature - 0.92m), 0.5) - 0.36m * pseudoReducedTemperature - 0.101m;
            decimal B = (0.62m - 0.23m * pseudoReducedTemperature) * pseudoReducedPressure +
                        (0.066m / (pseudoReducedTemperature - 0.86m) - 0.037m) * pseudoReducedPressure * pseudoReducedPressure +
                        0.32m * (decimal)Math.Pow((double)pseudoReducedPressure, 6.0) / (decimal)Math.Pow(10.0, (double)(9.0m * (pseudoReducedTemperature - 1.0m)));
            decimal C = 0.132m - 0.32m * (decimal)Math.Log10((double)pseudoReducedTemperature);
            decimal D = (decimal)Math.Pow(10.0, (double)(0.3106m - 0.49m * pseudoReducedTemperature + 0.1824m * pseudoReducedTemperature * pseudoReducedTemperature));

            decimal zFactor = A + (1.0m - A) * (decimal)Math.Exp((double)(-B)) + C * (decimal)Math.Pow((double)pseudoReducedPressure, (double)D);

            return Math.Max(0.1m, Math.Min(2.0m, zFactor)); // Clamp to reasonable range
        }

        /// <summary>
        /// Calculates Z-factor using Hall-Yarborough correlation.
        /// </summary>
        /// <param name="pressure">Pressure in psia.</param>
        /// <param name="temperature">Temperature in Rankine.</param>
        /// <param name="specificGravity">Gas specific gravity (relative to air).</param>
        /// <returns>Z-factor (dimensionless).</returns>
        public static decimal CalculateHallYarborough(
            decimal pressure,
            decimal temperature,
            decimal specificGravity)
        {
            if (pressure <= 0)
                throw new ArgumentException("Pressure must be greater than zero.", nameof(pressure));

            if (temperature <= 0)
                throw new ArgumentException("Temperature must be greater than zero.", nameof(temperature));

            if (specificGravity <= 0)
                throw new ArgumentException("Specific gravity must be greater than zero.", nameof(specificGravity));

            // Calculate pseudo-critical properties
            decimal pseudoCriticalPressure = 756.8m - 131.0m * specificGravity - 3.6m * specificGravity * specificGravity;
            decimal pseudoCriticalTemperature = 169.2m + 349.5m * specificGravity - 74.0m * specificGravity * specificGravity;

            // Calculate pseudo-reduced properties
            decimal pseudoReducedPressure = pressure / pseudoCriticalPressure;
            decimal pseudoReducedTemperature = temperature / pseudoCriticalTemperature;

            // Hall-Yarborough correlation
            decimal t = 1.0m / pseudoReducedTemperature;

            // Coefficients for Hall-Yarborough equation
            decimal A1 = 0.06125m * t * (decimal)Math.Exp((double)(-1.2m * (1.0m - t) * (1.0m - t)));
            decimal A2 = 14.76m * t - 9.76m * t * t + 4.58m * t * t * t;
            decimal A3 = 90.7m * t - 242.2m * t * t + 42.4m * t * t * t;
            decimal A4 = 2.18m + 2.82m * t;

            // Solve for Y using Newton-Raphson method
            decimal Y = 0.001m; // Initial guess
            decimal YOld = 0m;
            int iterations = 0;
            const int maxIterations = 100;
            const decimal tolerance = 0.0001m;

            while (Math.Abs(Y - YOld) > tolerance && iterations < maxIterations)
            {
                YOld = Y;
                decimal f = -A1 * pseudoReducedPressure + (Y + Y * Y + Y * Y * Y - Y * Y * Y * Y) / (decimal)Math.Pow((double)(1.0m - Y), 3.0) -
                           A2 * Y * Y + A3 * Y * (decimal)Math.Pow((double)Y, (double)A4);
                decimal df = 1.0m / (decimal)Math.Pow((double)(1.0m - Y), 3.0) * (1.0m + 4.0m * Y + 4.0m * Y * Y - 4.0m * Y * Y * Y + Y * Y * Y * Y) -
                           2.0m * A2 * Y + A3 * (1.0m + A4) * (decimal)Math.Pow((double)Y, (double)A4);

                if (Math.Abs(df) < 0.0001m)
                    break;

                Y = Y - f / df;

                // Ensure Y is in valid range
                if (Y < 0.001m)
                    Y = 0.001m;
                if (Y > 0.999m)
                    Y = 0.999m;

                iterations++;
            }

            decimal zFactor = A1 * pseudoReducedPressure / Y;

            return Math.Max(0.1m, Math.Min(2.0m, zFactor)); // Clamp to reasonable range
        }

        /// <summary>
        /// Calculates Z-factor using Standing-Katz chart correlation (Dranchuk-Abu-Kassem).
        /// </summary>
        /// <param name="pressure">Pressure in psia.</param>
        /// <param name="temperature">Temperature in Rankine.</param>
        /// <param name="specificGravity">Gas specific gravity (relative to air).</param>
        /// <returns>Z-factor (dimensionless).</returns>
        public static decimal CalculateStandingKatz(
            decimal pressure,
            decimal temperature,
            decimal specificGravity)
        {
            if (pressure <= 0)
                throw new ArgumentException("Pressure must be greater than zero.", nameof(pressure));

            if (temperature <= 0)
                throw new ArgumentException("Temperature must be greater than zero.", nameof(temperature));

            if (specificGravity <= 0)
                throw new ArgumentException("Specific gravity must be greater than zero.", nameof(specificGravity));

            // Calculate pseudo-critical properties
            decimal pseudoCriticalPressure = 756.8m - 131.0m * specificGravity - 3.6m * specificGravity * specificGravity;
            decimal pseudoCriticalTemperature = 169.2m + 349.5m * specificGravity - 74.0m * specificGravity * specificGravity;

            // Calculate pseudo-reduced properties
            decimal pseudoReducedPressure = pressure / pseudoCriticalPressure;
            decimal pseudoReducedTemperature = temperature / pseudoCriticalTemperature;

            // Dranchuk-Abu-Kassem correlation (approximation of Standing-Katz)
            decimal t = 1.0m / pseudoReducedTemperature;
            decimal A1 = 0.3265m;
            decimal A2 = -1.0700m;
            decimal A3 = -0.5339m;
            decimal A4 = 0.01569m;
            decimal A5 = -0.05165m;
            decimal A6 = 0.5475m;
            decimal A7 = -0.7361m;
            decimal A8 = 0.1844m;
            decimal A9 = 0.1056m;
            decimal A10 = 0.6134m;
            decimal A11 = 0.7210m;

            // Solve for density using Newton-Raphson
            decimal rhoR = 0.27m * pseudoReducedPressure / pseudoReducedTemperature; // Initial guess
            decimal rhoROld = 0m;
            int iterations = 0;
            const int maxIterations = 100;
            const decimal tolerance = 0.0001m;

            while (Math.Abs(rhoR - rhoROld) > tolerance && iterations < maxIterations)
            {
                rhoROld = rhoR;
                decimal rhoR2 = rhoR * rhoR;
                decimal rhoR4 = rhoR2 * rhoR2;
                decimal rhoR5 = rhoR4 * rhoR;

                decimal f = 1.0m + A1 * rhoR + A2 * rhoR + A3 * rhoR2 + A4 * rhoR4 + A5 * rhoR5 +
                           (A6 + A7 * rhoR + A8 * rhoR2) * rhoR2 * (decimal)Math.Exp((double)(-A9 * rhoR2)) -
                           A10 * (1.0m + A11 * rhoR2) * rhoR2 * (decimal)Math.Exp((double)(-A11 * rhoR2)) -
                           0.27m * pseudoReducedPressure / (pseudoReducedTemperature * rhoR);

                decimal df = A1 + 2.0m * A3 * rhoR + 4.0m * A4 * rhoR2 * rhoR + 5.0m * A5 * rhoR4 +
                            (2.0m * A6 * rhoR + 3.0m * A7 * rhoR2 + 4.0m * A8 * rhoR2 * rhoR) * (decimal)Math.Exp((double)(-A9 * rhoR2)) -
                            (A6 + A7 * rhoR + A8 * rhoR2) * rhoR2 * 2.0m * A9 * rhoR * (decimal)Math.Exp((double)(-A9 * rhoR2)) -
                            A10 * (2.0m * A11 * rhoR) * (decimal)Math.Exp((double)(-A11 * rhoR2)) +
                            A10 * (1.0m + A11 * rhoR2) * rhoR2 * 2.0m * A11 * rhoR * (decimal)Math.Exp((double)(-A11 * rhoR2)) +
                            0.27m * pseudoReducedPressure / (pseudoReducedTemperature * rhoR * rhoR);

                if (Math.Abs(df) < 0.0001m)
                    break;

                rhoR = rhoR - f / df;

                if (rhoR < 0.001m)
                    rhoR = 0.001m;
                if (rhoR > 3.0m)
                    rhoR = 3.0m;

                iterations++;
            }

            decimal zFactor = 0.27m * pseudoReducedPressure / (pseudoReducedTemperature * rhoR);

            return Math.Max(0.1m, Math.Min(2.0m, zFactor)); // Clamp to reasonable range
        }

        /// <summary>
        /// Calculates pseudo-critical properties from gas composition.
        /// </summary>
        public static (decimal PseudoCriticalPressure, decimal PseudoCriticalTemperature) CalculatePseudoCriticalProperties(
            GasComposition composition)
        {
            if (composition == null)
                throw new ArgumentNullException(nameof(composition));

            if (!composition.IsValid())
                throw new ArgumentException("Gas composition fractions must sum to 1.0.", nameof(composition));

            // Component critical properties (pressure in psia, temperature in Rankine)
            decimal pc1 = 667.8m, tc1 = 343.0m; // Methane
            decimal pc2 = 707.8m, tc2 = 549.8m; // Ethane
            decimal pc3 = 616.3m, tc3 = 665.7m; // Propane
            decimal pc4i = 529.1m, tc4i = 734.1m; // i-Butane
            decimal pc4n = 550.7m, tc4n = 765.3m; // n-Butane
            decimal pc5i = 490.4m, tc5i = 828.8m; // i-Pentane
            decimal pc5n = 488.6m, tc5n = 845.4m; // n-Pentane
            decimal pc6 = 436.9m, tc6 = 913.4m; // Hexane+
            decimal pcN2 = 493.1m, tcN2 = 227.2m; // Nitrogen
            decimal pcCO2 = 1071.0m, tcCO2 = 547.5m; // CO2
            decimal pcH2S = 1306.0m, tcH2S = 672.4m; // H2S

            decimal pseudoCriticalPressure = composition.MethaneFraction * pc1 +
                                             composition.EthaneFraction * pc2 +
                                             composition.PropaneFraction * pc3 +
                                             composition.IButaneFraction * pc4i +
                                             composition.NButaneFraction * pc4n +
                                             composition.IPentaneFraction * pc5i +
                                             composition.NPentaneFraction * pc5n +
                                             composition.HexanePlusFraction * pc6 +
                                             composition.NitrogenFraction * pcN2 +
                                             composition.CarbonDioxideFraction * pcCO2 +
                                             composition.HydrogenSulfideFraction * pcH2S;

            decimal pseudoCriticalTemperature = composition.MethaneFraction * tc1 +
                                               composition.EthaneFraction * tc2 +
                                               composition.PropaneFraction * tc3 +
                                               composition.IButaneFraction * tc4i +
                                               composition.NButaneFraction * tc4n +
                                               composition.IPentaneFraction * tc5i +
                                               composition.NPentaneFraction * tc5n +
                                               composition.HexanePlusFraction * tc6 +
                                               composition.NitrogenFraction * tcN2 +
                                               composition.CarbonDioxideFraction * tcCO2 +
                                               composition.HydrogenSulfideFraction * tcH2S;

            return (pseudoCriticalPressure, pseudoCriticalTemperature);
        }
    }
}


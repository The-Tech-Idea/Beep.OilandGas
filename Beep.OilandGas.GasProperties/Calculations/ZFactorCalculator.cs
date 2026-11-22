using Beep.OilandGas.Models.Data.Company;
using System;
using Beep.OilandGas.Models.Data.GasProperties;
using Beep.OilandGas.GasProperties.Constants;
using Beep.OilandGas.Models.Core.Refusals;

namespace Beep.OilandGas.GasProperties.Calculations
{
    /// <summary>
    /// Provides Z-factor (compressibility factor) calculation methods.
    /// </summary>
    public static class ZFactorCalculator
    {
        /// <summary>
        /// The pseudo-reduced temperature at and below which the Brill-Beggs correlation is not defined: its first term
        /// takes the square root of <c>Tpr - 0.92</c>.
        /// </summary>
        public const decimal BrillBeggsMinimumPseudoReducedTemperature = 0.92m;

        /// <summary>
        /// Whether the Brill-Beggs correlation is defined for this temperature (°R) and gas gravity: the pseudo-reduced
        /// temperature, on Sutton's pseudo-critical temperature, must be above
        /// <see cref="BrillBeggsMinimumPseudoReducedTemperature"/>.
        /// </summary>
        public static bool IsBrillBeggsDefined(decimal temperature, decimal specificGravity)
        {
            if (temperature <= 0 || specificGravity <= 0)
                return false;

            decimal pseudoCriticalTemperature = 169.2m + 349.5m * specificGravity - 74.0m * specificGravity * specificGravity;
            return pseudoCriticalTemperature > 0 &&
                   temperature / pseudoCriticalTemperature > BrillBeggsMinimumPseudoReducedTemperature;
        }

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
            decimal? specificGravity)
        {
            if (pressure <= 0)
                throw RefusalException.Invalid("Pressure must be greater than zero.");

            if (temperature <= 0)
                throw RefusalException.Invalid("Temperature must be greater than zero.");

            if (specificGravity is null)
                throw RefusalException.Invalid("Specific gravity is required.");

            if (specificGravity <= 0)
                throw RefusalException.Invalid("Specific gravity must be greater than zero.");

            // Below its range the correlation's square root is of a negative number, and the cast of NaN threw an
            // OverflowException from deep inside whichever calculation asked (OILGAS-CATCH-01). The inputs are refused.
            if (!IsBrillBeggsDefined(temperature, specificGravity.Value))
                throw RefusalException.Invalid(
                    "The temperature is too low for the Brill-Beggs Z-factor correlation at this gas gravity: it needs a pseudo-reduced temperature above 0.92.");

            // Calculate pseudo-critical properties
            decimal pseudoCriticalPressure = (decimal)(756.8m - 131.0m * specificGravity - 3.6m * specificGravity * specificGravity);
            decimal pseudoCriticalTemperature = (decimal)(169.2m + 349.5m * specificGravity - 74.0m * specificGravity * specificGravity);

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

            return Clamp(zFactor, 0.1m, 2.0m); // Clamp to reasonable range
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
                throw RefusalException.Invalid("Pressure must be greater than zero.");

            if (temperature <= 0)
                throw RefusalException.Invalid("Temperature must be greater than zero.");

            if (specificGravity <= 0)
                throw RefusalException.Invalid("Specific gravity must be greater than zero.");

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

            return Clamp(zFactor, 0.1m, 2.0m); // Clamp to reasonable range
        }

        /// <summary>
        /// Calculates Z-factor using Standing-Katz chart correlation (Dranchuk-Abu-Kassem).
        /// </summary>
        /// <param name="pressure">Pressure in psia.</param>
        /// <param name="temperature">Temperature in Rankine.</param>
        /// <param name="specificGravity">Gas specific gravity (relative to air).</param>
        /// <returns>Z-factor (dimensionless).</returns>
        /// <summary>
        /// Dranchuk-Abou-Kassem coefficients (Dranchuk &amp; Abou-Kassem, 1975), the correlation the Standing-Katz
        /// method implements.
        /// </summary>
        private static readonly decimal[] Dak =
        [
            0.3265m, -1.0700m, -0.5339m, 0.01569m, -0.05165m,
            0.5475m, -0.7361m, 0.1844m, 0.1056m, 0.6134m, 0.7210m,
        ];

        /// <summary>The lowest pseudo-reduced temperature DAK is defined for.</summary>
        public const decimal DAKMinimumPseudoReducedTemperature = 1.0m;

        /// <summary>The highest pseudo-reduced temperature DAK is defined for.</summary>
        public const decimal DAKMaximumPseudoReducedTemperature = 3.0m;

        /// <summary>The highest pseudo-reduced pressure DAK is defined for.</summary>
        public const decimal DAKMaximumPseudoReducedPressure = 30m;

        /// <summary>
        /// Calculates the Standing-Katz Z-factor through the Dranchuk-Abou-Kassem equation, reporting whether the
        /// equation actually converged.
        /// </summary>
        /// <remarks>
        /// The equation is
        /// <c>Z = 1 + (A1 + A2/T + A3/T^3 + A4/T^4 + A5/T^5) r + (A6 + A7/T + A8/T^2) r^2
        /// - A9 (A7/T + A8/T^2) r^5 + A10 (1 + A11 r^2) (r^2/T^3) e^(-A11 r^2)</c>
        /// with <c>r = 0.27 Ppr / (Z Tpr)</c>, solved by successive substitution.
        /// <para>
        /// <c>CalculateStandingKatz</c> has always returned a number and clamped it to 0.1..2.0, which meant a
        /// non-converging case (for example Tpr = 1.5, Ppr = 3.0 with this file's previous iteration) came back as
        /// exactly 2.0 - a number that reads like a result. This method is the honest form: it returns false when the
        /// equation does not converge or the point is outside the correlation's range, so a caller that must show a
        /// failure (RES-01) can. <c>CalculateStandingKatz</c> keeps its old shape for existing callers.
        /// </para>
        /// </remarks>
        public static bool TryCalculateStandingKatz(
            decimal pressure,
            decimal temperature,
            decimal specificGravity,
            out decimal zFactor)
        {
            if (pressure <= 0)
                throw RefusalException.Invalid("Pressure must be greater than zero.");

            if (temperature <= 0)
                throw RefusalException.Invalid("Temperature must be greater than zero.");

            if (specificGravity <= 0)
                throw RefusalException.Invalid("Specific gravity must be greater than zero.");

            decimal pseudoCriticalPressure = 756.8m - 131.0m * specificGravity - 3.6m * specificGravity * specificGravity;
            decimal pseudoCriticalTemperature = 169.2m + 349.5m * specificGravity - 74.0m * specificGravity * specificGravity;

            decimal pseudoReducedPressure = pressure / pseudoCriticalPressure;
            decimal pseudoReducedTemperature = temperature / pseudoCriticalTemperature;

            // Outside this range the correlation is not defined, and a number would be a fabrication.
            if (pseudoReducedTemperature <= DAKMinimumPseudoReducedTemperature
                || pseudoReducedTemperature > DAKMaximumPseudoReducedTemperature
                || pseudoReducedPressure <= 0m
                || pseudoReducedPressure > DAKMaximumPseudoReducedPressure)
            {
                zFactor = 0m;
                return false;
            }

            decimal inverseT = 1.0m / pseudoReducedTemperature;

            decimal z = 1.0m;
            var converged = false;

            for (var iteration = 0; iteration < 100; iteration++)
            {
                decimal reducedDensity = 0.27m * pseudoReducedPressure / (z * pseudoReducedTemperature);

                if (reducedDensity <= 0m)
                {
                    break;
                }

                decimal next = DAKZAt(reducedDensity, inverseT);

                if (next <= 0m)
                {
                    break;
                }

                if (Math.Abs(next - z) <= 0.0000001m)
                {
                    z = next;
                    converged = true;
                    break;
                }

                z = next;
            }

            zFactor = z;
            return converged;
        }

        /// <summary>The Dranchuk-Abou-Kassem right-hand side at one reduced density.</summary>
        private static decimal DAKZAt(decimal reducedDensity, decimal inverseT)
        {
            decimal a1 = Dak[0], a2 = Dak[1], a3 = Dak[2], a4 = Dak[3], a5 = Dak[4];
            decimal a6 = Dak[5], a7 = Dak[6], a8 = Dak[7], a9 = Dak[8], a10 = Dak[9], a11 = Dak[10];

            decimal t2 = inverseT * inverseT;
            decimal t3 = t2 * inverseT;
            decimal t4 = t3 * inverseT;
            decimal t5 = t4 * inverseT;

            decimal r = reducedDensity;
            decimal r2 = r * r;
            decimal r5 = r2 * r2 * r;

            return 1.0m
                   + (a1 + a2 * inverseT + a3 * t3 + a4 * t4 + a5 * t5) * r
                   + (a6 + a7 * inverseT + a8 * t2) * r2
                   - a9 * (a7 * inverseT + a8 * t2) * r5
                   + a10 * (1.0m + a11 * r2) * (r2 * t3) * (decimal)Math.Exp((double)(-a11 * r2));
        }

        /// <summary>
        /// Calculates Z-factor using the Standing-Katz correlation (Dranchuk-Abou-Kassem).
        /// </summary>
        /// <param name="pressure">Pressure in psia.</param>
        /// <param name="temperature">Temperature in Rankine.</param>
        /// <param name="specificGravity">Gas specific gravity (relative to air).</param>
        /// <returns>Z-factor (dimensionless), clamped to 0.1..2.0 as this method always has.</returns>
        /// <remarks>
        /// Kept for callers that must have a number. A caller that must distinguish a non-converged point should call
        /// <see cref="TryCalculateStandingKatz"/> instead.
        /// </remarks>
        public static decimal CalculateStandingKatz(
            decimal pressure,
            decimal temperature,
            decimal specificGravity)
        {
            TryCalculateStandingKatz(pressure, temperature, specificGravity, out var zFactor);

            return Clamp(zFactor <= 0m ? 0.1m : zFactor, 0.1m, 2.0m);
        }

        private static decimal Clamp(decimal value, decimal min, decimal max)
        {
            if (value < min)
            {
                return min;
            }

            if (value > max)
            {
                return max;
            }

            return value;
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
                throw RefusalException.Invalid("Gas composition fractions must sum to 1.0.");

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


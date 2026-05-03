using System;
using System.Collections.Generic;
using Beep.OilandGas.GasProperties.Calculations;
using Beep.OilandGas.Models.Data.GasProperties;

namespace Beep.OilandGas.GasProperties.Validation
{
    /// <summary>
    /// Soft applicability hints for correlation-based gas property calculations (does not replace hard range validation).
    /// </summary>
    public static partial class GasPropertiesValidator
    {
        /// <summary>
        /// Pseudo-reduced pressure and temperature from Sutton-style gas-specific-gravity mixing rules
        /// (same basis as <see cref="ZFactorCalculator.CalculateBrillBeggs"/> for γg-only paths).
        /// </summary>
        public static (decimal PseudoReducedPressure, decimal PseudoReducedTemperature) GetPseudoReducedPropertiesFromSpecificGravity(
            decimal pressurePsia,
            decimal temperatureRankine,
            decimal specificGravity)
        {
            decimal pPc = 756.8m - 131.0m * specificGravity - 3.6m * specificGravity * specificGravity;
            decimal tPc = 169.2m + 349.5m * specificGravity - 74.0m * specificGravity * specificGravity;

            if (pPc <= 0m || tPc <= 0m)
                return (0m, 0m);

            return (pressurePsia / pPc, temperatureRankine / tPc);
        }

        /// <summary>
        /// Returns non-fatal applicability messages (chart limits, γg-only vs composition, sour gas heuristics).
        /// Call after <see cref="ValidateCalculationParameters"/> when inputs are already in hard-valid ranges.
        /// </summary>
        /// <param name="composition">When null, a γg-only mixing path is assumed for messaging.</param>
        public static IReadOnlyList<string> GetApplicabilityWarnings(
            decimal pressurePsia,
            decimal temperatureRankine,
            decimal specificGravity,
            GasComposition? composition)
        {
            var warnings = new List<string>();

            var (pr, tr) = GetPseudoReducedPropertiesFromSpecificGravity(
                pressurePsia, temperatureRankine, specificGravity);

            if (pr > 25m)
            {
                warnings.Add(
                    "Pseudo-reduced pressure is high; Standing-Katz-style correlations may have increased uncertainty.");
            }

            if (pr is > 0m and < 0.1m)
            {
                warnings.Add("Pseudo-reduced pressure is very low; verify low-pressure correlation behavior.");
            }

            if (tr is > 0m and < 1.0m)
            {
                warnings.Add(
                    "Pseudo-reduced temperature is below 1; two-phase effects or correlation limits may apply.");
            }

            if (tr > 3.5m)
            {
                warnings.Add("Pseudo-reduced temperature is high; verify correlation applicability.");
            }

            if (composition == null)
            {
                warnings.Add(
                    "Using gas-specific-gravity mixing rules only; mole-fraction-based properties are preferred for rich gas, inerts/acid gases, or blended streams.");
            }
            else if (composition.HydrogenSulfideFraction >= 0.05m)
            {
                warnings.Add(
                    "Hydrogen sulfide mole fraction is elevated; sweet-gas correlations may be biased—consider composition-specific adjustments or EOS-based methods.");
            }

            return warnings;
        }
    }
}

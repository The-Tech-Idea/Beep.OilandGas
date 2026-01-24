using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data.FlashCalculations;
using Beep.OilandGas.GasProperties.Calculations;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Calculations;

namespace Beep.OilandGas.FlashCalculations.Calculations
{
    /// <summary>
    /// Provides flash calculation methods.
    /// </summary>
    public static class FlashCalculator
    {
        /// <summary>
        /// Performs isothermal flash calculation (pressure and temperature specified).
        /// </summary>
        /// <param name="conditions">Flash calculation conditions.</param>
        /// <returns>Flash calculation results.</returns>
        public static FlashResult PerformIsothermalFlash(FlashConditions conditions)
        {
            if (conditions == null)
                throw new ArgumentNullException(nameof(conditions));

            if (conditions.FeedComposition == null || conditions.FeedComposition.Count == 0)
                throw new ArgumentException("Feed composition cannot be empty.", nameof(conditions));

            var result = new FlashResult();

            // Normalize feed composition
            decimal totalMoleFraction = conditions.FeedComposition.Sum(c => c.MoleFraction);
            if (Math.Abs(totalMoleFraction - 1.0m) > 0.01m)
            {
                // Normalize
                foreach (var component in conditions.FeedComposition)
                {
                    component.MoleFraction /= totalMoleFraction;
                }
            }

            // Initialize K-values using Wilson correlation
            var kValues = InitializeKValues(conditions);

            // Perform flash calculation using Rachford-Rice equation
            decimal vaporFraction = SolveRachfordRice(conditions, kValues, result);

            result.VaporFraction = vaporFraction;
            result.LiquidFraction = 1.0m - vaporFraction;
            result.KValues = kValues;

            // Calculate phase compositions
            CalculatePhaseCompositions(conditions, kValues, vaporFraction, result);

            return result;
        }

        /// <summary>
        /// Initializes K-values using Wilson correlation.
        /// </summary>
        private static Dictionary<string, decimal> InitializeKValues(FlashConditions conditions)
        {
            var kValues = new Dictionary<string, decimal>();

            foreach (var component in conditions.FeedComposition)
            {
                // Wilson correlation: K = (Pc/P) * exp(5.37 * (1 + ω) * (1 - Tc/T))
                decimal kValue = (component.CriticalPressure / conditions.Pressure) *
                               (decimal)Math.Exp((double)(5.37m * (1.0m + component.AcentricFactor) *
                                                          (1.0m - component.CriticalTemperature / conditions.Temperature)));

                kValues[component.Name] = Math.Max(0.001m, Math.Min(1000m, kValue));
            }

            return kValues;
        }

        /// <summary>
        /// Solves Rachford-Rice equation for vapor fraction.
        /// </summary>
        private static decimal SolveRachfordRice(
            FlashConditions conditions,
            Dictionary<string, decimal> kValues,
            FlashResult result)
        {
            // Rachford-Rice: Σ(zi * (Ki - 1) / (1 + V * (Ki - 1))) = 0
            // Where V = vapor fraction, zi = feed mole fraction, Ki = K-value

            decimal vaporFraction = 0.5m; // Initial guess
            decimal oldVaporFraction = 0m;
            int iterations = 0;
            const int maxIterations = 100;
            const decimal tolerance = 0.0001m;

            while (Math.Abs(vaporFraction - oldVaporFraction) > tolerance && iterations < maxIterations)
            {
                oldVaporFraction = vaporFraction;

                // Calculate function value
                decimal functionValue = 0m;
                decimal derivative = 0m;

                foreach (var component in conditions.FeedComposition)
                {
                    decimal kValue = kValues[component.Name];
                    decimal zi = component.MoleFraction;
                    decimal denominator = 1.0m + vaporFraction * (kValue - 1.0m);

                    if (Math.Abs(denominator) < 0.0001m)
                        denominator = 0.0001m;

                    functionValue += zi * (kValue - 1.0m) / denominator;
                    derivative -= zi * (kValue - 1.0m) * (kValue - 1.0m) / (denominator * denominator);
                }

                // Newton-Raphson iteration
                if (Math.Abs(derivative) > 0.0001m)
                {
                    vaporFraction = vaporFraction - functionValue / derivative;
                }
                else
                {
                    // Use bisection if derivative is too small
                    vaporFraction = (vaporFraction + oldVaporFraction) / 2m;
                }

                // Clamp to valid range
                vaporFraction = Math.Max(0.0m, Math.Min(1.0m, vaporFraction));

                iterations++;

                // Update K-values if needed (simplified - would use more sophisticated method)
                if (iterations % 5 == 0)
                {
                    UpdateKValues(conditions, kValues, vaporFraction);
                }
            }

            result.Iterations = iterations;
            result.Converged = iterations < maxIterations;
            result.ConvergenceError = Math.Abs(vaporFraction - oldVaporFraction);

            return vaporFraction;
        }

        /// <summary>
        /// Updates K-values based on current conditions.
        /// </summary>
        private static void UpdateKValues(
            FlashConditions conditions,
            Dictionary<string, decimal> kValues,
            decimal vaporFraction)
        {
            // Simplified K-value update
            // In practice, would use more sophisticated methods (Peng-Robinson, Soave-Redlich-Kwong, etc.)

            foreach (var component in conditions.FeedComposition)
            {
                // Use Wilson correlation with updated conditions
                decimal kValue = (component.CriticalPressure / conditions.Pressure) *
                               (decimal)Math.Exp((double)(5.37m * (1.0m + component.AcentricFactor) *
                                                          (1.0m - component.CriticalTemperature / conditions.Temperature)));

                // Blend with existing K-value
                kValues[component.Name] = 0.7m * kValues[component.Name] + 0.3m * kValue;
                kValues[component.Name] = Math.Max(0.001m, Math.Min(1000m, kValues[component.Name]));
            }
        }

        /// <summary>
        /// Calculates phase compositions.
        /// </summary>
        private static void CalculatePhaseCompositions(
            FlashConditions conditions,
            Dictionary<string, decimal> kValues,
            decimal vaporFraction,
            FlashResult result)
        {
            foreach (var component in conditions.FeedComposition)
            {
                decimal zi = component.MoleFraction;
                decimal ki = kValues[component.Name];

                // Liquid composition: xi = zi / (1 + V * (Ki - 1))
                decimal denominator = 1.0m + vaporFraction * (ki - 1.0m);
                if (Math.Abs(denominator) < 0.0001m)
                    denominator = 0.0001m;

                decimal xi = zi / denominator;

                // Vapor composition: yi = Ki * xi
                decimal yi = ki * xi;

                result.LiquidComposition[component.Name] = Math.Max(0m, Math.Min(1m, xi));
                result.VaporComposition[component.Name] = Math.Max(0m, Math.Min(1m, yi));
            }

            // Normalize compositions
            NormalizeComposition(result.LiquidComposition);
            NormalizeComposition(result.VaporComposition);
        }

        /// <summary>
        /// Normalizes a composition to sum to 1.0.
        /// </summary>
        private static void NormalizeComposition(Dictionary<string, decimal> composition)
        {
            decimal sum = composition.Values.Sum();
            if (sum > 0m)
            {
                var keys = composition.Keys.ToList();
                foreach (var key in keys)
                {
                    composition[key] /= sum;
                }
            }
        }

        /// <summary>
        /// Calculates phase properties.
        /// </summary>
        public static PhasePropertiesData CalculateVaporProperties(
            FlashResult flashResult,
            FlashConditions conditions)
        {
            var properties = new PhasePropertiesData();

            // Calculate molecular weight
            decimal molecularWeight = 0m;
            foreach (var component in conditions.FeedComposition)
            {
                decimal yi = flashResult.VaporComposition.ContainsKey(component.Name)
                    ? flashResult.VaporComposition[component.Name]
                    : 0m;
                molecularWeight += yi * component.MolecularWeight;
            }

            properties.MolecularWeight = molecularWeight;

            // Calculate density using ideal gas law
            decimal zFactor = ZFactorCalculator.CalculateBrillBeggs(
                conditions.Pressure, conditions.Temperature, molecularWeight / 28.9645m);

            properties.Density = (conditions.Pressure * molecularWeight) /
                               (zFactor * 10.7316m * conditions.Temperature);

            properties.SpecificGravity = molecularWeight / 28.9645m;

            return properties;
        }

        /// <summary>
        /// Calculates liquid phase properties.
        /// </summary>
        public static PhasePropertiesData CalculateLiquidProperties(
            FlashResult flashResult,
            FlashConditions conditions)
        {
            var properties = new PhasePropertiesData();

            // Calculate molecular weight
            decimal molecularWeight = 0m;
            foreach (var component in conditions.FeedComposition)
            {
                decimal xi = flashResult.LiquidComposition.ContainsKey(component.Name)
                    ? flashResult.LiquidComposition[component.Name]
                    : 0m;
                molecularWeight += xi * component.MolecularWeight;
            }

            properties.MolecularWeight = molecularWeight;

            // Simplified liquid density calculation
            // In practice, would use more sophisticated methods
            decimal averageCriticalTemperature = 0m;
            decimal averageCriticalPressure = 0m;

            foreach (var component in conditions.FeedComposition)
            {
                decimal xi = flashResult.LiquidComposition.ContainsKey(component.Name)
                    ? flashResult.LiquidComposition[component.Name]
                    : 0m;
                averageCriticalTemperature += xi * component.CriticalTemperature;
                averageCriticalPressure += xi * component.CriticalPressure;
            }

            // Simplified density calculation
            decimal reducedTemperature = conditions.Temperature / averageCriticalTemperature;
            decimal reducedPressure = conditions.Pressure / averageCriticalPressure;

            // Simplified correlation
            decimal liquidDensity = molecularWeight * 62.4m / (1.0m + 0.5m * reducedTemperature);

            properties.Density = liquidDensity;
            properties.SpecificGravity = molecularWeight / 28.9645m;

            return properties;
        }
    }
}


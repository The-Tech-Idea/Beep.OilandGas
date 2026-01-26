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
        private static List<FlashComponentKValue> InitializeKValues(FlashConditions conditions)
        {
            var kValues = new List<FlashComponentKValue>();

            foreach (var component in conditions.FeedComposition)
            {
                // Wilson correlation: K = (Pc/P) * exp(5.37 * (1 + ω) * (1 - Tc/T))
                decimal kValue = (component.CriticalPressure / conditions.Pressure) *
                               (decimal)Math.Exp((double)(5.37m * (1.0m + component.AcentricFactor) *
                                                          (1.0m - component.CriticalTemperature / conditions.Temperature)));

                kValues.Add(new FlashComponentKValue
                {
                    ComponentName = component.Name,
                    KValue = Math.Max(0.001m, Math.Min(1000m, kValue))
                });
            }

            return kValues;
        }

        /// <summary>
        /// Solves Rachford-Rice equation for vapor fraction.
        /// </summary>
        private static decimal SolveRachfordRice(
            FlashConditions conditions,
            List<FlashComponentKValue> kValues,
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
                    decimal kValue = GetKValue(kValues, component.Name);
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
            List<FlashComponentKValue> kValues,
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
                var existing = GetKValue(kValues, component.Name);
                var updated = 0.7m * existing + 0.3m * kValue;
                SetKValue(kValues, component.Name, Math.Max(0.001m, Math.Min(1000m, updated)));
            }
        }

        /// <summary>
        /// Calculates phase compositions.
        /// </summary>
        private static void CalculatePhaseCompositions(
            FlashConditions conditions,
            List<FlashComponentKValue> kValues,
            decimal vaporFraction,
            FlashResult result)
        {
            result.LiquidComposition ??= new List<FlashComponentFraction>();
            result.VaporComposition ??= new List<FlashComponentFraction>();

            foreach (var component in conditions.FeedComposition)
            {
                decimal zi = component.MoleFraction;
                decimal ki = GetKValue(kValues, component.Name);

                // Liquid composition: xi = zi / (1 + V * (Ki - 1))
                decimal denominator = 1.0m + vaporFraction * (ki - 1.0m);
                if (Math.Abs(denominator) < 0.0001m)
                    denominator = 0.0001m;

                decimal xi = zi / denominator;

                // Vapor composition: yi = Ki * xi
                decimal yi = ki * xi;

                SetCompositionFraction(result.LiquidComposition, component.Name, Math.Max(0m, Math.Min(1m, xi)));
                SetCompositionFraction(result.VaporComposition, component.Name, Math.Max(0m, Math.Min(1m, yi)));
            }

            // Normalize compositions
            NormalizeComposition(result.LiquidComposition);
            NormalizeComposition(result.VaporComposition);
        }

        /// <summary>
        /// Normalizes a composition to sum to 1.0.
        /// </summary>
        private static void NormalizeComposition(List<FlashComponentFraction> composition)
        {
            decimal sum = composition.Sum(c => c.Fraction);
            if (sum > 0m)
            {
                foreach (var component in composition)
                {
                    component.Fraction /= sum;
                }
            }
        }

        private static decimal GetKValue(List<FlashComponentKValue> kValues, string componentName)
        {
            var entry = kValues.FirstOrDefault(k =>
                string.Equals(k.ComponentName, componentName, StringComparison.OrdinalIgnoreCase));
            return entry?.KValue ?? 1m;
        }

        private static void SetKValue(List<FlashComponentKValue> kValues, string componentName, decimal value)
        {
            var entry = kValues.FirstOrDefault(k =>
                string.Equals(k.ComponentName, componentName, StringComparison.OrdinalIgnoreCase));
            if (entry == null)
            {
                kValues.Add(new FlashComponentKValue
                {
                    ComponentName = componentName,
                    KValue = value
                });
            }
            else
            {
                entry.KValue = value;
            }
        }

        private static decimal GetCompositionFraction(List<FlashComponentFraction>? composition, string componentName)
        {
            if (composition == null)
            {
                return 0m;
            }

            var entry = composition.FirstOrDefault(c =>
                string.Equals(c.ComponentName, componentName, StringComparison.OrdinalIgnoreCase));
            return entry?.Fraction ?? 0m;
        }

        private static void SetCompositionFraction(
            List<FlashComponentFraction> composition,
            string componentName,
            decimal fraction)
        {
            var entry = composition.FirstOrDefault(c =>
                string.Equals(c.ComponentName, componentName, StringComparison.OrdinalIgnoreCase));
            if (entry == null)
            {
                composition.Add(new FlashComponentFraction
                {
                    ComponentName = componentName,
                    Fraction = fraction
                });
            }
            else
            {
                entry.Fraction = fraction;
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
                decimal yi = GetCompositionFraction(flashResult.VaporComposition, component.Name);
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
                decimal xi = GetCompositionFraction(flashResult.LiquidComposition, component.Name);
                molecularWeight += xi * component.MolecularWeight;
            }

            properties.MolecularWeight = molecularWeight;

            // Simplified liquid density calculation
            // In practice, would use more sophisticated methods
            decimal averageCriticalTemperature = 0m;
            decimal averageCriticalPressure = 0m;

            foreach (var component in conditions.FeedComposition)
            {
                decimal xi = GetCompositionFraction(flashResult.LiquidComposition, component.Name);
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


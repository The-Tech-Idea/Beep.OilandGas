using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.GasProperties;
using Beep.OilandGas.GasProperties.Constants;

namespace Beep.OilandGas.GasProperties.Calculations
{
    /// <summary>
    /// Provides gas mixing rule calculations for gas mixtures.
    /// </summary>
    public static class GasMixingRuleCalculator
    {
        /// <summary>
        /// Calculates mixture properties using Kay's mixing rule.
        /// </summary>
        /// <param name="components">List of gas components with their mole fractions and properties.</param>
        /// <returns>Mixture properties (pseudo-critical pressure and temperature).</returns>
        public static (decimal PseudoCriticalPressure, decimal PseudoCriticalTemperature) CalculateKaysMixingRule(
            List<GasComponent> components)
        {
            if (components == null || components.Count == 0)
                throw new ArgumentException("Components list cannot be null or empty.", nameof(components));

            // Validate mole fractions sum to 1.0
            decimal totalMoleFraction = components.Sum(c => c.MoleFraction);
            if (Math.Abs(totalMoleFraction - 1.0m) > 0.01m)
            {
                throw new ArgumentException(
                    $"Mole fractions must sum to 1.0 (current sum: {totalMoleFraction:F4}).", 
                    nameof(components));
            }

            // Kay's mixing rule: weighted average of critical properties
            decimal pseudoCriticalPressure = 0m;
            decimal pseudoCriticalTemperature = 0m;

            foreach (var component in components)
            {
                pseudoCriticalPressure += component.MoleFraction * component.CriticalPressure;
                pseudoCriticalTemperature += component.MoleFraction * component.CriticalTemperature;
            }

            return (pseudoCriticalPressure, pseudoCriticalTemperature);
        }

        /// <summary>
        /// Calculates mixture specific gravity from component properties.
        /// </summary>
        /// <param name="components">List of gas components with their mole fractions and molecular weights.</param>
        /// <returns>Mixture specific gravity (relative to air).</returns>
        public static decimal CalculateMixtureSpecificGravity(List<GasComponent> components)
        {
            if (components == null || components.Count == 0)
                throw new ArgumentException("Components list cannot be null or empty.", nameof(components));

            // Calculate average molecular weight
            decimal averageMolecularWeight = 0m;

            foreach (var component in components)
            {
                averageMolecularWeight += component.MoleFraction * component.MolecularWeight;
            }

            // Specific gravity = molecular weight / air molecular weight
            decimal specificGravity = averageMolecularWeight / GasPropertiesConstants.AirMolecularWeight;

            return Math.Max(0.3m, Math.Min(1.5m, specificGravity));
        }

        /// <summary>
        /// Calculates mixture Z-factor using mixing rule and Z-factor calculator.
        /// </summary>
        /// <param name="components">List of gas components.</param>
        /// <param name="pressure">System pressure in psia.</param>
        /// <param name="temperature">System temperature in Rankine.</param>
        /// <param name="zFactorMethod">Z-factor calculation method.</param>
        /// <returns>Mixture Z-factor.</returns>
        public static decimal CalculateMixtureZFactor(
            List<GasComponent> components,
            decimal pressure,
            decimal temperature,
            Func<decimal, decimal, decimal, decimal> zFactorMethod)
        {
            if (components == null || components.Count == 0)
                throw new ArgumentException("Components list cannot be null or empty.", nameof(components));

            // Calculate pseudo-critical properties using Kay's mixing rule
            var (pseudoCriticalPressure, pseudoCriticalTemperature) = CalculateKaysMixingRule(components);

            // Calculate pseudo-reduced properties
            decimal pseudoReducedPressure = pressure / pseudoCriticalPressure;
            decimal pseudoReducedTemperature = temperature / pseudoCriticalTemperature;

            // Calculate mixture specific gravity
            decimal mixtureSpecificGravity = CalculateMixtureSpecificGravity(components);

            // Calculate Z-factor using the provided method
            decimal zFactor = zFactorMethod(pressure, temperature, mixtureSpecificGravity);

            return zFactor;
        }

        /// <summary>
        /// Calculates mixture viscosity using mixing rule.
        /// </summary>
        /// <param name="components">List of gas components.</param>
        /// <param name="pressure">System pressure in psia.</param>
        /// <param name="temperature">System temperature in Rankine.</param>
        /// <param name="zFactor">Mixture Z-factor.</param>
        /// <param name="viscosityMethod">Viscosity calculation method.</param>
        /// <returns>Mixture viscosity in centipoise.</returns>
        public static decimal CalculateMixtureViscosity(
            List<GasComponent> components,
            decimal pressure,
            decimal temperature,
            decimal zFactor,
            Func<decimal, decimal, decimal, decimal, decimal> viscosityMethod)
        {
            if (components == null || components.Count == 0)
                throw new ArgumentException("Components list cannot be null or empty.", nameof(components));

            // Calculate mixture specific gravity
            decimal mixtureSpecificGravity = CalculateMixtureSpecificGravity(components);

            // Calculate viscosity using the provided method
            decimal viscosity = viscosityMethod(pressure, temperature, mixtureSpecificGravity, zFactor);

            return viscosity;
        }

        /// <summary>
        /// Calculates mixture density.
        /// </summary>
        /// <param name="components">List of gas components.</param>
        /// <param name="pressure">System pressure in psia.</param>
        /// <param name="temperature">System temperature in Rankine.</param>
        /// <param name="zFactor">Mixture Z-factor.</param>
        /// <returns>Mixture density in lb/ft³.</returns>
        public static decimal CalculateMixtureDensity(
            List<GasComponent> components,
            decimal pressure,
            decimal temperature,
            decimal zFactor)
        {
            if (components == null || components.Count == 0)
                throw new ArgumentException("Components list cannot be null or empty.", nameof(components));

            // Calculate average molecular weight
            decimal averageMolecularWeight = 0m;
            foreach (var component in components)
            {
                averageMolecularWeight += component.MoleFraction * component.MolecularWeight;
            }

            // Calculate density using ideal gas law
            decimal density = (pressure * averageMolecularWeight) / 
                           (zFactor * GasPropertiesConstants.UniversalGasConstant * temperature);

            return Math.Max(0m, density);
        }
    }
}


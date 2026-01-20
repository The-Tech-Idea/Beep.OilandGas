using System;
using System.Linq;
using Beep.OilandGas.Models.Data.FlashCalculations;
using Beep.OilandGas.FlashCalculations.Constants;
using Beep.OilandGas.FlashCalculations.Exceptions;

namespace Beep.OilandGas.FlashCalculations.Validation
{
    /// <summary>
    /// Provides validation for flash calculations.
    /// </summary>
    public static class FlashValidator
    {
        /// <summary>
        /// Validates flash conditions.
        /// </summary>
        public static void ValidateFlashConditions(FlashConditions conditions)
        {
            if (conditions == null)
                throw new ArgumentNullException(nameof(conditions));

            if (conditions.Pressure <= 0)
                throw new InvalidFlashConditionsException("Pressure must be greater than zero.");

            if (conditions.Temperature <= 0)
                throw new InvalidFlashConditionsException("Temperature must be greater than zero.");

            if (conditions.FeedComposition == null || conditions.FeedComposition.Count == 0)
                throw new InvalidFlashConditionsException("Feed composition cannot be empty.");

            // Validate components
            foreach (var component in conditions.FeedComposition)
            {
                ValidateFlashComponent(component);
            }

            // Validate mole fractions sum to approximately 1.0
            decimal totalMoleFraction = conditions.FeedComposition.Sum(c => c.MoleFraction);
            if (Math.Abs(totalMoleFraction - 1.0m) > 0.1m)
            {
                throw new InvalidFlashConditionsException(
                    $"Feed composition mole fractions must sum to approximately 1.0 (current sum: {totalMoleFraction:F4}).");
            }
        }

        /// <summary>
        /// Validates component properties.
        /// </summary>
        public static void ValidateComponent(Component component)
        {
            if (component == null)
                throw new ArgumentNullException(nameof(component));

            if (string.IsNullOrWhiteSpace(component.Name))
                throw new InvalidComponentException("Component name cannot be empty.");

            if (component.MoleFraction < 0 || component.MoleFraction > 1)
                throw new InvalidComponentException($"Component {component.Name}: Mole fraction must be between 0 and 1.");

            if (component.CriticalTemperature <= 0)
                throw new InvalidComponentException($"Component {component.Name}: Critical temperature must be greater than zero.");

            if (component.CriticalPressure <= 0)
                throw new InvalidComponentException($"Component {component.Name}: Critical pressure must be greater than zero.");

            if (component.MolecularWeight <= 0)
                throw new InvalidComponentException($"Component {component.Name}: Molecular weight must be greater than zero.");
        }

        /// <summary>
        /// Validates flash component properties.
        /// </summary>
        public static void ValidateFlashComponent(FlashComponent component)
        {
            if (component == null)
                throw new ArgumentNullException(nameof(component));

            if (string.IsNullOrWhiteSpace(component.Name))
                throw new InvalidComponentException("Component name cannot be empty.");

            if (component.MoleFraction < 0 || component.MoleFraction > 1)
                throw new InvalidComponentException($"Component {component.Name}: Mole fraction must be between 0 and 1.");

            if (component.CriticalTemperature <= 0)
                throw new InvalidComponentException($"Component {component.Name}: Critical temperature must be greater than zero.");

            if (component.CriticalPressure <= 0)
                throw new InvalidComponentException($"Component {component.Name}: Critical pressure must be greater than zero.");

            if (component.MolecularWeight <= 0)
                throw new InvalidComponentException($"Component {component.Name}: Molecular weight must be greater than zero.");
        }

        /// <summary>
        /// Validates flash result.
        /// </summary>
        public static void ValidateFlashResult(FlashResult result)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));

            if (!result.Converged)
            {
                throw new FlashConvergenceException(
                    $"Flash calculation failed to converge after {result.Iterations} iterations. " +
                    $"Convergence error: {result.ConvergenceError:F6}.");
            }

            if (result.VaporFraction < 0 || result.VaporFraction > 1)
            {
                throw new FlashConvergenceException(
                    $"Invalid vapor fraction: {result.VaporFraction:F4}. Must be between 0 and 1.");
            }
        }
    }
}


using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.PumpPerformance.Constants;
using Beep.OilandGas.PumpPerformance.Exceptions;
using static Beep.OilandGas.PumpPerformance.Constants.PumpConstants;

namespace Beep.OilandGas.PumpPerformance.Validation
{
    /// <summary>
    /// Provides validation methods for pump performance calculation inputs.
    /// </summary>
    public static class PumpDataValidator
    {
        /// <summary>
        /// Validates flow rate values.
        /// </summary>
        /// <param name="flowRates">Array of flow rate values.</param>
        /// <param name="paramName">Name of the parameter for error messages.</param>
        /// <exception cref="ArgumentNullException">Thrown when flowRates is null.</exception>
        /// <exception cref="InvalidInputException">Thrown when flow rates are invalid.</exception>
        public static void ValidateFlowRates(double[] flowRates, string paramName)
        {
            if (flowRates == null)
                throw new ArgumentNullException(paramName);

            if (flowRates.Length < MinDataPoints)
                throw new InvalidInputException(paramName, 
                    $"Flow rates array must contain at least {MinDataPoints} data points. Provided: {flowRates.Length}.");

            for (int i = 0; i < flowRates.Length; i++)
            {
                if (double.IsNaN(flowRates[i]) || double.IsInfinity(flowRates[i]))
                    throw new InvalidInputException(paramName, 
                        $"Flow rate at index {i} is not a valid number. Value: {flowRates[i]}.");

                if (flowRates[i] < MinFlowRate)
                    throw new InvalidInputException(paramName, 
                        $"Flow rate at index {i} must be non-negative. Value: {flowRates[i]}.");

                if (flowRates[i] > MaxFlowRate)
                    throw new InvalidInputException(paramName, 
                        $"Flow rate at index {i} exceeds maximum reasonable value ({MaxFlowRate}). Value: {flowRates[i]}.");
            }
        }

        /// <summary>
        /// Validates head values.
        /// </summary>
        /// <param name="heads">Array of head values.</param>
        /// <param name="paramName">Name of the parameter for error messages.</param>
        /// <exception cref="ArgumentNullException">Thrown when heads is null.</exception>
        /// <exception cref="InvalidInputException">Thrown when heads are invalid.</exception>
        public static void ValidateHeads(double[] heads, string paramName)
        {
            if (heads == null)
                throw new ArgumentNullException(paramName);

            if (heads.Length < MinDataPoints)
                throw new InvalidInputException(paramName, 
                    $"Heads array must contain at least {MinDataPoints} data points. Provided: {heads.Length}.");

            for (int i = 0; i < heads.Length; i++)
            {
                if (double.IsNaN(heads[i]) || double.IsInfinity(heads[i]))
                    throw new InvalidInputException(paramName, 
                        $"Head at index {i} is not a valid number. Value: {heads[i]}.");

                if (heads[i] < MinHead)
                    throw new InvalidInputException(paramName, 
                        $"Head at index {i} must be non-negative. Value: {heads[i]}.");

                if (heads[i] > MaxHead)
                    throw new InvalidInputException(paramName, 
                        $"Head at index {i} exceeds maximum reasonable value ({MaxHead}). Value: {heads[i]}.");
            }
        }

        /// <summary>
        /// Validates power values.
        /// </summary>
        /// <param name="powers">Array of power values.</param>
        /// <param name="paramName">Name of the parameter for error messages.</param>
        /// <exception cref="ArgumentNullException">Thrown when powers is null.</exception>
        /// <exception cref="InvalidInputException">Thrown when powers are invalid.</exception>
        public static void ValidatePowers(double[] powers, string paramName)
        {
            if (powers == null)
                throw new ArgumentNullException(paramName);

            if (powers.Length < MinDataPoints)
                throw new InvalidInputException(paramName, 
                    $"Powers array must contain at least {MinDataPoints} data points. Provided: {powers.Length}.");

            for (int i = 0; i < powers.Length; i++)
            {
                if (double.IsNaN(powers[i]) || double.IsInfinity(powers[i]))
                    throw new InvalidInputException(paramName, 
                        $"Power at index {i} is not a valid number. Value: {powers[i]}.");

                if (powers[i] < MinPower)
                    throw new InvalidInputException(paramName, 
                        $"Power at index {i} must be non-negative. Value: {powers[i]}.");

                if (powers[i] > MaxPower)
                    throw new InvalidInputException(paramName, 
                        $"Power at index {i} exceeds maximum reasonable value ({MaxPower}). Value: {powers[i]}.");
            }
        }

        /// <summary>
        /// Validates that arrays have matching lengths.
        /// </summary>
        /// <param name="array1">First array.</param>
        /// <param name="array2">Second array.</param>
        /// <param name="array1Name">Name of the first array.</param>
        /// <param name="array2Name">Name of the second array.</param>
        /// <exception cref="InvalidInputException">Thrown when arrays have different lengths.</exception>
        public static void ValidateMatchingLengths(double[] array1, double[] array2, string array1Name, string array2Name)
        {
            if (array1 == null || array2 == null)
                return; // Null check handled by individual validators

            if (array1.Length != array2.Length)
                throw new InvalidInputException(
                    $"{array1Name} and {array2Name} must have the same length. " +
                    $"{array1Name}: {array1.Length}, {array2Name}: {array2.Length}.");
        }

        /// <summary>
        /// Validates a single flow rate value.
        /// </summary>
        /// <param name="flowRate">Flow rate value.</param>
        /// <param name="paramName">Name of the parameter.</param>
        /// <exception cref="InvalidInputException">Thrown when flow rate is invalid.</exception>
        public static void ValidateFlowRate(double flowRate, string paramName)
        {
            if (double.IsNaN(flowRate) || double.IsInfinity(flowRate))
                throw new InvalidInputException(paramName, 
                    $"Flow rate must be a valid number. Value: {flowRate}.");

            if (flowRate < MinFlowRate)
                throw new InvalidInputException(paramName, 
                    $"Flow rate must be non-negative. Value: {flowRate}.");

            if (flowRate > MaxFlowRate)
                throw new InvalidInputException(paramName, 
                    $"Flow rate exceeds maximum reasonable value ({MaxFlowRate}). Value: {flowRate}.");
        }

        /// <summary>
        /// Validates a single head value.
        /// </summary>
        /// <param name="head">Head value.</param>
        /// <param name="paramName">Name of the parameter.</param>
        /// <exception cref="InvalidInputException">Thrown when head is invalid.</exception>
        public static void ValidateHead(double head, string paramName)
        {
            if (double.IsNaN(head) || double.IsInfinity(head))
                throw new InvalidInputException(paramName, 
                    $"Head must be a valid number. Value: {head}.");

            if (head < MinHead)
                throw new InvalidInputException(paramName, 
                    $"Head must be non-negative. Value: {head}.");

            if (head > MaxHead)
                throw new InvalidInputException(paramName, 
                    $"Head exceeds maximum reasonable value ({MaxHead}). Value: {head}.");
        }

        /// <summary>
        /// Validates a single power value.
        /// </summary>
        /// <param name="power">Power value.</param>
        /// <param name="paramName">Name of the parameter.</param>
        /// <exception cref="InvalidInputException">Thrown when power is invalid.</exception>
        public static void ValidatePower(double power, string paramName)
        {
            if (double.IsNaN(power) || double.IsInfinity(power))
                throw new InvalidInputException(paramName, 
                    $"Power must be a valid number. Value: {power}.");

            if (power < MinPower)
                throw new InvalidInputException(paramName, 
                    $"Power must be non-negative. Value: {power}.");

            if (power > MaxPower)
                throw new InvalidInputException(paramName, 
                    $"Power exceeds maximum reasonable value ({MaxPower}). Value: {power}.");
        }

        /// <summary>
        /// Validates specific gravity value.
        /// </summary>
        /// <param name="specificGravity">Specific gravity value.</param>
        /// <param name="paramName">Name of the parameter.</param>
        /// <exception cref="InvalidInputException">Thrown when specific gravity is invalid.</exception>
        public static void ValidateSpecificGravity(double specificGravity, string paramName)
        {
            if (double.IsNaN(specificGravity) || double.IsInfinity(specificGravity))
                throw new InvalidInputException(paramName, 
                    $"Specific gravity must be a valid number. Value: {specificGravity}.");

            if (specificGravity < MinSpecificGravity || specificGravity > MaxSpecificGravity)
                throw new InvalidInputException(paramName, 
                    $"Specific gravity must be between {MinSpecificGravity} and {MaxSpecificGravity}. Value: {specificGravity}.");
        }

        /// <summary>
        /// Validates efficiency value.
        /// </summary>
        /// <param name="efficiency">Efficiency value (0 to 1, or slightly above for theoretical).</param>
        /// <param name="paramName">Name of the parameter.</param>
        /// <exception cref="InvalidInputException">Thrown when efficiency is invalid.</exception>
        public static void ValidateEfficiency(double efficiency, string paramName)
        {
            if (double.IsNaN(efficiency) || double.IsInfinity(efficiency))
                throw new InvalidInputException(paramName, 
                    $"Efficiency must be a valid number. Value: {efficiency}.");

            if (efficiency < MinEfficiency)
                throw new InvalidInputException(paramName, 
                    $"Efficiency must be non-negative. Value: {efficiency}.");

            if (efficiency > MaxEfficiency)
                throw new InvalidInputException(paramName, 
                    $"Efficiency exceeds maximum reasonable value ({MaxEfficiency}). Value: {efficiency}.");
        }
    }
}


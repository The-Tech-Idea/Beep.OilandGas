using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data.GasProperties;
using Beep.OilandGas.GasProperties.Calculations;

namespace Beep.OilandGas.GasProperties.Calculations
{
    /// <summary>
    /// Provides average gas property calculation methods.
    /// </summary>
    public static class AveragePropertiesCalculator
    {
        /// <summary>
        /// Calculates average temperature and Z-factor using pressure-weighted method.
        /// </summary>
        /// <param name="pressures">List of pressures in psia.</param>
        /// <param name="temperatures">List of temperatures in Rankine.</param>
        /// <param name="specificGravity">Gas specific gravity (relative to air).</param>
        /// <param name="zFactorMethod">Z-factor calculation method.</param>
        /// <returns>Average gas properties.</returns>
        public static AVERAGE_GAS_PROPERTIES CalculatePressureWeightedAverage(
            List<decimal> pressures,
            List<decimal> temperatures,
            decimal specificGravity,
            Func<decimal, decimal, decimal, decimal> zFactorMethod)
        {
            if (pressures == null || pressures.Count == 0)
                throw new ArgumentException("Pressures list cannot be null or empty.", nameof(pressures));

            if (temperatures == null || temperatures.Count == 0)
                throw new ArgumentException("Temperatures list cannot be null or empty.", nameof(temperatures));

            if (pressures.Count != temperatures.Count)
                throw new ArgumentException("Pressures and temperatures lists must have the same count.");

            if (specificGravity <= 0)
                throw new ArgumentException("Specific gravity must be greater than zero.", nameof(specificGravity));

            decimal totalPressure = 0m;
            decimal totalTemperature = 0m;
            decimal totalZFactor = 0m;
            decimal totalWeight = 0m;

            for (int i = 0; i < pressures.Count; i++)
            {
                decimal pressure = pressures[i];
                decimal temperature = temperatures[i];
                decimal weight = pressure; // Pressure-weighted

                decimal zFactor = zFactorMethod(pressure, temperature, specificGravity);

                totalPressure += pressure * weight;
                totalTemperature += temperature * weight;
                totalZFactor += zFactor * weight;
                totalWeight += weight;
            }

            if (totalWeight == 0)
                throw new InvalidOperationException("Total weight cannot be zero.");

            return new AVERAGE_GAS_PROPERTIES
            {
                AVERAGE_PRESSURE = totalPressure / totalWeight,
                AVERAGE_TEMPERATURE = totalTemperature / totalWeight,
                AVERAGE_Z_FACTOR = totalZFactor / totalWeight
            };
        }

        /// <summary>
        /// Calculates average temperature and Z-factor using simple arithmetic mean.
        /// </summary>
        public static AVERAGE_GAS_PROPERTIES CalculateArithmeticAverage(
            List<decimal> pressures,
            List<decimal> temperatures,
            decimal specificGravity,
            Func<decimal, decimal, decimal, decimal> zFactorMethod)
        {
            if (pressures == null || pressures.Count == 0)
                throw new ArgumentException("Pressures list cannot be null or empty.", nameof(pressures));

            if (temperatures == null || temperatures.Count == 0)
                throw new ArgumentException("Temperatures list cannot be null or empty.", nameof(temperatures));

            if (pressures.Count != temperatures.Count)
                throw new ArgumentException("Pressures and temperatures lists must have the same count.");

            decimal averagePressure = pressures.Average();
            decimal averageTemperature = temperatures.Average();
            decimal averageZFactor = 0m;

            // Calculate Z-factor at average conditions
            if (averagePressure > 0 && averageTemperature > 0)
            {
                averageZFactor = zFactorMethod(averagePressure, averageTemperature, specificGravity);
            }

            return new AVERAGE_GAS_PROPERTIES
            {
                AVERAGE_PRESSURE = averagePressure,
                AVERAGE_TEMPERATURE = averageTemperature,
                AVERAGE_Z_FACTOR = averageZFactor
            };
        }

        /// <summary>
        /// Calculates average temperature and Z-factor over a pressure range.
        /// </summary>
        public static AVERAGE_GAS_PROPERTIES CalculateAverageOverRange(
            decimal minPressure,
            decimal maxPressure,
            decimal temperature,
            decimal specificGravity,
            int numberOfPoints,
            Func<decimal, decimal, decimal, decimal> zFactorMethod,
            Func<decimal, decimal, decimal, decimal, decimal> viscosityMethod)
        {
            if (minPressure <= 0)
                throw new ArgumentException("Minimum pressure must be greater than zero.", nameof(minPressure));

            if (maxPressure <= minPressure)
                throw new ArgumentException("Maximum pressure must be greater than minimum pressure.", nameof(maxPressure));

            if (numberOfPoints < 2)
                throw new ArgumentException("Number of points must be at least 2.", nameof(numberOfPoints));

            decimal pressureStep = (maxPressure - minPressure) / (numberOfPoints - 1);
            decimal totalZFactor = 0m;
            decimal totalViscosity = 0m;

            for (int i = 0; i < numberOfPoints; i++)
            {
                decimal pressure = minPressure + i * pressureStep;
                decimal zFactor = zFactorMethod(pressure, temperature, specificGravity);
                decimal viscosity = viscosityMethod(pressure, temperature, specificGravity, zFactor);

                totalZFactor += zFactor;
                totalViscosity += viscosity;
            }

            return new AVERAGE_GAS_PROPERTIES
            {
                AVERAGE_PRESSURE = (minPressure + maxPressure) / 2m,
                AVERAGE_TEMPERATURE = temperature,
                AVERAGE_Z_FACTOR = totalZFactor / numberOfPoints,
                AVERAGE_VISCOSITY = totalViscosity / numberOfPoints
            };
        }
    }
}


using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.NodalAnalysis;
using Beep.OilandGas.GasProperties.Calculations;

namespace Beep.OilandGas.NodalAnalysis.Calculations
{
    /// <summary>
    /// Provides Bottom Hole Pressure (BHP) correlation methods for nodal analysis.
    /// </summary>
    public static class BHPCorrelations
    {
        /// <summary>
        /// Calculates BHP using Poettmann-Carpenter correlation.
        /// </summary>
        /// <param name="wellheadPressure">Wellhead pressure in psia.</param>
        /// <param name="depth">Well depth in feet.</param>
        /// <param name="flowRate">Flow rate in bbl/day.</param>
        /// <param name="gasLiquidRatio">Gas-liquid ratio in scf/bbl.</param>
        /// <param name="oilGravity">Oil gravity in API.</param>
        /// <param name="gasSpecificGravity">Gas specific gravity (relative to air).</param>
        /// <param name="temperature">Average temperature in Rankine.</param>
        /// <returns>Bottom hole pressure in psia.</returns>
        public static decimal CalculatePoettmannCarpenter(
            decimal wellheadPressure,
            decimal depth,
            decimal flowRate,
            decimal gasLiquidRatio,
            decimal oilGravity,
            decimal gasSpecificGravity,
            decimal temperature)
        {
            if (wellheadPressure <= 0)
                throw new ArgumentException("Wellhead pressure must be greater than zero.", nameof(wellheadPressure));

            if (depth <= 0)
                throw new ArgumentException("Depth must be greater than zero.", nameof(depth));

            if (flowRate <= 0)
                throw new ArgumentException("Flow rate must be greater than zero.", nameof(flowRate));

            // Calculate average pressure (simplified)
            decimal averagePressure = wellheadPressure * 1.1m; // Approximation

            // Calculate Z-factor at average conditions
            decimal zFactor = ZFactorCalculator.CalculateBrillBeggs(
                averagePressure, temperature, gasSpecificGravity);

            // Calculate oil density
            decimal oilDensity = 141.5m / (131.5m + oilGravity); // Specific gravity

            // Calculate gas density
            decimal gasDensity = (averagePressure * gasSpecificGravity * 28.9645m) /
                                (zFactor * 10.7316m * temperature);

            // Calculate mixture density
            decimal liquidDensity = oilDensity * 62.4m; // lb/ft³
            decimal gasVolumeFactor = gasLiquidRatio * zFactor * temperature / (averagePressure * 5.614m);
            decimal mixtureDensity = (liquidDensity + gasDensity * gasVolumeFactor) / (1.0m + gasVolumeFactor);

            // Poettmann-Carpenter correlation
            decimal frictionFactor = CalculateFrictionFactor(flowRate, depth, mixtureDensity);
            decimal pressureGradient = mixtureDensity / 144.0m; // psia/ft
            decimal frictionLoss = frictionFactor * flowRate * flowRate / (depth * depth);

            decimal bottomHolePressure = wellheadPressure + pressureGradient * depth + frictionLoss;

            return Math.Max(wellheadPressure, bottomHolePressure);
        }

        /// <summary>
        /// Calculates BHP using Hagedorn-Brown correlation.
        /// </summary>
        public static decimal CalculateHagedornBrown(
            decimal wellheadPressure,
            decimal depth,
            decimal flowRate,
            decimal gasLiquidRatio,
            decimal oilGravity,
            decimal gasSpecificGravity,
            decimal temperature,
            decimal waterCut = 0m)
        {
            if (wellheadPressure <= 0)
                throw new ArgumentException("Wellhead pressure must be greater than zero.", nameof(wellheadPressure));

            if (depth <= 0)
                throw new ArgumentException("Depth must be greater than zero.", nameof(depth));

            // Hagedorn-Brown correlation (simplified version)
            decimal averagePressure = wellheadPressure * 1.1m;
            decimal zFactor = ZFactorCalculator.CalculateBrillBeggs(
                averagePressure, temperature, gasSpecificGravity);

            // Calculate mixture properties
            decimal oilDensity = (141.5m / (131.5m + oilGravity)) * 62.4m;
            decimal waterDensity = 62.4m;
            decimal mixtureDensity = oilDensity * (1.0m - waterCut) + waterDensity * waterCut;

            // Gas properties
            decimal gasDensity = (averagePressure * gasSpecificGravity * 28.9645m) /
                                (zFactor * 10.7316m * temperature);

            // Hagedorn-Brown flow pattern and pressure drop
            decimal pressureGradient = CalculateHagedornBrownGradient(
                flowRate, depth, mixtureDensity, gasDensity, gasLiquidRatio, zFactor, temperature, averagePressure);

            decimal bottomHolePressure = wellheadPressure + pressureGradient * depth;

            return Math.Max(wellheadPressure, bottomHolePressure);
        }

        /// <summary>
        /// Calculates BHP using Cullender-Smith method for gas wells.
        /// </summary>
        public static decimal CalculateCullenderSmith(
            decimal wellheadPressure,
            decimal depth,
            decimal flowRate,
            decimal gasSpecificGravity,
            decimal temperature,
            decimal wellheadTemperature)
        {
            if (wellheadPressure <= 0)
                throw new ArgumentException("Wellhead pressure must be greater than zero.", nameof(wellheadPressure));

            if (depth <= 0)
                throw new ArgumentException("Depth must be greater than zero.", nameof(depth));

            // Cullender-Smith method for gas wells
            decimal averageTemperature = (temperature + wellheadTemperature) / 2.0m;
            decimal temperatureGradient = (temperature - wellheadTemperature) / depth;

            // Iterative solution
            decimal bottomHolePressure = wellheadPressure;
            decimal oldBHP = 0m;
            int iterations = 0;
            const int maxIterations = 100;
            const decimal tolerance = 0.1m;

            while (Math.Abs(bottomHolePressure - oldBHP) > tolerance && iterations < maxIterations)
            {
                oldBHP = bottomHolePressure;
                decimal averagePressure = (wellheadPressure + bottomHolePressure) / 2.0m;
                decimal averageTemp = wellheadTemperature + temperatureGradient * depth / 2.0m;

                decimal zFactor = ZFactorCalculator.CalculateBrillBeggs(
                    averagePressure, averageTemp, gasSpecificGravity);

                // Cullender-Smith equation
                decimal pseudoPressure = CalculatePseudoPressureForGas(
                    wellheadPressure, bottomHolePressure, averageTemp, gasSpecificGravity, zFactor);

                decimal frictionFactor = CalculateGasFrictionFactor(flowRate, depth, averagePressure, averageTemp, zFactor, gasSpecificGravity);
                decimal pressureDrop = frictionFactor * flowRate * flowRate * depth;

                bottomHolePressure = wellheadPressure + pressureDrop;

                iterations++;
            }

            return Math.Max(wellheadPressure, bottomHolePressure);
        }

        /// <summary>
        /// Calculates BHP using Guo-Ghalambor correlation.
        /// </summary>
        public static decimal CalculateGuoGhalambor(
            decimal wellheadPressure,
            decimal depth,
            decimal flowRate,
            decimal gasLiquidRatio,
            decimal oilGravity,
            decimal gasSpecificGravity,
            decimal temperature)
        {
            if (wellheadPressure <= 0)
                throw new ArgumentException("Wellhead pressure must be greater than zero.", nameof(wellheadPressure));

            if (depth <= 0)
                throw new ArgumentException("Depth must be greater than zero.", nameof(depth));

            // Guo-Ghalambor correlation
            decimal averagePressure = wellheadPressure * 1.1m;
            decimal zFactor = ZFactorCalculator.CalculateBrillBeggs(
                averagePressure, temperature, gasSpecificGravity);

            // Calculate mixture properties
            decimal oilDensity = (141.5m / (131.5m + oilGravity)) * 62.4m;
            decimal gasDensity = (averagePressure * gasSpecificGravity * 28.9645m) /
                                (zFactor * 10.7316m * temperature);

            // Guo-Ghalambor specific calculations
            decimal mixtureDensity = CalculateGuoGhalamborMixtureDensity(
                oilDensity, gasDensity, gasLiquidRatio, zFactor, temperature, averagePressure);

            decimal pressureGradient = mixtureDensity / 144.0m;
            decimal frictionLoss = CalculateGuoGhalamborFriction(flowRate, depth, mixtureDensity);

            decimal bottomHolePressure = wellheadPressure + pressureGradient * depth + frictionLoss;

            return Math.Max(wellheadPressure, bottomHolePressure);
        }

        /// <summary>
        /// Calculates BHP using Gray-Gray correlation for oil wells.
        /// Gray-Gray is a multiphase flow correlation that accounts for gas-liquid interactions.
        /// </summary>
        /// <param name="wellheadPressure">Wellhead pressure in psia.</param>
        /// <param name="depth">Well depth in feet.</param>
        /// <param name="flowRate">Flow rate in bbl/day.</param>
        /// <param name="gasLiquidRatio">Gas-liquid ratio in scf/bbl.</param>
        /// <param name="oilGravity">Oil gravity in API.</param>
        /// <param name="gasSpecificGravity">Gas specific gravity (relative to air).</param>
        /// <param name="temperature">Average temperature in Rankine.</param>
        /// <param name="waterCut">Water cut (fraction, 0-1).</param>
        /// <param name="tubingDiameter">Tubing diameter in inches.</param>
        /// <returns>Bottom hole pressure in psia.</returns>
        public static decimal CalculateGrayGray(
            decimal wellheadPressure,
            decimal depth,
            decimal flowRate,
            decimal gasLiquidRatio,
            decimal oilGravity,
            decimal gasSpecificGravity,
            decimal temperature,
            decimal waterCut = 0m,
            decimal tubingDiameter = 2.875m)
        {
            if (wellheadPressure <= 0)
                throw new ArgumentException("Wellhead pressure must be greater than zero.", nameof(wellheadPressure));

            if (depth <= 0)
                throw new ArgumentException("Depth must be greater than zero.", nameof(depth));

            if (flowRate <= 0)
                throw new ArgumentException("Flow rate must be greater than zero.", nameof(flowRate));

            // Gray-Gray correlation for multiphase flow
            // Iterative solution for pressure drop calculation

            decimal bottomHolePressure = wellheadPressure;
            decimal oldBHP = 0m;
            int iterations = 0;
            const int maxIterations = 100;
            const decimal tolerance = 0.1m;

            while (Math.Abs(bottomHolePressure - oldBHP) > tolerance && iterations < maxIterations)
            {
                oldBHP = bottomHolePressure;
                decimal averagePressure = (wellheadPressure + bottomHolePressure) / 2.0m;

                // Calculate Z-factor at average conditions
                decimal zFactor = ZFactorCalculator.CalculateBrillBeggs(
                    averagePressure, temperature, gasSpecificGravity);

                // Calculate fluid properties
                decimal oilDensity = (141.5m / (131.5m + oilGravity)) * 62.4m; // lb/ft³
                decimal waterDensity = 62.4m; // lb/ft³
                decimal liquidDensity = oilDensity * (1.0m - waterCut) + waterDensity * waterCut;

                // Gas density
                decimal gasDensity = (averagePressure * gasSpecificGravity * 28.9645m) /
                                    (zFactor * 10.7316m * temperature);

                // Gas volume factor
                decimal gasVolumeFactor = gasLiquidRatio * zFactor * temperature / (averagePressure * 5.614m);

                // Mixture density
                decimal mixtureDensity = (liquidDensity + gasDensity * gasVolumeFactor) / (1.0m + gasVolumeFactor);

                // Gray-Gray specific calculations
                decimal pressureGradient = CalculateGrayGrayGradient(
                    flowRate, depth, mixtureDensity, liquidDensity, gasDensity, gasLiquidRatio,
                    zFactor, temperature, averagePressure, tubingDiameter);

                // Friction loss
                decimal frictionLoss = CalculateGrayGrayFriction(
                    flowRate, depth, mixtureDensity, tubingDiameter);

                bottomHolePressure = wellheadPressure + pressureGradient * depth + frictionLoss;

                iterations++;
            }

            return Math.Max(wellheadPressure, bottomHolePressure);
        }

        // Helper methods

        private static decimal CalculateFrictionFactor(decimal flowRate, decimal depth, decimal density)
        {
            // Simplified friction factor calculation
            decimal reynoldsNumber = flowRate * density / (0.001m * depth); // Simplified
            if (reynoldsNumber < 2000m)
                return 64m / reynoldsNumber; // Laminar
            else
                return 0.3164m / (decimal)Math.Pow((double)reynoldsNumber, 0.25); // Turbulent (Blasius)
        }

        private static decimal CalculateHagedornBrownGradient(
            decimal flowRate, decimal depth, decimal liquidDensity, decimal gasDensity,
            decimal gasLiquidRatio, decimal zFactor, decimal temperature, decimal pressure)
        {
            // Simplified Hagedorn-Brown gradient calculation
            decimal gasVolumeFactor = gasLiquidRatio * zFactor * temperature / (pressure * 5.614m);
            decimal mixtureDensity = (liquidDensity + gasDensity * gasVolumeFactor) / (1.0m + gasVolumeFactor);
            return mixtureDensity / 144.0m;
        }

        private static decimal CalculatePseudoPressureForGas(
            decimal p1, decimal p2, decimal temperature, decimal gasSpecificGravity, decimal zFactor)
        {
            // Simplified pseudo-pressure calculation
            return (p2 * p2 - p1 * p1) / (2.0m * zFactor);
        }

        private static decimal CalculateGasFrictionFactor(
            decimal flowRate, decimal depth, decimal pressure, decimal temperature,
            decimal zFactor, decimal gasSpecificGravity)
        {
            // Simplified gas friction factor
            decimal gasDensity = (pressure * gasSpecificGravity * 28.9645m) / (zFactor * 10.7316m * temperature);
            decimal reynoldsNumber = flowRate * gasDensity / (0.001m * depth);
            if (reynoldsNumber < 2000m)
                return 64m / reynoldsNumber;
            else
                return 0.3164m / (decimal)Math.Pow((double)reynoldsNumber, 0.25);
        }

        private static decimal CalculateGuoGhalamborMixtureDensity(
            decimal oilDensity, decimal gasDensity, decimal gasLiquidRatio,
            decimal zFactor, decimal temperature, decimal pressure)
        {
            decimal gasVolumeFactor = gasLiquidRatio * zFactor * temperature / (pressure * 5.614m);
            return (oilDensity + gasDensity * gasVolumeFactor) / (1.0m + gasVolumeFactor);
        }

        private static decimal CalculateGuoGhalamborFriction(decimal flowRate, decimal depth, decimal density)
        {
            decimal reynoldsNumber = flowRate * density / (0.001m * depth);
            decimal frictionFactor = reynoldsNumber < 2000m
                ? 64m / reynoldsNumber
                : 0.3164m / (decimal)Math.Pow((double)reynoldsNumber, 0.25);
            return frictionFactor * flowRate * flowRate / (depth * depth);
        }

        private static decimal CalculateGrayGrayGradient(
            decimal flowRate, decimal depth, decimal mixtureDensity, decimal liquidDensity,
            decimal gasDensity, decimal gasLiquidRatio, decimal zFactor, decimal temperature,
            decimal pressure, decimal tubingDiameter)
        {
            // Gray-Gray gradient calculation
            // Accounts for slip between gas and liquid phases

            // Calculate superficial velocities
            decimal tubingArea = (decimal)Math.PI * tubingDiameter * tubingDiameter / 4m; // square inches
            decimal tubingAreaFt2 = tubingArea / 144m; // square feet

            decimal liquidFlowRate = flowRate * 5.615m / 86400m; // ft³/s
            decimal gasFlowRate = gasLiquidRatio * flowRate * 5.615m / 86400m / 1000m; // ft³/s (simplified)

            decimal liquidSuperficialVelocity = liquidFlowRate / tubingAreaFt2; // ft/s
            decimal gasSuperficialVelocity = gasFlowRate / tubingAreaFt2; // ft/s

            // Calculate slip velocity (Gray-Gray specific)
            decimal slipVelocity = CalculateGrayGraySlipVelocity(
                liquidDensity, gasDensity, gasLiquidRatio, tubingDiameter);

            // Mixture velocity
            decimal mixtureVelocity = liquidSuperficialVelocity + gasSuperficialVelocity;

            // Pressure gradient (hydrostatic + acceleration)
            decimal hydrostaticGradient = mixtureDensity / 144.0m; // psia/ft

            // Acceleration term (Gray-Gray specific)
            decimal accelerationGradient = CalculateGrayGrayAcceleration(
                mixtureVelocity, slipVelocity, liquidDensity, gasDensity, gasLiquidRatio);

            return hydrostaticGradient + accelerationGradient;
        }

        private static decimal CalculateGrayGraySlipVelocity(
            decimal liquidDensity, decimal gasDensity, decimal gasLiquidRatio, decimal tubingDiameter)
        {
            // Gray-Gray slip velocity calculation
            // Simplified model based on density difference and flow regime

            decimal densityDifference = liquidDensity - gasDensity;
            decimal slipVelocity = 0.5m * (decimal)Math.Sqrt((double)(densityDifference / gasDensity)) * 
                                 (decimal)Math.Pow((double)(tubingDiameter / 12m), 0.25);

            // Adjust for gas-liquid ratio
            decimal glrFactor = 1.0m + (gasLiquidRatio / 1000m) * 0.1m;
            slipVelocity *= glrFactor;

            return Math.Max(0.1m, Math.Min(5.0m, slipVelocity)); // Clamp to reasonable range
        }

        private static decimal CalculateGrayGrayAcceleration(
            decimal mixtureVelocity, decimal slipVelocity, decimal liquidDensity,
            decimal gasDensity, decimal gasLiquidRatio)
        {
            // Acceleration gradient in Gray-Gray correlation
            // Accounts for kinetic energy changes

            decimal velocityTerm = mixtureVelocity * slipVelocity / (32.174m * 144m); // psia/ft
            decimal densityTerm = (liquidDensity + gasDensity) / 2m;

            return velocityTerm * densityTerm / 144m;
        }

        private static decimal CalculateGrayGrayFriction(
            decimal flowRate, decimal depth, decimal mixtureDensity, decimal tubingDiameter)
        {
            // Gray-Gray friction calculation
            // Uses mixture properties for friction factor

            decimal tubingArea = (decimal)Math.PI * tubingDiameter * tubingDiameter / 4m;
            decimal velocity = (flowRate * 5.615m) / (86400m * tubingArea / 144m); // ft/s

            decimal reynoldsNumber = mixtureDensity * velocity * (tubingDiameter / 12m) / 0.001m;
            decimal frictionFactor = reynoldsNumber < 2000m
                ? 64m / reynoldsNumber
                : 0.3164m / (decimal)Math.Pow((double)reynoldsNumber, 0.25);

            decimal frictionGradient = frictionFactor * (depth / (tubingDiameter / 12m)) *
                                      (velocity * velocity) / (2m * 32.174m) * mixtureDensity / 144m;

            return frictionGradient * depth; // Total friction loss
        }
    }
}


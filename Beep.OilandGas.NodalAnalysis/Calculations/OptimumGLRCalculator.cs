using System;
using System.Collections.Generic;
using Beep.OilandGas.GasProperties.Calculations;

namespace Beep.OilandGas.NodalAnalysis.Calculations
{
    /// <summary>
    /// Provides optimum gas-liquid ratio (GLR) calculations for production optimization.
    /// </summary>
    public static class OptimumGLRCalculator
    {
        /// <summary>
        /// Calculates optimum GLR for oil wells using Vogel's IPR and flow correlations.
        /// </summary>
        /// <param name="reservoirPressure">Reservoir pressure in psia.</param>
        /// <param name="wellheadPressure">Wellhead pressure in psia.</param>
        /// <param name="tubingDiameter">Tubing diameter in inches.</param>
        /// <param name="tubingLength">Tubing length in feet.</param>
        /// <param name="oilRate">Desired oil production rate in bbl/day.</param>
        /// <param name="oilGravity">Oil API gravity.</param>
        /// <param name="gasSpecificGravity">Gas specific gravity (relative to air).</param>
        /// <param name="temperature">Average temperature in Rankine.</param>
        /// <param name="waterCut">Water cut (fraction).</param>
        /// <returns>Optimum GLR in scf/STB.</returns>
        public static decimal CalculateOptimumGLRForOilWell(
            decimal reservoirPressure,
            decimal wellheadPressure,
            decimal tubingDiameter,
            decimal tubingLength,
            decimal oilRate,
            decimal oilGravity,
            decimal gasSpecificGravity,
            decimal temperature,
            decimal waterCut = 0m)
        {
            if (reservoirPressure <= 0)
                throw new ArgumentException("Reservoir pressure must be greater than zero.", nameof(reservoirPressure));

            if (wellheadPressure <= 0)
                throw new ArgumentException("Wellhead pressure must be greater than zero.", nameof(wellheadPressure));

            if (oilRate <= 0)
                throw new ArgumentException("Oil rate must be greater than zero.", nameof(oilRate));

            // Calculate average pressure
            decimal averagePressure = (reservoirPressure + wellheadPressure) / 2m;

            // Calculate Z-factor
            decimal zFactor = ZFactorCalculator.CalculateBrillBeggs(
                averagePressure, temperature, gasSpecificGravity);

            // Initial GLR estimate
            decimal optimumGLR = 200m; // scf/STB (initial guess)
            decimal oldGLR = 0m;
            int iterations = 0;
            const int maxIterations = 50;
            const decimal tolerance = 0.1m;

            // Iterate to find optimum GLR that minimizes pressure drop
            while (Math.Abs(optimumGLR - oldGLR) > tolerance && iterations < maxIterations)
            {
                oldGLR = optimumGLR;

                // Calculate pressure drop for current GLR
                decimal pressureDrop = CalculatePressureDropForGLR(
                    reservoirPressure, wellheadPressure, tubingDiameter, tubingLength,
                    oilRate, oilGravity, gasSpecificGravity, temperature, optimumGLR, waterCut, zFactor);

                // Calculate pressure drop for slightly higher GLR
                decimal glrHigh = optimumGLR * 1.01m;
                decimal pressureDropHigh = CalculatePressureDropForGLR(
                    reservoirPressure, wellheadPressure, tubingDiameter, tubingLength,
                    oilRate, oilGravity, gasSpecificGravity, temperature, glrHigh, waterCut, zFactor);

                // Calculate pressure drop for slightly lower GLR
                decimal glrLow = optimumGLR * 0.99m;
                decimal pressureDropLow = CalculatePressureDropForGLR(
                    reservoirPressure, wellheadPressure, tubingDiameter, tubingLength,
                    oilRate, oilGravity, gasSpecificGravity, temperature, glrLow, waterCut, zFactor);

                // Find minimum pressure drop
                if (pressureDropHigh < pressureDrop && pressureDropHigh < pressureDropLow)
                {
                    optimumGLR = glrHigh;
                }
                else if (pressureDropLow < pressureDrop && pressureDropLow < pressureDropHigh)
                {
                    optimumGLR = glrLow;
                }
                else
                {
                    break; // Found optimum
                }

                // Clamp to reasonable range
                optimumGLR = Math.Max(50m, Math.Min(5000m, optimumGLR));

                iterations++;
            }

            return optimumGLR;
        }

        /// <summary>
        /// Calculates pressure drop for given GLR using Hagedorn-Brown correlation.
        /// </summary>
        private static decimal CalculatePressureDropForGLR(
            decimal reservoirPressure,
            decimal wellheadPressure,
            decimal tubingDiameter,
            decimal tubingLength,
            decimal oilRate,
            decimal oilGravity,
            decimal gasSpecificGravity,
            decimal temperature,
            decimal glr,
            decimal waterCut,
            decimal zFactor)
        {
            // Simplified pressure drop calculation using Hagedorn-Brown
            // This is a simplified version - in practice would use full Hagedorn-Brown correlation

            decimal averagePressure = (reservoirPressure + wellheadPressure) / 2m;
            decimal oilSpecificGravity = 141.5m / (oilGravity + 131.5m);

            // Calculate mixture properties
            decimal liquidRate = oilRate * (1m - waterCut) + oilRate * waterCut * 1.0m; // Simplified
            decimal gasRate = oilRate * glr / 1000m; // Mscf/day

            // Simplified pressure gradient calculation
            decimal liquidDensity = oilSpecificGravity * 62.4m;
            decimal gasDensity = (averagePressure * gasSpecificGravity * 28.9645m) /
                               (zFactor * 10.7316m * temperature);

            // Mixture density
            decimal mixtureDensity = (liquidDensity * liquidRate + gasDensity * gasRate) /
                                   (liquidRate + gasRate);

            // Friction factor (simplified)
            decimal reynoldsNumber = CalculateReynoldsNumber(
                tubingDiameter, liquidRate, gasRate, mixtureDensity, temperature);

            decimal frictionFactor = reynoldsNumber < 2000m ? 64m / reynoldsNumber : 0.02m;

            // Pressure gradient
            decimal velocity = CalculateFlowVelocity(tubingDiameter, liquidRate, gasRate, averagePressure, temperature, zFactor);
            decimal pressureGradient = frictionFactor * mixtureDensity * velocity * velocity / (2m * 32.174m * (tubingDiameter / 12m));

            // Total pressure drop
            decimal pressureDrop = pressureGradient * tubingLength;

            return pressureDrop;
        }

        /// <summary>
        /// Calculates Reynolds number for multiphase flow.
        /// </summary>
        private static decimal CalculateReynoldsNumber(
            decimal tubingDiameter,
            decimal liquidRate,
            decimal gasRate,
            decimal mixtureDensity,
            decimal temperature)
        {
            // Simplified Reynolds number calculation
            decimal diameterFt = tubingDiameter / 12m;
            decimal area = (decimal)Math.PI * diameterFt * diameterFt / 4m;
            decimal velocity = 5m; // Simplified estimate
            decimal viscosity = 1.0m * 0.000672m; // cp to lb/(ft-s)

            return mixtureDensity * velocity * diameterFt / viscosity;
        }

        /// <summary>
        /// Calculates flow velocity.
        /// </summary>
        private static decimal CalculateFlowVelocity(
            decimal tubingDiameter,
            decimal liquidRate,
            decimal gasRate,
            decimal pressure,
            decimal temperature,
            decimal zFactor)
        {
            decimal diameterFt = tubingDiameter / 12m;
            decimal area = (decimal)Math.PI * diameterFt * diameterFt / 4m;

            // Convert rates to ft³/s
            decimal liquidRateFt3PerSec = liquidRate * 5.615m / 86400m; // bbl/day to ft³/s
            decimal gasRateFt3PerSec = gasRate * 1000m * 379.0m / 86400m / 1000m; // Simplified

            decimal totalVelocity = (liquidRateFt3PerSec + gasRateFt3PerSec) / area;

            return Math.Max(0.1m, totalVelocity);
        }

        /// <summary>
        /// Calculates optimum GLR for gas lift optimization.
        /// </summary>
        /// <param name="reservoirPressure">Reservoir pressure in psia.</param>
        /// <param name="wellheadPressure">Wellhead pressure in psia.</param>
        /// <param name="oilRate">Desired oil production rate in bbl/day.</param>
        /// <param name="oilGravity">Oil API gravity.</param>
        /// <param name="gasSpecificGravity">Gas specific gravity (relative to air).</param>
        /// <param name="temperature">Average temperature in Rankine.</param>
        /// <returns>Optimum GLR for gas lift in scf/STB.</returns>
        public static decimal CalculateOptimumGLRForGasLift(
            decimal reservoirPressure,
            decimal wellheadPressure,
            decimal oilRate,
            decimal oilGravity,
            decimal gasSpecificGravity,
            decimal temperature)
        {
            // For gas lift, optimum GLR is typically higher
            // This is a simplified calculation - in practice would use gas lift performance curves

            decimal averagePressure = (reservoirPressure + wellheadPressure) / 2m;
            decimal zFactor = ZFactorCalculator.CalculateBrillBeggs(
                averagePressure, temperature, gasSpecificGravity);

            // Simplified optimum GLR calculation for gas lift
            // Typically ranges from 500 to 2000 scf/STB depending on conditions
            decimal pressureRatio = wellheadPressure / reservoirPressure;
            decimal optimumGLR = 1000m * (1m - pressureRatio) + 500m;

            return Math.Max(500m, Math.Min(2000m, optimumGLR));
        }

        /// <summary>
        /// Calculates production rate for given GLR.
        /// </summary>
        /// <param name="reservoirPressure">Reservoir pressure in psia.</param>
        /// <param name="wellheadPressure">Wellhead pressure in psia.</param>
        /// <param name="glr">Gas-liquid ratio in scf/STB.</param>
        /// <param name="tubingDiameter">Tubing diameter in inches.</param>
        /// <param name="tubingLength">Tubing length in feet.</param>
        /// <param name="oilGravity">Oil API gravity.</param>
        /// <param name="gasSpecificGravity">Gas specific gravity (relative to air).</param>
        /// <param name="temperature">Average temperature in Rankine.</param>
        /// <returns>Estimated production rate in bbl/day.</returns>
        public static decimal CalculateProductionRateForGLR(
            decimal reservoirPressure,
            decimal wellheadPressure,
            decimal glr,
            decimal tubingDiameter,
            decimal tubingLength,
            decimal oilGravity,
            decimal gasSpecificGravity,
            decimal temperature)
        {
            // Simplified production rate calculation
            // In practice would use full nodal analysis

            decimal averagePressure = (reservoirPressure + wellheadPressure) / 2m;
            decimal zFactor = ZFactorCalculator.CalculateBrillBeggs(
                averagePressure, temperature, gasSpecificGravity);

            // Calculate pressure drop
            decimal pressureDrop = reservoirPressure - wellheadPressure;

            // Simplified rate calculation based on pressure drop and GLR
            decimal oilRate = pressureDrop * 10m / (1m + glr / 1000m); // Simplified correlation

            return Math.Max(0m, oilRate);
        }
    }
}


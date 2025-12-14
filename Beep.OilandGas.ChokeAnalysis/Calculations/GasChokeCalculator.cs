using System;
using Beep.OilandGas.ChokeAnalysis.Models;
using Beep.OilandGas.GasProperties.Calculations;

namespace Beep.OilandGas.ChokeAnalysis.Calculations
{
    /// <summary>
    /// Provides gas choke flow calculation methods.
    /// </summary>
    public static class GasChokeCalculator
    {
        /// <summary>
        /// Calculates gas flow rate through a downhole choke.
        /// </summary>
        /// <param name="choke">Choke properties.</param>
        /// <param name="gasProperties">Gas properties.</param>
        /// <returns>Choke flow result.</returns>
        public static ChokeFlowResult CalculateDownholeChokeFlow(
            ChokeProperties choke,
            GasChokeProperties gasProperties)
        {
            if (choke == null)
                throw new ArgumentNullException(nameof(choke));

            if (gasProperties == null)
                throw new ArgumentNullException(nameof(gasProperties));

            // Calculate Z-factor if not provided
            decimal zFactor = gasProperties.ZFactor;
            if (zFactor <= 0)
            {
                zFactor = ZFactorCalculator.CalculateBrillBeggs(
                    gasProperties.UpstreamPressure,
                    gasProperties.Temperature,
                    gasProperties.GasSpecificGravity);
            }

            // Calculate critical pressure ratio
            decimal criticalPressureRatio = CalculateCriticalPressureRatio(gasProperties.GasSpecificGravity);
            decimal pressureRatio = gasProperties.DownstreamPressure / gasProperties.UpstreamPressure;

            FlowRegime flowRegime = pressureRatio < criticalPressureRatio
                ? FlowRegime.Sonic
                : FlowRegime.Subsonic;

            decimal flowRate;

            if (flowRegime == FlowRegime.Sonic)
            {
                // Sonic (critical) flow
                flowRate = CalculateSonicFlowRate(choke, gasProperties, zFactor);
            }
            else
            {
                // Subsonic flow
                flowRate = CalculateSubsonicFlowRate(choke, gasProperties, zFactor, pressureRatio);
            }

            return new ChokeFlowResult
            {
                FlowRate = flowRate,
                DownstreamPressure = gasProperties.DownstreamPressure,
                UpstreamPressure = gasProperties.UpstreamPressure,
                PressureRatio = pressureRatio,
                FlowRegime = flowRegime,
                CriticalPressureRatio = criticalPressureRatio
            };
        }

        /// <summary>
        /// Calculates gas flow rate through an uphole choke.
        /// </summary>
        public static ChokeFlowResult CalculateUpholeChokeFlow(
            ChokeProperties choke,
            GasChokeProperties gasProperties)
        {
            if (choke == null)
                throw new ArgumentNullException(nameof(choke));

            if (gasProperties == null)
                throw new ArgumentNullException(nameof(gasProperties));

            // Uphole choke calculations are similar but may have different conditions
            // For now, use the same calculation as downhole
            return CalculateDownholeChokeFlow(choke, gasProperties);
        }

        /// <summary>
        /// Calculates downstream pressure for given flow rate.
        /// </summary>
        public static decimal CalculateDownstreamPressure(
            ChokeProperties choke,
            GasChokeProperties gasProperties,
            decimal flowRate)
        {
            if (choke == null)
                throw new ArgumentNullException(nameof(choke));

            if (gasProperties == null)
                throw new ArgumentNullException(nameof(gasProperties));

            // Calculate Z-factor
            decimal zFactor = gasProperties.ZFactor;
            if (zFactor <= 0)
            {
                zFactor = ZFactorCalculator.CalculateBrillBeggs(
                    gasProperties.UpstreamPressure,
                    gasProperties.Temperature,
                    gasProperties.GasSpecificGravity);
            }

            // Calculate critical pressure ratio
            decimal criticalPressureRatio = CalculateCriticalPressureRatio(gasProperties.GasSpecificGravity);

            // Try sonic flow first
            decimal sonicFlowRate = CalculateSonicFlowRate(choke, gasProperties, zFactor);

            if (flowRate >= sonicFlowRate)
            {
                // Sonic flow - downstream pressure is critical
                return gasProperties.UpstreamPressure * criticalPressureRatio;
            }
            else
            {
                // Subsonic flow - solve iteratively
                return SolveDownstreamPressure(choke, gasProperties, zFactor, flowRate);
            }
        }

        /// <summary>
        /// Calculates required choke size for given flow rate and pressure conditions.
        /// </summary>
        public static decimal CalculateRequiredChokeSize(
            GasChokeProperties gasProperties,
            decimal flowRate)
        {
            if (gasProperties == null)
                throw new ArgumentNullException(nameof(gasProperties));

            if (flowRate <= 0)
                throw new ArgumentException("Flow rate must be greater than zero.", nameof(flowRate));

            // Calculate Z-factor
            decimal zFactor = gasProperties.ZFactor;
            if (zFactor <= 0)
            {
                zFactor = ZFactorCalculator.CalculateBrillBeggs(
                    gasProperties.UpstreamPressure,
                    gasProperties.Temperature,
                    gasProperties.GasSpecificGravity);
            }

            // Calculate critical pressure ratio
            decimal criticalPressureRatio = CalculateCriticalPressureRatio(gasProperties.GasSpecificGravity);
            decimal pressureRatio = gasProperties.DownstreamPressure / gasProperties.UpstreamPressure;

            decimal chokeArea;

            if (pressureRatio < criticalPressureRatio)
            {
                // Sonic flow
                chokeArea = CalculateSonicChokeArea(gasProperties, zFactor, flowRate);
            }
            else
            {
                // Subsonic flow
                chokeArea = CalculateSubsonicChokeArea(gasProperties, zFactor, flowRate, pressureRatio);
            }

            // Convert area to diameter
            decimal chokeDiameter = (decimal)Math.Sqrt((double)(4m * chokeArea / (decimal)Math.PI));

            return Math.Max(0.01m, Math.Min(2.0m, chokeDiameter)); // Clamp to reasonable range
        }

        // Helper methods

        private static decimal CalculateCriticalPressureRatio(decimal gasSpecificGravity)
        {
            // Critical pressure ratio for isentropic flow
            // For ideal gas: P2/P1 = (2/(k+1))^(k/(k-1))
            // k (specific heat ratio) ≈ 1.3 for natural gas
            decimal k = 1.3m;
            decimal ratio = 2m / (k + 1m);
            return (decimal)Math.Pow((double)ratio, (double)(k / (k - 1m)));
        }

        private static decimal CalculateSonicFlowRate(
            ChokeProperties choke,
            GasChokeProperties gasProperties,
            decimal zFactor)
        {
            // Sonic flow rate equation
            // q = C * A * P1 * sqrt(k * g / (R * T * Z * (k+1))) * (2/(k+1))^((k+1)/(2*(k-1)))
            decimal k = 1.3m; // Specific heat ratio
            decimal g = 32.174m; // Gravitational acceleration (ft/s²)
            decimal R = 10.7316m; // Gas constant (psia·ft³/(lbmol·°R))
            decimal molecularWeight = gasProperties.GasSpecificGravity * 28.9645m;

            decimal area = choke.ChokeArea / 144m; // Convert to ft²
            decimal upstreamPressure = gasProperties.UpstreamPressure;
            decimal temperature = gasProperties.Temperature;

            decimal criticalRatio = 2m / (k + 1m);
            decimal exponent = (k + 1m) / (2m * (k - 1m));
            decimal criticalTerm = (decimal)Math.Pow((double)criticalRatio, (double)exponent);

            decimal sqrtTerm = (decimal)Math.Sqrt((double)(k * g / (R * temperature * zFactor * (k + 1m))));

            decimal flowRate = choke.DischargeCoefficient * area * upstreamPressure * sqrtTerm * criticalTerm;

            // Convert to Mscf/day
            return flowRate * 1000m / 5.614m; // Simplified conversion
        }

        private static decimal CalculateSubsonicFlowRate(
            ChokeProperties choke,
            GasChokeProperties gasProperties,
            decimal zFactor,
            decimal pressureRatio)
        {
            // Subsonic flow rate equation
            decimal k = 1.3m;
            decimal g = 32.174m;
            decimal R = 10.7316m;
            decimal molecularWeight = gasProperties.GasSpecificGravity * 28.9645m;

            decimal area = choke.ChokeArea / 144m;
            decimal upstreamPressure = gasProperties.UpstreamPressure;
            decimal temperature = gasProperties.Temperature;

            decimal term1 = (decimal)Math.Pow((double)pressureRatio, (double)(2m / k));
            decimal term2 = (decimal)Math.Pow((double)pressureRatio, (double)((k + 1m) / k));
            decimal sqrtTerm = (decimal)Math.Sqrt((double)(2m * k / ((k - 1m) * R * temperature * zFactor) * (term1 - term2)));

            decimal flowRate = choke.DischargeCoefficient * area * upstreamPressure * sqrtTerm;

            // Convert to Mscf/day
            return flowRate * 1000m / 5.614m; // Simplified conversion
        }

        private static decimal SolveDownstreamPressure(
            ChokeProperties choke,
            GasChokeProperties gasProperties,
            decimal zFactor,
            decimal targetFlowRate)
        {
            // Iterative solution for downstream pressure
            decimal minPressure = 0.1m;
            decimal maxPressure = gasProperties.UpstreamPressure;
            decimal downstreamPressure = (minPressure + maxPressure) / 2m;
            decimal oldPressure = 0m;
            int iterations = 0;
            const int maxIterations = 100;
            const decimal tolerance = 0.1m;

            while (Math.Abs(downstreamPressure - oldPressure) > tolerance && iterations < maxIterations)
            {
                oldPressure = downstreamPressure;

                var testProperties = new GasChokeProperties
                {
                    UpstreamPressure = gasProperties.UpstreamPressure,
                    DownstreamPressure = downstreamPressure,
                    Temperature = gasProperties.Temperature,
                    GasSpecificGravity = gasProperties.GasSpecificGravity,
                    ZFactor = zFactor
                };

                var result = CalculateDownholeChokeFlow(choke, testProperties);
                decimal calculatedFlowRate = result.FlowRate;

                if (Math.Abs(calculatedFlowRate - targetFlowRate) < 0.01m)
                    break;

                if (calculatedFlowRate > targetFlowRate)
                {
                    // Need higher downstream pressure (lower flow)
                    minPressure = downstreamPressure;
                }
                else
                {
                    // Need lower downstream pressure (higher flow)
                    maxPressure = downstreamPressure;
                }

                downstreamPressure = (minPressure + maxPressure) / 2m;
                iterations++;
            }

            return downstreamPressure;
        }

        private static decimal CalculateSonicChokeArea(
            GasChokeProperties gasProperties,
            decimal zFactor,
            decimal flowRate)
        {
            // Reverse of sonic flow rate equation
            decimal k = 1.3m;
            decimal g = 32.174m;
            decimal R = 10.7316m;

            decimal criticalRatio = 2m / (k + 1m);
            decimal exponent = (k + 1m) / (2m * (k - 1m));
            decimal criticalTerm = (decimal)Math.Pow((double)criticalRatio, (double)exponent);

            decimal sqrtTerm = (decimal)Math.Sqrt((double)(k * g / (R * gasProperties.Temperature * zFactor * (k + 1m))));

            decimal flowRateScf = flowRate * 5.614m / 1000m; // Convert from Mscf/day
            decimal area = flowRateScf / (0.85m * gasProperties.UpstreamPressure * sqrtTerm * criticalTerm);

            return area * 144m; // Convert to square inches
        }

        private static decimal CalculateSubsonicChokeArea(
            GasChokeProperties gasProperties,
            decimal zFactor,
            decimal flowRate,
            decimal pressureRatio)
        {
            // Reverse of subsonic flow rate equation
            decimal k = 1.3m;
            decimal g = 32.174m;
            decimal R = 10.7316m;

            decimal term1 = (decimal)Math.Pow((double)pressureRatio, (double)(2m / k));
            decimal term2 = (decimal)Math.Pow((double)pressureRatio, (double)((k + 1m) / k));
            decimal sqrtTerm = (decimal)Math.Sqrt((double)(2m * k / ((k - 1m) * R * gasProperties.Temperature * zFactor) * (term1 - term2)));

            decimal flowRateScf = flowRate * 5.614m / 1000m; // Convert from Mscf/day
            decimal area = flowRateScf / (0.85m * gasProperties.UpstreamPressure * sqrtTerm);

            return area * 144m; // Convert to square inches
        }
    }
}


using System;
using Beep.OilandGas.Models.Data.CompressorAnalysis;
using Beep.OilandGas.GasProperties.Calculations;

namespace Beep.OilandGas.CompressorAnalysis.Calculations
{
    /// <summary>
    /// Provides compressor pressure calculations.
    /// </summary>
    public static class CompressorPressureCalculator
    {
        /// <summary>
        /// Calculates required discharge pressure for given flow rate.
        /// </summary>
        /// <param name="operatingConditions">Operating conditions.</param>
        /// <param name="requiredFlowRate">Required flow rate in Mscf/day.</param>
        /// <param name="maxPower">Maximum available power in horsepower.</param>
        /// <param name="compressorEfficiency">Compressor efficiency (0-1).</param>
        /// <returns>Compressor pressure calculation results.</returns>
        public static COMPRESSOR_PRESSURE_RESULT CalculateRequiredPressure(
            COMPRESSOR_OPERATING_CONDITIONS operatingConditions,
            decimal requiredFlowRate,
            decimal maxPower = 1000m,
            decimal compressorEfficiency = 0.75m)
        {
            if (operatingConditions == null)
                throw new ArgumentNullException(nameof(operatingConditions));

            var result = new COMPRESSOR_PRESSURE_RESULT();

            // Calculate average temperature
            decimal averageTemperature = (operatingConditions.SUCTION_TEMPERATURE + 
                                        operatingConditions.DISCHARGE_TEMPERATURE) / 2m;

            // Calculate average pressure (initial estimate)
            decimal averagePressure = operatingConditions.SUCTION_PRESSURE * 1.5m;

            // Calculate Z-factor
            decimal zFactor = ZFactorCalculator.CalculateBrillBeggs(
                averagePressure, averageTemperature, operatingConditions.GAS_SPECIFIC_GRAVITY);

            // Calculate adiabatic head for different compression ratios
            decimal k = 1.3m; // Typical for gas
            decimal molecularWeight = operatingConditions.GAS_SPECIFIC_GRAVITY * 28.9645m;
            decimal R = 1545.0m;

            // Iterate to find maximum compression ratio within power limit
            decimal maxCompressionRatio = 10m; // Maximum reasonable ratio
            decimal minCompressionRatio = 1.0m;
            decimal optimalCompressionRatio = 1.0m;
            decimal optimalPower = 0m;

            for (decimal compressionRatio = minCompressionRatio; compressionRatio <= maxCompressionRatio; compressionRatio += 0.1m)
            {
                // Calculate adiabatic head
                decimal adiabaticHead = (zFactor * R * operatingConditions.SUCTION_TEMPERATURE / molecularWeight) *
                                       (k / (k - 1m)) *
                                       ((decimal)Math.Pow((double)compressionRatio, (double)((k - 1m) / k)) - 1m);

                // Calculate weight flow rate
                decimal flowRateScfMin = requiredFlowRate * 1000m / 1440m; // scf/min
                decimal weightFlowRate = flowRateScfMin * molecularWeight / 379.0m; // lbm/min

                // Calculate required power
                decimal requiredPower = (weightFlowRate * adiabaticHead) / (33000m * compressorEfficiency);

                if (requiredPower <= maxPower && requiredPower > optimalPower)
                {
                    optimalPower = requiredPower;
                    optimalCompressionRatio = compressionRatio;
                }
            }

            // Calculate discharge pressure
            result.REQUIRED_DISCHARGE_PRESSURE = operatingConditions.SUCTION_PRESSURE * optimalCompressionRatio;
            result.COMPRESSION_RATIO = optimalCompressionRatio;
            result.REQUIRED_POWER = optimalPower;

            // Calculate discharge temperature
            result.DISCHARGE_TEMPERATURE = operatingConditions.SUCTION_TEMPERATURE *
                                        (decimal)Math.Pow((double)optimalCompressionRatio, (double)((k - 1m) / k));

            // Check feasibility
            result.IS_FEASIBLE = optimalPower > 0m && optimalPower <= maxPower;

            return result;
        }

        /// <summary>
        /// Calculates maximum flow rate for given pressure ratio.
        /// </summary>
        public static decimal CalculateMaximumFlowRate(
            COMPRESSOR_OPERATING_CONDITIONS operatingConditions,
            decimal compressionRatio,
            decimal maxPower,
            decimal compressorEfficiency = 0.75m)
        {
            if (operatingConditions == null)
                throw new ArgumentNullException(nameof(operatingConditions));

            // Calculate adiabatic head
            decimal k = 1.3m;
            decimal molecularWeight = operatingConditions.GAS_SPECIFIC_GRAVITY * 28.9645m;
            decimal R = 1545.0m;
            decimal averagePressure = operatingConditions.SUCTION_PRESSURE * (1m + compressionRatio) / 2m;
            decimal averageTemperature = (operatingConditions.SUCTION_TEMPERATURE + 
                                        operatingConditions.DISCHARGE_TEMPERATURE) / 2m;

            decimal zFactor = ZFactorCalculator.CalculateBrillBeggs(
                averagePressure, averageTemperature, operatingConditions.GAS_SPECIFIC_GRAVITY);

            decimal adiabaticHead = (zFactor * R * operatingConditions.SUCTION_TEMPERATURE / molecularWeight) *
                                   (k / (k - 1m)) *
                                   ((decimal)Math.Pow((double)compressionRatio, (double)((k - 1m) / k)) - 1m);

            // Calculate maximum weight flow rate
            decimal maxWeightFlowRate = (maxPower * 33000m * compressorEfficiency) / adiabaticHead; // lbm/min

            // Convert to Mscf/day
            decimal maxFlowRateScfMin = maxWeightFlowRate * 379.0m / molecularWeight; // scf/min
            decimal maxFlowRateMscfDay = maxFlowRateScfMin * 1440m / 1000m; // Mscf/day

            return Math.Max(0m, maxFlowRateMscfDay);
        }
    }
}


using System;
using Beep.OilandGas.Models.Data.CompressorAnalysis;
using Beep.OilandGas.GasProperties.Calculations;

namespace Beep.OilandGas.CompressorAnalysis.Calculations
{
    /// <summary>
    /// Provides centrifugal compressor power calculations.
    /// </summary>
    public static class CentrifugalCompressorCalculator
    {
        /// <summary>
        /// Calculates centrifugal compressor power requirements.
        /// </summary>
        /// <param name="compressorProperties">Centrifugal compressor properties.</param>
        /// <param name="useSIUnits">Whether to use SI units (false = US field units).</param>
        /// <returns>Compressor power calculation results.</returns>
        public static COMPRESSOR_POWER_RESULT CalculatePower(
            CENTRIFUGAL_COMPRESSOR_PROPERTIES compressorProperties,
            bool useSIUnits = false)
        {
            if (compressorProperties == null)
                throw new ArgumentNullException(nameof(compressorProperties));

            if (compressorProperties.OPERATING_CONDITIONS == null)
                throw new ArgumentNullException(nameof(compressorProperties.OPERATING_CONDITIONS));

            var result = new COMPRESSOR_POWER_RESULT();

            var conditions = compressorProperties.OPERATING_CONDITIONS;

            // Calculate compression ratio
            result.COMPRESSION_RATIO = conditions.DISCHARGE_PRESSURE / conditions.SUCTION_PRESSURE;

            // Calculate average pressure and temperature
            decimal averagePressure = (conditions.SUCTION_PRESSURE + conditions.DISCHARGE_PRESSURE) / 2m;
            decimal averageTemperature = (conditions.SUCTION_TEMPERATURE + conditions.DISCHARGE_TEMPERATURE) / 2m;

            // Calculate Z-factor at average conditions
            decimal zFactor = ZFactorCalculator.CalculateBrillBeggs(
                averagePressure, averageTemperature, conditions.GAS_SPECIFIC_GRAVITY);

            // Calculate polytropic head
            result.POLYTROPIC_HEAD = CalculatePolytropicHead(
                conditions.SUCTION_PRESSURE,
                conditions.DISCHARGE_PRESSURE,
                conditions.SUCTION_TEMPERATURE,
                compressorProperties.SPECIFIC_HEAT_RATIO,
                compressorProperties.POLYTROPIC_EFFICIENCY,
                zFactor,
                conditions.GAS_SPECIFIC_GRAVITY);

            // Calculate adiabatic head
            result.ADIABATIC_HEAD = CalculateAdiabaticHead(
                conditions.SUCTION_PRESSURE,
                conditions.DISCHARGE_PRESSURE,
                conditions.SUCTION_TEMPERATURE,
                compressorProperties.SPECIFIC_HEAT_RATIO,
                zFactor,
                conditions.GAS_SPECIFIC_GRAVITY);

            // Calculate theoretical power
            result.THEORETICAL_POWER = CalculateTheoreticalPower(
                conditions.GAS_FLOW_RATE,
                result.POLYTROPIC_HEAD,
                conditions.GAS_SPECIFIC_GRAVITY,
                averageTemperature,
                zFactor,
                averagePressure);

            // Calculate brake horsepower
            result.BRAKE_HORSEPOWER = result.THEORETICAL_POWER / compressorProperties.POLYTROPIC_EFFICIENCY;

            // Calculate motor horsepower
            result.MOTOR_HORSEPOWER = result.BRAKE_HORSEPOWER / conditions.MECHANICAL_EFFICIENCY;

            // Calculate power consumption
            if (useSIUnits)
            {
                result.POWER_CONSUMPTION_KW = result.MOTOR_HORSEPOWER * 0.746m;
            }
            else
            {
                result.POWER_CONSUMPTION_KW = result.MOTOR_HORSEPOWER * 0.746m; // Still in kW for consistency
            }

            // Calculate discharge temperature
            result.DISCHARGE_TEMPERATURE = CalculateDischargeTemperature(
                conditions.SUCTION_TEMPERATURE,
                result.COMPRESSION_RATIO,
                compressorProperties.SPECIFIC_HEAT_RATIO,
                compressorProperties.POLYTROPIC_EFFICIENCY);

            // Overall efficiency
            result.OVERALL_EFFICIENCY = compressorProperties.POLYTROPIC_EFFICIENCY * conditions.MECHANICAL_EFFICIENCY;

            return result;
        }

        /// <summary>
        /// Calculates polytropic head.
        /// </summary>
        private static decimal CalculatePolytropicHead(
            decimal SUCTION_PRESSURE,
            decimal DISCHARGE_PRESSURE,
            decimal SUCTION_TEMPERATURE,
            decimal SPECIFIC_HEAT_RATIO,
            decimal POLYTROPIC_EFFICIENCY,
            decimal zFactor,
            decimal GAS_SPECIFIC_GRAVITY)
        {
            // Polytropic head: Hp = (Z_avg * R * T1 / MW) * (n / (n-1)) * [(P2/P1)^((n-1)/n) - 1]
            // Where n = polytropic exponent = (k * Î·p) / (k - Î·p * (k - 1))

            decimal k = SPECIFIC_HEAT_RATIO;
            decimal etaP = POLYTROPIC_EFFICIENCY;

            // Polytropic exponent
            decimal n = (k * etaP) / (k - etaP * (k - 1m));

            // Compression ratio
            decimal compressionRatio = DISCHARGE_PRESSURE / SUCTION_PRESSURE;

            // Average Z-factor (simplified - using suction Z)
            decimal zAvg = zFactor;

            // Gas constant
            decimal R = 1545.0m; // ft-lbf/(lbmol-R)

            // Molecular weight
            decimal molecularWeight = GAS_SPECIFIC_GRAVITY * 28.9645m;

            // Polytropic head
            decimal head = (zAvg * R * SUCTION_TEMPERATURE / molecularWeight) *
                          (n / (n - 1m)) *
                          ((decimal)Math.Pow((double)compressionRatio, (double)((n - 1m) / n)) - 1m);

            return Math.Max(0m, head);
        }

        /// <summary>
        /// Calculates adiabatic head.
        /// </summary>
        private static decimal CalculateAdiabaticHead(
            decimal SUCTION_PRESSURE,
            decimal DISCHARGE_PRESSURE,
            decimal SUCTION_TEMPERATURE,
            decimal SPECIFIC_HEAT_RATIO,
            decimal zFactor,
            decimal GAS_SPECIFIC_GRAVITY)
        {
            // Adiabatic head: Ha = (Z_avg * R * T1 / MW) * (k / (k-1)) * [(P2/P1)^((k-1)/k) - 1]

            decimal k = SPECIFIC_HEAT_RATIO;
            decimal compressionRatio = DISCHARGE_PRESSURE / SUCTION_PRESSURE;
            decimal zAvg = zFactor;
            decimal R = 1545.0m;
            decimal molecularWeight = GAS_SPECIFIC_GRAVITY * 28.9645m;

            decimal head = (zAvg * R * SUCTION_TEMPERATURE / molecularWeight) *
                          (k / (k - 1m)) *
                          ((decimal)Math.Pow((double)compressionRatio, (double)((k - 1m) / k)) - 1m);

            return Math.Max(0m, head);
        }

        /// <summary>
        /// Calculates theoretical power.
        /// </summary>
        private static decimal CalculateTheoreticalPower(
            decimal GAS_FLOW_RATE,
            decimal polytropicHead,
            decimal GAS_SPECIFIC_GRAVITY,
            decimal averageTemperature,
            decimal zFactor,
            decimal averagePressure)
        {
            // Theoretical power: P = (W * Hp) / (33000 * Î·)
            // Where W = weight flow rate

            // Convert gas flow rate from Mscf/day to scf/min
            decimal flowRateScfMin = GAS_FLOW_RATE * 1000m / 1440m; // scf/min

            // Calculate weight flow rate
            decimal molecularWeight = GAS_SPECIFIC_GRAVITY * 28.9645m;
            decimal weightFlowRate = flowRateScfMin * molecularWeight / 379.0m; // lbm/min

            // Theoretical power
            decimal theoreticalPower = (weightFlowRate * polytropicHead) / 33000m;

            return Math.Max(0m, theoreticalPower);
        }

        /// <summary>
        /// Calculates discharge temperature.
        /// </summary>
        private static decimal CalculateDischargeTemperature(
            decimal SUCTION_TEMPERATURE,
            decimal compressionRatio,
            decimal SPECIFIC_HEAT_RATIO,
            decimal POLYTROPIC_EFFICIENCY)
        {
            // Discharge temperature: T2 = T1 * (P2/P1)^((n-1)/n)
            // Where n = polytropic exponent

            decimal k = SPECIFIC_HEAT_RATIO;
            decimal etaP = POLYTROPIC_EFFICIENCY;
            decimal n = (k * etaP) / (k - etaP * (k - 1m));

            decimal dischargeTemperature = SUCTION_TEMPERATURE *
                                         (decimal)Math.Pow((double)compressionRatio, (double)((n - 1m) / n));

            return Math.Max(SUCTION_TEMPERATURE, dischargeTemperature);
        }
    }
}


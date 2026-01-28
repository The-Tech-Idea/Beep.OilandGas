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
            result.COMPRESSION_RATIO = conditions.DischargePressure / conditions.SuctionPressure;

            // Calculate average pressure and temperature
            decimal averagePressure = (conditions.SuctionPressure + conditions.DischargePressure) / 2m;
            decimal averageTemperature = (conditions.SuctionTemperature + conditions.DISCHARGE_TEMPERATURE) / 2m;

            // Calculate Z-factor at average conditions
            decimal zFactor = ZFactorCalculator.CalculateBrillBeggs(
                averagePressure, averageTemperature, conditions.GasSpecificGravity);

            // Calculate polytropic head
            result.POLYTROPIC_HEAD = CalculatePolytropicHead(
                conditions.SuctionPressure,
                conditions.DischargePressure,
                conditions.SuctionTemperature,
                compressorProperties.SPECIFIC_HEAT_RATIO,
                compressorProperties.POLYTROPIC_EFFICIENCY,
                zFactor,
                conditions.GasSpecificGravity);

            // Calculate adiabatic head
            result.ADIABATIC_HEAD = CalculateAdiabaticHead(
                conditions.SuctionPressure,
                conditions.DischargePressure,
                conditions.SuctionTemperature,
                compressorProperties.SPECIFIC_HEAT_RATIO,
                zFactor,
                conditions.GasSpecificGravity);

            // Calculate theoretical power
            result.THEORETICAL_POWER = CalculateTheoreticalPower(
                conditions.GasFlowRate,
                result.POLYTROPIC_HEAD,
                conditions.GasSpecificGravity,
                averageTemperature,
                zFactor,
                averagePressure);

            // Calculate brake horsepower
            result.BRAKE_HORSEPOWER = result.THEORETICAL_POWER / compressorProperties.POLYTROPIC_EFFICIENCY;

            // Calculate motor horsepower
            result.MOTOR_HORSEPOWER = result.BRAKE_HORSEPOWER / conditions.MechanicalEfficiency;

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
                conditions.SuctionTemperature,
                result.COMPRESSION_RATIO,
                compressorProperties.SPECIFIC_HEAT_RATIO,
                compressorProperties.POLYTROPIC_EFFICIENCY);

            // Overall efficiency
            result.OVERALL_EFFICIENCY = compressorProperties.POLYTROPIC_EFFICIENCY * conditions.MechanicalEfficiency;

            return result;
        }

        /// <summary>
        /// Calculates polytropic head.
        /// </summary>
        private static decimal CalculatePolytropicHead(
            decimal suctionPressure,
            decimal dischargePressure,
            decimal suctionTemperature,
            decimal specificHeatRatio,
            decimal polytropicEfficiency,
            decimal zFactor,
            decimal gasSpecificGravity)
        {
            // Polytropic head: Hp = (Z_avg * R * T1 / MW) * (n / (n-1)) * [(P2/P1)^((n-1)/n) - 1]
            // Where n = polytropic exponent = (k * Î·p) / (k - Î·p * (k - 1))

            decimal k = specificHeatRatio;
            decimal etaP = polytropicEfficiency;

            // Polytropic exponent
            decimal n = (k * etaP) / (k - etaP * (k - 1m));

            // Compression ratio
            decimal compressionRatio = dischargePressure / suctionPressure;

            // Average Z-factor (simplified - using suction Z)
            decimal zAvg = zFactor;

            // Gas constant
            decimal R = 1545.0m; // ft-lbf/(lbmol-R)

            // Molecular weight
            decimal molecularWeight = gasSpecificGravity * 28.9645m;

            // Polytropic head
            decimal head = (zAvg * R * suctionTemperature / molecularWeight) *
                          (n / (n - 1m)) *
                          ((decimal)Math.Pow((double)compressionRatio, (double)((n - 1m) / n)) - 1m);

            return Math.Max(0m, head);
        }

        /// <summary>
        /// Calculates adiabatic head.
        /// </summary>
        private static decimal CalculateAdiabaticHead(
            decimal suctionPressure,
            decimal dischargePressure,
            decimal suctionTemperature,
            decimal specificHeatRatio,
            decimal zFactor,
            decimal gasSpecificGravity)
        {
            // Adiabatic head: Ha = (Z_avg * R * T1 / MW) * (k / (k-1)) * [(P2/P1)^((k-1)/k) - 1]

            decimal k = specificHeatRatio;
            decimal compressionRatio = dischargePressure / suctionPressure;
            decimal zAvg = zFactor;
            decimal R = 1545.0m;
            decimal molecularWeight = gasSpecificGravity * 28.9645m;

            decimal head = (zAvg * R * suctionTemperature / molecularWeight) *
                          (k / (k - 1m)) *
                          ((decimal)Math.Pow((double)compressionRatio, (double)((k - 1m) / k)) - 1m);

            return Math.Max(0m, head);
        }

        /// <summary>
        /// Calculates theoretical power.
        /// </summary>
        private static decimal CalculateTheoreticalPower(
            decimal gasFlowRate,
            decimal polytropicHead,
            decimal gasSpecificGravity,
            decimal averageTemperature,
            decimal zFactor,
            decimal averagePressure)
        {
            // Theoretical power: P = (W * Hp) / (33000 * Î·)
            // Where W = weight flow rate

            // Convert gas flow rate from Mscf/day to scf/min
            decimal flowRateScfMin = gasFlowRate * 1000m / 1440m; // scf/min

            // Calculate weight flow rate
            decimal molecularWeight = gasSpecificGravity * 28.9645m;
            decimal weightFlowRate = flowRateScfMin * molecularWeight / 379.0m; // lbm/min

            // Theoretical power
            decimal theoreticalPower = (weightFlowRate * polytropicHead) / 33000m;

            return Math.Max(0m, theoreticalPower);
        }

        /// <summary>
        /// Calculates discharge temperature.
        /// </summary>
        private static decimal CalculateDischargeTemperature(
            decimal suctionTemperature,
            decimal compressionRatio,
            decimal specificHeatRatio,
            decimal polytropicEfficiency)
        {
            // Discharge temperature: T2 = T1 * (P2/P1)^((n-1)/n)
            // Where n = polytropic exponent

            decimal k = specificHeatRatio;
            decimal etaP = polytropicEfficiency;
            decimal n = (k * etaP) / (k - etaP * (k - 1m));

            decimal dischargeTemperature = suctionTemperature *
                                         (decimal)Math.Pow((double)compressionRatio, (double)((n - 1m) / n));

            return Math.Max(suctionTemperature, dischargeTemperature);
        }
    }
}


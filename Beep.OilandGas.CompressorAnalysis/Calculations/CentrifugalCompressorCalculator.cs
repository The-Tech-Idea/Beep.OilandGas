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
        public static CompressorPowerResult CalculatePower(
            CentrifugalCompressorProperties compressorProperties,
            bool useSIUnits = false)
        {
            if (compressorProperties == null)
                throw new ArgumentNullException(nameof(compressorProperties));

            if (compressorProperties.OperatingConditions == null)
                throw new ArgumentNullException(nameof(compressorProperties.OperatingConditions));

            var result = new CompressorPowerResult();

            var conditions = compressorProperties.OperatingConditions;

            // Calculate compression ratio
            result.CompressionRatio = conditions.DischargePressure / conditions.SuctionPressure;

            // Calculate average pressure and temperature
            decimal averagePressure = (conditions.SuctionPressure + conditions.DischargePressure) / 2m;
            decimal averageTemperature = (conditions.SuctionTemperature + conditions.DischargeTemperature) / 2m;

            // Calculate Z-factor at average conditions
            decimal zFactor = ZFactorCalculator.CalculateBrillBeggs(
                averagePressure, averageTemperature, conditions.GasSpecificGravity);

            // Calculate polytropic head
            result.PolytropicHead = CalculatePolytropicHead(
                conditions.SuctionPressure,
                conditions.DischargePressure,
                conditions.SuctionTemperature,
                compressorProperties.SpecificHeatRatio,
                compressorProperties.PolytropicEfficiency,
                zFactor,
                conditions.GasSpecificGravity);

            // Calculate adiabatic head
            result.AdiabaticHead = CalculateAdiabaticHead(
                conditions.SuctionPressure,
                conditions.DischargePressure,
                conditions.SuctionTemperature,
                compressorProperties.SpecificHeatRatio,
                zFactor,
                conditions.GasSpecificGravity);

            // Calculate theoretical power
            result.TheoreticalPower = CalculateTheoreticalPower(
                conditions.GasFlowRate,
                result.PolytropicHead,
                conditions.GasSpecificGravity,
                averageTemperature,
                zFactor,
                averagePressure);

            // Calculate brake horsepower
            result.BrakeHorsepower = result.TheoreticalPower / compressorProperties.PolytropicEfficiency;

            // Calculate motor horsepower
            result.MotorHorsepower = result.BrakeHorsepower / conditions.MechanicalEfficiency;

            // Calculate power consumption
            if (useSIUnits)
            {
                result.PowerConsumptionKW = result.MotorHorsepower * 0.746m;
            }
            else
            {
                result.PowerConsumptionKW = result.MotorHorsepower * 0.746m; // Still in kW for consistency
            }

            // Calculate discharge temperature
            result.DischargeTemperature = CalculateDischargeTemperature(
                conditions.SuctionTemperature,
                result.CompressionRatio,
                compressorProperties.SpecificHeatRatio,
                compressorProperties.PolytropicEfficiency);

            // Overall efficiency
            result.OverallEfficiency = compressorProperties.PolytropicEfficiency * conditions.MechanicalEfficiency;

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
            // Where n = polytropic exponent = (k * ηp) / (k - ηp * (k - 1))

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
            // Theoretical power: P = (W * Hp) / (33000 * η)
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


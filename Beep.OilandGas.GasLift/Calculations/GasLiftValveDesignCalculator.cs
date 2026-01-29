using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data.GasLift;

using Beep.OilandGas.GasProperties.Calculations;

namespace Beep.OilandGas.GasLift.Calculations
{
    /// <summary>
    /// Provides gas lift valve design calculations.
    /// </summary>
    public static class GasLiftValveDesignCalculator
    {
        /// <summary>
        /// Designs gas lift valves for a well (US field units).
        /// </summary>
        /// <param name="wellProperties">Well properties.</param>
        /// <param name="gasInjectionPressure">Gas injection pressure in psia.</param>
        /// <param name="numberOfValves">Number of valves to design.</param>
        /// <returns>Gas lift valve design result.</returns>
        public static GAS_LIFT_VALVE_DESIGN_RESULT DesignValvesUS(
            GAS_LIFT_WELL_PROPERTIES wellProperties,
            decimal gasInjectionPressure,
            int numberOfValves = 5)
        {
            return DesignValves(wellProperties, gasInjectionPressure, numberOfValves, useSIUnits: false);
        }

        /// <summary>
        /// Designs gas lift valves for a well (SI units).
        /// </summary>
        public static GAS_LIFT_VALVE_DESIGN_RESULT DesignValvesSI(
            GAS_LIFT_WELL_PROPERTIES wellProperties,
            decimal gasInjectionPressure,
            int numberOfValves = 5)
        {
            return DesignValves(wellProperties, gasInjectionPressure, numberOfValves, useSIUnits: true);
        }

        /// <summary>
        /// Designs gas lift valves for a well.
        /// </summary>
        private static GAS_LIFT_VALVE_DESIGN_RESULT DesignValves(
            GAS_LIFT_WELL_PROPERTIES wellProperties,
            decimal gasInjectionPressure,
            int numberOfValves,
            bool useSIUnits)
        {
            if (wellProperties == null)
                throw new ArgumentNullException(nameof(wellProperties));

            if (gasInjectionPressure <= wellProperties.WELLHEAD_PRESSURE)
                throw new ArgumentException("Gas injection pressure must be greater than wellhead pressure.", nameof(gasInjectionPressure));

            var result = new GAS_LIFT_VALVE_DESIGN_RESULT();

            // Calculate valve spacing
            var spacingResult = GasLiftValveSpacingCalculator.CalculateValveSpacing(
                wellProperties, gasInjectionPressure, numberOfValves);

            // Design each valve
            for (int i = 0; i < spacingResult.NumberOfValves; i++)
            {
                decimal valveDepth = spacingResult.ValveDepths[i];
                decimal openingPressure = spacingResult.OpeningPressures[i];

                // Calculate temperature at valve depth
                decimal temperatureGradient = (wellProperties.BOTTOM_HOLE_TEMPERATURE - wellProperties.WELLHEAD_TEMPERATURE) /
                                             wellProperties.WELL_DEPTH;
                decimal valveTemperature = wellProperties.WELLHEAD_TEMPERATURE + temperatureGradient * valveDepth;

                // Calculate Z-factor at valve conditions
                decimal zFactor = ZFactorCalculator.CalculateBrillBeggs(
                    openingPressure, valveTemperature, wellProperties.GAS_SPECIFIC_GRAVITY);

                // Design valve port size
                decimal portSize = CalculateValvePortSize(
                    wellProperties, valveDepth, openingPressure, gasInjectionPressure, zFactor, valveTemperature);

                // Calculate gas injection rate through valve
                decimal gasInjectionRate = CalculateValveGasInjectionRate(
                    portSize, openingPressure, gasInjectionPressure, zFactor, valveTemperature, wellProperties.GAS_SPECIFIC_GRAVITY);

                var valve = new GAS_LIFT_VALVE
                {
                    DEPTH = valveDepth,
                    PORT_SIZE = portSize,
                    OPENING_PRESSURE = openingPressure,
                    CLOSING_PRESSURE = openingPressure * 0.9m, // 10% below opening
                    VALVE_TYPE = GasLiftValveType.InjectionPressureOperated,
                    TEMPERATURE = valveTemperature,
                    GAS_INJECTION_RATE = gasInjectionRate
                };

                result.Valves.Add(valve);
            }

            // Calculate total gas injection rate
            result.TOTAL_GAS_INJECTION_RATE = result.Valves.Sum(v => v.GAS_INJECTION_RATE);

            // Estimate production rate
            result.EXPECTED_PRODUCTION_RATE = EstimateProductionRate(wellProperties, result.TOTAL_GAS_INJECTION_RATE);

            // Calculate system efficiency
            result.SYSTEM_EFFICIENCY = CalculateSystemEfficiency(wellProperties, result);

            return result;
        }

        /// <summary>
        /// Calculates valve port size.
        /// </summary>
        private static decimal CalculateValvePortSize(
            GAS_LIFT_WELL_PROPERTIES wellProperties,
            decimal valveDepth,
            decimal openingPressure,
            decimal gasInjectionPressure,
            decimal zFactor,
            decimal temperature)
        {
            // Port size calculation based on gas flow requirements
            // Simplified: port size based on depth and pressure differential

            decimal pressureDifferential = gasInjectionPressure - openingPressure;
            if (pressureDifferential <= 0)
                pressureDifferential = 50m; // Minimum

            // Typical port sizes: 1/4", 3/8", 1/2", 5/8", 3/4", 1"
            decimal[] standardPortSizes = { 0.25m, 0.375m, 0.5m, 0.625m, 0.75m, 1.0m };

            // Select port size based on depth and pressure
            decimal requiredArea = CalculateRequiredPortArea(
                wellProperties, valveDepth, pressureDifferential, zFactor, temperature);

            decimal requiredDiameter = (decimal)Math.Sqrt((double)(4m * requiredArea / (decimal)Math.PI));

            // Select nearest standard size
            decimal portSize = standardPortSizes
                .OrderBy(s => Math.Abs(s - requiredDiameter))
                .First();

            return Math.Max(0.25m, Math.Min(1.0m, portSize));
        }

        /// <summary>
        /// Calculates required port area.
        /// </summary>
        private static decimal CalculateRequiredPortArea(
            GAS_LIFT_WELL_PROPERTIES wellProperties,
            decimal valveDepth,
            decimal pressureDifferential,
            decimal zFactor,
            decimal temperature)
        {
            // Simplified port area calculation
            // Based on gas flow equation through orifice

            decimal gasDensity = (pressureDifferential * wellProperties.GAS_SPECIFIC_GRAVITY * 28.9645m) /
                                (zFactor * 10.7316m * temperature);

            // Required flow area (simplified)
            decimal flowRate = 500m; // Mscf/day (typical per valve)
            decimal area = flowRate / (pressureDifferential * gasDensity * 100m); // Simplified

            return Math.Max(0.05m, Math.Min(1.0m, area)); // Clamp to reasonable range
        }

        /// <summary>
        /// Calculates gas injection rate through valve.
        /// </summary>
        private static decimal CalculateValveGasInjectionRate(
            decimal portSize,
            decimal openingPressure,
            decimal gasInjectionPressure,
            decimal zFactor,
            decimal temperature,
            decimal gasSpecificGravity)
        {
            // Orifice flow equation for gas
            decimal pressureDifferential = gasInjectionPressure - openingPressure;
            if (pressureDifferential <= 0)
                return 0m;

            decimal portArea = (decimal)Math.PI * portSize * portSize / 4m; // square inches
            decimal portAreaFt2 = portArea / 144m; // square feet

            // Gas flow rate through orifice
            decimal k = 1.3m; // Specific heat ratio
            decimal criticalRatio = 2m / (k + 1m);
            decimal pressureRatio = openingPressure / gasInjectionPressure;

            decimal flowRate;

            if (pressureRatio < criticalRatio)
            {
                // Sonic flow
                decimal criticalTerm = (decimal)Math.Pow((double)criticalRatio, (double)(k / (k - 1m)));
                flowRate = 0.85m * portAreaFt2 * gasInjectionPressure *
                          (decimal)Math.Sqrt((double)(k * 32.174m / (10.7316m * temperature * zFactor * (k + 1m))));
            }
            else
            {
                // Subsonic flow
                decimal term1 = (decimal)Math.Pow((double)pressureRatio, (double)(2m / k));
                decimal term2 = (decimal)Math.Pow((double)pressureRatio, (double)((k + 1m) / k));
                flowRate = 0.85m * portAreaFt2 * gasInjectionPressure *
                          (decimal)Math.Sqrt((double)(2m * k / ((k - 1m) * 10.7316m * temperature * zFactor) * (term1 - term2)));
            }

            // Convert to Mscf/day
            return flowRate * 1000m / 5.614m; // Simplified conversion
        }

        /// <summary>
        /// Estimates production rate with gas lift.
        /// </summary>
        private static decimal EstimateProductionRate(
            GAS_LIFT_WELL_PROPERTIES wellProperties,
            decimal totalGasInjectionRate)
        {
            // Use gas lift potential calculator
            var potentialResult = GasLiftPotentialCalculator.AnalyzeGasLiftPotential(
                wellProperties, totalGasInjectionRate * 0.8m, totalGasInjectionRate * 1.2m, 10);

            return potentialResult.MAXIMUM_PRODUCTION_RATE;
        }

        /// <summary>
        /// Calculates system efficiency.
        /// </summary>
        private static decimal CalculateSystemEfficiency(
            GAS_LIFT_WELL_PROPERTIES wellProperties,
            GAS_LIFT_VALVE_DESIGN_RESULT designResult)
        {
            // System efficiency = (Production benefit) / (Gas injection cost)
            decimal baseProduction = wellProperties.DESIRED_PRODUCTION_RATE * 0.3m;
            decimal productionIncrease = designResult.EXPECTED_PRODUCTION_RATE - baseProduction;

            if (designResult.TOTAL_GAS_INJECTION_RATE <= 0)
                return 0m;

            decimal efficiency = productionIncrease / (designResult.TOTAL_GAS_INJECTION_RATE / 10m); // Simplified

            return Math.Max(0m, Math.Min(1.0m, efficiency));
        }
    }
}


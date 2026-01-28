using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data.PumpPerformance;
using Beep.OilandGas.GasProperties.Calculations;

namespace Beep.OilandGas.PumpPerformance.Calculations
{
    /// <summary>
    /// Provides ESP (Electric Submersible Pump) design calculations.
    /// </summary>
    public static class ESPDesignCalculator
    {
        /// <summary>
        /// Designs an ESP system based on well and production requirements.
        /// </summary>
        /// <param name="designProperties">ESP design properties.</param>
        /// <param name="useSIUnits">Whether to use SI units (false = US field units).</param>
        /// <returns>ESP design result.</returns>
        public static ESP_DESIGN_RESULT DesignESP(
            ESP_DESIGN_PROPERTIES designProperties,
            bool useSIUnits = false)
        {
            if (designProperties == null)
                throw new ArgumentNullException(nameof(designProperties));

            // Calculate fluid properties
            decimal fluidDensity = CalculateFluidDensity(designProperties);
            decimal fluidViscosity = CalculateFluidViscosity(designProperties);

            // Calculate total dynamic head
            decimal totalDynamicHead = CalculateTotalDynamicHead(designProperties, fluidDensity);

            // Select pump stages
            int pumpStages = SelectPumpStages(designProperties, totalDynamicHead);

            // Calculate pump performance
            var performancePoints = GeneratePumpPerformanceCurve(
                designProperties, pumpStages, totalDynamicHead);

            // Find operating point
            var operatingPoint = FindOperatingPoint(performancePoints, designProperties.DESIRED_FLOW_RATE);

            // Calculate required horsepower
            decimal requiredHorsepower = CalculateRequiredHorsepower(
                operatingPoint.FLOW_RATE, operatingPoint.HEAD, fluidDensity, operatingPoint.EFFICIENCY);

            // Select motor
            var motor = SelectMotor(requiredHorsepower, designProperties);

            // Select cable
            var cable = SelectCable(motor, designProperties);

            // Calculate system efficiency
            decimal systemEfficiency = CalculateSystemEfficiency(
                operatingPoint.EFFICIENCY, motor.EFFICIENCY, cable);

            // Calculate power consumption
            decimal powerConsumption = CalculatePowerConsumption(
                requiredHorsepower, motor.EFFICIENCY, useSIUnits);

            return new ESP_DESIGN_RESULT
            {
                PumpStages = pumpStages,
                RequiredHorsepower = requiredHorsepower,
                MotorHorsepower = motor.HORSEPOWER,
                MotorVoltage = motor.VOLTAGE,
                MotorCurrent = CalculateMotorCurrent(motor, requiredHorsepower),
                CableSize = cable.CABLE_SIZE,
                CableLength = cable.CABLE_LENGTH,
                SystemEfficiency = systemEfficiency,
                PumpEfficiency = operatingPoint.EFFICIENCY,
                MotorEfficiency = motor.EFFICIENCY,
                PowerConsumption = powerConsumption,
                OperatingFlowRate = operatingPoint.FLOW_RATE,
                OperatingHead = operatingPoint.HEAD,
                PerformancePoints = performancePoints
            };
        }

        /// <summary>
        /// Calculates fluid density.
        /// </summary>
        private static decimal CalculateFluidDensity(ESP_DESIGN_PROPERTIES properties)
        {
            // Oil density
            decimal oilSpecificGravity = 141.5m / (131.5m + properties.OIL_GRAVITY);
            decimal oilDensity = oilSpecificGravity * 62.4m; // lb/ftÂ³

            // Water density
            decimal waterDensity = 62.4m; // lb/ftÂ³

            // Mixture density
            decimal mixtureDensity = oilDensity * (1.0m - properties.WATER_CUT) +
                                   waterDensity * properties.WATER_CUT;

            // Account for gas
            if (properties.GAS_OIL_RATIO > 0)
            {
                // Calculate Z-factor
                decimal averagePressure = (properties.WELLHEAD_PRESSURE + 
                                          properties.WELLHEAD_PRESSURE + 0.433m * properties.PUMP_SETTING_DEPTH) / 2m;
                decimal zFactor = ZFactorCalculator.CalculateBrillBeggs(
                    averagePressure, properties.BOTTOM_HOLE_TEMPERATURE, properties.GAS_SPECIFIC_GRAVITY);

                // Gas density
                decimal gasDensity = (averagePressure * properties.GAS_SPECIFIC_GRAVITY * 28.9645m) /
                                    (zFactor * 10.7316m * properties.BOTTOM_HOLE_TEMPERATURE);

                // Gas volume factor
                decimal gasVolumeFactor = properties.GAS_OIL_RATIO * zFactor * properties.BOTTOM_HOLE_TEMPERATURE /
                                         (averagePressure * 5.614m);

                // Adjust mixture density for gas
                mixtureDensity = (mixtureDensity + gasDensity * gasVolumeFactor) / (1.0m + gasVolumeFactor);
            }

            return mixtureDensity;
        }

        /// <summary>
        /// Calculates fluid viscosity.
        /// </summary>
        private static decimal CalculateFluidViscosity(ESP_DESIGN_PROPERTIES properties)
        {
            // Simplified viscosity calculation
            decimal oilViscosity = 1.5m; // cp (approximate)
            decimal waterViscosity = 1.0m; // cp

            return oilViscosity * (1.0m - properties.WATER_CUT) + waterViscosity * properties.WATER_CUT;
        }

        /// <summary>
        /// Calculates total dynamic head.
        /// </summary>
        private static decimal CalculateTotalDynamicHead(
            ESP_DESIGN_PROPERTIES properties,
            decimal fluidDensity)
        {
            // Static head (depth to pump)
            decimal staticHead = properties.PUMP_SETTING_DEPTH;

            // Friction head (simplified)
            decimal frictionHead = CalculateFrictionHead(properties, fluidDensity);

            // Wellhead pressure head
            decimal wellheadHead = properties.WELLHEAD_PRESSURE * 144m / fluidDensity;

            // Total dynamic head
            return staticHead + frictionHead + wellheadHead;
        }

        /// <summary>
        /// Calculates friction head.
        /// </summary>
        private static decimal CalculateFrictionHead(
            ESP_DESIGN_PROPERTIES properties,
            decimal fluidDensity)
        {
            // Simplified friction calculation
            decimal tubingArea = (decimal)Math.PI * (properties.TUBING_DIAMETER / 12m) *
                               (properties.TUBING_DIAMETER / 12m) / 4m;
            decimal velocity = (properties.DESIRED_FLOW_RATE * 5.615m) / (86400m * tubingArea); // ft/s

            decimal reynoldsNumber = fluidDensity * velocity * (properties.TUBING_DIAMETER / 12m) / 0.001m;
            decimal frictionFactor = reynoldsNumber < 2000m
                ? 64m / reynoldsNumber
                : 0.3164m / (decimal)Math.Pow((double)reynoldsNumber, 0.25);

            decimal frictionHead = frictionFactor * (properties.PUMP_SETTING_DEPTH / (properties.TUBING_DIAMETER / 12m)) *
                                  (velocity * velocity) / (2m * 32.174m);

            return frictionHead;
        }

        /// <summary>
        /// Selects pump stages based on head requirements.
        /// </summary>
        private static int SelectPumpStages(
            ESP_DESIGN_PROPERTIES properties,
            decimal totalDynamicHead)
        {
            // Typical ESP stage head: 20-40 feet per stage
            decimal headPerStage = 30m; // feet (typical)
            int stages = (int)Math.Ceiling((double)(totalDynamicHead / headPerStage));

            return Math.Max(1, Math.Min(500, stages)); // Clamp to reasonable range
        }

        /// <summary>
        /// Generates pump performance curve.
        /// </summary>
        private static List<ESP_PUMP_POINT> GeneratePumpPerformanceCurve(
            ESP_DESIGN_PROPERTIES properties,
            int pumpStages,
            decimal totalDynamicHead)
        {
            var points = new List<ESP_PUMP_POINT>();
            int numberOfPoints = 20;

            decimal minFlowRate = properties.DESIRED_FLOW_RATE * 0.5m;
            decimal maxFlowRate = properties.DESIRED_FLOW_RATE * 1.5m;
            decimal flowStep = (maxFlowRate - minFlowRate) / numberOfPoints;

            for (int i = 0; i <= numberOfPoints; i++)
            {
                decimal flowRate = minFlowRate + i * flowStep;
                decimal head = CalculatePumpHead(flowRate, properties.DESIRED_FLOW_RATE, totalDynamicHead, pumpStages);
                decimal efficiency = CalculatePumpEfficiency(flowRate, properties.DESIRED_FLOW_RATE);
                decimal horsepower = CalculatePumpHorsepower(flowRate, head, properties, efficiency);

                points.Add(new ESP_PUMP_POINT
                {
                    FlowRate = flowRate,
                    Head = head,
                    Efficiency = efficiency,
                    Horsepower = horsepower
                });
            }

            return points;
        }

        /// <summary>
        /// Calculates pump head at given flow rate.
        /// </summary>
        private static decimal CalculatePumpHead(
            decimal flowRate,
            decimal designFlowRate,
            decimal designHead,
            int stages)
        {
            // Pump head curve: H = H0 * (1 - (Q/Q0)Â²)
            decimal flowRatio = flowRate / designFlowRate;
            decimal headPerStage = designHead / stages;
            decimal head = headPerStage * stages * (1.0m - flowRatio * flowRatio);

            return Math.Max(0m, head);
        }

        /// <summary>
        /// Calculates pump efficiency at given flow rate.
        /// </summary>
        private static decimal CalculatePumpEfficiency(decimal flowRate, decimal designFlowRate)
        {
            // Efficiency curve: peak at design flow rate
            decimal flowRatio = flowRate / designFlowRate;
            decimal efficiency = 0.7m * (1.0m - (decimal)Math.Pow((double)(flowRatio - 1.0m), 2.0));

            return Math.Max(0.3m, Math.Min(0.9m, efficiency));
        }

        /// <summary>
        /// Calculates pump horsepower.
        /// </summary>
        private static decimal CalculatePumpHorsepower(
            decimal flowRate,
            decimal head,
            ESP_DESIGN_PROPERTIES properties,
            decimal efficiency)
        {
            decimal fluidDensity = CalculateFluidDensity(properties);
            decimal horsepower = (flowRate * 5.615m / 86400m) * head * fluidDensity / (550m * efficiency);

            return Math.Max(0m, horsepower);
        }

        /// <summary>
        /// Finds operating point on pump curve.
        /// </summary>
        private static ESP_PUMP_POINT FindOperatingPoint(
            List<ESP_PUMP_POINT> performancePoints,
            decimal desiredFlowRate)
        {
            // Find closest point to desired flow rate
            var closestPoint = performancePoints
                .OrderBy(p => Math.Abs(p.FLOW_RATE - desiredFlowRate))
                .First();

            return closestPoint;
        }

        /// <summary>
        /// Calculates required horsepower.
        /// </summary>
        private static decimal CalculateRequiredHorsepower(
            decimal flowRate,
            decimal head,
            decimal fluidDensity,
            decimal efficiency)
        {
            decimal horsepower = (flowRate * 5.615m / 86400m) * head * fluidDensity / (550m * efficiency);
            return Math.Max(0m, horsepower);
        }

        /// <summary>
        /// Selects motor based on horsepower requirements.
        /// </summary>
        private static ESP_MOTOR_PROPERTIES SelectMotor(
            decimal requiredHorsepower,
            ESP_DESIGN_PROPERTIES properties)
        {
            // Standard motor sizes: 10, 15, 20, 25, 30, 40, 50, 60, 75, 100, 125, 150, 200, 250, 300 HP
            decimal[] motorSizes = { 10m, 15m, 20m, 25m, 30m, 40m, 50m, 60m, 75m, 100m, 125m, 150m, 200m, 250m, 300m };

            decimal selectedHorsepower = motorSizes.FirstOrDefault(s => s >= requiredHorsepower * 1.2m);
            if (selectedHorsepower == 0m)
                selectedHorsepower = motorSizes.Last();

            // Standard voltages: 230, 460, 575, 1000, 2000, 2300, 3000, 4000, 5000 V
            decimal[] voltages = { 230m, 460m, 575m, 1000m, 2000m, 2300m, 3000m, 4000m, 5000m };
            decimal selectedVoltage = voltages.FirstOrDefault(v => v >= 1000m);
            if (selectedVoltage == 0m)
                selectedVoltage = voltages.Last();

            return new ESP_MOTOR_PROPERTIES
            {
                Horsepower = selectedHorsepower,
                Voltage = selectedVoltage,
                Efficiency = 0.9m,
                PowerFactor = 0.85m
            };
        }

        /// <summary>
        /// Selects cable based on motor and depth.
        /// </summary>
        private static ESP_CABLE_PROPERTIES SelectCable(
            ESP_MOTOR_PROPERTIES motor,
            ESP_DESIGN_PROPERTIES properties)
        {
            // Cable size selection based on motor current and depth
            decimal motorCurrent = CalculateMotorCurrent(motor, motor.HORSEPOWER);
            decimal cableLength = properties.PUMP_SETTING_DEPTH + 100m; // Extra for surface

            // Standard cable sizes: 1, 2, 4, 6 AWG
            // Resistance per 1000 feet (approximate): 1 AWG = 0.1, 2 AWG = 0.15, 4 AWG = 0.25, 6 AWG = 0.4 ohms
            int cableSize;
            decimal resistance;

            if (motorCurrent < 50m)
            {
                cableSize = 6;
                resistance = 0.4m;
            }
            else if (motorCurrent < 100m)
            {
                cableSize = 4;
                resistance = 0.25m;
            }
            else if (motorCurrent < 150m)
            {
                cableSize = 2;
                resistance = 0.15m;
            }
            else
            {
                cableSize = 1;
                resistance = 0.1m;
            }

            decimal voltageDrop = motorCurrent * (resistance * cableLength / 1000m);

            return new ESP_CABLE_PROPERTIES
            {
                CableSize = cableSize,
                CableLength = cableLength,
                ResistancePer1000Feet = resistance,
                VoltageDrop = voltageDrop
            };
        }

        /// <summary>
        /// Calculates motor current.
        /// </summary>
        private static decimal CalculateMotorCurrent(ESP_MOTOR_PROPERTIES motor, decimal horsepower)
        {
            // I = (HP * 746) / (V * PF * Efficiency)
            decimal powerWatts = horsepower * 746m;
            decimal current = powerWatts / (motor.VOLTAGE * motor.POWER_FACTOR * motor.EFFICIENCY);

            return Math.Max(0m, current);
        }

        /// <summary>
        /// Calculates system efficiency.
        /// </summary>
        private static decimal CalculateSystemEfficiency(
            decimal pumpEfficiency,
            decimal motorEfficiency,
            ESP_CABLE_PROPERTIES cable)
        {
            // Cable efficiency (voltage drop effect)
            decimal cableEfficiency = 1.0m - (cable.VOLTAGE_DROP / 1000m); // Simplified
            cableEfficiency = Math.Max(0.95m, Math.Min(1.0m, cableEfficiency));

            return pumpEfficiency * motorEfficiency * cableEfficiency;
        }

        /// <summary>
        /// Calculates power consumption.
        /// </summary>
        private static decimal CalculatePowerConsumption(
            decimal horsepower,
            decimal motorEfficiency,
            bool useSIUnits)
        {
            if (useSIUnits)
            {
                // Convert to kW
                return (horsepower * 0.746m) / motorEfficiency;
            }
            else
            {
                // Return in HP
                return horsepower / motorEfficiency;
            }
        }
    }
}


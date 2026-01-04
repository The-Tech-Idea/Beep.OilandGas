using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.PumpPerformance;
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
        public static ESPDesignResult DesignESP(
            ESPDesignProperties designProperties,
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
            var operatingPoint = FindOperatingPoint(performancePoints, designProperties.DesiredFlowRate);

            // Calculate required horsepower
            decimal requiredHorsepower = CalculateRequiredHorsepower(
                operatingPoint.FlowRate, operatingPoint.Head, fluidDensity, operatingPoint.Efficiency);

            // Select motor
            var motor = SelectMotor(requiredHorsepower, designProperties);

            // Select cable
            var cable = SelectCable(motor, designProperties);

            // Calculate system efficiency
            decimal systemEfficiency = CalculateSystemEfficiency(
                operatingPoint.Efficiency, motor.Efficiency, cable);

            // Calculate power consumption
            decimal powerConsumption = CalculatePowerConsumption(
                requiredHorsepower, motor.Efficiency, useSIUnits);

            return new ESPDesignResult
            {
                PumpStages = pumpStages,
                RequiredHorsepower = requiredHorsepower,
                MotorHorsepower = motor.Horsepower,
                MotorVoltage = motor.Voltage,
                MotorCurrent = CalculateMotorCurrent(motor, requiredHorsepower),
                CableSize = cable.CableSize,
                CableLength = cable.CableLength,
                SystemEfficiency = systemEfficiency,
                PumpEfficiency = operatingPoint.Efficiency,
                MotorEfficiency = motor.Efficiency,
                PowerConsumption = powerConsumption,
                OperatingFlowRate = operatingPoint.FlowRate,
                OperatingHead = operatingPoint.Head,
                PerformancePoints = performancePoints
            };
        }

        /// <summary>
        /// Calculates fluid density.
        /// </summary>
        private static decimal CalculateFluidDensity(ESPDesignProperties properties)
        {
            // Oil density
            decimal oilSpecificGravity = 141.5m / (131.5m + properties.OilGravity);
            decimal oilDensity = oilSpecificGravity * 62.4m; // lb/ft³

            // Water density
            decimal waterDensity = 62.4m; // lb/ft³

            // Mixture density
            decimal mixtureDensity = oilDensity * (1.0m - properties.WaterCut) +
                                   waterDensity * properties.WaterCut;

            // Account for gas
            if (properties.GasOilRatio > 0)
            {
                // Calculate Z-factor
                decimal averagePressure = (properties.WellheadPressure + 
                                          properties.WellheadPressure + 0.433m * properties.PumpSettingDepth) / 2m;
                decimal zFactor = ZFactorCalculator.CalculateBrillBeggs(
                    averagePressure, properties.BottomHoleTemperature, properties.GasSpecificGravity);

                // Gas density
                decimal gasDensity = (averagePressure * properties.GasSpecificGravity * 28.9645m) /
                                    (zFactor * 10.7316m * properties.BottomHoleTemperature);

                // Gas volume factor
                decimal gasVolumeFactor = properties.GasOilRatio * zFactor * properties.BottomHoleTemperature /
                                         (averagePressure * 5.614m);

                // Adjust mixture density for gas
                mixtureDensity = (mixtureDensity + gasDensity * gasVolumeFactor) / (1.0m + gasVolumeFactor);
            }

            return mixtureDensity;
        }

        /// <summary>
        /// Calculates fluid viscosity.
        /// </summary>
        private static decimal CalculateFluidViscosity(ESPDesignProperties properties)
        {
            // Simplified viscosity calculation
            decimal oilViscosity = 1.5m; // cp (approximate)
            decimal waterViscosity = 1.0m; // cp

            return oilViscosity * (1.0m - properties.WaterCut) + waterViscosity * properties.WaterCut;
        }

        /// <summary>
        /// Calculates total dynamic head.
        /// </summary>
        private static decimal CalculateTotalDynamicHead(
            ESPDesignProperties properties,
            decimal fluidDensity)
        {
            // Static head (depth to pump)
            decimal staticHead = properties.PumpSettingDepth;

            // Friction head (simplified)
            decimal frictionHead = CalculateFrictionHead(properties, fluidDensity);

            // Wellhead pressure head
            decimal wellheadHead = properties.WellheadPressure * 144m / fluidDensity;

            // Total dynamic head
            return staticHead + frictionHead + wellheadHead;
        }

        /// <summary>
        /// Calculates friction head.
        /// </summary>
        private static decimal CalculateFrictionHead(
            ESPDesignProperties properties,
            decimal fluidDensity)
        {
            // Simplified friction calculation
            decimal tubingArea = (decimal)Math.PI * (properties.TubingDiameter / 12m) *
                               (properties.TubingDiameter / 12m) / 4m;
            decimal velocity = (properties.DesiredFlowRate * 5.615m) / (86400m * tubingArea); // ft/s

            decimal reynoldsNumber = fluidDensity * velocity * (properties.TubingDiameter / 12m) / 0.001m;
            decimal frictionFactor = reynoldsNumber < 2000m
                ? 64m / reynoldsNumber
                : 0.3164m / (decimal)Math.Pow((double)reynoldsNumber, 0.25);

            decimal frictionHead = frictionFactor * (properties.PumpSettingDepth / (properties.TubingDiameter / 12m)) *
                                  (velocity * velocity) / (2m * 32.174m);

            return frictionHead;
        }

        /// <summary>
        /// Selects pump stages based on head requirements.
        /// </summary>
        private static int SelectPumpStages(
            ESPDesignProperties properties,
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
        private static List<ESPPumpPoint> GeneratePumpPerformanceCurve(
            ESPDesignProperties properties,
            int pumpStages,
            decimal totalDynamicHead)
        {
            var points = new List<ESPPumpPoint>();
            int numberOfPoints = 20;

            decimal minFlowRate = properties.DesiredFlowRate * 0.5m;
            decimal maxFlowRate = properties.DesiredFlowRate * 1.5m;
            decimal flowStep = (maxFlowRate - minFlowRate) / numberOfPoints;

            for (int i = 0; i <= numberOfPoints; i++)
            {
                decimal flowRate = minFlowRate + i * flowStep;
                decimal head = CalculatePumpHead(flowRate, properties.DesiredFlowRate, totalDynamicHead, pumpStages);
                decimal efficiency = CalculatePumpEfficiency(flowRate, properties.DesiredFlowRate);
                decimal horsepower = CalculatePumpHorsepower(flowRate, head, properties, efficiency);

                points.Add(new ESPPumpPoint
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
            // Pump head curve: H = H0 * (1 - (Q/Q0)²)
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
            ESPDesignProperties properties,
            decimal efficiency)
        {
            decimal fluidDensity = CalculateFluidDensity(properties);
            decimal horsepower = (flowRate * 5.615m / 86400m) * head * fluidDensity / (550m * efficiency);

            return Math.Max(0m, horsepower);
        }

        /// <summary>
        /// Finds operating point on pump curve.
        /// </summary>
        private static ESPPumpPoint FindOperatingPoint(
            List<ESPPumpPoint> performancePoints,
            decimal desiredFlowRate)
        {
            // Find closest point to desired flow rate
            var closestPoint = performancePoints
                .OrderBy(p => Math.Abs(p.FlowRate - desiredFlowRate))
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
        private static ESPMotorProperties SelectMotor(
            decimal requiredHorsepower,
            ESPDesignProperties properties)
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

            return new ESPMotorProperties
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
        private static ESPCableProperties SelectCable(
            ESPMotorProperties motor,
            ESPDesignProperties properties)
        {
            // Cable size selection based on motor current and depth
            decimal motorCurrent = CalculateMotorCurrent(motor, motor.Horsepower);
            decimal cableLength = properties.PumpSettingDepth + 100m; // Extra for surface

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

            return new ESPCableProperties
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
        private static decimal CalculateMotorCurrent(ESPMotorProperties motor, decimal horsepower)
        {
            // I = (HP * 746) / (V * PF * Efficiency)
            decimal powerWatts = horsepower * 746m;
            decimal current = powerWatts / (motor.Voltage * motor.PowerFactor * motor.Efficiency);

            return Math.Max(0m, current);
        }

        /// <summary>
        /// Calculates system efficiency.
        /// </summary>
        private static decimal CalculateSystemEfficiency(
            decimal pumpEfficiency,
            decimal motorEfficiency,
            ESPCableProperties cable)
        {
            // Cable efficiency (voltage drop effect)
            decimal cableEfficiency = 1.0m - (cable.VoltageDrop / 1000m); // Simplified
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


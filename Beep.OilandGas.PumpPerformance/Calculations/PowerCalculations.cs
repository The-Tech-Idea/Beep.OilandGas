using System;
using Beep.OilandGas.PumpPerformance.Constants;
using Beep.OilandGas.PumpPerformance.Exceptions;
using Beep.OilandGas.PumpPerformance.Validation;
using static Beep.OilandGas.PumpPerformance.Constants.PumpConstants;

namespace Beep.OilandGas.PumpPerformance.Calculations
{
    /// <summary>
    /// Provides power calculation methods for pumps.
    /// </summary>
    public static class PowerCalculations
    {
        /// <summary>
        /// Calculates brake horsepower (BHP) required by the pump.
        /// Formula: BHP = (Q * H * SG) / (3960 * η)
        /// </summary>
        /// <param name="flowRate">Flow rate in gallons per minute (GPM).</param>
        /// <param name="head">Head in feet.</param>
        /// <param name="specificGravity">Specific gravity of the fluid (default: 1.0 for water).</param>
        /// <param name="efficiency">Pump efficiency as a decimal (0 to 1). Default: 0.75 (75%).</param>
        /// <returns>Brake horsepower required.</returns>
        /// <exception cref="InvalidInputException">Thrown when input parameters are invalid.</exception>
        public static double CalculateBrakeHorsepower(
            double flowRate,
            double head,
            double specificGravity = WaterSpecificGravity,
            double efficiency = 0.75)
        {
            PumpDataValidator.ValidateFlowRate(flowRate, nameof(flowRate));
            PumpDataValidator.ValidateHead(head, nameof(head));
            PumpDataValidator.ValidateSpecificGravity(specificGravity, nameof(specificGravity));
            PumpDataValidator.ValidateEfficiency(efficiency, nameof(efficiency));

            if (Math.Abs(efficiency) < Epsilon)
                throw new InvalidInputException(nameof(efficiency), 
                    " EFFICIENCY cannot be zero for brake horsepower calculation.");

            double hydraulicPower = (flowRate * head * specificGravity) / HorsepowerConversionFactor;
            return hydraulicPower / efficiency;
        }

        /// <summary>
        /// Calculates hydraulic horsepower (water horsepower).
        /// Formula: WHP = (Q * H * SG) / 3960
        /// </summary>
        /// <param name="flowRate">Flow rate in gallons per minute (GPM).</param>
        /// <param name="head">Head in feet.</param>
        /// <param name="specificGravity">Specific gravity of the fluid (default: 1.0 for water).</param>
        /// <returns>Hydraulic horsepower (water horsepower).</returns>
        /// <exception cref="InvalidInputException">Thrown when input parameters are invalid.</exception>
        public static double CalculateHydraulicHorsepower(
            double flowRate,
            double head,
            double specificGravity = WaterSpecificGravity)
        {
            PumpDataValidator.ValidateFlowRate(flowRate, nameof(flowRate));
            PumpDataValidator.ValidateHead(head, nameof(head));
            PumpDataValidator.ValidateSpecificGravity(specificGravity, nameof(specificGravity));

            return (flowRate * head * specificGravity) / HorsepowerConversionFactor;
        }

        /// <summary>
        /// Calculates motor input power required.
        /// Formula: Motor Power = BHP / Motor Efficiency
        /// </summary>
        /// <param name="brakeHorsepower">Brake horsepower (BHP).</param>
        /// <param name="motorEfficiency">Motor efficiency as a decimal (0 to 1). Default: 0.90 (90%).</param>
        /// <returns>Motor input power in horsepower.</returns>
        /// <exception cref="InvalidInputException">Thrown when input parameters are invalid.</exception>
        public static double CalculateMotorInputPower(
            double brakeHorsepower,
            double motorEfficiency = 0.90)
        {
            PumpDataValidator.ValidatePower(brakeHorsepower, nameof(brakeHorsepower));
            PumpDataValidator.ValidateEfficiency(motorEfficiency, nameof(motorEfficiency));

            if (Math.Abs(motorEfficiency) < Epsilon)
                throw new InvalidInputException(nameof(motorEfficiency), 
                    "Motor efficiency cannot be zero for motor input power calculation.");

            return brakeHorsepower / motorEfficiency;
        }

        /// <summary>
        /// Converts horsepower to kilowatts.
        /// </summary>
        /// <param name="horsepower">Power in horsepower.</param>
        /// <returns>Power in kilowatts.</returns>
        public static double HorsepowerToKilowatts(double horsepower)
        {
            return horsepower * Constants.PumpConstants.HorsepowerToKilowatts;
        }

        /// <summary>
        /// Converts kilowatts to horsepower.
        /// </summary>
        /// <param name="kilowatts">Power in kilowatts.</param>
        /// <returns>Power in horsepower.</returns>
        public static double KilowattsToHorsepower(double kilowatts)
        {
            return kilowatts * Constants.PumpConstants.KilowattsToHorsepower;
        }

        /// <summary>
        /// Calculates power consumption in kilowatts.
        /// </summary>
        /// <param name="flowRate">Flow rate in GPM.</param>
        /// <param name="head">Head in feet.</param>
        /// <param name="specificGravity">Specific gravity (default: 1.0).</param>
        /// <param name="pumpEfficiency">Pump efficiency (default: 0.75).</param>
        /// <param name="motorEfficiency">Motor efficiency (default: 0.90).</param>
        /// <returns>Power consumption in kilowatts.</returns>
        public static double CalculatePowerConsumption(
            double flowRate,
            double head,
            double specificGravity = WaterSpecificGravity,
            double pumpEfficiency = 0.75,
            double motorEfficiency = 0.90)
        {
            double bhp = CalculateBrakeHorsepower(flowRate, head, specificGravity, pumpEfficiency);
            double motorPower = CalculateMotorInputPower(bhp, motorEfficiency);
            return HorsepowerToKilowatts(motorPower);
        }

        /// <summary>
        /// Calculates energy consumption over time.
        /// </summary>
        /// <param name="powerKilowatts">Power consumption in kilowatts.</param>
        /// <param name="hours">Operating hours.</param>
        /// <returns>Energy consumption in kilowatt-hours (kWh).</returns>
        public static double CalculateEnergyConsumption(
            double powerKilowatts,
            double hours)
        {
            if (hours < 0)
                throw new InvalidInputException(nameof(hours), 
                    "Operating hours cannot be negative.");

            return powerKilowatts * hours;
        }
    }
}


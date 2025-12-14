using System;
using Beep.OilandGas.PumpPerformance.Calculations;
using Beep.OilandGas.PumpPerformance.Constants;
using Beep.OilandGas.PumpPerformance.Exceptions;
using Beep.OilandGas.PumpPerformance.Validation;
using static Beep.OilandGas.PumpPerformance.Constants.PumpConstants;

namespace Beep.OilandGas.PumpPerformance.PumpTypes
{
    /// <summary>
    /// Provides specialized calculations for Electric Submersible Pumps (ESP).
    /// </summary>
    public static class SubmersiblePump
    {
        /// <summary>
        /// Calculates total head for an ESP system.
        /// Formula: H_total = H_stage * Number_of_stages
        /// </summary>
        /// <param name="headPerStage">Head per stage in feet.</param>
        /// <param name="numberOfStages">Number of stages.</param>
        /// <returns>Total head in feet.</returns>
        /// <exception cref="InvalidInputException">Thrown when input parameters are invalid.</exception>
        public static double CalculateTotalHead(
            double headPerStage,
            int numberOfStages)
        {
            PumpDataValidator.ValidateHead(headPerStage, nameof(headPerStage));

            if (numberOfStages <= 0)
                throw new InvalidInputException(nameof(numberOfStages), 
                    "Number of stages must be positive.");

            return headPerStage * numberOfStages;
        }

        /// <summary>
        /// Calculates required number of stages for a given total head.
        /// </summary>
        /// <param name="totalHead">Required total head in feet.</param>
        /// <param name="headPerStage">Head per stage in feet.</param>
        /// <returns>Number of stages required (rounded up).</returns>
        /// <exception cref="InvalidInputException">Thrown when input parameters are invalid.</exception>
        public static int CalculateRequiredStages(
            double totalHead,
            double headPerStage)
        {
            PumpDataValidator.ValidateHead(totalHead, nameof(totalHead));
            PumpDataValidator.ValidateHead(headPerStage, nameof(headPerStage));

            if (Math.Abs(headPerStage) < Epsilon)
                throw new InvalidInputException(nameof(headPerStage), 
                    "Head per stage cannot be zero.");

            return (int)Math.Ceiling(totalHead / headPerStage);
        }

        /// <summary>
        /// Calculates motor power requirement for ESP.
        /// Formula: P_motor = (Q * H * SG) / (3960 * η_pump * η_motor)
        /// </summary>
        /// <param name="flowRate">Flow rate in GPM.</param>
        /// <param name="totalHead">Total head in feet.</param>
        /// <param name="specificGravity">Specific gravity (default: 1.0).</param>
        /// <param name="pumpEfficiency">Pump efficiency (default: 0.60 for ESP).</param>
        /// <param name="motorEfficiency">Motor efficiency (default: 0.85).</param>
        /// <returns>Motor power requirement in horsepower.</returns>
        /// <exception cref="InvalidInputException">Thrown when input parameters are invalid.</exception>
        public static double CalculateMotorPower(
            double flowRate,
            double totalHead,
            double specificGravity = WaterSpecificGravity,
            double pumpEfficiency = 0.60,
            double motorEfficiency = 0.85)
        {
            PumpDataValidator.ValidateFlowRate(flowRate, nameof(flowRate));
            PumpDataValidator.ValidateHead(totalHead, nameof(totalHead));
            PumpDataValidator.ValidateSpecificGravity(specificGravity, nameof(specificGravity));
            PumpDataValidator.ValidateEfficiency(pumpEfficiency, nameof(pumpEfficiency));
            PumpDataValidator.ValidateEfficiency(motorEfficiency, nameof(motorEfficiency));

            if (Math.Abs(pumpEfficiency) < Epsilon || Math.Abs(motorEfficiency) < Epsilon)
                throw new InvalidInputException("Efficiency", 
                    "Pump and motor efficiency cannot be zero.");

            double hydraulicPower = (flowRate * totalHead * specificGravity) / HorsepowerConversionFactor;
            return hydraulicPower / (pumpEfficiency * motorEfficiency);
        }

        /// <summary>
        /// Calculates optimal stage count based on production requirements.
        /// </summary>
        /// <param name="requiredFlowRate">Required flow rate in GPM.</param>
        /// <param name="requiredHead">Required total head in feet.</param>
        /// <param name="headPerStage">Head per stage at required flow rate in feet.</param>
        /// <param name="maxStages">Maximum number of stages allowed.</param>
        /// <returns>Optimal number of stages.</returns>
        /// <exception cref="InvalidInputException">Thrown when input parameters are invalid.</exception>
        public static int CalculateOptimalStageCount(
            double requiredFlowRate,
            double requiredHead,
            double headPerStage,
            int maxStages = 400)
        {
            PumpDataValidator.ValidateFlowRate(requiredFlowRate, nameof(requiredFlowRate));
            PumpDataValidator.ValidateHead(requiredHead, nameof(requiredHead));
            PumpDataValidator.ValidateHead(headPerStage, nameof(headPerStage));

            if (maxStages <= 0)
                throw new InvalidInputException(nameof(maxStages), 
                    "Maximum stages must be positive.");

            int calculatedStages = CalculateRequiredStages(requiredHead, headPerStage);
            return Math.Min(calculatedStages, maxStages);
        }

        /// <summary>
        /// Calculates production rate for ESP system.
        /// Formula: Q = (P_motor * η_pump * η_motor * 3960) / (H * SG)
        /// </summary>
        /// <param name="motorPower">Motor power in horsepower.</param>
        /// <param name="totalHead">Total head in feet.</param>
        /// <param name="specificGravity">Specific gravity (default: 1.0).</param>
        /// <param name="pumpEfficiency">Pump efficiency (default: 0.60).</param>
        /// <param name="motorEfficiency">Motor efficiency (default: 0.85).</param>
        /// <returns>Production rate in GPM.</returns>
        /// <exception cref="InvalidInputException">Thrown when input parameters are invalid.</exception>
        public static double CalculateProductionRate(
            double motorPower,
            double totalHead,
            double specificGravity = WaterSpecificGravity,
            double pumpEfficiency = 0.60,
            double motorEfficiency = 0.85)
        {
            PumpDataValidator.ValidatePower(motorPower, nameof(motorPower));
            PumpDataValidator.ValidateHead(totalHead, nameof(totalHead));
            PumpDataValidator.ValidateSpecificGravity(specificGravity, nameof(specificGravity));
            PumpDataValidator.ValidateEfficiency(pumpEfficiency, nameof(pumpEfficiency));
            PumpDataValidator.ValidateEfficiency(motorEfficiency, nameof(motorEfficiency));

            if (Math.Abs(totalHead) < Epsilon)
                throw new InvalidInputException(nameof(totalHead), 
                    "Total head cannot be zero for production rate calculation.");

            double effectivePower = motorPower * pumpEfficiency * motorEfficiency;
            return (effectivePower * HorsepowerConversionFactor) / (totalHead * specificGravity);
        }

        /// <summary>
        /// Calculates power consumption in kilowatts for ESP.
        /// </summary>
        /// <param name="motorPower">Motor power in horsepower.</param>
        /// <returns>Power consumption in kilowatts.</returns>
        public static double CalculatePowerConsumptionKW(double motorPower)
        {
            PumpDataValidator.ValidatePower(motorPower, nameof(motorPower));
            return PowerCalculations.HorsepowerToKilowatts(motorPower);
        }

        /// <summary>
        /// Calculates daily energy consumption for ESP.
        /// </summary>
        /// <param name="motorPower">Motor power in horsepower.</param>
        /// <param name="operatingHours">Operating hours per day (default: 24).</param>
        /// <returns>Energy consumption in kilowatt-hours.</returns>
        public static double CalculateDailyEnergyConsumption(
            double motorPower,
            double operatingHours = 24.0)
        {
            PumpDataValidator.ValidatePower(motorPower, nameof(motorPower));

            if (operatingHours < 0 || operatingHours > 24)
                throw new InvalidInputException(nameof(operatingHours), 
                    "Operating hours must be between 0 and 24.");

            double powerKW = CalculatePowerConsumptionKW(motorPower);
            return PowerCalculations.CalculateEnergyConsumption(powerKW, operatingHours);
        }

        /// <summary>
        /// Calculates ESP efficiency factor (combination of pump and motor efficiency).
        /// </summary>
        /// <param name="pumpEfficiency">Pump efficiency (default: 0.60).</param>
        /// <param name="motorEfficiency">Motor efficiency (default: 0.85).</param>
        /// <returns>Overall ESP efficiency.</returns>
        public static double CalculateOverallEfficiency(
            double pumpEfficiency = 0.60,
            double motorEfficiency = 0.85)
        {
            PumpDataValidator.ValidateEfficiency(pumpEfficiency, nameof(pumpEfficiency));
            PumpDataValidator.ValidateEfficiency(motorEfficiency, nameof(motorEfficiency));

            return pumpEfficiency * motorEfficiency;
        }
    }
}


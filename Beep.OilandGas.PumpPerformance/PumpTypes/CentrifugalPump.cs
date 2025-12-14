using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.PumpPerformance.Calculations;
using Beep.OilandGas.PumpPerformance.Constants;
using Beep.OilandGas.PumpPerformance.Exceptions;
using Beep.OilandGas.PumpPerformance.Validation;
using static Beep.OilandGas.PumpPerformance.Constants.PumpConstants;

namespace Beep.OilandGas.PumpPerformance.PumpTypes
{
    /// <summary>
    /// Provides specialized calculations for centrifugal pumps.
    /// </summary>
    public static class CentrifugalPump
    {
        /// <summary>
        /// Calculates specific speed for a centrifugal pump.
        /// Formula: Ns = (N * √Q) / (H^0.75)
        /// Where N = speed (RPM), Q = flow rate (GPM), H = head per stage (feet)
        /// </summary>
        /// <param name="speed">Pump speed in RPM.</param>
        /// <param name="flowRate">Flow rate in GPM.</param>
        /// <param name="headPerStage">Head per stage in feet.</param>
        /// <returns>Specific speed (dimensionless).</returns>
        /// <exception cref="InvalidInputException">Thrown when input parameters are invalid.</exception>
        public static double CalculateSpecificSpeed(
            double speed,
            double flowRate,
            double headPerStage)
        {
            PumpDataValidator.ValidateFlowRate(flowRate, nameof(flowRate));
            PumpDataValidator.ValidateHead(headPerStage, nameof(headPerStage));

            if (speed <= 0)
                throw new InvalidInputException(nameof(speed), 
                    "Pump speed must be positive.");

            if (Math.Abs(headPerStage) < Epsilon)
                throw new InvalidInputException(nameof(headPerStage), 
                    "Head per stage cannot be zero for specific speed calculation.");

            return (speed * Math.Sqrt(flowRate)) / Math.Pow(headPerStage, 0.75);
        }

        /// <summary>
        /// Calculates total head for a multi-stage centrifugal pump.
        /// Formula: H_total = H_per_stage * Number_of_stages
        /// </summary>
        /// <param name="headPerStage">Head per stage in feet.</param>
        /// <param name="numberOfStages">Number of stages.</param>
        /// <returns>Total head in feet.</returns>
        /// <exception cref="InvalidInputException">Thrown when input parameters are invalid.</exception>
        public static double CalculateMultiStageHead(
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
        /// Calculates impeller tip speed.
        /// Formula: U = (π * D * N) / 720
        /// Where D = diameter (inches), N = speed (RPM)
        /// </summary>
        /// <param name="impellerDiameter">Impeller diameter in inches.</param>
        /// <param name="speed">Pump speed in RPM.</param>
        /// <returns>Tip speed in feet per second.</returns>
        /// <exception cref="InvalidInputException">Thrown when input parameters are invalid.</exception>
        public static double CalculateImpellerTipSpeed(
            double impellerDiameter,
            double speed)
        {
            if (impellerDiameter <= 0)
                throw new InvalidInputException(nameof(impellerDiameter), 
                    "Impeller diameter must be positive.");

            if (speed <= 0)
                throw new InvalidInputException(nameof(speed), 
                    "Pump speed must be positive.");

            return (Math.PI * impellerDiameter * speed) / 720.0; // Convert to ft/s
        }

        /// <summary>
        /// Calculates theoretical head based on impeller tip speed.
        /// Formula: H = U² / (2 * g)
        /// Where U = tip speed (ft/s), g = gravity (ft/s²)
        /// </summary>
        /// <param name="tipSpeed">Impeller tip speed in feet per second.</param>
        /// <returns>Theoretical head in feet.</returns>
        /// <exception cref="InvalidInputException">Thrown when input parameters are invalid.</exception>
        public static double CalculateTheoreticalHeadFromTipSpeed(double tipSpeed)
        {
            if (tipSpeed < 0)
                throw new InvalidInputException(nameof(tipSpeed), 
                    "Tip speed cannot be negative.");

            return Math.Pow(tipSpeed, 2) / (2.0 * GravityFeetPerSecondSquared);
        }

        /// <summary>
        /// Estimates performance degradation factor based on wear.
        /// </summary>
        /// <param name="originalHead">Original head in feet.</param>
        /// <param name="currentHead">Current head in feet.</param>
        /// <returns>Degradation factor (0 to 1, where 1 = no degradation).</returns>
        /// <exception cref="InvalidInputException">Thrown when input parameters are invalid.</exception>
        public static double CalculatePerformanceDegradation(
            double originalHead,
            double currentHead)
        {
            PumpDataValidator.ValidateHead(originalHead, nameof(originalHead));
            PumpDataValidator.ValidateHead(currentHead, nameof(currentHead));

            if (Math.Abs(originalHead) < Epsilon)
                throw new InvalidInputException(nameof(originalHead), 
                    "Original head cannot be zero.");

            double degradationFactor = currentHead / originalHead;
            return Math.Max(0, Math.Min(1, degradationFactor)); // Clamp between 0 and 1
        }

        /// <summary>
        /// Calculates required impeller diameter for a given head and speed.
        /// Formula: D = (720 * √(2 * g * H)) / (π * N)
        /// </summary>
        /// <param name="requiredHead">Required head in feet.</param>
        /// <param name="speed">Pump speed in RPM.</param>
        /// <param name="efficiency">Pump efficiency (default: 0.75).</param>
        /// <returns>Required impeller diameter in inches.</returns>
        /// <exception cref="InvalidInputException">Thrown when input parameters are invalid.</exception>
        public static double CalculateRequiredImpellerDiameter(
            double requiredHead,
            double speed,
            double efficiency = 0.75)
        {
            PumpDataValidator.ValidateHead(requiredHead, nameof(requiredHead));
            PumpDataValidator.ValidateEfficiency(efficiency, nameof(efficiency));

            if (speed <= 0)
                throw new InvalidInputException(nameof(speed), 
                    "Pump speed must be positive.");

            // Account for efficiency
            double theoreticalHead = requiredHead / efficiency;
            double tipSpeed = Math.Sqrt(2.0 * GravityFeetPerSecondSquared * theoreticalHead);
            double diameter = (720.0 * tipSpeed) / (Math.PI * speed);

            return Math.Max(0, diameter);
        }

        /// <summary>
        /// Classifies pump type based on specific speed.
        /// </summary>
        /// <param name="specificSpeed">Specific speed value.</param>
        /// <returns>Pump type classification string.</returns>
        public static string ClassifyPumpType(double specificSpeed)
        {
            if (specificSpeed < 500)
                return "Radial Flow";
            else if (specificSpeed < 2000)
                return "Mixed Flow";
            else if (specificSpeed < 8000)
                return "Axial Flow";
            else
                return "High Specific Speed";
        }
    }
}


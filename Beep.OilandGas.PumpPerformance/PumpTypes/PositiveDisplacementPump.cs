using System;
using Beep.OilandGas.PumpPerformance.Constants;
using Beep.OilandGas.PumpPerformance.Exceptions;
using Beep.OilandGas.PumpPerformance.Validation;
using static Beep.OilandGas.PumpPerformance.Constants.PumpConstants;

namespace Beep.OilandGas.PumpPerformance.PumpTypes
{
    /// <summary>
    /// Provides specialized calculations for positive displacement pumps.
    /// </summary>
    public static class PositiveDisplacementPump
    {
        /// <summary>
        /// Calculates theoretical flow rate for a positive displacement pump.
        /// Formula: Q_theoretical = (V * N * η_v) / 231
        /// Where V = displacement per revolution (in³), N = speed (RPM), η_v = volumetric efficiency
        /// </summary>
        /// <param name="displacementPerRevolution">Displacement per revolution in cubic inches.</param>
        /// <param name="speed">Pump speed in RPM.</param>
        /// <param name="volumetricEfficiency">Volumetric efficiency (default: 0.90).</param>
        /// <returns>Theoretical flow rate in GPM.</returns>
        /// <exception cref="InvalidInputException">Thrown when input parameters are invalid.</exception>
        public static double CalculateTheoreticalFlowRate(
            double displacementPerRevolution,
            double speed,
            double volumetricEfficiency = 0.90)
        {
            if (displacementPerRevolution <= 0)
                throw new InvalidInputException(nameof(displacementPerRevolution), 
                    "Displacement per revolution must be positive.");

            if (speed <= 0)
                throw new InvalidInputException(nameof(speed), 
                    "Pump speed must be positive.");

            PumpDataValidator.ValidateEfficiency(volumetricEfficiency, nameof(volumetricEfficiency));

            return (displacementPerRevolution * speed * volumetricEfficiency) / 231.0;
        }

        /// <summary>
        /// Calculates slip (internal leakage) for a positive displacement pump.
        /// Formula: Slip = Q_theoretical - Q_actual
        /// </summary>
        /// <param name="theoreticalFlowRate">Theoretical flow rate in GPM.</param>
        /// <param name="actualFlowRate">Actual flow rate in GPM.</param>
        /// <returns>Slip in GPM.</returns>
        /// <exception cref="InvalidInputException">Thrown when input parameters are invalid.</exception>
        public static double CalculateSlip(
            double theoreticalFlowRate,
            double actualFlowRate)
        {
            PumpDataValidator.ValidateFlowRate(theoreticalFlowRate, nameof(theoreticalFlowRate));
            PumpDataValidator.ValidateFlowRate(actualFlowRate, nameof(actualFlowRate));

            return Math.Max(0, theoreticalFlowRate - actualFlowRate);
        }

        /// <summary>
        /// Calculates slip percentage.
        /// Formula: Slip % = (Slip / Q_theoretical) * 100
        /// </summary>
        /// <param name="theoreticalFlowRate">Theoretical flow rate in GPM.</param>
        /// <param name="actualFlowRate">Actual flow rate in GPM.</param>
        /// <returns>Slip percentage.</returns>
        /// <exception cref="InvalidInputException">Thrown when input parameters are invalid.</exception>
        public static double CalculateSlipPercentage(
            double theoreticalFlowRate,
            double actualFlowRate)
        {
            PumpDataValidator.ValidateFlowRate(theoreticalFlowRate, nameof(theoreticalFlowRate));
            PumpDataValidator.ValidateFlowRate(actualFlowRate, nameof(actualFlowRate));

            if (Math.Abs(theoreticalFlowRate) < Epsilon)
                throw new InvalidInputException(nameof(theoreticalFlowRate), 
                    "Theoretical flow rate cannot be zero for slip percentage calculation.");

            double slip = CalculateSlip(theoreticalFlowRate, actualFlowRate);
            return (slip / theoreticalFlowRate) * 100.0;
        }

        /// <summary>
        /// Calculates volumetric efficiency from slip.
        /// Formula: η_v = Q_actual / Q_theoretical
        /// </summary>
        /// <param name="theoreticalFlowRate">Theoretical flow rate in GPM.</param>
        /// <param name="actualFlowRate">Actual flow rate in GPM.</param>
        /// <returns>Volumetric efficiency (0 to 1).</returns>
        /// <exception cref="InvalidInputException">Thrown when input parameters are invalid.</exception>
        public static double CalculateVolumetricEfficiency(
            double theoreticalFlowRate,
            double actualFlowRate)
        {
            PumpDataValidator.ValidateFlowRate(theoreticalFlowRate, nameof(theoreticalFlowRate));
            PumpDataValidator.ValidateFlowRate(actualFlowRate, nameof(actualFlowRate));

            if (Math.Abs(theoreticalFlowRate) < Epsilon)
                throw new InvalidInputException(nameof(theoreticalFlowRate), 
                    "Theoretical flow rate cannot be zero for volumetric efficiency calculation.");

            return Math.Min(1.0, actualFlowRate / theoreticalFlowRate);
        }

        /// <summary>
        /// Calculates power for a positive displacement pump.
        /// Formula: P = (Q * ΔP) / (1714 * η)
        /// Where Q = flow rate (GPM), ΔP = pressure differential (psi), η = efficiency
        /// </summary>
        /// <param name="flowRate">Flow rate in GPM.</param>
        /// <param name="pressureDifferential">Pressure differential in psi.</param>
        /// <param name="efficiency">Pump efficiency (default: 0.85).</param>
        /// <returns>Power in horsepower.</returns>
        /// <exception cref="InvalidInputException">Thrown when input parameters are invalid.</exception>
        public static double CalculatePower(
            double flowRate,
            double pressureDifferential,
            double efficiency = 0.85)
        {
            PumpDataValidator.ValidateFlowRate(flowRate, nameof(flowRate));
            PumpDataValidator.ValidateEfficiency(efficiency, nameof(efficiency));

            if (pressureDifferential < 0)
                throw new InvalidInputException(nameof(pressureDifferential), 
                    "Pressure differential cannot be negative.");

            if (Math.Abs(efficiency) < Epsilon)
                throw new InvalidInputException(nameof(efficiency), 
                    " EFFICIENCY cannot be zero for power calculation.");

            return (flowRate * pressureDifferential) / (1714.0 * efficiency);
        }

        /// <summary>
        /// Calculates pulsation frequency for a reciprocating pump.
        /// Formula: f = (N * Number_of_cylinders) / 60
        /// </summary>
        /// <param name="speed">Pump speed in RPM.</param>
        /// <param name="numberOfCylinders">Number of cylinders.</param>
        /// <returns>Pulsation frequency in Hz.</returns>
        /// <exception cref="InvalidInputException">Thrown when input parameters are invalid.</exception>
        public static double CalculatePulsationFrequency(
            double speed,
            int numberOfCylinders)
        {
            if (speed <= 0)
                throw new InvalidInputException(nameof(speed), 
                    "Pump speed must be positive.");

            if (numberOfCylinders <= 0)
                throw new InvalidInputException(nameof(numberOfCylinders), 
                    "Number of cylinders must be positive.");

            return (speed * numberOfCylinders) / 60.0;
        }

        /// <summary>
        /// Calculates displacement per revolution for a reciprocating pump.
        /// Formula: V = (π * D² * L * Number_of_cylinders) / 4
        /// Where D = cylinder diameter (inches), L = stroke length (inches)
        /// </summary>
        /// <param name="cylinderDiameter">Cylinder diameter in inches.</param>
        /// <param name="strokeLength">Stroke length in inches.</param>
        /// <param name="numberOfCylinders">Number of cylinders.</param>
        /// <returns>Displacement per revolution in cubic inches.</returns>
        /// <exception cref="InvalidInputException">Thrown when input parameters are invalid.</exception>
        public static double CalculateDisplacementPerRevolution(
            double cylinderDiameter,
            double strokeLength,
            int numberOfCylinders)
        {
            if (cylinderDiameter <= 0)
                throw new InvalidInputException(nameof(cylinderDiameter), 
                    "Cylinder diameter must be positive.");

            if (strokeLength <= 0)
                throw new InvalidInputException(nameof(strokeLength), 
                    "Stroke length must be positive.");

            if (numberOfCylinders <= 0)
                throw new InvalidInputException(nameof(numberOfCylinders), 
                    "Number of cylinders must be positive.");

            double cylinderVolume = (Math.PI * Math.Pow(cylinderDiameter, 2) * strokeLength) / 4.0;
            return cylinderVolume * numberOfCylinders;
        }

        /// <summary>
        /// Calculates flow rate for a rotary pump.
        /// Formula: Q = (V * N * η_v) / 231
        /// </summary>
        /// <param name="displacementPerRevolution">Displacement per revolution in cubic inches.</param>
        /// <param name="speed">Pump speed in RPM.</param>
        /// <param name="volumetricEfficiency">Volumetric efficiency (default: 0.90).</param>
        /// <returns>Flow rate in GPM.</returns>
        public static double CalculateRotaryPumpFlowRate(
            double displacementPerRevolution,
            double speed,
            double volumetricEfficiency = 0.90)
        {
            return CalculateTheoreticalFlowRate(displacementPerRevolution, speed, volumetricEfficiency);
        }
    }
}


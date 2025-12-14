using System;
using Beep.OilandGas.PumpPerformance.Constants;
using Beep.OilandGas.PumpPerformance.Exceptions;
using Beep.OilandGas.PumpPerformance.Validation;
using static Beep.OilandGas.PumpPerformance.Constants.PumpConstants;

namespace Beep.OilandGas.PumpPerformance.Calculations
{
    /// <summary>
    /// Provides Affinity Laws calculations for pump performance prediction at different speeds and impeller diameters.
    /// </summary>
    public static class AffinityLaws
    {
        /// <summary>
        /// Calculates new flow rate when pump speed changes.
        /// Affinity Law: Q₂ = Q₁ * (N₂ / N₁)
        /// </summary>
        /// <param name="originalFlowRate">Original flow rate in GPM.</param>
        /// <param name="originalSpeed">Original pump speed in RPM.</param>
        /// <param name="newSpeed">New pump speed in RPM.</param>
        /// <returns>New flow rate in GPM.</returns>
        /// <exception cref="InvalidInputException">Thrown when input parameters are invalid.</exception>
        public static double CalculateFlowRateAtNewSpeed(
            double originalFlowRate,
            double originalSpeed,
            double newSpeed)
        {
            PumpDataValidator.ValidateFlowRate(originalFlowRate, nameof(originalFlowRate));

            if (originalSpeed <= 0)
                throw new InvalidInputException(nameof(originalSpeed), 
                    "Original speed must be positive.");

            if (newSpeed <= 0)
                throw new InvalidInputException(nameof(newSpeed), 
                    "New speed must be positive.");

            return originalFlowRate * (newSpeed / originalSpeed);
        }

        /// <summary>
        /// Calculates new head when pump speed changes.
        /// Affinity Law: H₂ = H₁ * (N₂ / N₁)²
        /// </summary>
        /// <param name="originalHead">Original head in feet.</param>
        /// <param name="originalSpeed">Original pump speed in RPM.</param>
        /// <param name="newSpeed">New pump speed in RPM.</param>
        /// <returns>New head in feet.</returns>
        /// <exception cref="InvalidInputException">Thrown when input parameters are invalid.</exception>
        public static double CalculateHeadAtNewSpeed(
            double originalHead,
            double originalSpeed,
            double newSpeed)
        {
            PumpDataValidator.ValidateHead(originalHead, nameof(originalHead));

            if (originalSpeed <= 0)
                throw new InvalidInputException(nameof(originalSpeed), 
                    "Original speed must be positive.");

            if (newSpeed <= 0)
                throw new InvalidInputException(nameof(newSpeed), 
                    "New speed must be positive.");

            double speedRatio = newSpeed / originalSpeed;
            return originalHead * Math.Pow(speedRatio, 2);
        }

        /// <summary>
        /// Calculates new power when pump speed changes.
        /// Affinity Law: P₂ = P₁ * (N₂ / N₁)³
        /// </summary>
        /// <param name="originalPower">Original power in horsepower.</param>
        /// <param name="originalSpeed">Original pump speed in RPM.</param>
        /// <param name="newSpeed">New pump speed in RPM.</param>
        /// <returns>New power in horsepower.</returns>
        /// <exception cref="InvalidInputException">Thrown when input parameters are invalid.</exception>
        public static double CalculatePowerAtNewSpeed(
            double originalPower,
            double originalSpeed,
            double newSpeed)
        {
            PumpDataValidator.ValidatePower(originalPower, nameof(originalPower));

            if (originalSpeed <= 0)
                throw new InvalidInputException(nameof(originalSpeed), 
                    "Original speed must be positive.");

            if (newSpeed <= 0)
                throw new InvalidInputException(nameof(newSpeed), 
                    "New speed must be positive.");

            double speedRatio = newSpeed / originalSpeed;
            return originalPower * Math.Pow(speedRatio, 3);
        }

        /// <summary>
        /// Calculates new flow rate when impeller diameter changes.
        /// Affinity Law: Q₂ = Q₁ * (D₂ / D₁)
        /// </summary>
        /// <param name="originalFlowRate">Original flow rate in GPM.</param>
        /// <param name="originalDiameter">Original impeller diameter in inches.</param>
        /// <param name="newDiameter">New impeller diameter in inches.</param>
        /// <returns>New flow rate in GPM.</returns>
        /// <exception cref="InvalidInputException">Thrown when input parameters are invalid.</exception>
        public static double CalculateFlowRateAtNewDiameter(
            double originalFlowRate,
            double originalDiameter,
            double newDiameter)
        {
            PumpDataValidator.ValidateFlowRate(originalFlowRate, nameof(originalFlowRate));

            if (originalDiameter <= 0)
                throw new InvalidInputException(nameof(originalDiameter), 
                    "Original diameter must be positive.");

            if (newDiameter <= 0)
                throw new InvalidInputException(nameof(newDiameter), 
                    "New diameter must be positive.");

            return originalFlowRate * (newDiameter / originalDiameter);
        }

        /// <summary>
        /// Calculates new head when impeller diameter changes.
        /// Affinity Law: H₂ = H₁ * (D₂ / D₁)²
        /// </summary>
        /// <param name="originalHead">Original head in feet.</param>
        /// <param name="originalDiameter">Original impeller diameter in inches.</param>
        /// <param name="newDiameter">New impeller diameter in inches.</param>
        /// <returns>New head in feet.</returns>
        /// <exception cref="InvalidInputException">Thrown when input parameters are invalid.</exception>
        public static double CalculateHeadAtNewDiameter(
            double originalHead,
            double originalDiameter,
            double newDiameter)
        {
            PumpDataValidator.ValidateHead(originalHead, nameof(originalHead));

            if (originalDiameter <= 0)
                throw new InvalidInputException(nameof(originalDiameter), 
                    "Original diameter must be positive.");

            if (newDiameter <= 0)
                throw new InvalidInputException(nameof(newDiameter), 
                    "New diameter must be positive.");

            double diameterRatio = newDiameter / originalDiameter;
            return originalHead * Math.Pow(diameterRatio, 2);
        }

        /// <summary>
        /// Calculates new power when impeller diameter changes.
        /// Affinity Law: P₂ = P₁ * (D₂ / D₁)³
        /// </summary>
        /// <param name="originalPower">Original power in horsepower.</param>
        /// <param name="originalDiameter">Original impeller diameter in inches.</param>
        /// <param name="newDiameter">New impeller diameter in inches.</param>
        /// <returns>New power in horsepower.</returns>
        /// <exception cref="InvalidInputException">Thrown when input parameters are invalid.</exception>
        public static double CalculatePowerAtNewDiameter(
            double originalPower,
            double originalDiameter,
            double newDiameter)
        {
            PumpDataValidator.ValidatePower(originalPower, nameof(originalPower));

            if (originalDiameter <= 0)
                throw new InvalidInputException(nameof(originalDiameter), 
                    "Original diameter must be positive.");

            if (newDiameter <= 0)
                throw new InvalidInputException(nameof(newDiameter), 
                    "New diameter must be positive.");

            double diameterRatio = newDiameter / originalDiameter;
            return originalPower * Math.Pow(diameterRatio, 3);
        }

        /// <summary>
        /// Scales an entire H-Q curve to a new speed.
        /// </summary>
        /// <param name="originalCurve">Original H-Q curve points.</param>
        /// <param name="originalSpeed">Original pump speed in RPM.</param>
        /// <param name="newSpeed">New pump speed in RPM.</param>
        /// <returns>Scaled H-Q curve points.</returns>
        public static List<HeadQuantityPoint> ScaleCurveToNewSpeed(
            System.Collections.Generic.List<HeadQuantityPoint> originalCurve,
            double originalSpeed,
            double newSpeed)
        {
            if (originalCurve == null)
                throw new ArgumentNullException(nameof(originalCurve));

            if (originalSpeed <= 0)
                throw new InvalidInputException(nameof(originalSpeed), 
                    "Original speed must be positive.");

            if (newSpeed <= 0)
                throw new InvalidInputException(nameof(newSpeed), 
                    "New speed must be positive.");

            var scaledCurve = new System.Collections.Generic.List<HeadQuantityPoint>();
            double speedRatio = newSpeed / originalSpeed;

            foreach (var point in originalCurve)
            {
                double newFlowRate = point.FlowRate * speedRatio;
                double newHead = point.Head * Math.Pow(speedRatio, 2);
                double newPower = point.Power * Math.Pow(speedRatio, 3);
                double newEfficiency = point.Efficiency; // Efficiency typically remains constant

                scaledCurve.Add(new HeadQuantityPoint(newFlowRate, newHead, newEfficiency, newPower));
            }

            return scaledCurve;
        }
    }
}


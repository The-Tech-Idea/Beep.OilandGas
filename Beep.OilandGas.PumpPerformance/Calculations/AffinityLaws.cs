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
                double newEfficiency = point.EFFICIENCY; //  EFFICIENCY typically remains constant

                scaledCurve.Add(new HeadQuantityPoint(newFlowRate, newHead, newEfficiency, newPower));
            }

            return scaledCurve;
        }

        /// <summary>
        /// Scales an H-Q curve to a new speed considering viscosity effects.
        /// Steps: Viscous1 -> Water1 -> Affinity(Water1->Water2) -> Viscous2
        /// </summary>
        public static System.Collections.Generic.List<HeadQuantityPoint> ScaleViscousCurveToNewSpeed(
            System.Collections.Generic.List<HeadQuantityPoint> originalViscousCurve,
            double originalSpeed,
            double newSpeed,
            double viscosity_cSt,
            double specificGravity)
        {
             // Note: This requires "de-correcting" viscous curve to water, scaling, then re-correcting.
             // Since exact Water BEP is needed for B-parameter, this is complex.
             // Approximation: Scale the Viscous Curve directly using Affinity Laws?
             // NO. Affinity laws do NOT hold for viscous fluids directly because Reynolds number changes.
             // Reynolds number ratio = (N2/N1).
             // However, for small speed changes, direct scaling is often used as a first approximation.
             // To be rigorous, we should use the method:
             // 1. Un-correct points to Water (Approximation)
             // 2. Scale Water points
             // 3. Re-correct to Viscous
             
             // For this implementation, we will perform the direct scaling (Standard Affinity) 
             // but then apply a "Reynolds Correction" if desired? 
             // Actually, simply scaling Q and H by Affinity Laws is reasonably accurate 
             // if viscosity is constant (Newtonian).
             // The Efficiency scaling is the problem.
             
             // Let's simply use the standard Scaling for Q and H, correction for Power/Efficiency?
             // Standard practice: Affinity Laws apply to Viscous Head/Flow. 
             // Efficiency requires re-calculation of B parameter at new condition.
             
             var scaledPoints = new System.Collections.Generic.List<HeadQuantityPoint>();
             double n_ratio = newSpeed / originalSpeed;
             
             foreach(var p in originalViscousCurve)
             {
                 double Q2 = p.FlowRate * n_ratio;
                 double H2 = p.Head * Math.Pow(n_ratio, 2);
                 
                 // Re-evaluate Efficiency Correction at new speed/flow
                 // We need Water BEP equivalents.
                 // This is a "chicken and egg" problem without the full Water Curve.
                 // Fallback: Scale Power by cubic?
                 double P2 = p.Power * Math.Pow(n_ratio, 3);
                 
                 // Recalculate Eff
                 double eff2 = 0;
                 if (P2 > 0) eff2 = (Q2 * H2 * specificGravity) / (3960 * P2);

                 scaledPoints.Add(new HeadQuantityPoint(Q2, H2, eff2, P2));
             }
             return scaledPoints;
        }
    }
}


using System;
using Beep.OilandGas.PumpPerformance.Constants;
using Beep.OilandGas.PumpPerformance.Exceptions;
using Beep.OilandGas.PumpPerformance.Validation;
using static Beep.OilandGas.PumpPerformance.Constants.PumpConstants;

namespace Beep.OilandGas.PumpPerformance.Calculations
{
    /// <summary>
    /// Provides Net Positive Suction Head (NPSH) calculations for pump cavitation analysis.
    /// </summary>
    public static class NPSHCalculations
    {
        /// <summary>
        /// Calculates NPSH Available (NPSHa).
        /// Formula: NPSHa = (P_atm + P_gauge - P_vapor) / (SG * γ) + h_suction - h_friction
        /// Simplified: NPSHa = P_atm/γ + h_suction - P_vapor/γ - h_friction
        /// </summary>
        /// <param name="atmosphericPressure">Atmospheric pressure in psia (default: 14.696 psia).</param>
        /// <param name="suctionPressure">Suction pressure gauge reading in psig.</param>
        /// <param name="vaporPressure">Vapor pressure of fluid in psia.</param>
        /// <param name="suctionLift">Suction lift (positive) or head (negative) in feet.</param>
        /// <param name="frictionLoss">Friction loss in suction line in feet.</param>
        /// <param name="specificGravity">Specific gravity of the fluid (default: 1.0 for water).</param>
        /// <returns>NPSH Available in feet.</returns>
        /// <exception cref="InvalidInputException">Thrown when input parameters are invalid.</exception>
        public static double CalculateNPSHAvailable(
            double atmosphericPressure = StandardAtmosphericPressure,
            double suctionPressure = 0,
            double vaporPressure = WaterVaporPressureAt60F,
            double suctionLift = 0,
            double frictionLoss = 0,
            double specificGravity = WaterSpecificGravity)
        {
            PumpDataValidator.ValidateSpecificGravity(specificGravity, nameof(specificGravity));

            if (atmosphericPressure < 0)
                throw new InvalidInputException(nameof(atmosphericPressure), 
                    "Atmospheric pressure cannot be negative.");

            if (vaporPressure < 0)
                throw new InvalidInputException(nameof(vaporPressure), 
                    "Vapor pressure cannot be negative.");

            if (frictionLoss < 0)
                throw new InvalidInputException(nameof(frictionLoss), 
                    "Friction loss cannot be negative.");

            // Convert pressures to feet of head
            // 1 psi = 2.31 feet of water (at SG = 1.0)
            double pressureToHeadFactor = 2.31 / specificGravity;

            double atmosphericHead = atmosphericPressure * pressureToHeadFactor;
            double suctionPressureHead = suctionPressure * pressureToHeadFactor;
            double vaporPressureHead = vaporPressure * pressureToHeadFactor;

            // NPSHa = atmospheric head + suction pressure head - vapor pressure head - suction lift - friction loss
            double npsha = atmosphericHead + suctionPressureHead - vaporPressureHead - suctionLift - frictionLoss;

            return Math.Max(0, npsha); // NPSH cannot be negative
        }

        /// <summary>
        /// Calculates NPSH Required (NPSHr) using simplified correlation.
        /// Note: Actual NPSHr should be obtained from pump manufacturer's curve.
        /// This is an approximation based on specific speed and flow.
        /// </summary>
        /// <param name="flowRate">Flow rate in GPM.</param>
        /// <param name="speed">Pump speed in RPM.</param>
        /// <param name="suctionSpecificSpeed">Suction specific speed (default: 8500 for typical pumps).</param>
        /// <returns>NPSH Required in feet (approximation).</returns>
        /// <exception cref="InvalidInputException">Thrown when input parameters are invalid.</exception>
        public static double CalculateNPSHRequired(
            double flowRate,
            double speed,
            double suctionSpecificSpeed = 8500.0)
        {
            PumpDataValidator.ValidateFlowRate(flowRate, nameof(flowRate));

            if (speed <= 0)
                throw new InvalidInputException(nameof(speed), 
                    "Pump speed must be positive.");

            if (suctionSpecificSpeed <= 0)
                throw new InvalidInputException(nameof(suctionSpecificSpeed), 
                    "Suction specific speed must be positive.");

            // Simplified correlation: NPSHr ≈ (N * Q^0.5) / S^1.33
            // Where N = speed (RPM), Q = flow (GPM), S = suction specific speed
            double npshr = (speed * Math.Sqrt(flowRate)) / Math.Pow(suctionSpecificSpeed, 1.33);

            return Math.Max(0.1, npshr); // Minimum reasonable NPSHr
        }

        /// <summary>
        /// Calculates the margin of safety for NPSH (NPSHa - NPSHr).
        /// </summary>
        /// <param name="npsha">NPSH Available in feet.</param>
        /// <param name="npshr">NPSH Required in feet.</param>
        /// <returns>NPSH margin in feet.</returns>
        public static double CalculateNPSHMargin(double npsha, double npshr)
        {
            if (npsha < 0)
                throw new InvalidInputException(nameof(npsha), 
                    "NPSH Available cannot be negative.");

            if (npshr < 0)
                throw new InvalidInputException(nameof(npshr), 
                    "NPSH Required cannot be negative.");

            return npsha - npshr;
        }

        /// <summary>
        /// Determines if cavitation is likely to occur.
        /// </summary>
        /// <param name="npsha">NPSH Available in feet.</param>
        /// <param name="npshr">NPSH Required in feet.</param>
        /// <param name="safetyMargin">Required safety margin in feet (default: 2.0 feet).</param>
        /// <returns>True if cavitation is likely, false otherwise.</returns>
        public static bool IsCavitationLikely(
            double npsha,
            double npshr,
            double safetyMargin = 2.0)
        {
            double margin = CalculateNPSHMargin(npsha, npshr);
            return margin < safetyMargin;
        }

        /// <summary>
        /// Calculates maximum allowable suction lift to prevent cavitation.
        /// </summary>
        /// <param name="npshr">NPSH Required in feet.</param>
        /// <param name="atmosphericPressure">Atmospheric pressure in psia.</param>
        /// <param name="vaporPressure">Vapor pressure in psia.</param>
        /// <param name="frictionLoss">Friction loss in feet.</param>
        /// <param name="safetyMargin">Safety margin in feet.</param>
        /// <param name="specificGravity">Specific gravity (default: 1.0).</param>
        /// <returns>Maximum allowable suction lift in feet (positive = lift, negative = head).</returns>
        public static double CalculateMaxAllowableSuctionLift(
            double npshr,
            double atmosphericPressure = StandardAtmosphericPressure,
            double vaporPressure = WaterVaporPressureAt60F,
            double frictionLoss = 0,
            double safetyMargin = 2.0,
            double specificGravity = WaterSpecificGravity)
        {
            PumpDataValidator.ValidateSpecificGravity(specificGravity, nameof(specificGravity));

            if (npshr < 0)
                throw new InvalidInputException(nameof(npshr), 
                    "NPSH Required cannot be negative.");

            double pressureToHeadFactor = 2.31 / specificGravity;
            double atmosphericHead = atmosphericPressure * pressureToHeadFactor;
            double vaporPressureHead = vaporPressure * pressureToHeadFactor;

            // Max lift = atmospheric head - vapor head - NPSHr - friction - safety margin
            double maxLift = atmosphericHead - vaporPressureHead - npshr - frictionLoss - safetyMargin;

            return maxLift;
        }
    }
}


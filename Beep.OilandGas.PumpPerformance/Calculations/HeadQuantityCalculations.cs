using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.PumpPerformance.Constants;
using Beep.OilandGas.PumpPerformance.Exceptions;
using Beep.OilandGas.PumpPerformance.Validation;
using static Beep.OilandGas.PumpPerformance.Constants.PumpConstants;

namespace Beep.OilandGas.PumpPerformance.Calculations
{
    /// <summary>
    /// Represents a point on a Head-Quantity (H-Q) curve.
    /// </summary>
    public class HeadQuantityPoint
    {
        /// <summary>
        /// Gets or sets the flow rate in GPM.
        /// </summary>
        public double FlowRate { get; set; }

        /// <summary>
        /// Gets or sets the head in feet.
        /// </summary>
        public double Head { get; set; }

        /// <summary>
        /// Gets or sets the efficiency at this point (0 to 1).
        /// </summary>
        public double  EFFICIENCY { get; set; }

        /// <summary>
        /// Gets or sets the power at this point in horsepower.
        /// </summary>
        public double Power { get; set; }

        public HeadQuantityPoint() { }

        public HeadQuantityPoint(double flowRate, double head, double efficiency = 0, double power = 0)
        {
            FlowRate = flowRate;
            Head = head;
             EFFICIENCY = efficiency;
            Power = power;
        }
    }

    /// <summary>
    /// Provides enhanced Head-Quantity (H-Q) curve calculations.
    /// </summary>
    public static class HeadQuantityCalculations
    {
        /// <summary>
        /// Generates a complete H-Q curve from data points.
        /// </summary>
        /// <param name="flowRates">Array of flow rates in GPM.</param>
        /// <param name="heads">Array of heads in feet.</param>
        /// <param name="powers">Optional array of power values.</param>
        /// <param name="specificGravity">Specific gravity (default: 1.0).</param>
        /// <returns>List of HeadQuantityPoint objects.</returns>
        /// <exception cref="InvalidInputException">Thrown when input data is invalid.</exception>
        public static List<HeadQuantityPoint> GenerateHQCurve(
            double[] flowRates,
            double[] heads,
            double[] powers = null,
            double specificGravity = WaterSpecificGravity)
        {
            PumpDataValidator.ValidateFlowRates(flowRates, nameof(flowRates));
            PumpDataValidator.ValidateHeads(heads, nameof(heads));
            PumpDataValidator.ValidateMatchingLengths(flowRates, heads, nameof(flowRates), nameof(heads));

            var curve = new List<HeadQuantityPoint>();

            for (int i = 0; i < flowRates.Length; i++)
            {
                double efficiency = 0;
                double power = 0;

                if (powers != null && i < powers.Length)
                {
                    power = powers[i];
                    efficiency = EfficiencyCalculations.CalculateOverallEfficiency(
                        flowRates[i], heads[i], powers[i], specificGravity);
                }

                curve.Add(new HeadQuantityPoint(flowRates[i], heads[i], efficiency, power));
            }

            return curve;
        }

        /// <summary>
        /// Calculates head at a specific flow rate using interpolation.
        /// </summary>
        /// <param name="curve">H-Q curve data points.</param>
        /// <param name="flowRate">Flow rate to interpolate.</param>
        /// <returns>Interpolated head value.</returns>
        public static double InterpolateHead(List<HeadQuantityPoint> curve, double flowRate)
        {
            if (curve == null || curve.Count == 0)
                throw new InvalidInputException(nameof(curve), 
                    "Curve data cannot be null or empty.");

            PumpDataValidator.ValidateFlowRate(flowRate, nameof(flowRate));

            // Find surrounding points
            var sortedCurve = curve.OrderBy(p => p.FlowRate).ToList();

            // Check if flow rate is outside range
            if (flowRate <= sortedCurve[0].FlowRate)
                return sortedCurve[0].Head;

            if (flowRate >= sortedCurve[sortedCurve.Count - 1].FlowRate)
                return sortedCurve[sortedCurve.Count - 1].Head;

            // Linear interpolation
            for (int i = 0; i < sortedCurve.Count - 1; i++)
            {
                if (flowRate >= sortedCurve[i].FlowRate && flowRate <= sortedCurve[i + 1].FlowRate)
                {
                    double t = (flowRate - sortedCurve[i].FlowRate) /
                              (sortedCurve[i + 1].FlowRate - sortedCurve[i].FlowRate);
                    return sortedCurve[i].Head + t * (sortedCurve[i + 1].Head - sortedCurve[i].Head);
                }
            }

            return sortedCurve[sortedCurve.Count - 1].Head;
        }

        /// <summary>
        /// Finds the Best  EFFICIENCY Point (BEP) on the H-Q curve.
        /// </summary>
        /// <param name="curve">H-Q curve data points.</param>
        /// <returns>HeadQuantityPoint at the best efficiency point.</returns>
        public static HeadQuantityPoint FindBestEfficiencyPoint(List<HeadQuantityPoint> curve)
        {
            if (curve == null || curve.Count == 0)
                throw new InvalidInputException(nameof(curve), 
                    "Curve data cannot be null or empty.");

            var bep = curve.OrderByDescending(p => p.EFFICIENCY).First();
            return bep;
        }

        /// <summary>
        /// Calculates shutoff head (head at zero flow).
        /// </summary>
        /// <param name="curve">H-Q curve data points.</param>
        /// <returns>Shutoff head in feet.</returns>
        public static double CalculateShutoffHead(List<HeadQuantityPoint> curve)
        {
            if (curve == null || curve.Count == 0)
                throw new InvalidInputException(nameof(curve), 
                    "Curve data cannot be null or empty.");

            // Extrapolate to zero flow using the first two points
            var sortedCurve = curve.OrderBy(p => p.FlowRate).ToList();

            if (sortedCurve.Count < 2)
                return sortedCurve[0].Head;

            // Linear extrapolation: H = H0 + (H1 - H0) * (0 - Q0) / (Q1 - Q0)
            double h0 = sortedCurve[0].Head;
            double h1 = sortedCurve[1].Head;
            double q0 = sortedCurve[0].FlowRate;
            double q1 = sortedCurve[1].FlowRate;

            if (Math.Abs(q1 - q0) < Epsilon)
                return h0;

            double slope = (h1 - h0) / (q1 - q0);
            return h0 - slope * q0;
        }
    }
}


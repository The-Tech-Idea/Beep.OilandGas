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
    /// Represents a point on a system resistance curve.
    /// </summary>
    public class SystemCurvePoint
    {
        /// <summary>
        /// Gets or sets the flow rate in GPM.
        /// </summary>
        public double FlowRate { get; set; }

        /// <summary>
        /// Gets or sets the required head in feet.
        /// </summary>
        public double RequiredHead { get; set; }

        public SystemCurvePoint() { }

        public SystemCurvePoint(double flowRate, double requiredHead)
        {
            FlowRate = flowRate;
            RequiredHead = requiredHead;
        }
    }

    /// <summary>
    /// Provides system curve calculations for pump system analysis.
    /// </summary>
    public static class SystemCurveCalculations
    {
        /// <summary>
        /// Calculates system resistance curve.
        /// Formula: H = H_static + K * Q²
        /// Where H_static is static head and K is system resistance coefficient.
        /// </summary>
        /// <param name="staticHead">Static head in feet (head when flow is zero).</param>
        /// <param name="systemResistanceCoefficient">System resistance coefficient K.</param>
        /// <param name="flowRates">Array of flow rates in GPM to calculate head for.</param>
        /// <returns>List of SystemCurvePoint objects.</returns>
        /// <exception cref="InvalidInputException">Thrown when input parameters are invalid.</exception>
        public static List<SystemCurvePoint> CalculateSystemCurve(
            double staticHead,
            double systemResistanceCoefficient,
            double[] flowRates)
        {
            PumpDataValidator.ValidateHead(staticHead, nameof(staticHead));
            PumpDataValidator.ValidateFlowRates(flowRates, nameof(flowRates));

            if (systemResistanceCoefficient < 0)
                throw new InvalidInputException(nameof(systemResistanceCoefficient), 
                    "System resistance coefficient cannot be negative.");

            var systemCurve = new List<SystemCurvePoint>();

            foreach (var flowRate in flowRates)
            {
                double requiredHead = staticHead + systemResistanceCoefficient * Math.Pow(flowRate, 2);
                systemCurve.Add(new SystemCurvePoint(flowRate, requiredHead));
            }

            return systemCurve;
        }

        /// <summary>
        /// Calculates system resistance coefficient from two known points.
        /// </summary>
        /// <param name="flowRate1">First flow rate in GPM.</param>
        /// <param name="head1">First head in feet.</param>
        /// <param name="flowRate2">Second flow rate in GPM.</param>
        /// <param name="head2">Second head in feet.</param>
        /// <param name="staticHead">Static head in feet.</param>
        /// <returns>System resistance coefficient K.</returns>
        /// <exception cref="InvalidInputException">Thrown when input parameters are invalid.</exception>
        public static double CalculateSystemResistanceCoefficient(
            double flowRate1,
            double head1,
            double flowRate2,
            double head2,
            double staticHead)
        {
            PumpDataValidator.ValidateFlowRate(flowRate1, nameof(flowRate1));
            PumpDataValidator.ValidateFlowRate(flowRate2, nameof(flowRate2));
            PumpDataValidator.ValidateHead(head1, nameof(head1));
            PumpDataValidator.ValidateHead(head2, nameof(head2));
            PumpDataValidator.ValidateHead(staticHead, nameof(staticHead));

            if (Math.Abs(flowRate1 - flowRate2) < Epsilon)
                throw new InvalidInputException(nameof(flowRate2), 
                    "Flow rates must be different to calculate resistance coefficient.");

            double dynamicHead1 = head1 - staticHead;
            double dynamicHead2 = head2 - staticHead;

            double q1Squared = Math.Pow(flowRate1, 2);
            double q2Squared = Math.Pow(flowRate2, 2);

            if (Math.Abs(q1Squared - q2Squared) < Epsilon)
                throw new InvalidInputException(nameof(flowRate2), 
                    "Flow rates must have different squares to calculate resistance coefficient.");

            double k = (dynamicHead1 - dynamicHead2) / (q1Squared - q2Squared);
            return Math.Max(0, k); // Ensure non-negative
        }

        /// <summary>
        /// Finds the operating point where pump curve intersects system curve.
        /// </summary>
        /// <param name="pumpCurve">Pump H-Q curve points.</param>
        /// <param name="systemCurve">System resistance curve points.</param>
        /// <returns>Operating point as (flowRate, head), or null if no intersection found.</returns>
        public static (double flowRate, double head)? FindOperatingPoint(
            List<HeadQuantityPoint> pumpCurve,
            List<SystemCurvePoint> systemCurve)
        {
            if (pumpCurve == null || pumpCurve.Count == 0)
                throw new InvalidInputException(nameof(pumpCurve), 
                    "Pump curve cannot be null or empty.");

            if (systemCurve == null || systemCurve.Count == 0)
                throw new InvalidInputException(nameof(systemCurve), 
                    "System curve cannot be null or empty.");

            // Find intersection by checking where pump head >= system head changes
            var sortedPump = pumpCurve.OrderBy(p => p.FlowRate).ToList();
            var sortedSystem = systemCurve.OrderBy(s => s.FlowRate).ToList();

            for (int i = 0; i < sortedPump.Count - 1; i++)
            {
                for (int j = 0; j < sortedSystem.Count - 1; j++)
                {
                    // Check if curves intersect in this segment
                    double pumpHead1 = sortedPump[i].Head;
                    double pumpHead2 = sortedPump[i + 1].Head;
                    double systemHead1 = sortedSystem[j].RequiredHead;
                    double systemHead2 = sortedSystem[j + 1].RequiredHead;

                    double pumpFlow1 = sortedPump[i].FlowRate;
                    double pumpFlow2 = sortedPump[i + 1].FlowRate;
                    double systemFlow1 = sortedSystem[j].FlowRate;
                    double systemFlow2 = sortedSystem[j + 1].FlowRate;

                    // Check if segments overlap in flow rate
                    double minFlow = Math.Max(pumpFlow1, systemFlow1);
                    double maxFlow = Math.Min(pumpFlow2, systemFlow2);

                    if (minFlow <= maxFlow)
                    {
                        // Interpolate both curves at midpoint
                        double testFlow = (minFlow + maxFlow) / 2.0;
                        double pumpHead = HeadQuantityCalculations.InterpolateHead(sortedPump, testFlow);
                        double systemHead = InterpolateSystemHead(sortedSystem, testFlow);

                        // Check if they're close (within tolerance)
                        if (Math.Abs(pumpHead - systemHead) < 0.1)
                        {
                            return (testFlow, (pumpHead + systemHead) / 2.0);
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Interpolates system head at a specific flow rate.
        /// </summary>
        private static double InterpolateSystemHead(List<SystemCurvePoint> systemCurve, double flowRate)
        {
            var sorted = systemCurve.OrderBy(s => s.FlowRate).ToList();

            if (flowRate <= sorted[0].FlowRate)
                return sorted[0].RequiredHead;

            if (flowRate >= sorted[sorted.Count - 1].FlowRate)
                return sorted[sorted.Count - 1].RequiredHead;

            for (int i = 0; i < sorted.Count - 1; i++)
            {
                if (flowRate >= sorted[i].FlowRate && flowRate <= sorted[i + 1].FlowRate)
                {
                    double t = (flowRate - sorted[i].FlowRate) /
                              (sorted[i + 1].FlowRate - sorted[i].FlowRate);
                    return sorted[i].RequiredHead + t * (sorted[i + 1].RequiredHead - sorted[i].RequiredHead);
                }
            }

            return sorted[sorted.Count - 1].RequiredHead;
        }
    }
}

